// 
// CallTests.cs
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

namespace Dessert.Tests.Events
{
    using NUnit.Framework;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    sealed class CallTests : TestBase
    {
        SimEvents Caller_NoExitValue()
        {
            var w = new IntWrapper();
            w.X += 5;
            yield return Env.Timeout(5);
            var call = Env.Call(Called_NoExitValue(Env.ActiveProcess, w));
            yield return call;
            Assert.AreEqual(35, w.X);
            Assert.AreEqual(null, call.Value);
        }

        SimEvents Caller_NoReturnValue_Nested()
        {
            var call = Env.Call(Caller_NoExitValue());
            yield return call;
        }

        SimEvents Called_NoExitValue(SimProcess caller, IntWrapper w)
        {
            Assert.AreSame(caller, Env.ActiveProcess);
            w.X += 10;
            yield return Env.Timeout(10);
            Assert.AreSame(caller, Env.ActiveProcess);
            w.X += 20;
            yield return Env.Timeout(20);
            Assert.AreSame(caller, Env.ActiveProcess);
        }

        SimEvents Caller_WithExitValue()
        {
            var w = new IntWrapper();
            w.X += 5;
            yield return Env.Timeout(5);
            var call = Env.Call(Called_WithExitValue(Env.ActiveProcess, w));
            yield return call;
            Assert.AreEqual(35, w.X);
            Assert.AreEqual(w, call.Value);
        }

        SimEvents Called_WithExitValue(SimProcess caller, IntWrapper w)
        {
            Assert.AreSame(caller, Env.ActiveProcess);
            w.X += 10;
            yield return Env.Timeout(10);
            Assert.AreSame(caller, Env.ActiveProcess);
            w.X += 20;
            yield return Env.Timeout(20);
            Assert.AreSame(caller, Env.ActiveProcess);
            yield return Env.Exit(w);
            Assert.Fail();
        }

        SimEvents Caller_WithExitValue_Nested()
        {
            var call = Env.Call(Caller_WithExitValue());
            yield return call;
        }

        SimEvents FibonacciFunc(int n)
        {
            if (n <= 0) {
                yield return Env.Exit(0);
            } else if (n == 1) {
                yield return Env.Exit(1);
            } else {
                var call = Env.Call<int>(FibonacciFunc(n - 1));
                yield return call;
                var n1 = call.Value;
                call = Env.Call<int>(FibonacciFunc(n - 2));
                yield return call;
                var n2 = call.Value;
                yield return Env.Exit(n1 + n2);
            }
        }

        [TestCase(0, 0), TestCase(1, 1), TestCase(2, 1), TestCase(3, 2), TestCase(4, 3), TestCase(5, 5), TestCase(6, 8)]
        public void Fibonacci(int n, int result)
        {
            var fib = Env.Process(FibonacciFunc(n));
            Env.Run();
            Assert.AreEqual(result, fib.Value);
        }

        sealed class IntWrapper
        {
            public int X;
        }

        [Test]
        public void NoReturnValue()
        {
            Env.Process(Caller_NoExitValue());
            Env.Run();
        }

        [Test]
        public void NoReturnValue_Nested()
        {
            Env.Process(Caller_NoReturnValue_Nested());
            Env.Run();
        }

        [Test]
        public void WithReturnValue()
        {
            Env.Process(Caller_WithExitValue());
            Env.Run();
        }

        [Test]
        public void WithReturnValue_Nested()
        {
            Env.Process(Caller_WithExitValue_Nested());
            Env.Run();
        }
    }
}