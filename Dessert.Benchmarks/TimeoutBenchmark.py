# 
# TimeoutBenchmark.py
#  
# Author(s):
#       Alessio Parma <alessio.parma@gmail.com>
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

import gc
import simpy
import sys
import time
from Common import *
from Tally import Tally

def timeoutBenchmark_PEM(env, counter):
    while True:
        yield env.timeout(counter.randomDelay())
        counter.increment()

def timeoutBenchmark(processCount):
    counterTally = Tally()
    memoryTally = Tally()
    for i in range(repetitionCount):
        gc.collect() 
        start = time.time()
        env = simpy.Environment()
        tally = Tally()
        env.process(memoryRecorder(env, tally))
        counter = Counter()
        for i in range(processCount):
            env.process(timeoutBenchmark_PEM(env, counter))
        env.run(until=simTime)
        end = time.time()
        counterTally.observe(counter.total() / (end-start))
        memoryTally.observe(tally.mean())
    return Result(counterTally.mean(), memoryTally.mean())

def run():
    outputName = "timeout-benchmark-{0}.csv".format(tag)
    output = open(outputName, "w")
    print("TIMEOUT BENCHMARK - SIMPY")
    print("* Warming up...")
    for i in range(repetitionCount):  
        timeoutBenchmark(processCounts[0])
    for processCount in processCounts:  
        result = timeoutBenchmark(processCount)
        evPerSec = result.eventCount()
        avgMemUsage = result.averageMemUsage()
        print("* %d processes: %d timeouts/sec, %d MB" % (processCount, evPerSec, avgMemUsage))
        sys.stdout.flush()
        output.write("{0};{1};{2}\n".format(processCount, int(evPerSec), int(avgMemUsage)))
        output.flush()
    output.close()