// 
// RecorderTests.cs
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
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Dessert.Recording;
    using NUnit.Framework;
    using Troschuetz.Random.Distributions.Continuous;

    abstract class RecorderTests<TRec> : TestBase where TRec : class, IRecorder
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            BoundRecorder = NewBoundRecorder();
            Assert.AreSame(Env, BoundRecorder.Env);
            Assert.AreEqual(0, BoundRecorder.Count);
            Assert.AreEqual(Env.Now, BoundRecorder.StartTime);
            // Since there are no observations, following line would throw an exception:
            // Assert.AreEqual(0, BoundRecorder.Total());
            UnboundRecorder = NewUnboundRecorder();
            Assert.AreEqual(0, UnboundRecorder.Count);
            Assert.AreEqual(UnboundRecorder.Env.Now, UnboundRecorder.StartTime);
            // Since there are no observations, following line would throw an exception:
            // Assert.AreEqual(0, UnboundRecorder.Total());
        }

        [TearDown]
        public override void TearDown()
        {
            BoundRecorder = null;
            UnboundRecorder = null;
            base.TearDown();
        }

        const int RepetitionCount = 100000;

        protected TRec BoundRecorder;
        protected TRec UnboundRecorder;

        protected abstract TRec NewBoundRecorder();

        protected abstract TRec NewUnboundRecorder();

        [TestCase(3, 1, 2, 3), TestCase(1, 2), TestCase(3, -1, 2, 5), TestCase(6, 5, 5, 5, 5, 5, 5),
         TestCase(7, 5, -5, 5, -5, 5, -5, 8)]
        public void Count_IntegerValues(params int[] values)
        {
            var expected = values[0];
            for (var i = 1; i < values.Length; ++i) {
                BoundRecorder.Observe(values[i]);
                UnboundRecorder.Observe(values[i]);
            }
            Assert.AreEqual(expected, BoundRecorder.Count);
            Assert.AreEqual(expected, UnboundRecorder.Count);
        }

        [TestCase(3, 1.5, 2, 2.5), TestCase(1, 2.34), TestCase(3, -1.5, 2, 5.5),
         TestCase(6, 5.25, 5.25, 5.25, 5.25, 5.25, 5.25), TestCase(6, 5.25, -5.25, 5.25, -5.25, 5.25, -5.25),
         TestCase(5, -5.25, 5.26, -5.25, 5.26, -5.25)]
        public void Count_DecimalValues(params double[] values)
        {
            var expected = values[0];
            for (var i = 1; i < values.Length; ++i) {
                BoundRecorder.Observe(values[i]);
                UnboundRecorder.Observe(values[i]);
            }
            Assert.AreEqual(expected, BoundRecorder.Count);
            Assert.AreEqual(expected, UnboundRecorder.Count);
        }

        [TestCase(2, 1, 2, 3), TestCase(2, 2), TestCase(2, -1, 2, 5), TestCase(5, 5, 5, 5, 5, 5, 5),
         TestCase(0, 5, -5, 5, -5, 5, -5)]
        public void Mean_IntegerValues(params int[] values)
        {
            var expected = values[0];
            for (var i = 1; i < values.Length; ++i) {
                BoundRecorder.Observe(values[i]);
                UnboundRecorder.Observe(values[i]);
            }
            Assert.AreEqual(expected, BoundRecorder.Mean());
            Assert.AreEqual(expected, UnboundRecorder.Mean());
        }

        [TestCase(2, 1.5, 2, 2.5), TestCase(2.34, 2.34), TestCase(2, -1.5, 2, 5.5),
         TestCase(5.25, 5.25, 5.25, 5.25, 5.25, 5.25, 5.25), TestCase(0, 5.25, -5.25, 5.25, -5.25, 5.25, -5.25),
         TestCase(0.005, 5.26, -5.25, 5.26, -5.25, 5.26, -5.25)]
        public void Mean_DecimalValues(params double[] values)
        {
            var expected = values[0];
            for (var i = 1; i < values.Length; ++i) {
                BoundRecorder.Observe(values[i]);
                UnboundRecorder.Observe(values[i]);
            }
            ApproxEquals(expected, BoundRecorder.Mean());
            ApproxEquals(expected, UnboundRecorder.Mean());
        }

        [TestCase(1), TestCase(10), TestCase(100), TestCase(1000)]
        public void ExponentialDistribution(double lambda)
        {
            var n = new ExponentialDistribution(lambda);
            var t = 0.0;
            foreach (var x in Enumerable.Range(0, RepetitionCount).Select(i => n.NextDouble())) {
                t += x;
                BoundRecorder.Observe(x);
                UnboundRecorder.Observe(x);
            }
            ApproxEquals(RepetitionCount, BoundRecorder.Count);
            ApproxEquals(RepetitionCount, UnboundRecorder.Count);
            ApproxEquals(t, BoundRecorder.Total());
            ApproxEquals(t, UnboundRecorder.Total());
            ApproxEquals(n.Mean, BoundRecorder.Mean());
            ApproxEquals(n.Mean, UnboundRecorder.Mean());
            ApproxEquals(n.Variance, BoundRecorder.Variance());
            ApproxEquals(n.Variance, UnboundRecorder.Variance());
            ApproxEquals(Math.Sqrt(n.Variance), BoundRecorder.StdDev());
            ApproxEquals(Math.Sqrt(n.Variance), UnboundRecorder.StdDev());
        }

        [TestCase(1), TestCase(10), TestCase(100), TestCase(1000)]
        public void ExponentialDistribution_WithUniformTimes(double lambda)
        {
            var n = new ExponentialDistribution(lambda);
            var t = 0.0;
            foreach (var i in Enumerable.Range(1, RepetitionCount)) {
                var x = n.NextDouble();
                t += x;
                BoundRecorder.Observe(x, i);
                UnboundRecorder.Observe(x, i);
            }
            ApproxEquals(RepetitionCount, BoundRecorder.Count);
            ApproxEquals(RepetitionCount, UnboundRecorder.Count);
            ApproxEquals(t, BoundRecorder.Total());
            ApproxEquals(t, UnboundRecorder.Total());
            ApproxEquals(n.Mean, BoundRecorder.TimeMean(RepetitionCount + 1));
            ApproxEquals(n.Mean, UnboundRecorder.TimeMean(RepetitionCount + 1));
            ApproxEquals(n.Variance, BoundRecorder.TimeVariance(RepetitionCount + 1));
            ApproxEquals(n.Variance, UnboundRecorder.TimeVariance(RepetitionCount + 1));
            ApproxEquals(Math.Sqrt(n.Variance), BoundRecorder.TimeStdDev(RepetitionCount + 1));
            ApproxEquals(Math.Sqrt(n.Variance), UnboundRecorder.TimeStdDev(RepetitionCount + 1));
        }

        [TestCase(1, 0.1), TestCase(10, 0.1), TestCase(1, 1), TestCase(100, 1), TestCase(100, 10)]
        public void NormalDistribution(double mu, double sigma)
        {
            var n = new NormalDistribution(mu, sigma);
            var t = 0.0;
            foreach (var x in Enumerable.Range(0, RepetitionCount).Select(i => n.NextDouble())) {
                t += x;
                BoundRecorder.Observe(x);
                UnboundRecorder.Observe(x);
            }
            ApproxEquals(RepetitionCount, BoundRecorder.Count);
            ApproxEquals(RepetitionCount, UnboundRecorder.Count);
            ApproxEquals(t, BoundRecorder.Total());
            ApproxEquals(t, UnboundRecorder.Total());
            ApproxEquals(n.Mean, BoundRecorder.Mean());
            ApproxEquals(n.Mean, UnboundRecorder.Mean());
            ApproxEquals(n.Variance, BoundRecorder.Variance());
            ApproxEquals(n.Variance, UnboundRecorder.Variance());
            ApproxEquals(Math.Sqrt(n.Variance), BoundRecorder.StdDev());
            ApproxEquals(Math.Sqrt(n.Variance), UnboundRecorder.StdDev());
        }

        [TestCase(1, 0.1), TestCase(10, 0.1), TestCase(1, 1), TestCase(100, 1), TestCase(100, 10)]
        public void NormalDistribution_WithUniformTimes(double mu, double sigma)
        {
            var n = new NormalDistribution(mu, sigma);
            var t = 0.0;
            foreach (var i in Enumerable.Range(1, RepetitionCount)) {
                var x = n.NextDouble();
                t += x;
                BoundRecorder.Observe(x, i);
                UnboundRecorder.Observe(x, i);
            }
            ApproxEquals(RepetitionCount, BoundRecorder.Count);
            ApproxEquals(RepetitionCount, UnboundRecorder.Count);
            ApproxEquals(t, BoundRecorder.Total());
            ApproxEquals(t, UnboundRecorder.Total());
            ApproxEquals(n.Mean, BoundRecorder.TimeMean(RepetitionCount + 1));
            ApproxEquals(n.Mean, UnboundRecorder.TimeMean(RepetitionCount + 1));
            ApproxEquals(n.Variance, BoundRecorder.TimeVariance(RepetitionCount + 1));
            ApproxEquals(n.Variance, UnboundRecorder.TimeVariance(RepetitionCount + 1));
            ApproxEquals(Math.Sqrt(n.Variance), BoundRecorder.TimeStdDev(RepetitionCount + 1));
            ApproxEquals(Math.Sqrt(n.Variance), UnboundRecorder.TimeStdDev(RepetitionCount + 1));
        }

        [TestCase(1, 6), TestCase(10, 100), TestCase(1, 2), TestCase(-1, 10), TestCase(-150, 300)]
        public void UniformDistribution(double alpha, double beta)
        {
            var n = new ContinuousUniformDistribution(alpha, beta);
            var t = 0.0;
            foreach (var x in Enumerable.Range(0, RepetitionCount).Select(i => n.NextDouble())) {
                t += x;
                BoundRecorder.Observe(x);
                UnboundRecorder.Observe(x);
            }
            ApproxEquals(RepetitionCount, BoundRecorder.Count);
            ApproxEquals(RepetitionCount, UnboundRecorder.Count);
            ApproxEquals(t, BoundRecorder.Total());
            ApproxEquals(t, UnboundRecorder.Total());
            ApproxEquals(n.Mean, BoundRecorder.Mean());
            ApproxEquals(n.Mean, UnboundRecorder.Mean());
            ApproxEquals(n.Variance, BoundRecorder.Variance());
            ApproxEquals(n.Variance, UnboundRecorder.Variance());
            ApproxEquals(Math.Sqrt(n.Variance), BoundRecorder.StdDev());
            ApproxEquals(Math.Sqrt(n.Variance), UnboundRecorder.StdDev());
        }

        [TestCase(1, 6), TestCase(10, 100), TestCase(1, 2), TestCase(-1, 10), TestCase(-150, 300)]
        public void UniformDistribution_WithUniformTimes(double alpha, double beta)
        {
            var n = new ContinuousUniformDistribution(alpha, beta);
            var t = 0.0;
            foreach (var i in Enumerable.Range(1, RepetitionCount)) {
                var x = n.NextDouble();
                t += x;
                BoundRecorder.Observe(x, i);
                UnboundRecorder.Observe(x, i);
            }
            ApproxEquals(RepetitionCount, BoundRecorder.Count);
            ApproxEquals(RepetitionCount, UnboundRecorder.Count);
            ApproxEquals(t, BoundRecorder.Total());
            ApproxEquals(t, UnboundRecorder.Total());
            ApproxEquals(n.Mean, BoundRecorder.TimeMean(RepetitionCount + 1));
            ApproxEquals(n.Mean, UnboundRecorder.TimeMean(RepetitionCount + 1));
            ApproxEquals(n.Variance, BoundRecorder.TimeVariance(RepetitionCount + 1));
            ApproxEquals(n.Variance, UnboundRecorder.TimeVariance(RepetitionCount + 1));
            ApproxEquals(Math.Sqrt(n.Variance), BoundRecorder.TimeStdDev(RepetitionCount + 1));
            ApproxEquals(Math.Sqrt(n.Variance), UnboundRecorder.TimeStdDev(RepetitionCount + 1));
        }

        [TestCase(2, 1, 0, 2, 3, 3, 6), TestCase(2, 2, 0, 2, 3, 2, 6), TestCase(2, 2, 0, 2, 9, 2, 81),
         TestCase(1.6875, 1, 0, 2, 9, 6, 15)]
        public void TimeMean_SomeSamples(params double[] values)
        {
            var expected = values[0];
            Debug.Assert(values.Length%2 == 1);
            for (var i = 1; i < values.Length; i += 2) {
                BoundRecorder.Observe(values[i], values[i + 1]);
                UnboundRecorder.Observe(values[i], values[i + 1]);
            }
            ApproxEquals(expected, BoundRecorder.TimeMean(values[values.Length - 1] + 1));
            ApproxEquals(expected, UnboundRecorder.TimeMean(values[values.Length - 1] + 1));
        }

        [Test]
        public void Reset_NoTime()
        {
            for (var i = 0; i < 10; ++i) {
                BoundRecorder.Observe(i);
                UnboundRecorder.Observe(i);
            }
            BoundRecorder.Reset();
            UnboundRecorder.Reset();
            Assert.AreSame(Env, BoundRecorder.Env);
            Assert.AreEqual(0, BoundRecorder.Count);
            Assert.AreEqual(Env.Now, BoundRecorder.StartTime);
            // Since there are no observations, following line would throw an exception:
            // Assert.AreEqual(0, BoundRecorder.Total());
            Assert.AreEqual(0, UnboundRecorder.Count);
            Assert.AreEqual(UnboundRecorder.Env.Now, UnboundRecorder.StartTime);
            // Since there are no observations, following line would throw an exception:
            // Assert.AreEqual(0, UnboundRecorder.Total());
        }

        [Test]
        public void Reset_WithDoubleTime()
        {
            const double x = 20;
            for (var i = 0; i < x; ++i) {
                BoundRecorder.Observe(i);
                UnboundRecorder.Observe(i);
            }
            BoundRecorder.Reset(x);
            UnboundRecorder.Reset(x);
            Assert.AreSame(Env, BoundRecorder.Env);
            Assert.AreEqual(0, BoundRecorder.Count);
            Assert.AreEqual(x, BoundRecorder.StartTime);
            // Since there are no observations, following line would throw an exception:
            // Assert.AreEqual(0, BoundRecorder.Total());
            Assert.AreEqual(0, UnboundRecorder.Count);
            Assert.AreEqual(x, UnboundRecorder.StartTime);
            // Since there are no observations, following line would throw an exception:
            // Assert.AreEqual(0, UnboundRecorder.Total());
        }

        [Test]
        public void Reset_WithIntTime()
        {
            const int x = 20;
            for (var i = 0; i < x; ++i) {
                BoundRecorder.Observe(i);
                UnboundRecorder.Observe(i);
            }
            BoundRecorder.Reset(x);
            UnboundRecorder.Reset(x);
            Assert.AreSame(Env, BoundRecorder.Env);
            Assert.AreEqual(0, BoundRecorder.Count);
            Assert.AreEqual(x, BoundRecorder.StartTime);
            // Since there are no observations, following line would throw an exception:
            // Assert.AreEqual(0, BoundRecorder.Total());
            Assert.AreEqual(0, UnboundRecorder.Count);
            Assert.AreEqual(x, UnboundRecorder.StartTime);
            // Since there are no observations, following line would throw an exception:
            // Assert.AreEqual(0, UnboundRecorder.Total());
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Total_NoObservations_Bound()
        {
            BoundRecorder.Total();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Total_NoObservations_Unbound()
        {
            UnboundRecorder.Total();
        }
    }
}