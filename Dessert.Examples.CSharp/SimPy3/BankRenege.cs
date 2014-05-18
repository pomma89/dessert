//
// BankRenege.cs
//  
// Author(s):
//       Alessio Parma <alessio.parma@gmail.com>
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

#region Original SimPy3 Example

//"""
//Bank renege example

//Covers:

//- Resources: Resource
//- Condition events

//Scenario:
//  A counter with a random service time and customers who renege. Based on the
//  program bank08.py from TheBank tutorial of SimPy 2. (KGM)

//"""
//import random

//import simpy

//RANDOM_SEED = 42
//NEW_CUSTOMERS = 5  # Total number of customers
//INTERVAL_CUSTOMERS = 10.0  # Generate new customers roughly every x seconds
//MIN_PATIENCE = 1  # Min. customer patience
//MAX_PATIENCE = 3  # Max. customer patience

//def source(env, number, interval, counter):
//    """Source generates customers randomly"""
//    for i in range(number):
//        c = customer(env, 'Customer%02d' % i, counter, time_in_bank=12.0)
//        env.start(c)
//        t = random.expovariate(1.0 / interval)
//        yield env.timeout(t)

//def customer(env, name, counter, time_in_bank):
//    """Customer arrives, is served and leaves."""
//    arrive = env.now
//    print('%7.4f %s: Here I am' % (arrive, name))

//    with counter.request() as req:
//        patience = random.uniform(MIN_PATIENCE, MAX_PATIENCE)
//        # Wait for the counter or abort at the end of our tether
//        results = yield req | env.timeout(patience)

//        wait = env.now - arrive

//        if req in results:
//            # We got to the counter
//            print('%7.4f %s: Waited %6.3f' % (env.now, name, wait))

//            tib = random.expovariate(1.0 / time_in_bank)
//            yield env.timeout(tib)
//            print('%7.4f %s: Finished' % (env.now, name))

//        else:
//            # We reneged
//            print('%7.4f %s: RENEGED after %6.3f' % (env.now, name, wait))

//# Setup and start the simulation
//print('Bank renege')
//random.seed(RANDOM_SEED)
//env = simpy.Environment()

//# Start processes and simulate
//counter = simpy.Resource(env, capacity=1)
//env.start(source(env, NEW_CUSTOMERS, INTERVAL_CUSTOMERS, counter))
//simpy.simulate(env)

#endregion

namespace Dessert.Examples.CSharp.SimPy3
{
    using System;
    using System.Linq;
    using Resources;
    using Troschuetz.Random;
    using Troschuetz.Random.Generators;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public static class BankRenege
    {
        const int RandomSeed = 42;
        const int NewCustomers = 5; // Total number of customers
        const int IntervalCustomers = 10; // Generate new customers roughly every x seconds
        const int MinPatience = 1; // Min. customer patience
        const int MaxPatience = 3; // Max. customer patience

        static readonly TRandom Random = new TRandom(new ALFGenerator(RandomSeed));

        /// <summary>
        ///   Source generates customers randomly.
        /// </summary>
        static SimEvents Source(SimEnvironment env, int number, double interval, Resource counter)
        {
            foreach (var i in Enumerable.Range(0, number)) {
                var n = string.Format("Customer{0:00}", i);
                var c = Customer(env, n, counter, timeInBank: 12.0);
                env.Process(c);
                var t = Random.Exponential(1.0/interval);
                yield return env.Timeout(t);
            }
        }

        /// <summary>
        ///   Customer arrives, is served and leaves.
        /// </summary>
        static SimEvents Customer(SimEnvironment env, string name, Resource counter, double timeInBank)
        {
            var arrive = env.Now;
            Console.WriteLine("{0:00.0000} {1}: Here I am", arrive, name);

            using (var req = counter.Request()) {
                var patience = Random.NextDouble(MinPatience, MaxPatience);
                // Wait for the counter or abort at the end of our tether
                yield return req.Or(env.Timeout(patience));

                var wait = env.Now - arrive;
                if (req.Succeeded) {
                    // We got to the counter
                    Console.WriteLine("{0:00.0000} {1}: Waited {2:0.000}", env.Now, name, wait);

                    var tib = Random.Exponential(1.0/timeInBank);
                    yield return env.Timeout(tib);
                    Console.WriteLine("{0:00.0000} {1}: Finished", env.Now, name);
                } else {
                    // We reneged
                    Console.WriteLine("{0:00.0000} {1}: RENEGED after {2:0.000}", env.Now, name, wait);
                }
            }
        }

        public static void Run()
        {
            // Setup and start the simulation
            Console.WriteLine("Bank renege");
            var env = Sim.Environment();

            // Start processes and simulate
            var counter = Sim.Resource(env, capacity: 1);
            env.Process(Source(env, NewCustomers, IntervalCustomers, counter));
            env.Run();
        }
    }
}