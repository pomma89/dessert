# 
# Server.py
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

import math
from simpy import *
from Common import *
from Packets import *

class AnswerInfo:
    def __init__(self, codePacket, sessionId):
        self.packet = codePacket
        self.sessionId = sessionId

class Server(Entity):

    def __init__(self, env, g, id):
        Entity.__init__(self, env, g)
        self._id = id
        self._isUp = True
        self._requests = Store(env)
        self._cache = {}
        self._packets = {}

    def receive(self, newReqPacket):
        if not self._isUp: return
        self._requests.put(newReqPacket)

    def run(self):
        while True:
            rp = yield self._requests.get()
            self.logger.info("Handling request %s", rp)

            # We gather the requested code packet
            if rp.owner in self._cache:
                cp = self._cache[rp.owner]
                self.logger.info("Gathering %s from cache, holding for %s", cp, ts(G.cacheAccessTime))
                yield self.env.timeout(G.cacheAccessTime)
            else:
                # Not in cache, packet must be recovered from disk
                cp = self._cache[rp.owner] = self._packets[rp.owner]
                w = self.g.random.uniform(0, G.maxAccessTime)
                self.logger.info("Gathering %s from disk and caching it, holding for %s", cp, ts(w))
                yield self.env.timeout(w)

            self.g.serverOSes[self._id].send(cp, rp.sessionId)  

            # With some probability, the server goes down for some time
            if self.g.random.random() >= G.serverDownProb: continue
            self._isUp = False
            w = self.g.random.expovariate(1.0/G.serverStopMean)
            self.logger.info("Going offline, holding for %s", ts(w))
            yield self.env.timeout(w)
            self._cache = {}
            self._isUp = True

    def storeCodePacket(self, codePacket):
        self._packets[codePacket.owner] = codePacket

class ServerOS(BaseOS):

    def __init__(self, env, g, osId):
        BaseOS.__init__(self, env, g, osId)
        self._sendRequests = Store(env)
    
    def run(self):
        incoming = self.incomingFrames.get()
        send = self._sendRequests.get()
        while True:
            if not incoming.triggered and not send.triggered:
                yield incoming | send
            if incoming.triggered:
                packet = incoming.value
                reqPacket = RequestPacket(packet.src, packet.sessionId)
                self.logger.info("Sending packet %s to server", reqPacket)
                self.g.servers[self.ID].receive(reqPacket)
                incoming = self.incomingFrames.get()
            if not send.triggered:
                continue

            sr = send.value
            cp = sr.packet
            sessionId = sr.sessionId

            # At this point we need to break the code packet into some UDP packets and send them
            nUdp = int(math.ceil(cp.len/G.MTU))
            lastPacketSize = cp.len - nUdp*G.MTU
            if lastPacketSize != 0: nUdp += 1
            self.logger.info("Breaking packet into %d UDP packets", nUdp)

            for i in range(nUdp-1):
                p = UdpPacket(sessionId, self.ID, cp.owner, G.MTU, ANSWER_TYPE, nUdp)
                # In Python 3.3, following instruction can be replaced with "yield from".
                for ev in self.waitForSend(p): yield ev
                self.g.switch.receive(p)

            # Must send the remaining part of the packet
            if lastPacketSize == 0:
                p = UdpPacket(sessionId, self.ID, cp.owner, G.MTU, ANSWER_TYPE, nUdp)
            else:
                p = UdpPacket(sessionId, self.ID, cp.owner, lastPacketSize, ANSWER_TYPE, nUdp)
            # In Python 3.3, following instruction can be replaced with "yield from".
            for ev in self.waitForSend(p): yield ev
            self.g.switch.receive(p)
            send = self._sendRequests.get()

    def send(self, codePacket, sessionId):
        self._sendRequests.put(AnswerInfo(codePacket, sessionId))