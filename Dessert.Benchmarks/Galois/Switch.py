# 
# Switch.py
#  
# Author(s):
#       Alessio Parma <alessio.parma@gmail.com>
#       Artur Tolstenco <tartur88@gmail.com>
# 
# Copyright (c) 2012-2014 Alessio Parma <alessio.parma@gmail.com>
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

from simpy import *
from simpy.resources.store import Store
from Common import *
from Packets import *

class Switch(Entity):
    
    def __init__(self, env, g):
        Entity.__init__(self, env, g)
        self._frames = Store(env, G.frameCount)
    
    def run(self):
        while True:
            p = yield self._frames.get()
            # Switch is awakened again by the Receive method.
            # In Python 3.3, following instruction can be replaced with "yield from".
            for ev in self.waitForSend(p): yield ev
            self._send(p);

    def receive(self, udpPacket):
        if len(self._frames.items) == G.frameCount:
            self.logger.info("Lost a packet at %g!", self.env.now)
            self.g.stats.messageLost()
        else:
            self.logger.info("Packet %s incoming", udpPacket)
            self._frames.put(udpPacket)

    def _send(self, udpPacket):
        if udpPacket.type == REQUEST_TYPE:
            self.g.serverOSes[udpPacket.dst].receive(udpPacket)
        else:
            assert udpPacket.type == ANSWER_TYPE
            self.g.clientOSes[udpPacket.dst].receive(udpPacket)

    def __str__(self):
        return "Switch[usedFrames: {0}]".format(self._frames.count)