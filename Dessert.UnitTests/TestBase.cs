// TestBase.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
//
// Copyright (c) 2012-2016 Alessio Parma <alessio.parma@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace DIBRIS.Dessert.Tests
{
    using Dessert.Events;
    using NUnit.Framework;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    [TestFixture]
    internal abstract class TestBase
    {
        [SetUp]
        public virtual void SetUp()
        {
            Env = Sim.Environment();
            Assert.IsNotNull(Env);
            Assert.IsInstanceOf(typeof(SimEnvironment), Env);
            Assert.AreEqual(0, Env.Now);
        }

        [TearDown]
        public virtual void TearDown()
        {
            Env = null;
        }

        protected const double TinyNeg = -0.01;
        protected const double SmallNeg = -1;
        protected const double LargeNeg = -100;

        private const double Delta = 0.000001;
        private const double Epsilon = 0.15; // Relative error: less than 15%

        protected SimEnvironment Env;

        protected static SimEvents EmptyProcess()
        {
            yield break;
        }

        protected static SimEvents EventTriggerer(SimEvent<object> ev, object value = null)
        {
            ev.Succeed(value);
            yield break;
        }

        protected SimEvents Interrupter(SimProcess victim, String value, Double timeout = 0)
        {
            while (true)
            {
                victim.Interrupt(value);
                yield return Env.Timeout(timeout);
            }
        }

        protected SimEvents TimeoutYielder()
        {
            yield return Env.Timeout(5);
        }

        protected static void ApproxEquals(double expected, double observed)
        {
            if (double.IsNaN(expected))
            {
                Assert.Fail("NaN should not be returned");
            }
            var errMsg = string.Format("Expected {0}, observed {1}", expected, observed);
            if (expected > -Delta && expected < Delta)
            {
                Assert.True(Math.Abs(expected - observed) < Epsilon, errMsg);
            }
            else {
                Assert.True(Math.Abs((expected - observed) / expected) < Epsilon, errMsg);
            }
        }
    }

    /// <summary>
    ///   Taken from following page:
    ///   http://blogs.iis.net/yigalatz/archive/2011/03/31/unit-tests-should-not-debug-assert.aspx
    ///   The code was then edited to better suit this project needs.
    /// </summary>
    [SetUpFixture]
    public class DebugPopupRemover
    {
        [SetUp]
        public static void SetUp()
        {
            var removeListener = Debug.Listeners.OfType<DefaultTraceListener>().FirstOrDefault();
            if (removeListener != null)
            {
                Debug.Listeners.Remove(removeListener);
#pragma warning disable CC0022 // Should dispose object
                Debug.Listeners.Add(new FailOnAssert());
#pragma warning restore CC0022 // Should dispose object
            }
        }

        private sealed class FailOnAssert : TraceListener
        {
            public override void Fail(string message)
            {
                var errMsg = "DEBUG.ASSERT: " + message;
                Console.WriteLine(errMsg);
                Assert.Fail(errMsg);
            }

            public override void Fail(string message, string detailMessage)
            {
                var errMsg = "DEBUG.ASSERT: " + message + Environment.NewLine + detailMessage;
                Console.WriteLine(errMsg);
                Assert.Fail(errMsg);
            }

            public override void Write(string message)
            {
            }

            public override void WriteLine(string message)
            {
            }
        }
    }
}