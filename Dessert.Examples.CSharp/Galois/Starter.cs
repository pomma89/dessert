//
// Galois.cs
//  
// Author(s):
//       Alessio Parma <alessio.parma@gmail.com>
//       Artur Tolstenco <tartur88@gmail.com>
// 
// Copyright (c) 2012-2014 Alessio Parma <alessio.parma@gmail.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace Dessert.Examples.CSharp.Galois
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    public static class Starter
    {
        public static int Run(short machineCount = 16, short frameCount = 128)
        {
            G.SetParameters(machineCount, frameCount);
            RunSimulations();
            return Stats.MemoryUsedTotalAvg();
        }

        static void RunSimulations()
        {
            var outputName = string.Format("output-mc{0}-fc{1}.txt", G.MachineCount, G.FrameCount);
            var results = new StreamWriter(outputName);
            for (var h = 0; h < G.MachineCount - G.RequestCount; ++h) {
                G.ExtraRequestCount = h;
                Console.WriteLine("### Simulating with h = {0}", h);
                for (var s = 0; s < G.SimCount; ++s) {
                    RunSimulation(s, h);
                }
                PrintTotalStats(h, results);
                results.Flush();
            }
            results.Close();
        }

        static void RunSimulation(int simId, int h)
        {
            var seed = (simId + 1)*(h + 1) + Environment.TickCount;
            var env = Sim.NewEnvironment(seed);
            var g = new G();

            // Processes creation (and activation)
            // 1. The switch
            g.Switch = new Switch(env, g);
            env.Process(g.Switch.Run());
            g.Switch.SetLogger("SWITCH");
            for (var i = 0; i < G.MachineCount; ++i) {
                // 2. The server operative systems
                var svOS = new ServerOS(env, g, i);
                env.Process(svOS.Run());
                svOS.SetLogger("SERVER_OS_" + i);
                g.ServerOSes[i] = svOS;
                // 3. The server processes
                var sv = new Server(env, g, i);
                env.Process(sv.Run());
                sv.SetLogger("SERVER_" + i);
                g.Servers[i] = sv;
                // 4. The client operative systems
                var clOS = new ClientOS(env, g, i);
                env.Process(clOS.Run());
                clOS.SetLogger("CLIENT_OS_" + i);
                g.ClientOSes[i] = clOS;
                // 5. The client processes
                var cl = new Client(env, g, i);
                env.Process(cl.Run());
                cl.SetLogger("CLIENT_" + i);
                g.Clients[i] = cl;
            }
            // 6. The memory recorder
            env.Process(MemoryRecorder.Run(env, g.Stats));

            Console.WriteLine("Starting simulation {0} (h = {1})", simId, h);
            StorePackets(g.Servers);
            env.Run(until: G.MaxSimTime);
            Debug.Assert(env.Now >= G.MaxSimTime, "Ended prematurely!");
            PrintStats(g.Stats, simId, h);
        }

        static void PrintStats(Stats stats, int simId, int h)
        {
            Console.WriteLine("Stats for simulation {0} (h = {1}):", simId, h);
            var cr = stats.ClientRequestsAvg();
            Console.WriteLine(" * Average client requests: {0:.#}", cr);
            var ct = stats.ClientTimeWaitedAvg()/1000;
            Console.WriteLine(" * Average client time waited: {0} ms", Entity.Ts(ct));
            var rf = stats.ReconstructedFiles();
            Console.WriteLine(" * Reconstructed files: {0}", rf);
            var lm = stats.LostSwitchMessages();
            Console.WriteLine(" * Lost switch messages: {0}", lm);
            var um = stats.MemoryUsedAvg();
            Console.WriteLine(" * Average used memory: {0} MB", um);
            stats.Reset();
        }

        static void PrintTotalStats(int h, TextWriter tw)
        {
            Console.WriteLine("Total stats for simulation with h = {0}:", h);
            var cr = Stats.ClientRequestsTotalAvg();
            Console.WriteLine(" * Average client requests: {0:.#}", cr);
            var ct = Stats.ClientTimeWaitedTotalAvg()/1000;
            Console.WriteLine(" * Average client time waited: {0} ms", Entity.Ts(ct));
            var rf = Stats.ReconstructedFilesAvg();
            Console.WriteLine(" * Average reconstructed files: {0}", rf);
            var lm = Stats.LostSwitchMessagesAvg();
            Console.WriteLine(" * Average lost switch messages: {0}", lm);
            tw.WriteLine("{0} {1} {2} {3} {4}", h, cr, ct, rf, lm);
            Stats.ResetMon();
        }

        static void StorePackets(IList<Server> servers)
        {
            var splitSize = G.FileSize/G.RequestCount;
            for (var i = 0; i < G.MachineCount; ++i) {
                for (var j = 0; j < G.MachineCount; ++j) {
                    if (i == j) {
                        continue;
                    }
                    var cp = new CodePacket(i, j, splitSize);
                    servers[j].StoreCodePacket(cp);
                }
            }
        }
    }
}