// 
// FilterStoreTests.cs
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

namespace DIBRIS.Dessert.Tests.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Dessert.Resources;
    using NUnit.Framework;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    sealed class FilterStoreTests : StoreTestBase
    {
        SimEvents StorePutter(FilterStore<int> store, int putCount, int timeout)
        {
            Debug.Assert(putCount >= 0);
            Debug.Assert(timeout >= 0);
            for (var i = 0; i < putCount; ++i) {
                Integers.Add(i);
                yield return store.Put(i);
                if (timeout > 0) {
                    yield return Env.Timeout(timeout);
                }
            }
        }

        SimEvents StorePutter_SameEvent(FilterStore<int> store)
        {
            var putEv = store.Put(0);
            while (true) {
                yield return putEv;
                yield return Env.Timeout(1);
            }
        }

        SimEvents Simple_PEM()
        {
            var filterStore = Sim.FilterStore<string>(Env, 2);
            var getEv = filterStore.Get(i => i == "b");
            yield return filterStore.Put("a");
            Assert.False(getEv.Succeeded);
            yield return filterStore.Put("b");
            Assert.True(getEv.Succeeded);
        }

        SimEvents StoreGetter(FilterStore<int> store, int getCount, int timeout)
        {
            Debug.Assert(getCount >= 0);
            Debug.Assert(timeout >= 0);
            for (var i = 0; i < getCount; ++i) {
                var getEv = store.Get();
                yield return getEv;
                Assert.AreEqual(Integers[GetIdx++], getEv.Value);
                if (timeout > 0) {
                    yield return Env.Timeout(timeout);
                }
            }
        }

        SimEvents StoreGetter_SameEvent(FilterStore<int> store)
        {
            var getEv = store.Get();
            while (true) {
                yield return store.Put(5);
                yield return getEv;
                yield return Env.Timeout(1);
            }
        }

        [TestCase(0), TestCase(1), TestCase(10), TestCase(100)]
        public void Put_InfiniteCapacity(int putCount)
        {
            var store = Sim.FilterStore<int>(Env);
            Assert.AreEqual(WaitPolicy.FIFO, store.ItemPolicy);
            Assert.AreEqual(WaitPolicy.FIFO, store.GetPolicy);
            Assert.AreEqual(WaitPolicy.FIFO, store.PutPolicy);
            Env.Process(StorePutter(store, putCount, 0));
            Env.Run(until: 100);
            Assert.AreEqual(putCount, store.Count);
            Assert.AreEqual(putCount, store.ItemQueue.Count());
            Assert.AreEqual(int.MaxValue, store.Capacity);
            Assert.IsEmpty(store.GetQueue);
            Assert.IsEmpty(store.PutQueue);
        }

        [TestCase(2, 1), TestCase(5, 2), TestCase(10, 9), TestCase(100, 10)]
        public void Put_BoundedCapacity(int putCount, int capacity)
        {
            Debug.Assert(putCount >= capacity);
            var store = Sim.FilterStore<int>(Env, capacity);
            Assert.AreEqual(WaitPolicy.FIFO, store.ItemPolicy);
            Assert.AreEqual(WaitPolicy.FIFO, store.GetPolicy);
            Assert.AreEqual(WaitPolicy.FIFO, store.PutPolicy);
            Env.Process(StorePutter(store, putCount, 0));
            Env.Run(until: 100);
            Assert.AreEqual(capacity, store.Count);
            Assert.AreEqual(capacity, store.ItemQueue.Count());
            Assert.AreEqual(capacity, store.Capacity);
            Assert.IsEmpty(store.GetQueue);
            Assert.True(store.PutQueue.Count() == 1);
        }

        [TestCase(2, 1), TestCase(5, 2), TestCase(10, 10), TestCase(100, 10)]
        public void Put_BoundedCapacity_ManyProducers(int putCount, int capacity)
        {
            Debug.Assert(putCount >= capacity);
            var store = Sim.FilterStore<int>(Env, capacity);
            Assert.AreEqual(WaitPolicy.FIFO, store.ItemPolicy);
            Assert.AreEqual(WaitPolicy.FIFO, store.GetPolicy);
            Assert.AreEqual(WaitPolicy.FIFO, store.PutPolicy);
            Env.Process(StorePutter(store, putCount, 1));
            Env.Process(StorePutter(store, putCount, 1));
            Env.Run(until: 100);
            Assert.AreEqual(capacity, store.Count);
            Assert.AreEqual(capacity, store.ItemQueue.Count());
            Assert.AreEqual(capacity, store.Capacity);
            Assert.IsEmpty(store.GetQueue);
            Assert.True(store.PutQueue.Count() == 2);
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void GetPolicy_Fifo(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount);
            var list = new List<Tuple<int, int>>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                store.Get().Callbacks.Add(e => list.Add(Tuple.Create(tmpI, e.Value)));
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i].Item1);
                Assert.AreEqual(i, list[i].Item2);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void GetPolicy_Lifo(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.LIFO, WaitPolicy.FIFO);
            var list = new List<Tuple<int, int>>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                store.Get().Callbacks.Add(e => list.Add(Tuple.Create(tmpI, e.Value)));
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[eventCount - i - 1].Item1);
                Assert.AreEqual(i, list[i].Item2);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void GetPolicy_Priority_Default(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.Priority, WaitPolicy.FIFO);
            var list = new List<Tuple<int, int>>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                store.Get().Callbacks.Add(e => list.Add(Tuple.Create(tmpI, e.Value)));
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i].Item1);
                Assert.AreEqual(i, list[i].Item2);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void GetPolicy_Priority_Increasing(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.Priority, WaitPolicy.FIFO);
            var list = new List<Tuple<int, int>>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                store.Get(i).Callbacks.Add(e => list.Add(Tuple.Create(tmpI, e.Value)));
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i].Item1);
                Assert.AreEqual(i, list[i].Item2);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void GetPolicy_Priority_Decreasing(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.Priority, WaitPolicy.FIFO);
            var list = new List<Tuple<int, int>>();
            for (var i = 0; i < eventCount; ++i) {
                var tmpI = i;
                store.Get(-i).Callbacks.Add(e => list.Add(Tuple.Create(tmpI, e.Value)));
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[eventCount - i - 1].Item1);
                Assert.AreEqual(i, list[i].Item2);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void PutPolicy_Fifo(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i).Callbacks.Add(e => list.Add(e.Item));
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Get();
            }
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void PutPolicy_Lifo(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.FIFO, WaitPolicy.LIFO);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i).Callbacks.Add(e => list.Add(e.Item));
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Get();
            }
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[eventCount - i - 1]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void PutPolicy_Priority_Default(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.FIFO, WaitPolicy.Priority);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i).Callbacks.Add(e => list.Add(e.Item));
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Get();
            }
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void PutPolicy_Priority_Increasing(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.FIFO, WaitPolicy.Priority);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i, i).Callbacks.Add(e => list.Add(e.Item));
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Get();
            }
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void PutPolicy_Priority_Decreasing(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.FIFO, WaitPolicy.Priority);
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i, -i).Callbacks.Add(e => list.Add(e.Item));
            }
            for (var i = 0; i < eventCount; ++i) {
                store.Get();
            }
            Env.Run();
            Assert.AreEqual(eventCount, list.Count);
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[eventCount - i - 1]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void ItemPolicy_Fifo(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount);
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                store.Get().Callbacks.Add(e => list.Add(e.Value));
            }
            Env.Run();
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void ItemPolicy_Lifo(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.FIFO, WaitPolicy.FIFO, WaitPolicy.LIFO);
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                store.Get().Callbacks.Add(e => list.Add(e.Value));
            }
            Env.Run();
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(eventCount - i - 1, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void ItemPolicy_Priority_Default(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.FIFO, WaitPolicy.FIFO, WaitPolicy.Priority);
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i);
            }
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                store.Get().Callbacks.Add(e => list.Add(e.Value));
            }
            Env.Run();
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void ItemPolicy_Priority_Increasing(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.FIFO, WaitPolicy.FIFO, WaitPolicy.Priority);
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i, 0, i);
            }
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                store.Get().Callbacks.Add(e => list.Add(e.Value));
            }
            Env.Run();
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void ItemPolicy_Priority_Decreasing(int eventCount)
        {
            var store = Sim.FilterStore<int>(Env, eventCount, WaitPolicy.FIFO, WaitPolicy.FIFO, WaitPolicy.Priority);
            for (var i = 0; i < eventCount; ++i) {
                store.Put(i, 0, -i);
            }
            var list = new List<int>();
            for (var i = 0; i < eventCount; ++i) {
                store.Get().Callbacks.Add(e => list.Add(e.Value));
            }
            Env.Run();
            for (var i = 0; i < eventCount; ++i) {
                Assert.AreEqual(eventCount - i - 1, list[i]);
            }
        }

        [Test]
        public void Construction_RightType()
        {
            Assert.IsInstanceOf(typeof(FilterStore<int>), Sim.FilterStore<int>(Env));
        }

        [Test]
        public void Get_SameEvent()
        {
            var store = Sim.FilterStore<int>(Env);
            Env.Process(StoreGetter_SameEvent(store));
            Env.Run(until: 100);
        }

        [Test]
        public void LastItemGoodForFilter()
        {
            var filterStore = Sim.FilterStore<int>(Env, 10);
            filterStore.Put(1);
            filterStore.Put(3);
            filterStore.Put(5);
            filterStore.Put(2);
            var getEv = filterStore.Get(i => i%2 == 0);
            Env.Run();
            Assert.True(getEv.Succeeded);
            Assert.AreEqual(2, getEv.Value);
        }

        [Test]
        public void ProducerConsumer_OneProducer_OneConsumer_BoundedCapacity()
        {
            var store = Sim.FilterStore<int>(Env, 50);
            Env.Process(StorePutter(store, 100, 1));
            Env.Process(StoreGetter(store, 10, 10));
            Env.Run(until: 1000);
            Assert.AreEqual(50, store.Count);
            Assert.AreEqual(50, store.ItemQueue.Count());
            Assert.AreEqual(50, store.Capacity);
            Assert.IsEmpty(store.GetQueue);
            Assert.True(store.PutQueue.Count() == 1);
        }

        [Test]
        public void ProducerConsumer_OneProducer_OneConsumer_UnboundedCapacity()
        {
            var store = Sim.FilterStore<int>(Env);
            Env.Process(StorePutter(store, 10, 1));
            Env.Process(StoreGetter(store, 10, 1));
            Env.Run(until: 100);
            Assert.AreEqual(0, store.Count);
            Assert.AreEqual(0, store.ItemQueue.Count());
            Assert.AreEqual(int.MaxValue, store.Capacity);
            Assert.IsEmpty(store.GetQueue);
            Assert.IsEmpty(store.PutQueue);
        }

        [Test]
        public void ProducerConsumer_TwoProducers_TwoConsumers_UnboundedCapacity()
        {
            var store = Sim.FilterStore<int>(Env);
            Env.Process(StorePutter(store, 100, 1));
            Env.Process(StorePutter(store, 100, 1));
            Env.Process(StoreGetter(store, 100, 1));
            Env.Process(StoreGetter(store, 100, 1));
            Env.Run(until: 1000);
            Assert.AreEqual(0, store.Count);
            Assert.AreEqual(0, store.ItemQueue.Count());
            Assert.AreEqual(int.MaxValue, store.Capacity);
            Assert.IsEmpty(store.GetQueue);
            Assert.IsEmpty(store.PutQueue);
        }

        [Test]
        public void Put_SameEvent()
        {
            var store = Sim.FilterStore<int>(Env);
            Env.Process(StorePutter_SameEvent(store));
            Env.Run(until: 100);
        }

        [Test]
        public void Simple()
        {
            Env.Process(Simple_PEM());
            Env.Run();
        }
    }
}