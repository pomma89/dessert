//
// SimProcess.cs
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

namespace Dessert
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using Core;
    using Events;
    using PommaLabs.Collections;

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///   This class could not be named "Process" in order to make its usage easier.
    ///   In fact, commonly used "System" namespace already contains a "Process" class.
    /// 
    ///   This class implements the <see cref="ILinkedList{T}"/> interface to perform an effective
    ///   optimization when one event is yielded by a process only, which is a common situation.
    /// </remarks>
    public sealed class SimProcess : SimEvent<SimProcess, object>, ILinkedList<SimProcess>
    {
        /// <summary>
        /// 
        /// </summary>
        IInternalCall _currentCall;

        uint _interruptCount;

        object _interruptValue;

        /// <summary>
        ///   Stores a reference to the <see cref="IEnumerator{T}"/> returned by the generator method.
        ///   It is used to activate and stop generator execution flow.
        /// </summary>
        IEnumerator<SimEvent> _steps;

        /// <summary>
        /// 
        /// </summary>
        object _value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="steps"></param>
        internal SimProcess(SimEnvironment env, IEnumerator<SimEvent> steps) : base(env)
        {
            _steps = steps;
        }

        /// <summary>
        ///   Called when this event is the first in the agenda.
        /// </summary>
        internal override void Step()
        {
            // Target has to be cleaned before process is started up again.
            Target = null;

            // In SimPy 3, interrupts are sent to processes as exceptions.
            // However, this cannot be done in .NET; therefore, we dediced to throw
            // an exception if a process was interrupted, but it did not check for that.
            // To achieve this, we need to record the number of interrupts
            // this process has and we will see if it has properly decreased.
            var oldInterruptCount = _interruptCount;

            // We start the process again and see if it produces more events.
            if (_steps.MoveNext() && !ReferenceEquals(Target = _steps.Current, Env.EndEvent)) {
                // User has yielded an event. Since users may yield null events,
                // we need to check for them, as they are not allowed.
                if (Target == null) {
                    throw new ArgumentNullException(ErrorMessages.NullEvent);
                }
                // If the interrupt count was zero, then we have nothing more to check.
                // Otherwise, we will need to check that it was decreased at least by one.
                if (oldInterruptCount != 0 && _interruptCount == oldInterruptCount) {
                    throw new InterruptUncaughtException();
                }
                Target.Subscribe(this);
                return;
            }

            // Body of generator has finished, therefore process or call has finished.
            // If the interrupt count was zero, then we have nothing more to check.
            // Otherwise, we will need to check that it was decreased at least by one.
            if (oldInterruptCount != 0 && _interruptCount == oldInterruptCount) {
                throw new InterruptUncaughtException();
            }

            // If target event is the end event, then the process should exit,
            // no matter how many interrupts have been received.
            if (_currentCall != null) {
                _steps = _currentCall.Steps;
                _currentCall = _currentCall.PreviousCall;
            } else {
                // We have to remove current process from the agenda.
                Env.UnscheduleActiveProcess();
                // Marks this event as succeeded, as processes cannot fail.
                End();
            }
        }

        internal void SetExitValue(object value)
        {
            if (_currentCall != null) {
                _currentCall.SetValue(value);
            } else {
                _value = value;
            }
        }

        /// <summary>
        ///   Updates current steps, so that <see cref="IInternalCall.Steps"/> are used instead.
        ///   Moreover, it adjusts the stack of calls, since that is necessary to maintain integrity.
        /// </summary>
        /// <param name="call">The call from which new steps are taken.</param>
        internal void PushCall(IInternalCall call)
        {
            var tmp = call.Steps;
            call.Steps = _steps;
            _steps = tmp;
            call.PreviousCall = _currentCall;
            _currentCall = call;
        }

        internal void ReceiveInterrupt(object value)
        {
            Debug.Assert(Target != null);
            Target.RemoveSubscriber(this);
            Env.ScheduleProcess(this);
            _interruptValue = value;
        }

        #region Public Members

        /// <summary>
        ///   Returns whether the process has been processed or not.
        /// </summary>
        [System.Diagnostics.Contracts.Pure]
        public bool IsAlive
        {
            get { return !Succeeded; }
        }

        /// <summary>
        ///   The event that the process is currently waiting for.
        ///   May be a null event if the process was just started 
        ///   or interrupted and it did not yet yield a new event.
        /// </summary>
        [Pure]
        public SimEvent Target { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///   Process it not alive or a process is trying to interrupt itself.
        /// </exception>
        public void Interrupt()
        {
            Contract.Requires<InvalidOperationException>(IsAlive, ErrorMessages.EndedProcess);
            Contract.Requires<InvalidOperationException>(!ReferenceEquals(this, Env.ActiveProcess), ErrorMessages.InterruptSameProcess);
            // Code below is the same in other Interrupt overloads, remember to update them.
            var interrupt = new Interrupt(Env, this, Default.Value);
            Env.ScheduleInterrupt(interrupt);
            _interruptCount++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException">
        ///   Process it not alive or a process is trying to interrupt itself.
        /// </exception>
        public void Interrupt(object value)
        {
            Contract.Requires<InvalidOperationException>(IsAlive, ErrorMessages.EndedProcess);
            Contract.Requires<InvalidOperationException>(!ReferenceEquals(this, Env.ActiveProcess), ErrorMessages.InterruptSameProcess);
            // Code below is the same in other Interrupt overloads, remember to update them.
            var interrupt = new Interrupt(Env, this, value);
            Env.ScheduleInterrupt(interrupt);
            _interruptCount++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        ///   Process it not alive or a process is trying to query another process for interrupts.
        /// </exception>
        public bool Interrupted()
        {
            Contract.Requires<InvalidOperationException>(IsAlive, ErrorMessages.EndedProcess);
            Contract.Requires<InvalidOperationException>(ReferenceEquals(this, Env.ActiveProcess), ErrorMessages.InterruptedDifferentProcess);
            // Code below is the same in other Interrupted overloads, remember to update them.
            if (_interruptCount > 0) {
                _interruptCount--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        ///   Process it not alive or a process is trying to query another process for interrupts.
        /// </exception>
        public bool Interrupted(out object value)
        {
            Contract.Requires<InvalidOperationException>(IsAlive, ErrorMessages.EndedProcess);
            Contract.Requires<InvalidOperationException>(ReferenceEquals(this, Env.ActiveProcess), ErrorMessages.InterruptedDifferentProcess);
            // Code below is the same in other Interrupted overloads, remember to update them.
            if (_interruptCount > 0) {
                _interruptCount--;
                value = _interruptValue;
                return true;
            }
            value = null;
            return false;
        }

        public bool Preempted()
        {
            object obj;
            if (Interrupted(out obj)) {
                if (obj is PreemptionInfo) {
                    return true;
                }
                _interruptCount++;
                return false;
            }
            return false;
        }

        public bool Preempted(out PreemptionInfo info)
        {
            object obj;
            if (Interrupted(out obj)) {
                info = obj as PreemptionInfo;
                if (info != null) {
                    return true;
                }
                _interruptCount++;
                return false;
            }
            info = null;
            return false;
        }

        #endregion

        #region SimEvent Members

        public override object Value
        {
            get { return _value; }
        }

        #endregion

        #region ILinkedList Members

        int ICollection<SimProcess>.Count
        {
            get { return 1; }
        }

        SimProcess IThinLinkedList<SimProcess>.First
        {
            get { return this; }
        }

        bool ICollection<SimProcess>.Contains(SimProcess item)
        {
            return Equals(item);
        }

        IEnumerator<SimProcess> IEnumerable<SimProcess>.GetEnumerator()
        {
            yield return this;
        }

        IEqualityComparer<SimProcess> IThinLinkedList<SimProcess>.EqualityComparer
        {
            get { throw new DessertException(ErrorMessages.InvalidMethod); }
        }

        bool ICollection<SimProcess>.IsReadOnly
        {
            get { throw new DessertException(ErrorMessages.InvalidMethod); }
        }

        SimProcess ILinkedList<SimProcess>.Last
        {
            get { throw new DessertException(ErrorMessages.InvalidMethod); }
        }

        void ICollection<SimProcess>.Add(SimProcess item)
        {
            throw new DessertException(ErrorMessages.InvalidMethod);
        }

        void IThinLinkedList<SimProcess>.AddFirst(SimProcess item)
        {
            throw new DessertException(ErrorMessages.InvalidMethod);
        }

        void ILinkedList<SimProcess>.AddLast(SimProcess item)
        {
            throw new DessertException(ErrorMessages.InvalidMethod);
        }

        void ILinkedList<SimProcess>.Append(ILinkedList<SimProcess> list)
        {
            throw new DessertException(ErrorMessages.InvalidMethod);
        }

        void ICollection<SimProcess>.Clear()
        {
            throw new DessertException(ErrorMessages.InvalidMethod);
        }

        void ICollection<SimProcess>.CopyTo(SimProcess[] array, int arrayIndex)
        {
            throw new DessertException(ErrorMessages.InvalidMethod);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new DessertException(ErrorMessages.InvalidMethod);
        }

        bool ICollection<SimProcess>.Remove(SimProcess item)
        {
            throw new DessertException(ErrorMessages.InvalidMethod);
        }

        SimProcess IThinLinkedList<SimProcess>.RemoveFirst()
        {
            throw new DessertException(ErrorMessages.InvalidMethod);
        }

        #endregion
    }

    public sealed class PreemptionInfo
    {
        readonly SimProcess _by;
        readonly Double _usageSince;

        internal PreemptionInfo(SimProcess by, Double usageSince)
        {
            _by = by;
            _usageSince = usageSince;
        }

        /// <summary>
        /// 
        /// </summary>
        [Pure]
        public SimProcess By
        {
            get { return _by; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.Contracts.Pure]
        public double UsageSince
        {
            get { return _usageSince; }
        }
    }
}