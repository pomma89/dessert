//
// TemplateEvents.cs
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
    using Core;
    using Resources;

    /// <summary>
    ///   Models aspects shared by all resource events.
    /// </summary>
    /// <typeparam name="TEv"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    public abstract class ResourceEvent<TEv, TVal> : SimEvent<TEv, TVal>, IDisposable
        where TEv : ResourceEvent<TEv, TVal>
    {
        readonly double _priority;

        internal ResourceEvent(SimEnvironment env, double priority) : base(env)
        {
            _priority = priority;
        }

        #region Public Members

        /// <summary>
        ///   Returns true if and only if event has been disposed; otherwise, it returns false.
        /// </summary>
        [System.Diagnostics.Contracts.Pure]
        public bool Disposed { get; protected set; }

        /// <summary>
        ///   The priority assigned to this resource event.
        ///   Usually, the priority is only considered when
        ///   the policy is set to <see cref="WaitPolicy.Priority"/>.
        /// </summary>
        public double Priority
        {
            get { return _priority; }
        }

        public abstract void Dispose();

        #endregion

        #region SimEvent Members

        protected override sealed State ValidStatesMask()
        {
            return base.ValidStatesMask();
        }

        #endregion
    }

    public abstract class StandaloneEvent<TEv, TVal> : SimEvent<TEv, TVal> where TEv : SimEvent<TEv, TVal>
    {
        internal StandaloneEvent(SimEnvironment env) : base(env)
        {
        }

        #region SimEvent Members

        protected override sealed bool CanHaveParents
        {
            get { return false; }
        }

        protected override sealed bool CanHaveSubscribers
        {
            get { return true; }
        }

        protected override State ValidStatesMask()
        {
            return State.Created;
        }

        #endregion
    }

    /// <summary>
    ///   Represents an event which cannot be "yielded" by any user process.
    ///   It is used internally to represent special events, like interrupts.
    /// </summary>
    abstract class InnerEvent : SimEvent<InnerEvent, object>
    {
        internal InnerEvent(SimEnvironment env) : base(env)
        {
        }

        #region SimEvent Members

        protected override sealed bool CanHaveParents
        {
            get { return false; }
        }

        protected override sealed bool CanHaveSubscribers
        {
            get { return false; }
        }

        protected override State ValidStatesMask()
        {
            return State.Created;
        }

        #endregion
    }
}