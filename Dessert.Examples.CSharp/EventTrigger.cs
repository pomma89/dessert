//
// EventTrigger.cs
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
    using Events;

    public static class EventTrigger
    {
        static IEnumerable<SimEvent> DoSucceed(SimEnvironment env, SimEvent<object> ev)
        {
            yield return env.Timeout(5);
            ev.Succeed("SI :)");
        }

        static IEnumerable<SimEvent> DoFail(SimEnvironment env, SimEvent<object> ev)
        {
            yield return env.Timeout(5);
            ev.Fail("NO :(");
        }

        static IEnumerable<SimEvent> Proc(SimEnvironment env)
        {
            var ev1 = env.Event();
            env.Process(DoSucceed(env, ev1));
            yield return ev1;
            if (ev1.Succeeded) {
                Console.WriteLine(ev1.Value);
            }

            var ev2 = env.Event();
            env.Process(DoFail(env, ev2));
            yield return ev2;
            if (ev2.Failed) {
                Console.WriteLine(ev2.Value);
            }
        }

        public static void Run()
        {
            var env = Sim.Environment();
            env.Process(Proc(env));
            env.Run();
        }
    }
}