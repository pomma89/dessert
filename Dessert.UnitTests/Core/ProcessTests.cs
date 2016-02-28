// 
// ProcessTests.cs
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

namespace DIBRIS.Dessert.Tests.Core
{
    using System;
    using Dessert.Events;
    using Dessert.Resources;
    using NUnit.Framework;
    using IEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    sealed class ProcessTests : TestBase
    {
        IEvents Interrupted_YieldingGetToken()
        {
            Container.GetEvent getEv;
            Env.Process(Interrupter(Env.ActiveProcess, 5));
            yield return getEv = Sim.Container(Env, 20, 10).Get(20);
            Assert.AreEqual(5, Env.Now);
            Assert.False(getEv.Disposed);
            Assert.True(Env.ActiveProcess.Interrupted());
            yield return Env.Timeout(7);
        }

        IEvents Interrupted_YieldingTimeoutToken()
        {
            Timeout<double> timeout;
            Env.Process(Interrupter(Env.ActiveProcess, 5));
            yield return timeout = Env.Timeout(7);
            Assert.AreEqual(5, Env.Now);
            Assert.False(timeout.Succeeded);
            Assert.True(Env.ActiveProcess.Interrupted());
            yield return Env.Timeout(7);
        }

        IEvents Interrupter(SimProcess victim, double delay = 0, object value = null)
        {
            yield return Env.Timeout(delay);
            if (value != null) {
                victim.Interrupt(value);
            } else {
                victim.Interrupt();
            }
        }

        IEvents TargetTester1()
        {
            while (true) {
                Assert.AreSame(null, Env.ActiveProcess.Target);
                yield return Env.Process(TargetTester2(Env.ActiveProcess));
                yield return Env.Timeout(10);
            }
        }

        IEvents TargetTester2(SimProcess starter)
        {
            Assert.AreSame(null, Env.ActiveProcess.Target);
            Assert.AreSame(Env.ActiveProcess, starter.Target);
            yield break;
        }

        IEvents TimeoutYielder_WithInner()
        {
            yield return Env.Timeout(5);
            var inner = Env.Process(TargetTester1());
            Assert.True(inner.IsAlive);
            yield return inner;
            Assert.False(inner.IsAlive);
        }

        IEvents TriggeredTimeout_PEM()
        {
            const string value = "I was already done";
            var ev = Env.Timeout(1, value);
            // Starts the child after the timeout has already happened.
            yield return Env.Timeout(2);
            var child = Env.Process(TriggeredTimeout_PEM_Child(ev));
            yield return child;
            Assert.AreEqual(value, child.Value);
        }

        IEvents TriggeredTimeout_PEM_Child(SimEvent ev)
        {
            yield return ev;
            Env.Exit(ev.Value);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Interrupt_DeadProcess()
        {
            var dead = Env.Process(EmptyProcess());
            Env.Process(Interrupter(dead));
            Env.Run();
        }

        [Test]
        public void Interrupt_YieldingGetToken()
        {
            Env.Process(Interrupted_YieldingGetToken());
            Env.Run();
            Assert.AreEqual(12, Env.Now);
        }

        [Test]
        public void Interrupt_YieldingTimeoutToken()
        {
            Env.Process(Interrupted_YieldingTimeoutToken());
            Env.Run();
            Assert.AreEqual(12, Env.Now);
        }

        [Test]
        public void IsAlive_TimeoutYielder()
        {
            var yielder = Env.Process(TimeoutYielder());
            Assert.True(yielder.IsAlive);
            Env.Run(100);
            Assert.False(yielder.IsAlive);
        }

        [Test]
        public void IsAlive_TimeoutYielder_WithInner()
        {
            var yielder = Env.Process(TimeoutYielder_WithInner());
            Assert.True(yielder.IsAlive);
            Env.Run(100);
            // Inner is a never ending process...
            Assert.True(yielder.IsAlive);
        }

        [Test]
        public void Target_YieldingProcess()
        {
            Env.Process(TargetTester1());
            Env.Run(100);
        }

        [Test]
        public void TriggeredTimeout()
        {
            Env.Run(Env.Process(TriggeredTimeout_PEM()));
        }
    }
}