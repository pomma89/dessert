# 
# Starter.py
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
import sys
import time
import MemoryRecorder
from simpy import *
from Client import *
from Common import *
from Packets import CodePacket
from Server import *
from Stats import Stats
from Switch import *

def memoryRecorder(env, stats):
    while True:
        yield env.timeout(G.memoryRecordingFrequency)
        stats.addUsedMemory(MemoryRecorder.memory_usage())

def runSimulations(machineCount, frameCount):
    G.machineCount = machineCount
    G.requestCount = machineCount/2
    G.frameCount = frameCount
    outputName = "output-mc{0}-fc{1}.txt".format(G.machineCount, G.frameCount)
    output = open(outputName, "w")
    for h in range(G.machineCount - G.requestCount):
        G.extraRequestCount = h
        print("### Simulating with h = {0}".format(h))
        for i in range(G.simCount):
            runSimulation(i, h)
        printTotalStats(h, output)
        output.flush()
    output.close()
    stats = Stats.Stats()
    return stats.usedMemoryTotalAvg()

def runSimulation(simId, h):
    seed = (simId+1)*(h+1) + time.clock()
    env = Environment()
    g = G(seed)

    # Processes creation (and activation)
    # 1. The switch
    g.switch = Switch(env, g)
    env.process(g.switch.run())
    g.switch.setLogger("SWITCH")
    for i in range(G.machineCount):
        ID = str(i)
        # 2. The server operative systems
        svOS = ServerOS(env, g, i)
        env.process(svOS.run())
        svOS.setLogger("SERVER_OS_" + ID)
        g.serverOSes[i] = svOS
        # 3. The server processes
        sv = Server(env, g, i)
        env.process(sv.run())
        sv.setLogger("SERVER_" + ID)
        g.servers[i] = sv
        # 4. The client operative systems
        clOS = ClientOS(env, g, i)
        env.process(clOS.run())
        clOS.setLogger("CLIENT_OS_" + ID)
        g.clientOSes[i] = clOS
        # 5. The client processes
        cl = Client(env, g, i)
        env.process(cl.run())
        cl.setLogger("CLIENT_" + ID)
        g.clients[i] = cl
    # 6. The memory recorder
    env.process(memoryRecorder(env, g.stats))

    print("Starting simulation {0} (h = {1})".format(simId, h))
    storePackets(g.servers)
    env.run(until = G.maxSimTime)
    assert env.now >= G.maxSimTime, "Ended prematurely!"
    printStats(g.stats, simId, h)

def printStats(stats, simId, h):
    print("Stats for simulation {0} (h = {1}):".format(simId, h))
    cr = stats.clientRequestsAvg()
    print(" * Average client requests: {0:.1f}".format(cr))
    ct = stats.clientTimeWaitedAvg()/1000
    print(" * Average client time waited: {0:.1f} ms".format(ct))
    rf = stats.reconstructedFiles()
    print(" * Reconstructed files: {0}".format(rf))
    lm = stats.lostSwitchMessages()
    print(" * Lost switch messages: {0}".format(lm))
    um = stats.usedMemoryAvg()
    print(" * Average used memory: {0} MB".format(um))
    stats.tmpReset()
    sys.stdout.flush()

def printTotalStats(h, out):
    stats = Stats.Stats()
    print("Total stats for simulation with h = {0}:".format(h))
    cr = stats.clientRequestsTotalAvg()
    print(" * Average client requests: {0:.1f}".format(cr))
    ct = stats.clientTimeWaitedTotalAvg()/1000
    print(" * Average client time waited: {0:.1f} ms".format(ct))
    rf = stats.reconstructedFilesAvg()
    print(" * Average reconstructed files: {0:.0f}".format(rf))
    lm = stats.lostSwitchMessagesAvg()
    print(" * Average lost switch messages: {0:.0f}".format(lm))
    out.write("{0} {1} {2} {3} {4}\n".format(h, cr, ct, rf, lm))
    stats.reset()
    sys.stdout.flush()

def storePackets(servers):
    splitSize = G.fileSize/G.requestCount
    for i in range(G.machineCount):
        for j in range(G.machineCount):
            if i == j: continue
            cp = CodePacket(i, j, splitSize)
            servers[j].storeCodePacket(cp)

if __name__ == "__main__":
    # logging.basicConfig(format='%(name)s:\t%(message)s', filename='Galois.log', level=logging.INFO)
    runSimulations(machineCount = 16, frameCount = 128)