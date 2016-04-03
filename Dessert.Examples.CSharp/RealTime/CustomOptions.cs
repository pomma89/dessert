// File name: CustomOptions.cs
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

using Common.Logging.Simple;
using Finsa.CodeServices.Clock;
using System;
using IEvents = System.Collections.Generic.IEnumerable<DIBRIS.Dessert.SimEvent>;

namespace DIBRIS.Dessert.Examples.CSharp.RealTime
{
    public static class CustomOptions
    {
        private static IEvents ShowOptions(SimEnvironment env, char id)
        {
            while (true)
            {
                Console.WriteLine("{0} - Sleeping at {1}, real {2}...", id, env.Now, env.RealTime.WallClock.UtcNow);
                yield return env.Timeout(3);
                Console.WriteLine("{0} - Awake at {1}, real {2}", id, env.Now, env.RealTime.WallClock.UtcNow);
            }
        }

        public static void Run()
        {
            Console.WriteLine("Custom real-time options");
            var env = Sim.RealTimeEnvironment(21, new SimEnvironment.RealTimeOptions
            {
                ScalingFactor = 3.0, // Each time unit lasts 3 seconds.
            });
            env.Process(ShowOptions(env, 'A'));
            env.DelayedProcess(ShowOptions(env, 'B'), 1);
            env.Run(9.5);
        }
    }
}