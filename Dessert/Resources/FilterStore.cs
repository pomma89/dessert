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
    using System;
    using System.Collections.Generic;
    using Core;
    using Events;

    public sealed class FilterStore<T> : SimEntity
    {
        readonly int _capacity;
        readonly IWaitQueue<GetEvent> _getQueue;
        readonly IWaitQueue<T> _itemQueue;
        readonly IWaitQueue<PutEvent> _putQueue;

        internal FilterStore(SimEnvironment env, int capacity, WaitPolicy getPolicy, WaitPolicy putPolicy,
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
            return new GetEvent(this, item => true, Default.Priority);
        }

        public GetEvent Get(double priority)
        {
            return new GetEvent(this, item => true, priority);
        }

        public GetEvent Get(Predicate<T> filter)
        {
            return new GetEvent(this, filter, Default.Priority);
        }

        public GetEvent Get(Predicate<T> filter, double priority)
        {
            return new GetEvent(this, filter, priority);
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
            readonly Predicate<T> _filter;
            readonly FilterStore<T> _filterStore;
            internal T Item;

            internal GetEvent(FilterStore<T> filterStore, Predicate<T> filter, double getPriority)
                : base(filterStore.Env, getPriority)
            {
                _filterStore = filterStore;
                _filter = filter;
                if (!TrySchedule()) {
                    _filterStore._getQueue.Add(this, getPriority);
                }
            }

            internal bool TrySchedule()
            {
                foreach (var item in _filterStore._itemQueue) {
                    if (_filter(item)) {
                        _filterStore._itemQueue.Remove(item);
                        Item = item;
                        Env.ScheduleEvent(this);
                        return true;                     
                    }
                }
                return false;
            }

            #region Public Members

            public Predicate<T> Filter
            {
                get { return _filter; }
            }

            public override void Dispose()
            {
                if (Disposed) {
                    // Nothing to do, event has already been disposed.
                    return;
                }
                // If event has not succeeded, then it means that
                // it may still be in the queue, from which it has to removed.
                if (!Succeeded && _filterStore._getQueue.Contains(this)) {
                    _filterStore._getQueue.Remove(this);
                    _filterStore.BalanceQueues();
                }
                // Marks token as disposed, so that the user and the system
                // can recognize the fact that this token cannot be used anymore.
                Disposed = true;
            }

            #endregion

            #region SimEvent Members

            public override T Value
            {
                get { return Item; }
            }

            protected override void OnEnd()
            {
                _filterStore.BalanceQueues();
            }

            #endregion
        }

        public sealed class PutEvent : ResourceEvent<PutEvent, T>
        {
            readonly FilterStore<T> _filterStore;
            readonly T _item;
            readonly double _itemPriority;

            internal PutEvent(FilterStore<T> filterStore, T item, double putPriority, double itemPriority)
                : base(filterStore.Env, putPriority)
            {
                _filterStore = filterStore;
                _item = item;
                _itemPriority = itemPriority;
                if (!TrySchedule()) {
                    _filterStore._putQueue.Add(this, putPriority);
                }
            }

            internal bool TrySchedule()
            {
                foreach (var getEv in _filterStore._getQueue) {
                    if (!getEv.Filter(_item)) {
                        continue;
                    }
                    _filterStore._getQueue.Remove(getEv);
                    getEv.Item = _item;
                    Env.ScheduleEvent(this);
                    Env.ScheduleEvent(getEv);
                    if (_filterStore._putQueue.Count > 0) {
                        var putEv = _filterStore._putQueue.RemoveFirst();
                        _filterStore._itemQueue.Add(putEv._item, putEv._itemPriority);
                    }
                    return true;
                }
                if (_filterStore.Count < _filterStore.Capacity) {
                    _filterStore._itemQueue.Add(_item, _itemPriority);
                    Env.ScheduleEvent(this);
                    return true;
                }
                return false;
            }

            #region Public Members

            public T Item
            {
                get { return _item; }
            }

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
                if (!Succeeded && _filterStore._putQueue.Contains(this)) {
                    _filterStore._putQueue.Remove(this);
                    _filterStore.BalanceQueues();
                }
                // Marks token as disposed, so that the user and the system
                // can recognize the fact that this token cannot be used anymore.
                Disposed = true;
            }

            #endregion

            #region SimEvent Members

            public override T Value
            {
                get { return Item; }
            }

            protected override void OnEnd()
            {
                _filterStore.BalanceQueues();
            }

            #endregion
        }
    }
}