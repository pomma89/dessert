//
// FibonacciProducerConsumer.cs
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
    using Resources;

    public static class FibonacciProducerConsumer
    {
        static IEnumerable<SimEvent> FibonacciFunc(SimEnvironment env, int n)
        {
            if (n <= 0) {
                yield return env.Exit(0);
            } else if (n == 1) {
                yield return env.Exit(1);
            } else {
                var call = env.Call<int>(FibonacciFunc(env, n - 1));
                yield return call;
                var n1 = call.Value;
                call = env.Call<int>(FibonacciFunc(env, n - 2));
                yield return call;
                var n2 = call.Value;
                yield return env.Exit(n1 + n2);
            }
        }

        static IEnumerable<SimEvent> Producer(SimEnvironment env, Store<int> store, int n)
        {
            var call = env.Call(FibonacciFunc(env, n));
            yield return call;
            store.Put((int) call.Value);
        }

        static IEnumerable<SimEvent> Consumer(Store<int> store)
        {
            var getEv = store.Get();
            yield return getEv;
            Console.WriteLine(getEv.Value);
        }

        public static void Run()
        {
            const int count = 10;
            var env = Sim.NewEnvironment();
            var store = Sim.NewStore<int>(env);
            for (var i = 0; i < count; ++i) {
                env.Process(Producer(env, store, i));
                env.Process(Consumer(store));
            }
            env.Run();
        }
    }
}