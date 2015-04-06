// 
// ConditionTests.cs
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
    using System.Collections.Generic;
    using System.Linq;
    using Dessert.Events;
    using Dessert.Resources;
    using NUnit.Framework;
    using IEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    sealed partial class ConditionTests : TestBase
    {
        IEvents And_Simple()
        {
            var timeouts = Enumerable.Range(0, 3).Select(i => Env.Timeout(i)).ToList();
            var cond = timeouts[0].And(timeouts[1]).And(timeouts[2]);
            Assert.AreSame(timeouts[0], cond.Ev1);
            Assert.AreSame(timeouts[1], cond.Ev2);
            Assert.AreSame(timeouts[2], cond.Ev3);
            yield return cond;
            Assert.True(cond.Ev1.Succeeded);
            Assert.True(cond.Ev2.Succeeded);
            Assert.True(cond.Ev3.Succeeded);
            Assert.AreEqual(0, cond.Ev1.Value);
            Assert.AreEqual(1, cond.Ev2.Value);
            Assert.AreEqual(2, cond.Ev3.Value);
            Assert.AreEqual(3, cond.Value.Count);
            Assert.True(cond.Value.Contains(timeouts[0]));
            Assert.True(cond.Value.Contains(timeouts[1]));
            Assert.True(cond.Value.Contains(timeouts[2]));
        }

        IEvents And_Simple4()
        {
            var timeouts = Enumerable.Range(0, 4).Select(i => Env.Timeout(i)).ToList();
            var cond = timeouts[0].And(timeouts[1]).And(timeouts[2]).And(timeouts[3]);
            Assert.AreSame(timeouts[0], cond.Ev1);
            Assert.AreSame(timeouts[1], cond.Ev2);
            Assert.AreSame(timeouts[2], cond.Ev3);
            Assert.AreSame(timeouts[3], cond.Ev4);
            yield return cond;
            Assert.True(cond.Ev1.Succeeded);
            Assert.True(cond.Ev2.Succeeded);
            Assert.True(cond.Ev3.Succeeded);
            Assert.True(cond.Ev4.Succeeded);
            Assert.AreEqual(0, cond.Ev1.Value);
            Assert.AreEqual(1, cond.Ev2.Value);
            Assert.AreEqual(2, cond.Ev3.Value);
            Assert.AreEqual(3, cond.Ev4.Value);
            Assert.AreEqual(4, cond.Value.Count);
            Assert.True(cond.Value.Contains(timeouts[0]));
            Assert.True(cond.Value.Contains(timeouts[1]));
            Assert.True(cond.Value.Contains(timeouts[2]));
            Assert.True(cond.Value.Contains(timeouts[3]));
        }

        IEvents And_Simple5()
        {
            var timeouts = Enumerable.Range(0, 5).Select(i => Env.Timeout(i)).ToList();
            var cond = timeouts[0].And(timeouts[1]).And(timeouts[2]).And(timeouts[3]).And(timeouts[4]);
            Assert.AreSame(timeouts[0], cond.Ev1);
            Assert.AreSame(timeouts[1], cond.Ev2);
            Assert.AreSame(timeouts[2], cond.Ev3);
            Assert.AreSame(timeouts[3], cond.Ev4);
            Assert.AreSame(timeouts[4], cond.Ev5);
            yield return cond;
            Assert.True(cond.Ev1.Succeeded);
            Assert.True(cond.Ev2.Succeeded);
            Assert.True(cond.Ev3.Succeeded);
            Assert.True(cond.Ev4.Succeeded);
            Assert.True(cond.Ev5.Succeeded);
            Assert.AreEqual(0, cond.Ev1.Value);
            Assert.AreEqual(1, cond.Ev2.Value);
            Assert.AreEqual(2, cond.Ev3.Value);
            Assert.AreEqual(3, cond.Ev4.Value);
            Assert.AreEqual(4, cond.Ev5.Value);
            Assert.AreEqual(5, cond.Value.Count);
            Assert.True(cond.Value.Contains(timeouts[0]));
            Assert.True(cond.Value.Contains(timeouts[1]));
            Assert.True(cond.Value.Contains(timeouts[2]));
            Assert.True(cond.Value.Contains(timeouts[3]));
            Assert.True(cond.Value.Contains(timeouts[4]));
        }

        IEvents And_Nested()
        {
            var timeouts = Enumerable.Range(0, 3).Select(i => Env.Timeout(i)).ToList();
            var cond = timeouts[0].And(timeouts[2]).Or(timeouts[1]);
            Assert.AreSame(timeouts[0], cond.Ev1);
            Assert.AreSame(timeouts[2], cond.Ev2);
            Assert.AreSame(timeouts[1], cond.Ev3);
            yield return cond;
            Assert.True(cond.Ev1.Succeeded);
            Assert.False(cond.Ev2.Succeeded);
            Assert.True(cond.Ev3.Succeeded);
            Assert.AreEqual(0, cond.Ev1.Value);
            Assert.AreEqual(2, cond.Ev2.Value);
            Assert.AreEqual(1, cond.Ev3.Value);
            Assert.AreEqual(2, cond.Value.Count);
            Assert.True(cond.Value.Contains(timeouts[0]));
            Assert.True(cond.Value.Contains(timeouts[1]));
            Assert.AreEqual(1, Env.Now);
        }

        IEvents IAnd_WithAndCond()
        {
            var cond = Env.Timeout(1, value: 1).And(Env.Timeout(2, value: 2));
            var cond2 = cond.And(Env.Timeout(0, value: 0));
            yield return cond2;
            var values = cond2.Value.Select(v => v.Value).ToList();
            values.Sort();
            Assert.True(new List<object> {0, 1, 2}.SequenceEqual(values));
        }

        IEvents IAnd_WithOrCond()
        {
            var cond = Env.Timeout(1, value: 1).Or(Env.Timeout(2, value: 2));
            var cond2 = cond.And(Env.Timeout(0, value: 0));
            yield return cond2;
            var values = cond2.Value.Select(v => v.Value).ToList();
            values.Sort();
            Assert.True(new List<object> {0, 1}.SequenceEqual(values));
        }

        IEvents Or_Simple()
        {
            var timeouts = Enumerable.Range(0, 3).Select(i => Env.Timeout(i)).ToList();
            var cond = Env.AnyOf(timeouts[0], timeouts[1], timeouts[2]);
            Assert.AreSame(timeouts[0], cond.Ev1);
            Assert.AreSame(timeouts[1], cond.Ev2);
            Assert.AreSame(timeouts[2], cond.Ev3);
            yield return cond;
            Assert.True(cond.Ev1.Succeeded);
            Assert.False(cond.Ev2.Succeeded);
            Assert.False(cond.Ev3.Succeeded);
            Assert.AreEqual(0, cond.Ev1.Value);
            Assert.AreEqual(1, cond.Ev2.Value);
            Assert.AreEqual(2, cond.Ev3.Value);
            Assert.AreEqual(1, cond.Value.Count);
            Assert.True(cond.Value.Contains(timeouts[0]));
        }

        IEvents Or_Simple4()
        {
            var timeouts = Enumerable.Range(0, 4).Select(i => Env.Timeout(i)).ToList();
            var cond = Env.AnyOf(timeouts[0], timeouts[1], timeouts[2], timeouts[3]);
            Assert.AreSame(timeouts[0], cond.Ev1);
            Assert.AreSame(timeouts[1], cond.Ev2);
            Assert.AreSame(timeouts[2], cond.Ev3);
            Assert.AreSame(timeouts[3], cond.Ev4);
            yield return cond;
            Assert.True(cond.Ev1.Succeeded);
            Assert.False(cond.Ev2.Succeeded);
            Assert.False(cond.Ev3.Succeeded);
            Assert.False(cond.Ev4.Succeeded);
            Assert.AreEqual(0, cond.Ev1.Value);
            Assert.AreEqual(1, cond.Ev2.Value);
            Assert.AreEqual(2, cond.Ev3.Value);
            Assert.AreEqual(3, cond.Ev4.Value);
            Assert.AreEqual(1, cond.Value.Count);
            Assert.True(cond.Value.Contains(timeouts[0]));
        }

        IEvents Or_Simple5()
        {
            var timeouts = Enumerable.Range(0, 5).Select(i => Env.Timeout(i)).ToList();
            var cond = Env.AnyOf(timeouts[0], timeouts[1], timeouts[2], timeouts[3], timeouts[4]);
            Assert.AreSame(timeouts[0], cond.Ev1);
            Assert.AreSame(timeouts[1], cond.Ev2);
            Assert.AreSame(timeouts[2], cond.Ev3);
            Assert.AreSame(timeouts[3], cond.Ev4);
            Assert.AreSame(timeouts[4], cond.Ev5);
            yield return cond;
            Assert.True(cond.Ev1.Succeeded);
            Assert.False(cond.Ev2.Succeeded);
            Assert.False(cond.Ev3.Succeeded);
            Assert.False(cond.Ev4.Succeeded);
            Assert.False(cond.Ev5.Succeeded);
            Assert.AreEqual(0, cond.Ev1.Value);
            Assert.AreEqual(1, cond.Ev2.Value);
            Assert.AreEqual(2, cond.Ev3.Value);
            Assert.AreEqual(3, cond.Ev4.Value);
            Assert.AreEqual(4, cond.Ev5.Value);
            Assert.AreEqual(1, cond.Value.Count);
            Assert.True(cond.Value.Contains(timeouts[0]));
        }

        IEvents Or_Nested()
        {
            var timeouts = Enumerable.Range(0, 3).Select(i => Env.Timeout(i)).ToList();
            var cond = timeouts[0].Or(timeouts[1]).And(timeouts[2]);
            Assert.AreSame(timeouts[0], cond.Ev1);
            Assert.AreSame(timeouts[1], cond.Ev2);
            Assert.AreSame(timeouts[2], cond.Ev3);
            yield return cond;
            Assert.True(cond.Ev1.Succeeded);
            Assert.True(cond.Ev2.Succeeded);
            Assert.True(cond.Ev3.Succeeded);
            Assert.AreEqual(0, cond.Ev1.Value);
            Assert.AreEqual(1, cond.Ev2.Value);
            Assert.AreEqual(2, cond.Ev3.Value);
            Assert.AreEqual(3, cond.Value.Count);
            Assert.True(cond.Value.Contains(timeouts[0]));
            Assert.True(cond.Value.Contains(timeouts[1]));
            Assert.True(cond.Value.Contains(timeouts[2]));
            Assert.AreEqual(2, Env.Now);
        }

        IEvents IOr_WithOrCond()
        {
            var cond = Env.Timeout(1, value: 1).Or(Env.Timeout(2, value: 2));
            var cond2 = cond.Or(Env.Timeout(0, value: 0));
            yield return cond2;
            var values = cond2.Value.Select(v => v.Value).ToList();
            values.Sort();
            Assert.True(new List<object> {0}.SequenceEqual(values));
        }

        IEvents IOr_WithAndCond()
        {
            var cond = Env.Timeout(1, value: 1).And(Env.Timeout(2, value: 2));
            var cond2 = cond.Or(Env.Timeout(0, value: 0));
            yield return cond2;
            var values = cond2.Value.Select(v => v.Value).ToList();
            values.Sort();
            Assert.True(new List<object> {0}.SequenceEqual(values));
        }

        IEvents All_ManyStorePutEvents(Store<int> store)
        {
            var put3 = store.Put(3);
            yield return Env.AllOf(store.Put(1), store.Put(2), put3);
            Assert.True(store.ItemQueue.Contains(1));
            Assert.True(store.ItemQueue.Contains(2));
            Assert.True(store.ItemQueue.Contains(3));
        }

        IEvents All_SucceededEvents(Store<int> store)
        {
            var put3 = store.Put(3);
            var put5 = store.Put(5);
            var put7 = store.Put(7);
            yield return Env.AllOf(put3, put5, put7);
            store.Put(9);
        }

        IEvents ContinueOnInterrupt_Timeouts()
        {
            Env.Process(Interrupter(Env.ActiveProcess, null, 10));
            var cond = Env.Timeout(1).And(Env.Timeout(2));
            yield return cond;
            Assert.True(Env.ActiveProcess.Interrupted());
            yield return cond;
            Assert.AreEqual(2, Env.Now);
        }

        IEvents ContinueOnInterrupt_Resource()
        {
            Env.Process(Interrupter(Env.ActiveProcess, null, 10));
            var ev1 = Env.Event();
            var ev2 = Env.Event();
            var cond = ev1.And(ev2).And(Env.Timeout(2));
            yield return cond;
            Assert.True(Env.ActiveProcess.Interrupted());
            Env.Process(EventTriggerer(ev1));
            Env.Process(EventTriggerer(ev2));
            yield return cond;
            Assert.AreEqual(2, Env.Now);
            Assert.True(ev1.Succeeded);
            Assert.True(ev2.Succeeded);
        }

        IEvents ContinueOnInterrupt_Generic()
        {
            Env.Process(Interrupter(Env.ActiveProcess, null, 10));
            var resource = Sim.Resource(Env, 1);
            var cond = resource.Request().And(Env.Timeout(2));
            yield return cond;
            Assert.True(Env.ActiveProcess.Interrupted());
            yield return cond;
            Assert.AreEqual(2, Env.Now);
            Assert.True(resource.Users.Count() == 1);
            cond.Ev1.Dispose();
        }

        IEvents CustomEvaluator1()
        {
            var ev1 = Env.Event();
            var ev2 = Env.Event();
            Env.Process(CustomEvaluator1_EventTrigger(ev1, ev2));
            var cond = Env.Condition(ev1, ev2, c => Equals(7, c.Ev1.Value) && Equals(3, c.Ev2.Value));
            yield return cond;
            Assert.True(ev1.Succeeded);
            Assert.True(ev2.Succeeded);
            Assert.AreSame(ev1, cond.Ev1);
            Assert.AreSame(ev2, cond.Ev2);
        }

        IEvents CustomEvaluator1_EventTrigger(SimEvent<object> ev1, SimEvent<object> ev2)
        {
            yield return Env.Timeout(5);
            ev1.Succeed(7);
            yield return Env.Timeout(5);
            ev2.Succeed(3);
        }

        IEvents CustomEvaluator2()
        {
            var ev1 = Env.Event();
            var ev2 = Env.Event();
            Env.Process(CustomEvaluator2_EventTrigger(ev1, ev2));
            var cond = Env.Condition(ev1, ev2, c => Equals(7, c.Ev1.Value) || Equals(3, c.Ev2.Value));
            yield return cond;
            Assert.True(ev1.Succeeded);
            Assert.False(ev2.Succeeded);
            Assert.AreSame(ev1, cond.Ev1);
            Assert.AreSame(ev2, cond.Ev2);
        }

        IEvents CustomEvaluator2_EventTrigger(SimEvent<object> ev1, SimEvent<object> ev2)
        {
            yield return Env.Timeout(5);
            ev1.Succeed(7);
            yield return Env.Timeout(5);
            ev2.Succeed(3);
        }

        IEvents ImmutableResults()
        {
            // Results of conditions should not change after they have been triggered.
            var timeouts = Enumerable.Range(0, 3).Select(i => Env.Timeout(i)).ToList();
            // The or condition in this expression will trigger immediately. 
            // The and condition will trigger later on.
            var condition = timeouts[0].Or(timeouts[1].And(timeouts[2]));

            yield return condition;
            Assert.AreEqual(1, condition.Value.Count);
            Assert.True(condition.Value.Contains(timeouts[0]));

            // Makes sure that the results of condition were frozen. The results of
            // the nested and condition do not become visible afterwards.
            yield return Env.Timeout(2);
            Assert.AreEqual(1, condition.Value.Count);
            Assert.True(condition.Value.Contains(timeouts[0]));
        }

        static IEvents SharedAndCondition_P1(IList<Timeout> timeouts, Condition<Timeout, Timeout> condition)
        {
            yield return condition;
            Assert.AreEqual(2, condition.Value.Count);
            Assert.True(condition.Value.Contains(timeouts[0]));
            Assert.True(condition.Value.Contains(timeouts[1]));
        }

        static IEvents SharedAndCondition_P2(IList<Timeout> timeouts, Condition<Timeout, Timeout, Timeout> condition)
        {
            yield return condition;
            Assert.AreEqual(3, condition.Value.Count);
            Assert.True(condition.Value.Contains(timeouts[0]));
            Assert.True(condition.Value.Contains(timeouts[1]));
            Assert.True(condition.Value.Contains(timeouts[2]));
        }

        static IEvents SharedOrCondition_P1(IList<Timeout> timeouts, Condition<Timeout, Timeout> condition)
        {
            yield return condition;
            Assert.AreEqual(1, condition.Value.Count);
            Assert.True(condition.Value.Contains(timeouts[0]));
        }

        static IEvents SharedOrCondition_P2(IList<Timeout> timeouts, Condition<Timeout, Timeout, Timeout> condition)
        {
            yield return condition;
            Assert.AreEqual(1, condition.Value.Count);
            Assert.True(condition.Value.Contains(timeouts[0]));
        }

        IEnumerable<SimEvent> SameEvent_AndCondition_PEM()
        {
            var t = Env.Timeout(5);
            var c = t & t;
            yield return c;
            Assert.AreEqual(2, c.Value.Count);
            Assert.AreSame(c.Value[0], t);
            Assert.AreSame(c.Value[1], t);
        }

        IEnumerable<SimEvent> SameEvent_OrCondition_PEM()
        {
            var t = Env.Timeout(5);
            var c = t | t;
            yield return c;
            Assert.AreEqual(1, c.Value.Count);
            Assert.AreSame(c.Value[0], t);
        }

        IEnumerable<SimEvent> SameSucceedEvent_AndCondition_PEM()
        {
            var t = Env.Timeout(5);
            yield return t;
            var c = t & t;
            yield return c;
            Assert.AreEqual(2, c.Value.Count);
            Assert.AreSame(c.Value[0], t);
            Assert.AreSame(c.Value[1], t);
        }

        IEnumerable<SimEvent> SameSucceedEvent_OrCondition_PEM()
        {
            var t = Env.Timeout(5);
            yield return t;
            var c = t | t;
            yield return c;
            Assert.AreEqual(1, c.Value.Count);
            Assert.AreSame(c.Value[0], t);
        }

        [Test]
        public void All_ManyStorePutEvents_UnlimitedStore()
        {
            var store = Sim.Store<int>(Env);
            Env.Process(All_ManyStorePutEvents(store));
            Env.Run();
            Assert.AreEqual(3, store.ItemQueue.Count());
        }

        [Test]
        public void All_SucceededEvents_UnlimitedStore()
        {
            var store = Sim.Store<int>(Env);
            Env.Process(All_SucceededEvents(store));
            Env.Run();
            Assert.AreEqual(4, store.ItemQueue.Count());
        }

        [Test]
        public void RunContinueOnInterrupt_Generic()
        {
            Env.Process(ContinueOnInterrupt_Generic());
            Env.Run(5);
        }

        [Test]
        public void RunContinueOnInterrupt_Resource()
        {
            Env.Process(ContinueOnInterrupt_Resource());
            Env.Run(5);
        }

        [Test]
        public void RunContinueOnInterrupt_Timeouts()
        {
            Env.Process(ContinueOnInterrupt_Timeouts());
            Env.Run(5);
        }

        [Test]
        public void RunCustomEvaluator1()
        {
            Env.Process(CustomEvaluator1());
            Env.Run();
        }

        [Test]
        public void RunCustomEvaluator2()
        {
            Env.Process(CustomEvaluator2());
            Env.Run();
        }

        [Test]
        public void Run_And_Nested()
        {
            Env.Process(And_Nested());
            Env.Run();
        }

        [Test]
        public void Run_And_Simple()
        {
            Env.Process(And_Simple());
            Env.Run();
        }

        [Test]
        public void Run_And_Simple4()
        {
            Env.Process(And_Simple4());
            Env.Run();
        }

        [Test]
        public void Run_And_Simple5()
        {
            Env.Process(And_Simple5());
            Env.Run();
        }

        [Test]
        public void Run_IAnd_WithAndCond()
        {
            Env.Process(IAnd_WithAndCond());
            Env.Run();
        }

        [Test]
        public void Run_IAnd_WithOrCond()
        {
            Env.Process(IAnd_WithOrCond());
            Env.Run();
        }

        [Test]
        public void Run_IOr_WithAndCond()
        {
            Env.Process(IOr_WithAndCond());
            Env.Run();
        }

        [Test]
        public void Run_IOr_WithOrCond()
        {
            Env.Process(IOr_WithOrCond());
            Env.Run();
        }

        [Test]
        public void Run_ImmutableResults()
        {
            Env.Process(ImmutableResults());
            Env.Run();
        }

        [Test]
        public void Run_Or_Nested()
        {
            Env.Process(Or_Nested());
            Env.Run();
        }

        [Test]
        public void Run_Or_Simple()
        {
            Env.Process(Or_Simple());
            Env.Run();
        }

        [Test]
        public void Run_Or_Simple4()
        {
            Env.Process(Or_Simple4());
            Env.Run();
        }

        [Test]
        public void Run_Or_Simple5()
        {
            Env.Process(Or_Simple5());
            Env.Run();
        }

        [Test]
        public void SameEvent_AndCondition()
        {
            Env.Process(SameEvent_AndCondition_PEM());
            Env.Run();
        }

        [Test]
        public void SameEvent_OrCondition()
        {
            Env.Process(SameEvent_OrCondition_PEM());
            Env.Run();
        }

        [Test]
        public void SameSucceedEvent_AndCondition()
        {
            Env.Process(SameSucceedEvent_AndCondition_PEM());
            Env.Run();
        }

        [Test]
        public void SameSucceedEvent_OrCondition()
        {
            Env.Process(SameSucceedEvent_OrCondition_PEM());
            Env.Run();
        }

        [Test]
        public void SharedAndCondition()
        {
            var timeouts = Enumerable.Range(0, 3).Select(i => Env.Timeout(i)).ToList();
            var c1 = timeouts[0].And(timeouts[1]);
            var c2 = c1.And(timeouts[2]);

            Env.Process(SharedAndCondition_P1(timeouts, c1));
            Env.Process(SharedAndCondition_P2(timeouts, c2));
            Env.Run();
        }

        [Test]
        public void SharedOrCondition()
        {
            var timeouts = Enumerable.Range(0, 3).Select(i => Env.Timeout(i)).ToList();
            var c1 = timeouts[0].Or(timeouts[1]);
            var c2 = c1.Or(timeouts[2]);

            Env.Process(SharedOrCondition_P1(timeouts, c1));
            Env.Process(SharedOrCondition_P2(timeouts, c2));
            Env.Run();
        }
    }
}