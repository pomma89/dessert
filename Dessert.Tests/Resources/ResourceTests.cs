// 
// ResourceTests.cs
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

namespace Dessert.Tests.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Dessert.Resources;
    using NUnit.Framework;
    using PommaLabs.Collections;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    sealed class ResourceTests : TestBase
    {
        SimEvents ResourceOccupier(Resource resource, double timeout)
        {
            using (var req = resource.Request()) {
                yield return req;
                yield return Env.Timeout(timeout);
            }
        }

        SimEvents ResourceRequester(Resource resource, ILinkedQueue<SimProcess> completed)
        {
            using (var req = resource.Request()) {
                yield return req;
                completed.Enqueue(Env.ActiveProcess);
                yield return Env.Timeout(10);
            }
        }

        SimEvents ResourceRequester_WithCount(Resource resource, int expectedCount)
        {
            using (var req = resource.Request()) {
                yield return req;
                Assert.AreEqual(expectedCount, resource.Count);
                yield return Env.Timeout(10);
            }
        }

        SimEvents ResourceRequester_WithPriority(Resource resource, ILinkedQueue<SimProcess> completed, double priority)
        {
            using (var req = resource.Request(priority)) {
                yield return req;
                completed.Enqueue(Env.ActiveProcess);
                yield return Env.Timeout(10);
            }
        }

        SimEvents ResourceRequester_Occupied_WithTimeout(Resource resource, double timeout)
        {
            var req = resource.Request();
            var cond = req.Or(Env.Timeout(timeout));
            yield return cond; // Resource is occupied
            Assert.False(cond.Ev1.Succeeded);
            Assert.True(cond.Ev2.Succeeded);
        }

        [TestCase(10, 1), TestCase(10, 2), TestCase(10, 5), TestCase(10, 9), TestCase(10, 10)]
        public void Request_ManyProcesses_CountCheck(int resourceCapacity, int userCount)
        {
            var resource = Sim.Resource(Env, resourceCapacity);
            for (var i = 1; i <= userCount; ++i) {
                Env.Process(ResourceRequester_WithCount(resource, userCount));
            }
            Env.Run(20);
        }

        [TestCase(10, 1), TestCase(10, 2), TestCase(10, 5), TestCase(10, 9), TestCase(10, 10), TestCase(10, 20),
         TestCase(10, 100)]
        public void Request_ManyProcesses_Fifo(int resourceCapacity, int userCount)
        {
            var resource = Sim.Resource(Env, resourceCapacity);
            var completed = new LinkedQueue<SimProcess>();
            var expected = new LinkedQueue<SimProcess>();
            for (var i = 1; i <= userCount; ++i) {
                expected.Enqueue(Env.Process(ResourceRequester(resource, completed)));
            }
            Env.Run();
            Assert.True(expected.SequenceEqual(completed));
        }

        [TestCase(10, 1), TestCase(10, 2), TestCase(10, 5), TestCase(10, 9), TestCase(10, 10), TestCase(10, 20),
         TestCase(10, 100)]
        public void Request_ManyProcesses_Lifo(int resourceCapacity, int userCount)
        {
            var resource = Sim.Resource(Env, resourceCapacity, WaitPolicy.LIFO);
            var completed = new LinkedQueue<SimProcess>();
            var expected1 = new LinkedQueue<SimProcess>();
            var expected2 = new LinkedStack<SimProcess>();
            for (var i = 1; i <= resourceCapacity; ++i) {
                expected1.Enqueue(Env.Process(ResourceRequester(resource, completed)));
            }
            for (var i = resourceCapacity + 1; i <= userCount; ++i) {
                expected2.Push(Env.Process(ResourceRequester(resource, completed)));
            }
            Env.Run();
            Assert.True(expected1.Union(expected2).SequenceEqual(completed));
        }

        [TestCase(10, 1), TestCase(10, 2), TestCase(10, 5), TestCase(10, 9), TestCase(10, 10), TestCase(10, 20),
         TestCase(10, 100)]
        public void Request_ManyProcesses_Priority_Default(int resourceCapacity, int userCount)
        {
            var resource = Sim.Resource(Env, resourceCapacity, WaitPolicy.Priority);
            var completed = new LinkedQueue<SimProcess>();
            var expected = new LinkedQueue<SimProcess>();
            for (var i = 1; i <= userCount; ++i) {
                expected.Enqueue(Env.Process(ResourceRequester(resource, completed)));
            }
            Env.Run();
            Assert.True(expected.SequenceEqual(completed));
        }

        [TestCase(10, 1), TestCase(10, 2), TestCase(10, 5), TestCase(10, 9), TestCase(10, 10), TestCase(10, 20),
         TestCase(10, 100)]
        public void Request_ManyProcesses_Priority_Increasing(int resourceCapacity, int userCount)
        {
            var resource = Sim.Resource(Env, resourceCapacity, WaitPolicy.Priority);
            var completed = new LinkedQueue<SimProcess>();
            var expected = new LinkedQueue<SimProcess>();
            for (var i = 1; i <= userCount; ++i) {
                expected.Enqueue(Env.Process(ResourceRequester_WithPriority(resource, completed, i)));
            }
            Env.Run();
            Assert.True(expected.SequenceEqual(completed));
        }

        [TestCase(10, 1), TestCase(10, 2), TestCase(10, 5), TestCase(10, 9), TestCase(10, 10), TestCase(10, 20),
         TestCase(10, 100)]
        public void Request_ManyProcesses_Priority_Decreasing(int resourceCapacity, int userCount)
        {
            var resource = Sim.Resource(Env, resourceCapacity, WaitPolicy.Priority);
            var completed = new LinkedQueue<SimProcess>();
            var expected1 = new LinkedQueue<SimProcess>();
            var expected2 = new LinkedStack<SimProcess>();
            for (var i = 1; i <= resourceCapacity; ++i) {
                expected1.Enqueue(Env.Process(ResourceRequester_WithPriority(resource, completed, userCount - i)));
            }
            for (var i = resourceCapacity + 1; i <= userCount; ++i) {
                expected2.Push(Env.Process(ResourceRequester_WithPriority(resource, completed, userCount - i)));
            }
            Env.Run();
            Assert.True(expected1.Union(expected2).SequenceEqual(completed));
        }

        [TestCase(1), TestCase(10), TestCase(100)]
        public void Request_OccupiedResource_WithTimeout(int userCount)
        {
            var resource = Sim.Resource(Env, 1);
            Env.Process(ResourceOccupier(resource, 1000));
            for (var i = 0; i < userCount; ++i) {
                Env.Process(ResourceRequester_Occupied_WithTimeout(resource, 10));
            }
            Env.Run();
        }

        SimEvents Simple_PEM(Resource resource, string name, ICollection<Tuple<string, double>> log)
        {
            var req = resource.Request();
            yield return req;
            Assert.AreEqual(1, resource.Count);

            yield return Env.Timeout(1);
            var rel = resource.Release(req);
            Assert.IsInstanceOf(typeof(Resource.ReleaseEvent), rel);

            log.Add(Tuple.Create(name, Env.Now));
        }

        SimEvents Request_DoubleDispose_WithoutYield_PEM()
        {
            var request = Sim.Resource(Env, 3).Request();
            request.Dispose();
            request.Dispose();
            yield break;
        }

        SimEvents ContextManager_PEM(Resource resource, string name, ICollection<Tuple<string, double>> log)
        {
            using (var req = resource.Request()) {
                yield return req;
                Assert.AreEqual(1, resource.Count);
                yield return Env.Timeout(1);
            }

            log.Add(Tuple.Create(name, Env.Now));
        }

        SimEvents Slots_PEM(Resource resource, string name, ICollection<Tuple<string, double>> log)
        {
            using (var req = resource.Request()) {
                yield return req;
                log.Add(Tuple.Create(name, Env.Now));
                yield return Env.Timeout(1);
            }
        }

        [Test]
        public void Construction_RightType()
        {
            Assert.IsInstanceOf(typeof(Resource), Sim.Resource(Env, 10));
        }

        [Test]
        public void ContextManager()
        {
            var resource = Sim.Resource(Env, 1);
            Assert.AreEqual(resource.Capacity, 1);
            Assert.AreEqual(resource.Count, 0);
            var log = new SinglyLinkedList<Tuple<string, double>>();
            Env.Process(ContextManager_PEM(resource, "a", log));
            Env.Process(ContextManager_PEM(resource, "b", log));
            Env.Run();

            Assert.AreEqual("a", log.First.Item1);
            Assert.AreEqual(1, log.First.Item2);
            log.RemoveFirst();
            Assert.AreEqual("b", log.First.Item1);
            Assert.AreEqual(2, log.First.Item2);
        }

        [Test]
        public void Request_DoubleDispose_WithoutYield()
        {
            Env.Process(Request_DoubleDispose_WithoutYield_PEM());
            Env.Run();
        }

        [Test]
        public void Simple()
        {
            var resource = Sim.Resource(Env, 1);
            Assert.AreEqual(resource.Capacity, 1);
            Assert.AreEqual(resource.Count, 0);
            var log = new SinglyLinkedList<Tuple<string, double>>();
            Env.Process(Simple_PEM(resource, "a", log));
            Env.Process(Simple_PEM(resource, "b", log));
            Env.Run();

            Assert.AreEqual("a", log.First.Item1);
            Assert.AreEqual(1, log.First.Item2);
            log.RemoveFirst();
            Assert.AreEqual("b", log.First.Item1);
            Assert.AreEqual(2, log.First.Item2);
        }

        [Test]
        public void Slots()
        {
            var resource = Sim.Resource(Env, 3);
            var log = new SinglyLinkedList<Tuple<string, double>>();
            for (var i = 0; i < 9; ++i) {
                Env.Process(Slots_PEM(resource, i.ToString(CultureInfo.InvariantCulture), log));
            }
            Env.Run();

            Assert.AreEqual("0", log.First.Item1);
            Assert.AreEqual(0, log.First.Item2);
            log.RemoveFirst();
            Assert.AreEqual("1", log.First.Item1);
            Assert.AreEqual(0, log.First.Item2);
            log.RemoveFirst();
            Assert.AreEqual("2", log.First.Item1);
            Assert.AreEqual(0, log.First.Item2);
            log.RemoveFirst();
            Assert.AreEqual("3", log.First.Item1);
            Assert.AreEqual(1, log.First.Item2);
            log.RemoveFirst();
            Assert.AreEqual("4", log.First.Item1);
            Assert.AreEqual(1, log.First.Item2);
            log.RemoveFirst();
            Assert.AreEqual("5", log.First.Item1);
            Assert.AreEqual(1, log.First.Item2);
            log.RemoveFirst();
            Assert.AreEqual("6", log.First.Item1);
            Assert.AreEqual(2, log.First.Item2);
            log.RemoveFirst();
            Assert.AreEqual("7", log.First.Item1);
            Assert.AreEqual(2, log.First.Item2);
            log.RemoveFirst();
            Assert.AreEqual("8", log.First.Item1);
            Assert.AreEqual(2, log.First.Item2);
        }
    }
}