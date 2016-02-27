//
// TargetShooting.cs
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
    using Events;
    using Troschuetz.Random;

    public static class TargetShooting
    {
        const double HitProb = 0.7;
        const double SimTime = 100;

        static readonly string[] Targets = {"Alieno", "Pollo", "Unicorno"};

        static Timeout<string> NewTarget(SimEnvironment env)
        {
            var delay = env.Random.DiscreteUniform(1, 20);
            var target = env.Random.Choice(Targets);
            return env.Timeout(delay, target);
        }

        static IEnumerable<SimEvent> Shooter(SimEnvironment env)
        {
            while (true) {
                var timeout = NewTarget(env);
                yield return timeout;
                var hit = env.Random.NextDouble();
                if (hit < HitProb) {
                    Console.WriteLine("{0}: {1} colpito, si!", env.Now, timeout.Value);
                } else {
                    Console.WriteLine("{0}: {1} mancato, no...", env.Now, timeout.Value);
                }
            }
        }

        public static void Run()
        {
            var env = Sim.Environment(21);
            env.Process(Shooter(env));
            env.Run(SimTime);
        }
    }
}