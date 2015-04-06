# 
# Client.py
#  
# Author(s):
#       Alessio Parma <alessio.parma@gmail.com>
#       Artur Tolstenco <tartur88@gmail.com>
# 
# Copyright (c) 2012-2016 Alessio Parma <alessio.parma@gmail.com>
# 
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.

from Common import *
from Packets import *

class ServerInfo:
    def __init__(self, id):
        self.ID = id
        self.isUp = True

    def __str__(self):
        return "ServerInfo[ID: {0}; isUp: {1}]".format(self.ID, self.isUp)

class Client(Entity):

    def __init__(self, env, g, id):
        Entity.__init__(self, env, g)
        self._id = id
        self._peers = [None]*(G.machineCount-1)
        self._receivedPackets = []
        self._packetConstructed = env.event()
        for i in range(1, G.machineCount):
            idx = (i+id) % G.machineCount
            self._peers[i-1] = ServerInfo(idx)

    def receive(self, packets):
        for codePacket in packets:
            self._receivedPackets.append(codePacket)
        if len(self._receivedPackets) >= G.requestCount and not self._packetConstructed.triggered:
            self._packetConstructed.succeed()
        
    def setPeersDown(self):
        for peer in self._peers:
            if peer.isUp and len(filter(lambda r: r.keeper == peer.ID, self._receivedPackets)):
                peer.isUp = False

    def run(self):
        while True:
            self.logger.info("Deciding whether to sleep or not")
            if self.g.random.random() < G.clientSleepProb:
                w = self.g.random.expovariate(1.0/G.clientStopMean)
                self.logger.info("Going inactive, holding for %s", ts(w))
                yield self.env.timeout(w)

            #  We reset the state of all peers, generating a new sessionId for the request;
            #  after that, we send the request to G.requestCount + G.extraRequestCount 
            #  peers starting after this node.
            for peer in self._peers:
                peer.isUp = True
            self._receivedPackets = []

            sessionId = "{0}-{1}".format(self._id, self.g.random.randint(0, 10000))
            reqCount = G.requestCount + G.extraRequestCount
            nextServer = 0

            # To collect statistics
            totalReqCount = 0
            startTime = self.env.now

            while reqCount > G.extraRequestCount:
                self.logger.info("Gathered code packets: %d", len(self._receivedPackets))
                counter = 0
                for s in range(nextServer, G.machineCount+nextServer-1):
                    dst = self._peers[s % (G.machineCount-1)]
                    if counter < reqCount:
                        if not dst.isUp: continue
                        self.logger.info("Sending request to %s", dst)
                        udpPacket = UdpPacket(sessionId, self._id, dst.ID, G.requestSize, REQUEST_TYPE, 1)
                        self.g.clientOSes[self._id].send(udpPacket)
                        counter += 1
                    else:
                        nextServer = s % (G.machineCount-1)
                        break

                self.logger.info("Starting timeout")
                if not self._packetConstructed.triggered:
                    yield self._packetConstructed | self.env.timeout(G.timeout*counter*8)
                self._packetConstructed = self.env.event()

                self.setPeersDown()
                reqCount = G.requestCount + G.extraRequestCount - len(self._receivedPackets)
                totalReqCount += counter

            # At this point we surely have:
            # len(self._receivedPackets) >= G.requestCount
            # So, we can reconstruct the data.
            self.g.stats.addClientTimeWaited(self.env.now - startTime)
            self.g.stats.addClientRequests(totalReqCount)
            self.g.stats.fileReconstructed()

class ClientOS(BaseOS):

    def __init__(self, env, g, osId):
        BaseOS.__init__(self, env, g, osId)
        self._arrivedFrames = []
        self._sendRequests = Store(env)
        self._currSessionId = None
           
    def send(self, udpPacket):
        self._currSessionId = udpPacket.sessionId
        self._sendRequests.put(udpPacket)

    def run(self):
        incoming = self.incomingFrames.get()
        send = self._sendRequests.get()
        while True:
            if not incoming.triggered and not send.triggered:
                yield incoming | send
            if incoming.triggered:
                p = incoming.value
                if p.sessionId == self._currSessionId:
                    self.logger.info("Received a new frame: %s", p)
                    self._arrivedFrames.append(p)
                    self.logger.info("Reconstructing packets from %d frames", len(self._arrivedFrames))
                    packets = self.reconstructPackets()
                    if len(packets) > 0:
                        self.g.clients[self.ID].receive(packets)
                        self.logger.info("Reconstructed and sent %d packets", len(packets))
                incoming = self.incomingFrames.get()
            if send.triggered:
                p = send.value
                # In Python 3.3, following instruction can be replaced with "yield from".
                for ev in self.waitForSend(p): yield ev
                self.g.switch.receive(p)
                send = self._sendRequests.get()

    def reconstructPackets(self):
        visited = set()
        usedServers = set()
        unusedFrames = []
        codePackets = []

        for f in self._arrivedFrames:
            if f.src in visited:
                if f.src not in usedServers:
                    unusedFrames.append(f)
                continue

            visited.add(f.src)
            frameCount = f.count
            actualCount = len(filter(lambda p: p.src == f.src, self._arrivedFrames))

            #  If there are enough UDP packets to make a code packet,
            #  then we put the new code packet into array.
            if actualCount >= frameCount:
                usedServers.add(f.src)
                cp = CodePacket(self.ID, f.src, f.len)
                codePackets.append(cp)
            else:
                unusedFrames.append(f)

        self._arrivedFrames = unusedFrames
        return codePackets