// File name: BankRenege.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
//
// Copyright (c) 2012-2016 Alessio Parma <alessio.parma@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace DIBRIS.Dessert.Examples.CSharp.SimPy3
{
    using Resources;
    using System;
    using System.Linq;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public static class BankRenege
    {
        private const int RandomSeed = 42;
        private const int NewCustomers = 5; // Total number of customers
        private const int IntervalCustomers = 10; // Generate new customers roughly every x seconds
        private const int MinPatience = 1; // Min. customer patience
        private const int MaxPatience = 3; // Max. customer patience

        /// <summary>
        ///   Source generates customers randomly.
        /// </summary>
        private static SimEvents Source(SimEnvironment env, int number, double interval, Resource counter)
        {
            foreach (var i in Enumerable.Range(0, number))
            {
                var n = string.Format("Customer{0:00}", i);
                var c = Customer(env, n, counter, timeInBank: 12.0);
                env.Process(c);
                var t = env.Random.Exponential(1.0 / interval);
                yield return env.Timeout(t);
            }
        }

        /// <summary>
        ///   Customer arrives, is served and leaves.
        /// </summary>
        private static SimEvents Customer(SimEnvironment env, string name, Resource counter, double timeInBank)
        {
            var arrive = env.Now;
            Console.WriteLine("{0:00.0000} {1}: Here I am", arrive, name);

            using (var req = counter.Request())
            {
                var patience = env.Random.NextDouble(MinPatience, MaxPatience);
                // Wait for the counter or abort at the end of our tether
                yield return req.Or(env.Timeout(patience));

                var wait = env.Now - arrive;
                if (req.Succeeded)
                {
                    // We got to the counter
                    Console.WriteLine("{0:00.0000} {1}: Waited {2:0.000}", env.Now, name, wait);

                    var tib = env.Random.Exponential(1.0 / timeInBank);
                    yield return env.Timeout(tib);
                    Console.WriteLine("{0:00.0000} {1}: Finished", env.Now, name);
                }
                else {
                    // We reneged
                    Console.WriteLine("{0:00.0000} {1}: RENEGED after {2:0.000}", env.Now, name, wait);
                }
            }
        }

        public static void Run()
        {
            // Setup and start the simulation
            Console.WriteLine("Bank renege");
            var env = Sim.Environment(RandomSeed);

            // Start processes and simulate
            var counter = Sim.Resource(env, capacity: 1);
            env.Process(Source(env, NewCustomers, IntervalCustomers, counter));
            env.Run();
        }
    }
}