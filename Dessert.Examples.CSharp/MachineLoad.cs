//
// MachineLoad.cs
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

namespace Dessert.Examples.CSharp
{
    using System;
    using System.Collections.Generic;
    using Troschuetz.Random;

    public static class MachineLoad
    {
        static readonly string[] Tasks = {"A", "B", "C"};
        static readonly double LoadTime;
        static readonly double WorkTime;
        static readonly double SimTime;

        static MachineLoad()
        {
            Sim.CurrentTimeUnit = TimeUnit.Minute;
            LoadTime = 5.Minutes();
            WorkTime = 25.Minutes();
            SimTime = 100.Minutes();
        }

        static IEnumerable<SimEvent> Worker(SimEnvironment env)
        {
            Console.WriteLine("{0}: Carico la macchina...", env.Now);
            yield return env.Timeout(LoadTime);
            env.Exit(env.Random.Choice(Tasks));
        }

        static IEnumerable<SimEvent> Machine(SimEnvironment env)
        {
            while (true) {
                var worker = env.Process(Worker(env));
                yield return worker;
                Console.WriteLine("{0}: Eseguo il comando {1}", env.Now, worker.Value);
                yield return env.Timeout(WorkTime);
            }
        }

        public static void Run()
        {
            var env = Sim.NewEnvironment(21);
            env.Process(Machine(env));
            env.Run(SimTime);
        }
    }
}