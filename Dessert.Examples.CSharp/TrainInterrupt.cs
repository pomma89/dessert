//
// TrainInterrupt.cs
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

namespace Dessert.Examples.CSharp
{
    using System;
    using System.Collections.Generic;

    public static class TrainInterrupt
    {
        static readonly double AvgTravelTime;
        static readonly double BreakTime;

        static TrainInterrupt()
        {
            Sim.CurrentTimeUnit = TimeUnit.Minute;
            AvgTravelTime = 20.Minutes();
            BreakTime = 50.Minutes();
        }

        static IEnumerable<SimEvent> Train(SimEnvironment env)
        {
            object cause;
            while (true) {
                var time = env.Random.Exponential(1.0/AvgTravelTime);
                Console.WriteLine("Treno in viaggio per {0:.00} minuti", time);
                yield return env.Timeout(time);
                if (env.ActiveProcess.Interrupted(out cause)) {
                    break;
                }
                Console.WriteLine("Arrivo in stazione, attesa passeggeri");
                yield return env.Timeout(2.Minutes());
                if (env.ActiveProcess.Interrupted(out cause)) {
                    break;
                }
            }
            Console.WriteLine("Al minuto {0:.00}: {1}", env.Now, cause);
        }

        static IEnumerable<SimEvent> EmergencyBrake(SimEnvironment env, SimProcess train)
        {
            yield return env.Timeout(BreakTime);
            train.Interrupt("FRENO EMERGENZA");
        }

        public static void Run()
        {
            var env = Sim.Environment(21);
            var train = env.Process(Train(env));
            env.Process(EmergencyBrake(env, train));
            env.Run();
        }
    }
}