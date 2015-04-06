# 
# Common.py
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

import logging
import random
import simpy
import Stats
from simpy.resources.store import Store

ANSWER_TYPE = 0
REQUEST_TYPE = 1

def ts(waitTime):
    return "{0} us".format(waitTime)

class G:
    # Quantities
    simCount = 20 # It should be a multiple of threadCount
    threadCount = 4
    requestCount = 8
    machineCount = 16
    frameCount = 128
    extraRequestCount = 0 # Between 0 and (MachineCount-RequestCount-1)

    # Sizes (in bytes)
    fileSize = 64*1024
    MTU = 1024.0
    requestSize = 64

    # Probabilities (between 0 and 1)
    clientSleepProb = 0.3
    serverDownProb = 0.6

    # Utilities
    seconds = 1000000
    milliseconds = 1000
    microseconds = 1
    nanoseconds = 0.001

    # Times
    maxSimTime = 10*seconds
    memoryRecordingFrequency = maxSimTime/10.0
    clientStopMean = 25*milliseconds # Exponential
    serverStopMean = 15*milliseconds # Exponential
    timeout = 1.5*milliseconds # Fixed
    cacheAccessTime = 100*microseconds # Fixed
    maxAccessTime = 1*microseconds # Fixed
    latency = 100*nanoseconds # Fixed
    bandwidth = 11*1024*1024 / (1.0*seconds) # Bandwidth MB/s = 11 * 1024^2 B / 1 sec

    def __init__(self, seed):
        self.clientOSes = [None]*G.machineCount
        self.clients = [None]*G.machineCount
        self.serverOSes = [None]*G.machineCount
        self.servers = [None]*G.machineCount
        self.switch = None
        self.stats = Stats.Stats()
        self.random = random.Random(seed)

class Entity:
    def __init__(self, env, g):
        self.env = env
        self.g = g

    def setLogger(self, name):
        self.logger = logging.getLogger(name)

    def waitForSend(self, packet):
        w = G.latency + packet.len/G.bandwidth;
        self.logger.info("Sending %s, waiting for %s", packet, ts(w));
        yield self.env.timeout(w)

class BaseOS(Entity):
    def __init__(self, env, g, osID):
        Entity.__init__(self, env, g)
        self.ID = osID
        self.incomingFrames = Store(env)

    def receive(self, udpPacket):
        self.incomingFrames.put(udpPacket)