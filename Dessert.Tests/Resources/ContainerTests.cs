// 
// ContainerTests.cs
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

namespace Dessert.Tests.Resources
{
    using System;
    using System.Collections.Generic;
    using Dessert.Resources;
    using NUnit.Framework;

    sealed class ContainerTests : TestBase
    {
        [TestCase(0), TestCase(TinyNeg), TestCase(SmallNeg), TestCase(LargeNeg),
         ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Construction_InvalidCapacity(double capacity)
        {
            Sim.Container(Env, capacity);
        }

        [TestCase(0, 0), TestCase(TinyNeg, TinyNeg), TestCase(SmallNeg, SmallNeg), TestCase(LargeNeg, LargeNeg),
         TestCase(1, TinyNeg), TestCase(1, SmallNeg), TestCase(1, LargeNeg), TestCase(0.1, TinyNeg),
         TestCase(0.1, SmallNeg), TestCase(0.1, LargeNeg), TestCase(0.1, 0.2), TestCase(1, 2),
         ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Construction_InvalidQuantityArguments(double capacity, double level)
        {
            Sim.Container(Env, capacity, level);
        }

        [TestCase(0, 0), TestCase(TinyNeg, TinyNeg), TestCase(SmallNeg, SmallNeg), TestCase(LargeNeg, LargeNeg),
         TestCase(1, TinyNeg), TestCase(1, SmallNeg), TestCase(1, LargeNeg), TestCase(0.1, TinyNeg),
         TestCase(0.1, SmallNeg), TestCase(0.1, LargeNeg), TestCase(0.1, 0.2), TestCase(1, 2),
         ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Construction_InvalidQuantityArguments_WithGetPolicy(double capacity, double level)
        {
            Sim.Container(Env, capacity, level, WaitPolicy.Random, WaitPolicy.FIFO);
        }

        [TestCase(0, 0), TestCase(TinyNeg, TinyNeg), TestCase(SmallNeg, SmallNeg), TestCase(LargeNeg, LargeNeg),
         TestCase(1, TinyNeg), TestCase(1, SmallNeg), TestCase(1, LargeNeg), TestCase(0.1, TinyNeg),
         TestCase(0.1, SmallNeg), TestCase(0.1, LargeNeg), TestCase(0.1, 0.2), TestCase(1, 2),
         ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Construction_InvalidQuantityArguments_WithBothPolicies(double capacity, double level)
        {
            Sim.Container(Env, capacity, level, WaitPolicy.Random, WaitPolicy.Random);
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void GetPolicy_Fifo(int eventCount)
        {
            const int quantity = 10;
            var container = Sim.Container(Env, quantity*eventCount);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                container.Get(quantity).Callbacks.Add(e => list.Add(tmpI));
            }
            container.Put(quantity*eventCount);
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void GetPolicy_Lifo(int eventCount)
        {
            const int quantity = 10;
            var container = Sim.Container(Env, quantity*eventCount, 0, WaitPolicy.LIFO, WaitPolicy.FIFO);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                container.Get(quantity).Callbacks.Add(e => list.Add(tmpI));
            }
            container.Put(quantity*eventCount);
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[eventCount - i - 1]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void GetPolicy_Priority_Default(int eventCount)
        {
            const int quantity = 10;
            var container = Sim.Container(Env, quantity*eventCount, 0, WaitPolicy.Priority, WaitPolicy.FIFO);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                container.Get(quantity).Callbacks.Add(e => list.Add(tmpI));
            }
            container.Put(quantity*eventCount);
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void GetPolicy_Priority_Increasing(int eventCount)
        {
            const int quantity = 10;
            var container = Sim.Container(Env, quantity*eventCount, 0, WaitPolicy.Priority, WaitPolicy.FIFO);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                container.Get(quantity, i).Callbacks.Add(e => list.Add(tmpI));
            }
            container.Put(quantity*eventCount);
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void GetPolicy_Priority_Decreasing(int eventCount)
        {
            const int quantity = 10;
            var container = Sim.Container(Env, quantity*eventCount, 0, WaitPolicy.Priority, WaitPolicy.FIFO);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                container.Get(quantity, -i).Callbacks.Add(e => list.Add(tmpI));
            }
            container.Put(quantity*eventCount);
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[eventCount - i - 1]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void PutPolicy_Fifo(int eventCount)
        {
            const int quantity = 10;
            var container = Sim.Container(Env, quantity*eventCount, quantity*eventCount);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                container.Put(quantity).Callbacks.Add(e => list.Add(tmpI));
            }
            container.Get(quantity*eventCount);
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void PutPolicy_Lifo(int eventCount)
        {
            const int quantity = 10;
            var container = Sim.Container(Env, quantity*eventCount, quantity*eventCount, WaitPolicy.FIFO,
                                             WaitPolicy.LIFO);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                container.Put(quantity).Callbacks.Add(e => list.Add(tmpI));
            }
            container.Get(quantity*eventCount);
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[eventCount - i - 1]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void PutPolicy_Priority_Default(int eventCount)
        {
            const int quantity = 10;
            var container = Sim.Container(Env, quantity*eventCount, quantity*eventCount, WaitPolicy.FIFO,
                                             WaitPolicy.Priority);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                container.Put(quantity).Callbacks.Add(e => list.Add(tmpI));
            }
            container.Get(quantity*eventCount);
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void PutPolicy_Priority_Increasing(int eventCount)
        {
            const int quantity = 10;
            var container = Sim.Container(Env, quantity*eventCount, quantity*eventCount, WaitPolicy.FIFO,
                                             WaitPolicy.Priority);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                container.Put(quantity, i).Callbacks.Add(e => list.Add(tmpI));
            }
            container.Get(quantity*eventCount);
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void PutPolicy_Priority_Decreasing(int eventCount)
        {
            const int quantity = 10;
            var container = Sim.Container(Env, quantity*eventCount, quantity*eventCount, WaitPolicy.FIFO,
                                             WaitPolicy.Priority);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                container.Put(quantity, -i).Callbacks.Add(e => list.Add(tmpI));
            }
            container.Get(quantity*eventCount);
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[eventCount - i - 1]);
            }
        }

        [Test]
        public void Construction_RightType()
        {
            Assert.IsInstanceOf(typeof(Container), Sim.Container(Env));
        }
    }
}