# 
# GaloisBenchmark.py
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

import time
from Common import tag
from Galois.Starter import runSimulations

parameters = [(4, 64), (8, 96), (16, 128), (32, 256)]

def run():
    outputName = "galois-benchmark-{0}.csv".format(tag)
    output = open(outputName, "w")
    for (machineCount, frameCount) in parameters:
        print("### Running Galois with (mc = {0}, fc = {1}) ###".format(machineCount, frameCount))
        start = time.time()
        usedMemory = runSimulations(machineCount, frameCount)
        end = time.time()
        execTime = (end-start)/60.0
        output.write("{0};{1};{2}\n".format(machineCount, execTime, int(usedMemory)))
        output.flush()
        print("### Execution time: {0} minutes ###".format(execTime))
        print("### Used memory: {0} MB ###".format(usedMemory))
        print("")
    output.close()