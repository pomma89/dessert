//
// SimEvent.cs
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

namespace Dessert.Events
{
    using System;
    using System.Diagnostics.Contracts;
    using Core;

    public sealed class SimEvent<T> : SimEvent<SimEvent<T>, T>
    {
        State _finalState = State.Created;

        /// <summary>
        /// 
        /// </summary>
        T _value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env">The <see cref="SimEnvironment"/> this token will belong to.</param>
        internal SimEvent(SimEnvironment env) : base(env)
        {
        }

        #region IEvent Members

        public bool Triggered
        {
            get { return _finalState != State.Created; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///   This event has already succeeded: therefore, it cannot fail anymore.
        /// </exception>
        public void Fail()
        {
            Contract.Requires<InvalidOperationException>(!Triggered);
            Trigger(State.Failed, default(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val">
        ///   An object that will be sent to processes waiting for this event to occur.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///   This event has already succeeded: therefore, it cannot fail anymore.
        /// </exception>
        public void Fail(T val)
        {
            Contract.Requires<InvalidOperationException>(!Triggered);
            Trigger(State.Failed, val);
        }

        public bool TryFail()
        {
            return TryTrigger(State.Failed, default(T));
        }

        public bool TryFail(T val)
        {
            return TryTrigger(State.Failed, val);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///   This event has already succeeded: therefore, it cannot fail anymore.
        /// </exception>
        public void Succeed()
        {
            Contract.Requires<InvalidOperationException>(!Triggered);
            Trigger(State.Succeeded, default(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val">
        ///   An object that will be sent to processes waiting for this event to occur.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///   This event has already succeeded: therefore, it cannot succeed anymore.
        /// </exception>
        public void Succeed(T val)
        {
            Contract.Requires<InvalidOperationException>(!Triggered);
            Trigger(State.Succeeded, val);
        }

        public bool TrySucceed()
        {
            return TryTrigger(State.Succeeded, default(T));
        }

        public bool TrySucceed(T val)
        {
            return TryTrigger(State.Succeeded, val);
        }

        void Trigger(State finalState, T val)
        {
            _finalState = finalState;
            _value = val;
            Env.ScheduleEvent(this);
        }

        bool TryTrigger(State finalState, T val)
        {
            if (_finalState != State.Created) {
                return false;
            }
            _finalState = finalState;
            _value = val;
            Env.ScheduleEvent(this);
            return true;
        }

        #endregion

        #region SimEvent Members

        protected override State FinalState
        {
            get { return _finalState; }
        }

        public override T Value
        {
            get { return _value; }
        }

        protected override State ValidStatesMask()
        {
            return base.ValidStatesMask() | State.Failed;
        }

        #endregion
    }
}