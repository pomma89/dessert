// 
// EnvironmentTests.cs
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

namespace Dessert.Tests.Core
{
    using System;
    using NUnit.Framework;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    sealed class EnvironmentTests : TestBase
    {
        SimEvents Interrupted()
        {
            const String sentValue = "PINO";
            Env.Process(Interrupter(Env.ActiveProcess, sentValue, 5));
            while (true) {
                yield return Env.Timeout(6);
                object value;
                Assert.True(Env.ActiveProcess.Interrupted(out value));
                Assert.AreEqual(sentValue, value);
            }
        }

        SimEvents Interrupted_NotCaught()
        {
            Env.Process(Interrupter(Env.ActiveProcess, null));
            while (true) {
                yield return Env.Timeout(5);
            }
        }

        static SimEvents NullYielder()
        {
            while (true) {
                yield return null;
            }
        }

        [Test]
        public void Interrupt_ManyTimes()
        {
            Env.Process(Interrupted());
            Env.Run(700);
        }

        [Test]
        public void Interrupt_OneTime()
        {
            Env.Process(Interrupted());
            Env.Run(3);
        }

        [Test, ExpectedException(typeof(InterruptUncaughtException))]
        public void Interrupt_Uncaught()
        {
            Env.Process(Interrupted_NotCaught());
            Env.Run(7);
        }

        [Test]
        public void Peek_NoActivities()
        {
            Assert.AreEqual(double.PositiveInfinity, Env.Peek);
        }

        [Test]
        public void Peek_OneActivity()
        {
            Env.Process(TimeoutYielder());
            Assert.AreEqual(0, Env.Peek);
        }

        [Test]
        public void Peek_OneActivity_WithDelay()
        {
            const double delay = 7;
            Env.DelayedProcess(TimeoutYielder(), delay);
            Assert.AreEqual(0, Env.Peek);
        }

        [Test]
        public void Peek_OneEvent()
        {
            const double delay = 7;
            Env.Timeout(delay);
            Assert.AreEqual(delay, Env.Peek);
        }

        [Test]
        public void Peek_TwoActivities()
        {
            const double shortDelay = 7;
            const double longDelay = 14;
            Env.DelayedProcess(TimeoutYielder(), longDelay);
            Env.DelayedProcess(TimeoutYielder(), shortDelay);
            Assert.AreEqual(0, Env.Peek);
        }

        [Test]
        public void Peek_TwoActivities_AfterRun()
        {
            const double shortDelay = 7;
            const double longDelay = 14;
            Env.DelayedProcess(TimeoutYielder(), longDelay);
            Env.DelayedProcess(TimeoutYielder(), shortDelay).Callbacks.Add(p => Assert.AreEqual(longDelay, Env.Peek));
            Env.Run();
            Assert.AreEqual(double.PositiveInfinity, Env.Peek);
        }

        [Test]
        public void Peek_TwoEvents()
        {
            const double shortDelay = 7;
            const double longDelay = 14;
            Env.Timeout(longDelay);
            Env.Timeout(shortDelay);
            Assert.AreEqual(shortDelay, Env.Peek);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Run_NoActivities()
        {
            Env.Run(10);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Yield_NullEvent()
        {
            Env.Process(NullYielder());
            Env.Run(1);
        }
    }
}