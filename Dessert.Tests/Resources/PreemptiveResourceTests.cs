// 
// PreemptiveResourceTests.cs
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
    using System.Linq;
    using Dessert.Resources;
    using NUnit.Framework;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;
    using Finsa.CodeServices.Common.Collections;

    sealed class PreemptiveResourceTests : TestBase
    {
        SimEvents ResourceOccupier(PreemptiveResource resource, double timeout)
        {
            using (var req = resource.Request()) {
                yield return req;
                yield return Env.Timeout(timeout);
            }
        }

        SimEvents ResourceRequester(PreemptiveResource resource, ILinkedQueue<SimProcess> completed)
        {
            using (var req = resource.Request()) {
                yield return req;
                completed.Enqueue(Env.ActiveProcess);
                yield return Env.Timeout(10);
            }
        }

        SimEvents ResourceRequester_Occupied_WithTimeout(PreemptiveResource resource, double timeout)
        {
            var req = resource.Request();
            var cond = req.Or(Env.Timeout(timeout));
            yield return cond; // Resource is occupied
            Assert.False(cond.Ev1.Succeeded);
            Assert.True(cond.Ev2.Succeeded);
        }

        SimEvents ResourceRequester_WithCount(PreemptiveResource resource, int expectedCount)
        {
            using (var req = resource.Request()) {
                yield return req;
                Assert.AreEqual(expectedCount, resource.Count);
                yield return Env.Timeout(10);
            }
        }

        SimEvents ResourceRequester_WithPriority(PreemptiveResource resource, ILinkedQueue<SimProcess> completed,
                                                 double priority)
        {
            using (var req = resource.Request(priority, false)) {
                yield return req;
                completed.Enqueue(Env.ActiveProcess);
                yield return Env.Timeout(10);
            }
        }

        SimEvents Simple_PEM(int id, PreemptiveResource res, double priority,
                             ICollection<Tuple<double, int, PreemptionInfo>> log)
        {
            using (var req = res.Request(priority)) {
                yield return req;
                yield return Env.Timeout(5);
                PreemptionInfo info;
                log.Add(Env.ActiveProcess.Preempted(out info)
                            ? Tuple.Create(Env.Now, id, info)
                            : Tuple.Create(Env.Now, id, (PreemptionInfo) null));
            }
        }

        SimEvents WithTimeout_PEM_A(PreemptiveResource resource, double priority)
        {
            using (var req = resource.Request(priority)) {
                yield return req;
                Assert.True(Env.ActiveProcess.Preempted());
                yield return Env.Event();
            }
        }

        SimEvents WithTimeout_PEM_B(PreemptiveResource resource, double priority)
        {
            using (var req = resource.Request(priority)) {
                yield return req;
            }
        }

        SimEvents MixedPreemption_PEM(int id, PreemptiveResource resource, double priority, bool preempt,
                                      ICollection<Tuple<double, int, PreemptionInfo>> log)
        {
            using (var req = resource.Request(priority, preempt)) {
                yield return req;
                yield return Env.Timeout(5);
                PreemptionInfo info;
                log.Add(Env.ActiveProcess.Preempted(out info)
                            ? Tuple.Create(Env.Now, id, info)
                            : Tuple.Create(Env.Now, id, (PreemptionInfo) null));
            }
        }

        [TestCase(10, 1), TestCase(10, 2), TestCase(10, 5), TestCase(10, 9), TestCase(10, 10)]
        public void Request_ManyProcesses_CountCheck(int resourceCapacity, int userCount)
        {
            var resource = Sim.PreemptiveResource(Env, resourceCapacity);
            for (var i = 1; i <= userCount; ++i) {
                Env.Process(ResourceRequester_WithCount(resource, userCount));
            }
            Env.Run(20);
        }

        [TestCase(10, 1), TestCase(10, 2), TestCase(10, 5), TestCase(10, 9), TestCase(10, 10), TestCase(10, 20),
         TestCase(10, 100)]
        public void Request_ManyProcesses_Priority_Default(int resourceCapacity, int userCount)
        {
            var resource = Sim.PreemptiveResource(Env, resourceCapacity);
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
            var resource = Sim.PreemptiveResource(Env, resourceCapacity);
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
            var resource = Sim.PreemptiveResource(Env, resourceCapacity);
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
            var resource = Sim.PreemptiveResource(Env, 1);
            Env.Process(ResourceOccupier(resource, 1000));
            for (var i = 0; i < userCount; ++i) {
                Env.Process(ResourceRequester_Occupied_WithTimeout(resource, 10));
            }
            Env.Run();
        }

        [Test]
        public void MixedPreemption()
        {
            var resource = Sim.PreemptiveResource(Env, 2);
            var log = new SinglyLinkedList<Tuple<double, int, PreemptionInfo>>();
            Env.Process(MixedPreemption_PEM(0, resource, 1, true, log));
            Env.Process(MixedPreemption_PEM(1, resource, 1, true, log));
            Env.DelayedProcess(MixedPreemption_PEM(2, resource, 0, false, log), delay: 1);
            var p3 = Env.DelayedProcess(MixedPreemption_PEM(3, resource, 0, true, log), delay: 1);
            Env.DelayedProcess(MixedPreemption_PEM(4, resource, 2, true, log), 2);
            Env.Run();

            Assert.AreEqual(1, log.First.Item1);
            Assert.AreEqual(1, log.First.Item2);
            Assert.AreEqual(p3, log.First.Item3.By);
            Assert.AreEqual(0, log.First.Item3.UsageSince);
            log.RemoveFirst();
            Assert.AreEqual(5, log.First.Item1);
            Assert.AreEqual(0, log.First.Item2);
            Assert.AreEqual(null, log.First.Item3);
            log.RemoveFirst();
            Assert.AreEqual(6, log.First.Item1);
            Assert.AreEqual(3, log.First.Item2);
            Assert.AreEqual(null, log.First.Item3);
            log.RemoveFirst();
            Assert.AreEqual(10, log.First.Item1);
            Assert.AreEqual(2, log.First.Item2);
            Assert.AreEqual(null, log.First.Item3);
            log.RemoveFirst();
            Assert.AreEqual(11, log.First.Item1);
            Assert.AreEqual(4, log.First.Item2);
            Assert.AreEqual(null, log.First.Item3);
        }

        [Test]
        public void Simple()
        {
            var resource = Sim.PreemptiveResource(Env, 2);
            var log = new SinglyLinkedList<Tuple<double, int, PreemptionInfo>>();
            Env.DelayedProcess(Simple_PEM(0, resource, 1, log), 0);
            Env.DelayedProcess(Simple_PEM(1, resource, 1, log), 0);
            var p2 = Env.DelayedProcess(Simple_PEM(2, resource, 0, log), delay: 1);
            Env.DelayedProcess(Simple_PEM(3, resource, 2, log), delay: 2);
            Env.Run();

            Assert.AreEqual(1, log.First.Item1);
            Assert.AreEqual(1, log.First.Item2);
            Assert.AreEqual(p2, log.First.Item3.By);
            Assert.AreEqual(0, log.First.Item3.UsageSince);
            log.RemoveFirst();
            Assert.AreEqual(5, log.First.Item1);
            Assert.AreEqual(0, log.First.Item2);
            Assert.AreEqual(null, log.First.Item3);
            log.RemoveFirst();
            Assert.AreEqual(6, log.First.Item1);
            Assert.AreEqual(2, log.First.Item2);
            Assert.AreEqual(null, log.First.Item3);
            log.RemoveFirst();
            Assert.AreEqual(10, log.First.Item1);
            Assert.AreEqual(3, log.First.Item2);
            Assert.AreEqual(null, log.First.Item3);
        }

        [Test]
        public void WithTimeout()
        {
            var resource = Sim.PreemptiveResource(Env, 1);
            Env.Process(WithTimeout_PEM_A(resource, 1));
            Env.Process(WithTimeout_PEM_B(resource, 0));
            Env.Run();
        }
    }
}