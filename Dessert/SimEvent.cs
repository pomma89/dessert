//
// SimEvent.cs
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

namespace DIBRIS.Dessert
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using Core;
    using Events;
    using Finsa.CodeServices.Common.Collections;

    /// <summary>
    ///   The interface common to each event; it should be used to declare generator methods.
    /// </summary>
    public abstract partial class SimEvent
    {
        /// <summary>
        ///   Represents the state of an event.
        /// </summary>
        [Flags]
        protected enum State : byte
        {
            /// <summary>
            ///   Event has been created and it is waiting something to happen.
            /// </summary>
            /// <remarks>
            ///   Its value should be zero, since it is not used in checks.
            /// </remarks>
            Created = 0,

            Succeeded = 1,

            Failed = 2
        }

        /// <summary>
        ///   Returns a mask with the final values that the state attribute can have.
        /// </summary>
        protected const State FinalStatesMask = State.Failed | State.Succeeded;

        /// <summary>
        ///   Stores the environment in which this event was created.
        /// </summary>
        readonly SimEnvironment _env;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///   It is important to use a LinkedList, because it behaves like a queue.
        /// </remarks>
        internal ILinkedList<IParentCondition> Conditions;

        /// <summary>
        ///   Stores the processes which yielded this event. List can be empty,
        ///   since not all events are yielded by some process (for example,
        ///   processes activated before simulation are like that).
        /// </summary>
        /// <remarks>
        ///   It is important to use a LinkedList, because it behaves like a queue.
        /// </remarks>
        internal ILinkedList<SimProcess> Subscribers;

        /// <summary>
        /// 
        /// </summary>
        State _state;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        internal SimEvent(SimEnvironment env)
        {
            _env = env;
        }

        protected bool InFinalState
        {
            get { return (_state & FinalStatesMask) > 0; }
        }

        protected void SetFinalState(State state)
        {
            Debug.Assert(!InFinalState);
            Debug.Assert((state & FinalStatesMask) > 0);
            Debug.Assert((state & ValidStatesMask()) > 0);
            _state = state;
        }

        /// <summary>
        ///   Called when this event is "yielded" by some process.
        /// </summary>
        internal void Subscribe(SimProcess subscriber)
        {
            if (InFinalState || !CanHaveSubscribers) {
                return;
            }
            if (Subscribers == null) {
                Subscribers = subscriber;
            } else if (Subscribers.Count == 1) {
                if (!Subscribers.First.Equals(subscriber)) {
                    var sub = new SinglyLinkedList<SimProcess>();
                    sub.AddLast(Subscribers as SimProcess);
                    sub.AddLast(subscriber);
                    Subscribers = sub;
                }
            } else if (!Subscribers.Contains(subscriber)) {
                Subscribers.AddLast(subscriber);
            }
            Debug.Assert(ReferenceEquals(Env.ActiveProcess, subscriber));
            Env.UnscheduleActiveProcess();
        }

        internal void RemoveSubscriber(SimProcess subscriber)
        {
            if (InFinalState || !CanHaveSubscribers) {
                return;
            }
            Debug.Assert(!ReferenceEquals(Subscribers, null));
            Debug.Assert(Subscribers.Contains(subscriber));
            Debug.Assert(!ReferenceEquals(Env.ActiveProcess, subscriber));
            if (Subscribers.Count == 1) {
                Subscribers = null;
            } else {
                Subscribers.Remove(subscriber);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        internal void AddParent(IParentCondition condition)
        {
            Debug.Assert(!Succeeded, "This method should not be called if event has succeeded");
            if (!CanHaveParents) {
                return;
            }
            if (Conditions == null) {
                Conditions = condition;
            } else if (Conditions.Count == 1) {
                if (!Conditions.First.Equals(condition)) {
                    var conds = new SinglyLinkedList<IParentCondition>();
                    conds.AddLast(Conditions as IParentCondition);
                    conds.AddLast(condition);
                    Conditions = conds;
                }
            } else if (!Conditions.Contains(condition)) {
                Conditions.AddLast(condition);
            }
        }

        #region Public Members

        /// <summary>
        ///   Returns the environment in which this entity was created.
        /// </summary>
        [Pure]
        public SimEnvironment Env
        {
            get { return _env; }
        }

        /// <summary>
        ///   Returns true if and only if event has failed; otherwise, it returns false.
        /// </summary>
        [System.Diagnostics.Contracts.Pure]
        public bool Failed
        {
            get { return _state == State.Failed; }
        }

        /// <summary>
        ///   Returns true if and only if event has been scheduled; otherwise, it returns false.
        /// 
        ///   Event has been scheduled in the agenda, it will call its callbacks (if any)
        ///   and activate waiting processes as soon as it will be its turn.
        /// </summary>
        [System.Diagnostics.Contracts.Pure]
        public bool Scheduled { get; internal set; }

        /// <summary>
        ///   Returns true if and only if event has succeeded; otherwise, it returns false.
        /// </summary>
        [System.Diagnostics.Contracts.Pure]
        public bool Succeeded
        {
            get { return _state == State.Succeeded; }
        }

        /// <summary>
        ///   The value returned by the event. This property contains the value that in SimPy
        ///   is "sent" to the process through the generator itself; since we cannot do anything
        ///   similar in .NET, we have to use this property to store that kind of values.<br/>
        /// </summary>
        /// <remarks>
        ///   As a rule of thumb, the value on this property will be ready only after
        ///   <see cref="Succeeded"/> or <see cref="Failed"/> will be true. However, 
        ///   this property can always be accessed: therefore, please pay attention to the fact that
        ///   this property will return a null value when a value is not ready or 
        ///   when an event does not have a proper value.
        /// </remarks>
        [Pure]
        public object Value
        {
            get { return GetValue(); }
        }

        public static implicit operator bool(SimEvent ev)
        {
            return ev._state == State.Succeeded;
        }

        #endregion

        #region Abstract Members

        protected virtual bool CanHaveParents
        {
            get { return true; }
        }

        protected virtual bool CanHaveSubscribers
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///   This method is not used in any execution path of Release builds.
        ///   In fact, this method is just used in Debug.Assert to enforce better integrity.
        /// </remarks>
        protected virtual State ValidStatesMask()
        {
            return State.Created | State.Succeeded;
        }

        protected virtual State FinalState
        {
            get { return State.Succeeded; }
        }

        internal abstract void End();

        internal virtual void Step()
        {
            throw new DessertException("Events should not be stepped!");
        }

        protected virtual void OnEnd()
        {
        }

        protected abstract object GetValue();

        #endregion

        #region OptimizedSkewHeap Helpers

        internal double At;

        internal SimEvent Left, Right;

        internal ulong Version;

        internal static bool IsSmaller(SimEvent h1, SimEvent h2)
        {
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            return h1.At < h2.At || (h1.At == h2.At && h1.Version < h2.Version);
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
        }

        #endregion
    }
}