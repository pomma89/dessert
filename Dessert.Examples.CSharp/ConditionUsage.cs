//
// ConditionUsage.cs
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

    public static class ConditionUsage
    {
        static IEnumerable<SimEvent> Process(SimEnvironment env)
        {
            var t1 = env.Timeout(3);
            var t2 = env.Timeout(7);
            var c1 = env.AllOf(t1, t2);
            yield return c1;
            Console.WriteLine(env.Now); // 7
            Console.WriteLine(c1.Value.Contains(t1)); // True
            Console.WriteLine(c1.Value.Contains(t2)); // True

            t1 = env.Timeout(3);
            t2 = env.Timeout(7);
            var c2 = env.Condition(t1, t2, c => c.Ev1.Succeeded || c.Ev2.Succeeded);
            yield return c2;
            Console.WriteLine(env.Now); // 10
            Console.WriteLine(c2.Value.Contains(t1)); // True
            Console.WriteLine(c2.Value.Contains(t2)); // False
        }

        public static void Run()
        {
            var env = Sim.NewEnvironment();
            env.Process(Process(env));
            env.Run();
        }
    }
}