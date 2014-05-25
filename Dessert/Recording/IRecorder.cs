//
// IRecorder.cs
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

namespace Dessert.Recording
{
    using System;
    using System.Diagnostics.Contracts;
    using Core;
    using Resources;

    [ContractClass(typeof(RecorderContract))]
    public interface IRecorder
    {
        /// <summary>
        ///    The current number of observations.
        /// </summary>
        [Pure]
        int Count { get; }

        /// <summary>
        ///   Returns the environment in which this entity was created.
        /// </summary>
        [Pure]
        SimEnvironment Env { get; }
        
        /// <summary>
        ///   The time of last recording.
        /// </summary>
        [Pure]
        double LastTime { get; }

        /// <summary>
        ///   The time at which recording has started.
        /// </summary>
        [Pure]
        double StartTime { get; }

        /// <summary>
        ///   Returns the simple average of the observed values, ignoring the times at which they were made.
        ///   This is equal to <code>Total/Count</code>.
        /// </summary>
        /// <returns>The simple average of the observed values, ignoring the times at which they were made.</returns>
        /// <exception cref="InvalidOperationException">There are no observations.</exception>
        [Pure]
        double Mean();

        /// <summary>
        ///   Records the current value of the variable <paramref name="sample"/>.
        ///   Since time has not been specified, it is set to <see cref="SimEnvironment.Now"/>.     
        /// </summary>
        /// <param name="sample">The value that has to be recorded.</param>
        /// <remarks>
        ///   An <see cref="Monitor"/> retains the two values as a pair (time, sample), while
        ///   a <see cref="Tally"/> uses them to update the accumulated statistics.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   Implicitly assigned time is less than the last observation time.
        /// </exception>
        void Observe(double sample);

        /// <summary>
        ///   Records the current value of the variable <paramref name="sample"/> at given <paramref name="time"/>.     
        /// </summary>
        /// <param name="sample">The value that has to be recorded.</param>
        /// <param name="time">The time that will be associated with given value.</param>
        /// <remarks>
        ///   An <see cref="Monitor"/> retains the two values as a pair (time, sample), while
        ///   a <see cref="Tally"/> uses them to update the accumulated statistics.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="time"/> is less than the last observation time.
        /// </exception>
        void Observe(double sample, double time);

        /// <summary>
        ///   Resets the observations. The recorded data is re-initialized,
        ///   and the observation starting time is set to <see cref="SimEnvironment.Now"/>.
        /// </summary>
        void Reset();

        /// <summary>
        ///   Resets the observations. The recorded data is re-initialized,
        ///   and the observation starting time is set to <paramref name="time"/>.
        /// </summary>
        void Reset(double time);

        /// <summary>
        ///   Returns the standard deviation of the observations, computed as the square root of <see cref="Variance"/>.
        /// </summary>
        /// <returns>The standard deviation of the observations, computed as the square root of <see cref="Variance"/>.</returns>
        /// <exception cref="InvalidOperationException">There are no observations.</exception>
        [Pure]
        double StdDev();

        /// <summary>
        ///   Returns the time-weighted mean, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to current time.
        /// </summary>
        /// <returns>
        ///   The time-weighted average, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to current time.
        /// </returns>
        [Pure]
        double TimeMean();

        /// <summary>
        ///   Returns the time-weighted mean, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to <paramref name="time"/>.
        /// </summary>
        /// <returns>
        ///   The time-weighted average, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to <paramref name="time"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="time"/> is less than <see cref="StartTime"/>.
        /// </exception>
        [Pure]
        double TimeMean(double time);

        /// <summary>
        ///   Returns the time-weighted variance, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to current time.
        /// </summary>
        /// <returns>
        ///   The time-weighted average, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to current time.
        /// </returns>
        [Pure]
        double TimeStdDev();

        /// <summary>
        ///   Returns the time-weighted variance, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to <paramref name="time"/>.
        /// </summary>
        /// <returns>
        ///   The time-weighted average, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to <paramref name="time"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="time"/> is less than <see cref="StartTime"/>.
        /// </exception>
        [Pure]
        double TimeStdDev(double time);

        /// <summary>
        ///   Returns the time-weighted variance, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to current time.
        /// </summary>
        /// <returns>
        ///   The time-weighted average, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to current time.
        /// </returns>
        [Pure]
        double TimeVariance();

        /// <summary>
        ///   Returns the time-weighted variance, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to <paramref name="time"/>.
        /// </summary>
        /// <returns>
        ///   The time-weighted average, calculated from time 0
        ///   (or the last time <see cref="Reset()"/> was called) to <paramref name="time"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="time"/> is less than <see cref="StartTime"/>.
        /// </exception>
        [Pure]
        double TimeVariance(double time);

