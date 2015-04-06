# 
# Common.py
#  
# Author(s):
#       Alessio Parma <alessio.parma@gmail.com>
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

import random
from Galois.MemoryRecorder import memory_usage

simTime = 1000
memRecFreq = simTime/5.0
minTimeout = simTime/100.0
maxTimeout = simTime/20.0
repetitionCount = 21
processCounts = range(500, 20500, 500)

import platform
if platform.system().lower().startswith("linux"):
    tag = "simpy-linux"
else:
    tag = "simpy-windows"

class Counter:
    def __init__(self):
        self._random = random.Random()
        self._total = 0

    def total(self):
        return self._total
    
    def increment(self):
        self._total += 1

    def randomDelay(self):
        return self._random.uniform(minTimeout, maxTimeout)

class Result:
    def __init__(self, eventCount, avgMemUsage):
        self._eventCount = eventCount
        self._avgMemUsage = avgMemUsage
    
    def eventCount(self):
        return self._eventCount
    
    def averageMemUsage(self):
        return self._avgMemUsage

def memoryRecorder(env, tally):
    while True:
        yield env.timeout(memRecFreq)
        tally.observe(memory_usage())
