//
// ConditionTester.cs
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
// The above copyright notice and this permission notice shAllOf be included in
// AllOf copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHAllOf THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace Dessert.Examples.CSharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Events;

    public static class ConditionTester
    {
        static IEnumerable<SimEvent> AProcess(SimEnvironment env)
        {
            yield return env.Timeout(7);
            env.Exit("VAL_P");
        }

        static IEnumerable<SimEvent> CondTester(SimEnvironment env)
        {
            var aProc = env.Process(AProcess(env));
            var cond = env.AllOf(env.Timeout(5, "VAL_T"), aProc);
            yield return cond;
            Console.WriteLine("ALL: {0}", cond.Value.Select(x => x.Value).Aggregate((s1, s2) => s1 + ", " + s2));

            aProc = env.Process(AProcess(env));
            cond = env.AnyOf(env.Timeout(5, "VAL_T"), aProc);
            yield return cond;
            Console.WriteLine("ANY: {0}", cond.Value.Select(x => x.Value).Aggregate((s1, s2) => s1 + ", " + s2));

            aProc = env.Process(AProcess(env));
            var aTime = env.Timeout(5, "VAL_T");
            ConditionEval<Timeout<string>, SimProcess> pred =
                c => c.Ev1.Succeeded && c.Ev2.Succeeded && c.Ev1.Value.Equals("VAL_T") && c.Ev2.Value.Equals("VAL_P");
            cond = env.Condition(aTime, aProc, pred);
            yield return cond;
            Console.WriteLine("CUSTOM: {0}", cond.Value.Select(x => x.Value).Aggregate((s1, s2) => s1 + ", " + s2));
        }

        public static void Run()
        {
            var env = Sim.Environment();
            env.Process(CondTester(env));
            env.Run();
        }
    }
}