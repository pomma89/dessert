//
// RealTime.cs
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
    using IEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public static class RealTime
    {
        static IEvents SayHello(SimEnvironment env)
        {
            while (true)
            {
                yield return env.Timeout(2);
                Console.WriteLine("Hello World at {0}!", env.WallClock.UtcNow);
            }
        }

        // Expected output:
        // Hello World simulation :)
        // Hello World at {date/time + 2 secs}!
        // Hello World at {date/time + 4 secs}!
        // Hello World at {date/time + 6 secs}!
        // Hello World at {date/time + 8 secs}!
        public static void Run()
        {
            Console.WriteLine("Hello World real-time simulation :)");
            var env = Sim.RealTimeEnvironment();
            env.Process(SayHello(env));
            env.DelayedProcess(SayHello(env), 1);
            env.Run(9.5);
        }
    }
}
