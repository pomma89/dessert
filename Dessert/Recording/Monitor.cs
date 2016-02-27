//
// Monitor.cs
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

namespace Dessert.Recording
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core;

    /// <summary>
    ///   An instance of this interface preserves a complete time-series of the observed data values, sample,
    ///   and their associated times, time. It calculates the data summaries using these series only when they are needed.
    ///   It is slower and uses more memory than <see cref="Tally"/>. In long simulations its memory demands may be a disadvantage.
    /// </summary>
    /// <remarks>
    ///   Monitors and tallies may not be bound to a specific <see cref="SimEnvironment"/>, in order to ease
    ///   their usage in inter environment recordings; when they are unbounded their <see cref="SimEntity.Env"/>
    ///   property points to a dummy environment.<br/>
    ///   However, please pay attention to the fact that both monitors and tallies are not thread safe:
    ///   therefore, recall this fact when you use them in a multi threaded simulation scenario. 
    /// </remarks>
    public sealed class Monitor : SimEntity, IRecorder
    {
        readonly IList<MonitorSample> _samples = new List<MonitorSample>();

        internal Monitor(SimEnvironment env) : base(env)
        {
            StartTime = env.Now;
        }

        void DoObserve(double sample, double time)
        {
            _samples.Add(new MonitorSample(sample, time));
        }

        void DoReset(double time)
        {
            _samples.Clear();
            StartTime = time;
        }

        #region IMonitor Members

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<MonitorSample> Samples
        {
            get { return _samples; }
        }

        /// <summary>
        ///   Returns the i-th sample recorded inside the monitor. 
        /// </summary>
        /// <param name="i">The index of the sample that has to be retrieved.</param>
        /// <returns>The i-th sample recorded inside the monitor.</returns>
        public MonitorSample this[int i]
        {
            get { return _samples[i]; }
        }

        public int Count
        {
            get { return _samples.Count; }
        }

        public double LastTime
        {
            get { return (_samples.Count > 0) ? _samples[_samples.Count - 1].Time : StartTime; }
        }

        public double StartTime { get; private set; }

        public double Mean()
        {
            return Total()/Count;
        }

        public void Observe(double sample)
        {
            DoObserve(sample, Env.Now);
        }

        public void Observe(double sample, double time)
        {
            DoObserve(sample, time);
        }

        public void Reset()
        {
            DoReset(Env.Now);
        }

        public void Reset(double time)
        {
            DoReset(time);
        }

        public double StdDev()
        {
            return Math.Sqrt(Variance());
        }

        public double TimeMean()
        {
            return TimeMean(Env.Now);
        }

        public double TimeMean(double time)
        {
            var sum = 0.0;
            var last = _samples[0];
            foreach (var s in _samples) {
                sum += last.Sample*(s.Time - last.Time);
                last = s;
            }
            sum += last.Sample*(time - last.Time);
            return sum/(time - _samples[0].Time);
        }

        public double TimeStdDev()
        {
            return TimeStdDev(Env.Now);
        }

        public double TimeStdDev(double time)
        {
            return Math.Sqrt(TimeVariance(time));
        }

        public double TimeVariance()
        {
            return TimeVariance(Env.Now);
        }

        public double TimeVariance(double time)
        {
            var sum = 0.0;
            var sumOfSquares = 0.0;
            var last = _samples[0];
            foreach (var s in _samples) {
                sum += last.Sample*(s.Time - last.Time);
                sumOfSquares += last.Sample*last.Sample*(s.Time - last.Time);
                last = s;
            }
            sum += last.Sample*(time - last.Time);
            sumOfSquares += last.Sample*last.Sample*(time - last.Time);
            var dt = time - _samples[0].Time;
            var mean = sum/dt;
            return sumOfSquares/dt - mean*mean;
        }

        public double Total()
        {
            return _samples.Sum(s => s.Sample);
        }

        public double Variance()
        {
            var sum = 0.0;
            var sumOfSquares = 0.0;
            foreach (var s in _samples) {
                sum += s.Sample;
                sumOfSquares += s.Sample*s.Sample;
            }
            return (sumOfSquares - (sum*sum)/Count)/Count;
        }

        #endregion
    }

    /// <summary>
    ///   Represents a sample recorded inside a <see cref="Monitor"/>.
    /// </summary>
    public struct MonitorSample
    {
        readonly double _sample;
        readonly double _time;

        internal MonitorSample(double sample, double time)
        {
            _sample = sample;
            _time = time;
        }

        /// <summary>
        ///   The sample recorded in the monitor.
        /// </summary>
        public double Sample
        {
            get { return _sample; }
        }

        /// <summary>
        ///   The time at which <see cref="Sample"/> was recorded.
        /// </summary>
        public double Time
        {
            get { return _time; }
        }
    }
}