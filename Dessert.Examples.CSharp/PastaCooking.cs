//
// PastaCooking.cs
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

namespace DIBRIS.Dessert.Examples.CSharp
{
    using System;
    using System.Collections.Generic;

    public static class PastaCooking
    {
        static readonly double AvgCookTime;
        static readonly double StdCookTime;
        static readonly double SimTime;

        static PastaCooking()
        {
            Sim.CurrentTimeUnit = TimeUnit.Minute;
            AvgCookTime = 10.Minutes();
            StdCookTime = 1.Minutes();
            SimTime = 50.Minutes();
        }

        static IEnumerable<SimEvent> PastaCook(SimEnvironment env)
        {
            while (true) {
                var cookTime = env.Random.Normal(AvgCookTime, StdCookTime);
                Console.WriteLine("Pasta in cottura per {0} minuti", cookTime);
                yield return env.Timeout(cookTime);
                if (cookTime < AvgCookTime - StdCookTime) {
                    Console.WriteLine("Pasta poco cotta!");
                } else if (cookTime > AvgCookTime + StdCookTime) {
                    Console.WriteLine("Pasta troppo cotta...");
                } else {
                    Console.WriteLine("Pasta ben cotta!!!");
                }
            }
        }

        public static void Run()
        {
            var env = Sim.Environment(21);
            env.Process(PastaCook(env));
            env.Run(SimTime);
        }
    }
}