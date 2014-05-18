# 
# ProducerConsumerBenchmark.py
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
from simpy import *
import sys
import time
from Common import *
from Tally import Tally

def prodConsBenchmark_Consumer(env, store, counter):
    while True:
        yield env.timeout(counter.randomDelay())
        yield store.get()
        counter.increment()

def prodConsBenchmark_Producer(env, store, counter):
    while True:
        yield env.timeout(counter.randomDelay())
        yield store.put(counter.randomDelay())
        counter.increment()

def prodConsBenchmark(processCount):
    counterTally = Tally()
    memoryTally = Tally()
    for i in range(repetitionCount): 
        gc.collect()
        start = time.time()
        env = Environment()
        store = Store(env)
        tally = Tally()
        env.process(memoryRecorder(env, tally))
        counter = Counter()
        for i in range(processCount/2):
            env.process(prodConsBenchmark_Consumer(env, store, counter))
        for i in range(processCount/2, processCount):
            env.process(prodConsBenchmark_Producer(env, store, counter))
        env.run(until=simTime)
        end = time.time()
        counterTally.observe(counter.total() / (end-start))
        memoryTally.observe(tally.mean())
    return Result(counterTally.mean(), memoryTally.mean())

def run():
    outputName = "prodcons-benchmark-{0}.csv".format(tag)
    output = open(outputName, "w")
    print("PRODUCER CONSUMER BENCHMARK - SIMPY")
    print("* Warming up...")
    for i in range(repetitionCount):  
		prodConsBenchmark(processCounts[0])
    for processCount in processCounts:  
        result = prodConsBenchmark(processCount)
        evPerSec = result.eventCount()
        avgMemUsage = result.averageMemUsage()
        print("* %d processes: %d storeEvents/sec, %d MB" % (processCount, evPerSec, avgMemUsage))
        sys.stdout.flush()
        output.write("{0};{1};{2}\n".format(processCount, int(evPerSec), int(avgMemUsage)))
        output.flush()
    output.close()