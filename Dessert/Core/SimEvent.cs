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

namespace Dessert.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using Events;
    using PommaLabs.Collections;

    /// <summary>
    ///   A stronger typed event, which adds type notation to many properties which are untyped in SimPy.
    /// </summary>
    /// <typeparam name="TEv">The type of the event which implements this interface.</typeparam>
    /// <typeparam name="TVal">The type of the value returned by this interface.</typeparam>
    /// <remarks>
    ///   This class could not be named "Event" in order to maintain compatibility
    ///   with Visual Basic code, where "Event" is a language keyword.
    /// </remarks>
    public abstract class SimEvent<TEv, TVal> : SimEvent where TEv : SimEvent<TEv, TVal>
    {
        /// <summary>
        /// 
        /// </summary>
        SinglyLinkedList<Action<TEv>> _callbacks;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        internal SimEvent(SimEnvironment env) : base(env)
        {
        }

        #region Abstract Members

        /// <summary>
        ///   The strongly typed value returned by the event. This property contains the value that in SimPy
        ///   is "sent" to the process through the generator itself; since we cannot do anything
        ///   similar in .NET, we have to use this property to store that kind of values.<br/>
        /// </summary>
        /// <remarks>
        ///   As a rule of thumb, the value on this property will be ready only after
        ///   <see cref="SimEvent.Succeeded"/> or <see cref="SimEvent.Failed"/> will be true. However, 
        ///   this property can always be accessed: therefore, please pay attention to the fact that
        ///   this property will return a null value when a value is not ready or 
        ///   when an event does not have a proper value.
        /// </remarks>
        [Pure]
        public new abstract TVal Value { get; }

        #endregion

        #region IEvent Members

        /// <summary>
        ///   Collection of functions that are called when the event is processed.
        /// </summary>
        [Pure]
        public ICollection<Action<TEv>> Callbacks
        {
            get { return _callbacks ?? (_callbacks = new SinglyLinkedList<Action<TEv>>()); }
        }

        protected override sealed object GetValue()
        {
            return Value;
        }

        #endregion

        #region SimEvent Members

        internal override void End()
        {
            OnEnd();
            var finalState = FinalState;
            Debug.Assert(!InFinalState && (FinalStatesMask & finalState) > 0);
            // Final state must be assigned before triggering conditions, because they will use it
            // to determine whether or not undo the events they control.
            SetFinalState(finalState);
            //
            if (_callbacks != null) {
                // Debug.Assert(_callbacks.Count > 0);
                // Assert above is potentially false, since the user may call
                // Clear on _callbacks, as it is an ICollection.
                foreach (var callback in _callbacks) {
                    callback(this as TEv);
                }
                _callbacks = null;
            }
            // Following piece of code is optimezed for cases in which an event
            // belongs to a single event, which represents the great majority of situations.
            if (Conditions != null) {
                Debug.Assert(Conditions.Count > 0);
                // Triggers all conditions to which this event belongs.
                IParentCondition condition;
                if (Conditions.Count == 1 && !(condition = Conditions.First).Succeeded) {
                    condition.Trigger(this);
                } else {
                    var en = Conditions.GetEnumerator();
                    while (en.MoveNext()) {
                        if (!(condition = en.Current).Succeeded) {
                            condition.Trigger(this);
                        }
                    }
                }
                Conditions = null;
            }
            // Following piece of code is optimezed for cases in which an event
            // is "yielded" by a single process, which represents the great majority of situations.
            if (Subscribers != null) {
                Debug.Assert(Subscribers.Count > 0);
                // Reschedules all processes which are not completed yet.
                // The "where" clause filters yielders, so that only those processes
                // which are still running are picked up for scheduling.
                SimProcess subscriber;
                if (Subscribers.Count == 1 && (subscriber = Subscribers.First).IsAlive) {
                    Env.ScheduleProcess(subscriber);
                } else {
                    var en = Subscribers.GetEnumerator();
                    while (en.MoveNext()) {
                        if ((subscriber = en.Current).IsAlive) {
                            Env.ScheduleProcess(subscriber);
                        }
                    }
                } 
                Subscribers = null;
            }
        }

        #endregion
    }
}