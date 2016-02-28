// 
// TimeoutTests.cs
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

namespace DIBRIS.Dessert.Tests.Events
{
    using System;
    using System.Diagnostics;
    using NUnit.Framework;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    sealed class TimeoutTests : TestBase
    {
        [TestCase(TinyNeg), TestCase(SmallNeg), TestCase(LargeNeg),
         ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Yield_Timeout_NegativeTime(double time)
        {
            Env.Process(TimeoutYielder(time));
            Env.Run(1);
        }

        [TestCase(Double.PositiveInfinity), TestCase(Double.NegativeInfinity), TestCase(Double.NaN),
         ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Yield_Timeout_Infinity(double time)
        {
            Env.Process(TimeoutYielder(time));
            Env.Run(1);
        }

        SimEvents AndCombinedTimeoutYielder(double t1, double t2)
        {
            Debug.Assert(t1 < t2);
            while (true) {
                var start = Env.Now;
                var min = Env.Timeout(t1);
                var max = Env.Timeout(t2);
                yield return min.And(max);
                Assert.True(min.Succeeded);
                Assert.True(max.Succeeded);
                Assert.AreEqual(Env.Now, start + t2);

                start = Env.Now;
                min = Env.Timeout(t1);
                max = Env.Timeout(t2);
                yield return Env.AllOf(min, max);
                Assert.True(min.Succeeded);
                Assert.True(max.Succeeded);
                Assert.AreEqual(Env.Now, start + t2);
            }
        }

        SimEvents OrCombinedTimeoutYielder(double t1, double t2)
        {
            Debug.Assert(t1 < t2);
            while (true) {
                var start = Env.Now;
                var min = Env.Timeout(t1);
                var max = Env.Timeout(t2);
                yield return min.Or(max);
                Assert.True(min.Succeeded);
                Assert.False(max.Succeeded);
                Assert.AreEqual(Env.Now, start + t1);

                start = Env.Now;
                min = Env.Timeout(t1);
                max = Env.Timeout(t2);
                yield return Env.AnyOf(min, max);
                Assert.True(min.Succeeded);
                Assert.False(max.Succeeded);
                Assert.AreEqual(Env.Now, start + t1);
            }
        }

        SimEvents TimeoutYielder(double time)
        {
            while (true) {
                var start = Env.Now;
                var ev = Env.Timeout(time, 0);
                yield return ev;
                Assert.True(ev.Succeeded);
                Assert.AreEqual(Env.Now, start + time);
            }
        }

        SimEvents TimeoutYielder_WithCallback(double time, Action<SimEvent> callback)
        {
            while (true) {
                var start = Env.Now;
                var ev = Env.Timeout(time);
                ev.Callbacks.Add(callback);
                yield return ev;
                Assert.True(ev.Succeeded);
                Assert.AreEqual(Env.Now, start + time);
            }
        }

        SimEvents ManyTimesTimeoutYielder()
        {
            var ev = Env.Timeout(10);
            for (var i = 0; i < 10; ++i) {
                yield return ev;
            }
        }

        [Test]
        public void Simulate_EnoughTime()
        {
            Env.Process(TimeoutYielder(10));
            Env.Run(95);
            Assert.AreEqual(95, Env.Now);
        }

        [Test]
        public void Simulate_TooEarly()
        {
            Env.Process(TimeoutYielder(100));
            Env.Run(5);
            Assert.AreEqual(Env.Now, 5);
        }

        [Test]
        public void Yield_AndCombined()
        {
            Env.Process(AndCombinedTimeoutYielder(5, 10));
            Env.Run(200);
            Assert.AreEqual(200, Env.Now);
        }

        [Test]
        public void Yield_ManyTimes()
        {
            Env.Process(ManyTimesTimeoutYielder());
            Env.Run(100);
            Assert.AreEqual(10, Env.Now);
        }

        [Test]
        public void Yield_OrCombined()
        {
            Env.Process(OrCombinedTimeoutYielder(5, 10));
            Env.Run(200);
            Assert.AreEqual(200, Env.Now);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Yield_Timeout_NegativeDelay()
        {
            Env.Process(TimeoutYielder(-5));
            Env.Run(1);
        }

        [Test]
        public void Yield_WithCallback()
        {
            var x = 0;
            Env.Process(TimeoutYielder_WithCallback(10, ev => ++x));
            Env.Run(until: 25);
            Assert.AreEqual(2, x);
        }
    }
}