// 
// BusBreakdown.cs
//  
// Author(s):
//       Alessio Parma <alessio.parma@gmail.com>
// 
// Copyright (c) 2012 Alessio Parma <alessio.parma@gmail.com>
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

namespace Dessert.Examples.CSharp.SimPy2
{
    using System;
    using IEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public static class BusBreakdown
    {
        static SimProcess _breakBus;

        static IEvents OperateBus(SimEnvironment env, double repairDuration, double tripLength)
        {
            var tripLeft = tripLength;
            while (tripLeft > 0) {
                var startTime = env.Now;
                yield return env.Timeout(tripLeft);
                object value;
                if (!env.ActiveProcess.Interrupted(out value)) {
                    break; // No more breakdowns, bus finished trip
                }
                Console.WriteLine("{0} at {1}", value, env.Now);
                tripLeft -= env.Now - startTime;
                _breakBus.Resume(delay: repairDuration);
                yield return env.Timeout(repairDuration);
                Console.WriteLine("Bus repaired at {0}", env.Now);
            }
            Console.WriteLine("Bus has arrived at {0}", env.Now);
        }

        static IEvents BreakBus(SimEnvironment env, SimProcess bus, double interval)
        {
            while (true) {
                yield return env.Timeout(interval);
                if (bus.Succeeded) {
                    break;
                }
                bus.Interrupt("Breakdown Bus");
                yield return env.Suspend();
            }
        }

        // Expected output:
        // Breakdown Bus at 300
        // Bus repaired at 320
        // Breakdown Bus at 620
        // Bus repaired at 640
        // Breakdown Bus at 940
        // Bus repaired at 960
        // Bus has arrived at 1060
        // Dessert: No more events at time 1260
        public static void Main()
        {
            var env = Sim.NewEnvironment();
            var bus = env.Process(OperateBus(env, repairDuration: 20, tripLength: 1000));
            _breakBus = env.Process(BreakBus(env, bus, interval: 300));
            env.Run(until: 4000);
            Console.WriteLine("Dessert: No more events at time {0}", env.Now);
        }
    }
}