# 
# Stats.py
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

from threading import RLock
from Tally import Tally

class Stats:
    clientRequestsTally = Tally()
    clientTimeWaitedTally = Tally()
    usedMemoryTally = Tally()
    _reconstructedFilesTally = Tally()
    lostSwitchMessagesTally = Tally()

    def __init__(self):
        self._clientRequestsTmpTally = Tally()
        self._clientTimeWaitedTmpTally = Tally()
        self._usedMemoryTmpTally = Tally()
        self._reconstructedFiles = 0
        self._lostSwitchMessages = 0

    def clientRequestsAvg(self): 
        return self._clientRequestsTmpTally.mean()

    def clientRequestsTotalAvg(self):
        return Stats.clientRequestsTally.mean()

    def addClientRequests(self, n):
        Stats.clientRequestsTally.observe(n)
        self._clientRequestsTmpTally.observe(n)

    def clientTimeWaitedAvg(self):
        return self._clientTimeWaitedTmpTally.mean()

    def clientTimeWaitedTotalAvg(self):
        return Stats.clientTimeWaitedTally.mean()

    def addClientTimeWaited(self, t):
        Stats.clientTimeWaitedTally.observe(t)
        self._clientTimeWaitedTmpTally.observe(t)

    def usedMemoryAvg(self):
        return int(self._usedMemoryTmpTally.mean())

    def usedMemoryTotalAvg(self):
        mem = int(Stats.usedMemoryTally.mean())
        Stats.usedMemoryTally.reset()
        return mem

    def addUsedMemory(self, m):
        Stats.usedMemoryTally.observe(m)
        self._usedMemoryTmpTally.observe(m)
        
    def reconstructedFiles(self):
    	return self._reconstructedFiles

    def lostSwitchMessages(self):
        return self._lostSwitchMessages
       
    def reconstructedFilesAvg(self):
    	return Stats._reconstructedFilesTally.mean()

    def lostSwitchMessagesAvg(self):
        return Stats.lostSwitchMessagesTally.mean()

    def fileReconstructed(self):
        self._reconstructedFiles += 1

    def messageLost(self):
        self._lostSwitchMessages += 1
    
    # Called when each simulation has ended.
    def tmpReset(self):
        Stats._reconstructedFilesTally.observe(self._reconstructedFiles)
        Stats.lostSwitchMessagesTally.observe(self._lostSwitchMessages)

    # Called when all simulations have ended.
    def reset(self):
        Stats.clientRequestsTally.reset()
        Stats.clientTimeWaitedTally.reset()
        Stats._reconstructedFilesTally.reset()
        Stats.lostSwitchMessagesTally.reset()