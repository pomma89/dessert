// 
// MonitorTests.cs
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

namespace Dessert.Tests.Recording
{
    using System.Diagnostics;
    using Dessert.Recording;
    using NUnit.Framework;

    sealed class MonitorTests : RecorderTests<Monitor>
    {
        protected override Monitor NewBoundRecorder()
        {
            var monitor = Sim.Monitor(Env);
            Assert.IsNotNull(monitor);
            Assert.IsInstanceOf(typeof(Monitor), monitor);
            return monitor;
        }

        protected override Monitor NewUnboundRecorder()
        {
            var monitor = Sim.Monitor();
            Assert.IsNotNull(monitor);
            Assert.IsInstanceOf(typeof(Monitor), monitor);
            return monitor;
        }

        [TestCase(1, 2, 3, 4), TestCase(1, 1, 3, 3, 5, 5), TestCase(1, 10)]
        public void Values_IntegerPairs(params int[] values)
        {
            Debug.Assert(values.Length%2 == 0);
            for (var i = 0; i < values.Length; i += 2) {
                BoundRecorder.Observe(values[i], values[i + 1]);
                UnboundRecorder.Observe(values[i], values[i + 1]);
            }
            var bEn = BoundRecorder.Samples.GetEnumerator();
            var uEn = UnboundRecorder.Samples.GetEnumerator();
            for (var i = 0; i < values.Length; i += 2) {
                Assert.True(bEn.MoveNext());
                Assert.True(uEn.MoveNext());
                Assert.AreEqual(values[i], bEn.Current.Sample);
                Assert.AreEqual(values[i + 1], bEn.Current.Time);
                Assert.AreEqual(values[i], BoundRecorder[i/2].Sample);
                Assert.AreEqual(values[i + 1], BoundRecorder[i/2].Time);
                Assert.AreEqual(values[i], uEn.Current.Sample);
                Assert.AreEqual(values[i + 1], uEn.Current.Time);
                Assert.AreEqual(values[i], UnboundRecorder[i/2].Sample);
                Assert.AreEqual(values[i + 1], UnboundRecorder[i/2].Time);
            }
        }

        [TestCase(1.4, 2.3, 3.6, 4.1), TestCase(1.3, 1, 3, 3, 5, 5.3), TestCase(1.23, 10.01)]
        public void Values_DecimalPairs(params double[] values)
        {
            Debug.Assert(values.Length%2 == 0);
            for (var i = 0; i < values.Length; i += 2) {
                BoundRecorder.Observe(values[i], values[i + 1]);
                UnboundRecorder.Observe(values[i], values[i + 1]);
            }
            var bEn = BoundRecorder.Samples.GetEnumerator();
            var uEn = UnboundRecorder.Samples.GetEnumerator();
            for (var i = 0; i < values.Length; i += 2) {
                Assert.True(bEn.MoveNext());
                Assert.True(uEn.MoveNext());
                Assert.AreEqual(values[i], bEn.Current.Sample);
                Assert.AreEqual(values[i + 1], bEn.Current.Time);
                Assert.AreEqual(values[i], BoundRecorder[i/2].Sample);
                Assert.AreEqual(values[i + 1], BoundRecorder[i/2].Time);
                Assert.AreEqual(values[i], uEn.Current.Sample);
                Assert.AreEqual(values[i + 1], uEn.Current.Time);
                Assert.AreEqual(values[i], UnboundRecorder[i/2].Sample);
                Assert.AreEqual(values[i + 1], UnboundRecorder[i/2].Time);
            }
        }
    }
}