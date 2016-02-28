//
// Tally.cs
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

namespace DIBRIS.Dessert.Recording
{
    using System;
    using Core;

    /// <summary>
    ///   An instance of this interface records enough information (such as sums and sums of squares)
    ///   while the simulation runs to return simple data summaries. This has the advantage of speed and low memory use.
    ///   However, they do not preserve a time-series usable in more advanced statistical analysis. 
    /// </summary>
    /// <remarks>
    ///   Monitors and tallies may not be bound to a specific <see cref="SimEnvironment"/>, in order to ease
    ///   their usage in inter environment recordings; when they are unbounded their <see cref="SimEntity.Env"/>
    ///   property points to a dummy environment.<br/>
    ///   However, please pay attention to the fact that both monitors and tallies are not thread safe:
    ///   therefore, recall this fact when you use them in a multi threaded simulation scenario. 
    /// </remarks>
    public sealed class Tally : SimEntity, IRecorder
    {
        double _integral;
        double _integral2;
        double _lastSample;
        double _sum;
        double _sumOfSquares;

        internal Tally(SimEnvironment env) : base(env)
        {
            StartTime = LastTime = env.Now;
        }

        void DoObserve(double sample, double time)
        {
            _integral += (time - LastTime) * _lastSample;
            _integral2 += (time - LastTime) * _lastSample * _lastSample;
            _lastSample = sample;
            LastTime = time;
            Count++;
            _sum += sample;
            _sumOfSquares += sample*sample;
        }

        void DoReset(double time)
        {
            StartTime = LastTime = time;
            _integral = _integral2 = 0;
            _lastSample = 0;
            Count = 0;
            _sum = _sumOfSquares = 0;
        }

        #region ITally Members

        public int Count { get; private set; }

        public double LastTime { get; private set; }

        public double StartTime { get; private set; }

        public double Mean()
        {
            return _sum/Count;
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
            var integral = _integral + (time - LastTime) * _lastSample;
            return integral/(time - StartTime);
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
            var timeMean = TimeMean(time);
            var integral2 = _integral2 + (time - LastTime) * _lastSample * _lastSample;
            return integral2/(time - StartTime) - timeMean*timeMean;
        }

        public double Total()
        {
            return _sum;
        }

        public double Variance()
        {
            return (_sumOfSquares - ((_sum*_sum)/Count))/Count;
        }

        #endregion
    }
}