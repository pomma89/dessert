//
// Store.cs
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

namespace Dessert.Resources
{
    using System.Collections.Generic;
    using Core;
    using Events;
    using System.Runtime.CompilerServices;

    public sealed class Store<T> : SimEntity
    {
        readonly int _capacity;
        readonly IWaitQueue<GetEvent> _getQueue;
        readonly IWaitQueue<T> _itemQueue;
        readonly IWaitQueue<PutEvent> _putQueue;

        internal Store(SimEnvironment env, int capacity, WaitPolicy getPolicy, WaitPolicy putPolicy,
                       WaitPolicy itemPolicy) : base(env)
        {
            _capacity = capacity;
            _getQueue = WaitQueue.New<GetEvent>(getPolicy, Env);
            _putQueue = WaitQueue.New<PutEvent>(putPolicy, Env);
            _itemQueue = WaitQueue.New<T>(itemPolicy, Env);
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

        public int Capacity
        {
            get { return _capacity; }
        }

        public int Count
        {
            get { return _itemQueue.Count; }
        }

        public WaitPolicy GetPolicy
        {
            get { return _getQueue.Policy; }
        }

        public IEnumerable<GetEvent> GetQueue
        {
            get { return _getQueue; }
        }

        public WaitPolicy ItemPolicy
        {
            get { return _itemQueue.Policy; }
        }

        public IEnumerable<T> ItemQueue
        {
            get { return _itemQueue; }
        }

        public WaitPolicy PutPolicy
        {
            get { return _putQueue.Policy; }
        }

        public IEnumerable<PutEvent> PutQueue
        {
            get { return _putQueue; }
        }

        public GetEvent Get()
        {
            return new GetEvent(this, Default.Priority);
        }

        public GetEvent Get(double priority)
        {
            return new GetEvent(this, priority);
        }

        public PutEvent Put(T item)
        {
            return new PutEvent(this, item, Default.Priority, Default.Priority);
        }

        public PutEvent Put(T item, double putPriority)
        {
            return new PutEvent(this, item, putPriority, Default.Priority);
        }

        public PutEvent Put(T item, double putPriority, double itemPriority)
        {
            return new PutEvent(this, item, putPriority, itemPriority);
        }

        #endregion

        public sealed class GetEvent : ResourceEvent<GetEvent, T>
        {
            readonly Store<T> _store;
            T _item;

            internal GetEvent(Store<T> store, double getPriority) : base(store.Env, getPriority)
            {
                _store = store;
                if (_store._getQueue.Count > 0 || !TrySchedule()) {
                    _store._getQueue.Add(this, getPriority);
                }
            }

            internal bool TrySchedule()
            {
                if (_store.Count > 0) {
                    _item = _store._itemQueue.RemoveFirst();
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
                if (!Succeeded && _store._getQueue.Contains(this)) {
                    _store._getQueue.Remove(this);
                    _store.BalanceQueues();
                }
                // Marks token as disposed, so that the user and the system
                // can recognize the fact that this token cannot be used anymore.
                Disposed = true;
            }

            #endregion

            #region SimEvent Members

            public override T Value
            {
                get { return _item; }
            }

            protected override void OnEnd()
            {
                _store.BalanceQueues();
            }

            #endregion
        }

        public sealed class PutEvent : ResourceEvent<PutEvent, T>
        {
            readonly T _item;
            readonly double _itemPriority;
            readonly Store<T> _store;

            internal PutEvent(Store<T> store, T item, double putPriority, double itemPriority)
                : base(store.Env, putPriority)
            {
                _store = store;
                _item = item;
                _itemPriority = itemPriority;
                if (_store._putQueue.Count > 0 || !TrySchedule()) {
                    _store._putQueue.Add(this, putPriority);
                }
            }

            internal bool TrySchedule()
            {
                if (_store.Count < _store.Capacity) {
                    _store._itemQueue.Add(_item, _itemPriority);
                    Env.ScheduleEvent(this);
                    return true;
                }
                return false;
            }

            #region Public Members

            public double ItemPriority
            {
                get { return _itemPriority; }
            }

            public override void Dispose()
            {
                if (Disposed) {
                    // Nothing to do, event has already been disposed.
                    return;
                }
                // If event has not succeeded, then it means that
                // it may still be in the queue, from which it has to removed.
                if (!Succeeded && _store._putQueue.Contains(this)) {
                    _store._putQueue.Remove(this);
                    _store.BalanceQueues();
                }
                // Marks token as disposed, so that the user and the system
                // can recognize the fact that this token cannot be used anymore.
                Disposed = true;
            }

            #endregion

            #region SimEvent Members

            public override T Value
            {
                get { return _item; }
            }

            protected override void OnEnd()
            {
                _store.BalanceQueues();
            }

            #endregion
        }
    }
}