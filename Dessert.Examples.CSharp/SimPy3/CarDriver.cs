//
// CarDriver.cs
//  
// Author(s):
//       Alessio Parma <alessio.parma@gmail.com>
// 
// Copyright (c) 2012-2016 Alessio Parma <alessio.parma@gmail.com>
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

//import simpy

//def driver(env, car):
//    yield env.timeout(3)
//    car.action.interrupt()

//class Car(object):
//    def __init__(self, env):
//        self.env = env
//        self.action = env.start(self.run())

//    def run(self):
//        while True:
//            print('Start parking and charging at %d' % env.now)
//            charge_duration = 5
//            # We may get interrupted while charging the battery
//            try:
//                yield env.start(self.charge(charge_duration))
//            except simpy.Interrupt:
//                # When we received an interrupt, we stop charging and
//                # switch to the "driving" state
//                print('Was interrupted. Hope, the battery is full enough ...')

//            print('Start driving at %d' % env.now)
//            trip_duration = 2
//            yield env.timeout(trip_duration)

//    def charge(self, duration):
//        yield self.env.timeout(duration)

//env = simpy.Environment()
//car = Car(env)
//env.start(driver(env, car))
//simpy.simulate(env, until=15)

#endregion

namespace Dessert.Examples.CSharp.SimPy3
{
    using System;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public static class CarDriver
    {
        static SimEvents Driver(SimEnvironment sim, Car car)
        {
            yield return sim.Timeout(3);
            car.Action.Interrupt();
        }

        public static void Run()
        {
            var sim = Sim.Environment();
            var car = new Car(sim);
            sim.Process(Driver(sim, car));
            sim.Run(until: 15);
        }

        sealed class Car
        {
            public readonly SimProcess Action;
            readonly SimEnvironment _env;

            public Car(SimEnvironment env)
            {
                _env = env;
                Action = env.Process(Start());
            }

            SimEvents Start()
            {
                while (true) {
                    Console.WriteLine("Start parking and charging at {0}", _env.Now);
                    const int chargeDuration = 5;
                    // We may get interrupted while charging the battery
                    yield return _env.Process(Charge(chargeDuration));
                    if (_env.ActiveProcess.Interrupted()) {
                        Console.WriteLine("Was interrupted. Hope, the battery is full enough ...");
                    }
                    Console.WriteLine("Start driving at {0}", _env.Now);
                    const int tripDuration = 2;
                    yield return _env.Timeout(tripDuration);
                }
            }

            SimEvents Charge(double duration)
            {
                yield return _env.Timeout(duration);
            }
        }
    }
}