        /// <summary>
        ///   Returns the sum of the observed values.
        /// </summary>
        /// <returns>The sum of the observed values.</returns>
        [Pure]
        double Total();

        /// <summary>
        ///   Returns the sample variance of the observations, ignoring the times at which they were made. 
        ///   If an unbiased estimate of the population variance is desired, the sample variance
        ///   should be multiplied by <code>Count/(Count - 1)</code>.
        /// </summary>
        /// <returns>The sample variance of the observations, ignoring the times at which they were made.</returns>
        /// <exception cref="InvalidOperationException">There are no observations.</exception>
        [Pure]
        double Variance();
    }

    [ContractClassFor(typeof(IRecorder))]
    public abstract class RecorderContract : IRecorder
    {
        private RecorderContract()
        {
            throw new InvalidOperationException(ErrorMessages.ContractClass);
        }

        public abstract int Count { get; }

        public abstract SimEnvironment Env { get; }

        public double LastTime
        {
            get
            {
                Contract.Ensures(LastTime >= StartTime);
                return default(double);
            }
        }

        public abstract double StartTime { get; }

        public abstract double Mean();

        public abstract void Observe(double sample);

        public void Observe(double sample, double time)
        {
            Contract.Requires<ArgumentOutOfRangeException>(time >= LastTime, ErrorMessages.InvalidRecordingTime);
        }

        public abstract void Reset();

        public abstract void Reset(double time);

        public double StdDev()
        {
            Contract.Requires<InvalidOperationException>(Count != 0, ErrorMessages.NoObservations);
            return default(double);
        }

        public double TimeMean()
        {
            Contract.Requires<InvalidOperationException>(Count != 0, ErrorMessages.NoObservations);
            return default(double);
        }

        public double TimeMean(double time)
        {
            Contract.Requires<InvalidOperationException>(Count != 0, ErrorMessages.NoObservations);
            Contract.Requires<InvalidOperationException>(time >= StartTime, ErrorMessages.InvalidRecordingTime);
            return default(double);
        }

        public double TimeStdDev()
        {
            Contract.Requires<InvalidOperationException>(Count != 0, ErrorMessages.NoObservations);
            return default(double);
        }

        public double TimeStdDev(double time)
        {
            Contract.Requires<InvalidOperationException>(Count != 0, ErrorMessages.NoObservations);
            Contract.Requires<InvalidOperationException>(time >= StartTime, ErrorMessages.InvalidRecordingTime);
            return default(double);
        }

        public double TimeVariance()
        {
            Contract.Requires<InvalidOperationException>(Count != 0, ErrorMessages.NoObservations);
            return default(double);
        }

        public double TimeVariance(double time)
        {
            Contract.Requires<InvalidOperationException>(Count != 0, ErrorMessages.NoObservations);
            Contract.Requires<InvalidOperationException>(time >= StartTime, ErrorMessages.InvalidRecordingTime);
            return default(double);
        }

        public double Total()
        {
            Contract.Requires<InvalidOperationException>(Count != 0, ErrorMessages.NoObservations);
            return default(double);
        }

        public double Variance()
        {
            Contract.Requires<InvalidOperationException>(Count != 0, ErrorMessages.NoObservations);
            return default(double);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IRecordedResource
    {
        /// <summary>
        ///   An instance of <see cref="Tally"/> that periodically records the number of requests
        ///   fulfilled by this resource. The frequency of recordings is given by <see cref="RecordingFrequency"/>.
        /// </summary>
        Tally FulfilledRequestsTally { get; }

        /// <summary>
        ///   The frequency at which some tallies of this interface update themselves.
        /// </summary>
        Double RecordingFrequency { get; }

        /// <summary>
        ///   An instance of <see cref="Tally"/> that periodically records the number of requests undone.
        ///   The frequency of recordings is given by <see cref="RecordingFrequency"/>.
        /// </summary>
        Tally UndoneRequestsTally { get; }

        /// <summary>
        ///   An instance of <see cref="Tally"/> that periodically records the number
        ///   of the users of this resource (given by <see cref="Resource.Count"/>).
        ///   The frequency of recordings is given by <see cref="RecordingFrequency"/>.
        /// </summary>
        Tally UsageTally { get; }

        /// <summary>
        ///   An instance of <see cref="Tally"/> that records the time waited by each process.
        /// </summary>
        Tally WaitingTimeTally { get; }
    }
}