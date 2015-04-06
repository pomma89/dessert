//
// Container.cs
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

namespace Dessert.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Core;
    using Events;

    public sealed class Container : SimEntity
    {
        readonly double _capacity;
        readonly IWaitQueue<GetEvent> _getQueue;
        readonly IWaitQueue<PutEvent> _putQueue;

        internal Container(SimEnvironment env, double capacity, double level, WaitPolicy getPolicy, WaitPolicy putPolicy)
            : base(env)
        {
            _capacity = capacity;
            Level = level;
            _getQueue = WaitQueue.New<GetEvent>(getPolicy, Env);
            _putQueue = WaitQueue.New<PutEvent>(putPolicy, Env);
        }

        void BalanceQueues()
        {
            if (_getQueue.Count > 0 && _getQueue.First.TrySchedule()) {
                _getQueue.RemoveFirst();
            }
            if (_putQueue.Count > 0 && _putQueue.First.TrySchedule()) {
                _putQueue.RemoveFirst();
            }
        }

        #region Public Members

        public double Capacity
        {
            get { return _capacity; }
        }

        public WaitPolicy GetPolicy
        {
            get { return _getQueue.Policy; }
        }

        [Pure]
        public IEnumerable<GetEvent> GetQueue
        {
            get { return _getQueue; }
        }

        public double Level { get; private set; }

        public WaitPolicy PutPolicy
        {
            get { return _putQueue.Policy; }
        }

        [Pure]
        public IEnumerable<PutEvent> PutQueue
        {
            get { return _putQueue; }
        }

        public GetEvent Get(double quantity)
        {
            Contract.Requires<ArgumentOutOfRangeException>(quantity >= 0, ErrorMessages.NegativeQuantity);
            Contract.Requires<ArgumentOutOfRangeException>(quantity <= Capacity, ErrorMessages.ExcessiveQuantity);
            return new GetEvent(this, quantity, Default.Priority);
        }

        public GetEvent Get(double quantity, double priority)
        {
            Contract.Requires<ArgumentOutOfRangeException>(quantity >= 0, ErrorMessages.NegativeQuantity);
            Contract.Requires<ArgumentOutOfRangeException>(quantity <= Capacity, ErrorMessages.ExcessiveQuantity);
            return new GetEvent(this, quantity, priority);
        }

        public PutEvent Put(double quantity)
        {
            Contract.Requires<ArgumentOutOfRangeException>(quantity >= 0, ErrorMessages.NegativeQuantity);
            Contract.Requires<ArgumentOutOfRangeException>(quantity <= Capacity, ErrorMessages.ExcessiveQuantity);
            return new PutEvent(this, quantity, Default.Priority);
        }

        public PutEvent Put(double quantity, double priority)
        {
            Contract.Requires<ArgumentOutOfRangeException>(quantity >= 0, ErrorMessages.NegativeQuantity);
            Contract.Requires<ArgumentOutOfRangeException>(quantity <= Capacity, ErrorMessages.ExcessiveQuantity);
            return new PutEvent(this, quantity, priority);
        }

        #endregion

        public sealed class GetEvent : ResourceEvent<GetEvent, double>
        {
            readonly Container _container;
            readonly double _quantity;

            internal GetEvent(Container container, double quantity, double priority) : base(container.Env, priority)
            {
                _container = container;
                _quantity = quantity;
                if (_container._getQueue.Count > 0 || !TrySchedule()) {
                    _container._getQueue.Add(this, priority);
                }
            }

            internal bool TrySchedule()
            {
                if (_container.Level - _quantity >= 0) {
                    _container.Level -= _quantity;
                    Env.ScheduleEvent(this);
                    return true;
                }
                return false;
            }

            #region Public Members

            public override void Dispose()
            {
                if (Disposed) {
                    // Nothing to do, event has already been disposed.
                    return;
                }
                // If event has not succeeded, then it means that
                // it may still be in the queue, from which it has to removed.
                if (!Succeeded && _container._getQueue.Contains(this)) {
                    _container._getQueue.Remove(this);
                    _container.BalanceQueues();
                }
                // Marks token as disposed, so that the user and the system
                // can recognize the fact that this token cannot be used anymore.
                Disposed = true;
            }

            #endregion

            #region SimEvent Members

            /// <summary>
            ///   QUANTITY
            /// </summary>
            public override double Value
            {
                get { return _quantity; }
            }

            protected override void OnEnd()
            {
                _container.BalanceQueues();
            }

            #endregion
        }

        public sealed class PutEvent : ResourceEvent<PutEvent, double>
        {
            readonly Container _container;
            readonly double _quantity;

            internal PutEvent(Container container, double quantity, double priority) : base(container.Env, priority)
            {
                _container = container;
                _quantity = quantity;
                if (_container._putQueue.Count > 0 || !TrySchedule()) {
                    _container._putQueue.Add(this, priority);
                }
            }

            internal bool TrySchedule()
            {
                if (_container.Level + _quantity <= _container.Capacity) {
                    _container.Level += _quantity;
                    Env.ScheduleEvent(this);
                    return true;
                }
                return false;
            }

            #region Public Members

            public override void Dispose()
            {
                if (Disposed) {
                    // Nothing to do, event has already been disposed.
                    return;
                }
                // If event has not succeeded, then it means that
                // it may still be in the queue, from which it has to removed.
                if (!Succeeded && _container._putQueue.Contains(this)) {
                    _container._putQueue.Remove(this);
                    _container.BalanceQueues();
                }
                // Marks token as disposed, so that the user and the system
                // can recognize the fact that this token cannot be used anymore.
                Disposed = true;
            }

            #endregion

            #region SimEvent Members

            /// <summary>
            ///   QUANTITY
            /// </summary>
            public override double Value
            {
                get { return _quantity; }
            }

            protected override void OnEnd()
            {
                _container.BalanceQueues();
            }

            #endregion
        }
    }
}