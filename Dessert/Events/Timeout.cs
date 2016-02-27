//
// Timeout.cs
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

namespace Dessert.Events
{
    using System;
    using System.Diagnostics.Contracts;
    using Core;

    /// <summary>
    ///   An event that is scheduled with a certain delay after its creation.<br/>
    ///   This event can be used by processes to wait (or hold their state) for delay time steps.
    ///   It is immediately scheduled at Env.Now + delay and has thus
    ///   (in contrast to <see cref="SimEvent{T}"/>) no Success() or Fail() methods.
    /// </summary>
    public class Timeout<T> : SimEvent<Timeout<T>, T>
    {
        /// <summary>
        ///   The delay at which this event was scheduled.
        /// </summary>
        readonly double _delay;

        /// <summary>
        ///   The value which was assigned to this event.
        /// </summary>
        readonly T _value;

        /// <summary>
        ///   Creates an event that is scheduled with a certain delay after its creation.
        /// </summary>
        /// <param name="env">The <see cref="SimEnvironment"/> this event will belong to.</param>
        /// <param name="delay">The delay at which the timeout event will be scheduled.</param>
        /// <param name="value">The value which will be returned on event success.</param>
        /// <param name="name">The name which can be optionally assigned to this event.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="delay"/> is less than zero, or <paramref name="delay"/> plus
        ///   current time clock is greater than <see cref="double.MaxValue"/>.
        /// </exception>
        internal Timeout(SimEnvironment env, double delay, T value) : base(env)
        {
            _delay = delay;
            _value = value;
            env.ScheduleTimeout(this, _delay);
        }

        #region ITimeout Members

        /// <summary>
        ///   The delay at which this event was scheduled.
        /// </summary>
        [Pure]
        public double Delay
        {
            get
            {
                Contract.Ensures(Contract.Result<double>() >= 0);
                Contract.Ensures(!double.IsPositiveInfinity(Contract.Result<double>()));
                return _delay;
            }
        }

        #endregion

        #region SimEvent Members

        public override T Value
        {
            get { return _value; }
        }

        protected override void OnEnd()
        {
            if (Env.RealTime && At != double.MaxValue)
            {
                var sleep = (int) ((At - Env.Now) * 1000.0);
                System.Threading.Tasks.Task.Delay(sleep).Wait();
            }
            Env.Now = At;
        }

        #endregion
    }

    public sealed class Timeout : Timeout<double>
    {
        internal Timeout(SimEnvironment env, double delay) : base(env, delay, delay)
        {
        }
    }
}