//
// WaitQueues.cs
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Core;
    using Troschuetz.Random;
    using Troschuetz.Random.Generators;
    using Finsa.CodeServices.Common.Collections;
    interface IWaitQueue<T> : ICollection<T>
    {
        T First { get; }

        WaitPolicy Policy { get; }

        void Add(T item, double priority);

        T RemoveFirst();
    }

    abstract class WaitQueueBase<T> : IWaitQueue<T>
    {
        #region ICollection Members

        public bool IsReadOnly
        {
            get { throw new DessertException(ErrorMessages.InternalError); }
        }

        public void Add(T item)
        {
            Add(item, Default.Priority);
        }

        public void Clear()
        {
            throw new DessertException(ErrorMessages.InternalError);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new DessertException(ErrorMessages.InternalError);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public abstract int Count { get; }

        public abstract T First { get; }

        public abstract WaitPolicy Policy { get; }

        public abstract void Add(T item, double priority);

        public abstract bool Contains(T item);

        public abstract IEnumerator<T> GetEnumerator();

        public abstract bool Remove(T item);

        public abstract T RemoveFirst();
    }

    static class WaitQueue
    {
        public static IWaitQueue<T> New<T>(WaitPolicy waitPolicy, SimEnvironment env)
        {
            switch (waitPolicy) {
                case WaitPolicy.FIFO:
                    return new FifoWaitQueue<T>();
                case WaitPolicy.LIFO:
                    return new LifoWaitQueue<T>();
                case WaitPolicy.Priority:
                    return new PriorityWaitQueue<T>();
                case WaitPolicy.Random:
                    return new RandomWaitQueue<T>(env.Random);
                default:
                    var msg = ErrorMessages.InvalidEnum<WaitPolicy>();
                    throw new ArgumentException(msg);
            }
        }

        public static Pair<T1, T2> NewPair<T1, T2>(T1 item1, T2 item2) where T2 : struct, IComparable<T2>
        {
            return new Pair<T1, T2>(item1, item2);
        }

        public sealed class Pair<T1, T2> : IComparable<Pair<T1, T2>>, IEquatable<Pair<T1, T2>>
            where T2 : struct, IComparable<T2>
        {
            public readonly T1 Item1;
            readonly T2 _item2;

            public Pair(T1 item1, T2 item2)
            {
                Item1 = item1;
                _item2 = item2;
            }

            public int CompareTo(Pair<T1, T2> other)
            {
                return _item2.CompareTo(other._item2);
            }

            public bool Equals(Pair<T1, T2> other)
            {
                return EqualityComparer<T1>.Default.Equals(Item1, other.Item1);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }
                return obj is Pair<T1, T2> && Equals((Pair<T1, T2>) obj);
            }

            public override int GetHashCode()
            {
                return EqualityComparer<T1>.Default.GetHashCode(Item1);
            }
        }
    }

    sealed class FifoWaitQueue<T> : WaitQueueBase<T>
    {
        readonly SinglyLinkedList<T> _items = new SinglyLinkedList<T>();

        public override int Count
        {
            get { return _items.Count; }
        }

        public override T First
        {
            get { return _items.First; }
        }

        public override WaitPolicy Policy
        {
            get { return WaitPolicy.FIFO; }
        }

        public override void Add(T item, double priority)
        {
            _items.AddLast(item);
        }

        public override bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public override bool Remove(T item)
        {
            Debug.Assert(_items.Contains(item));
            return _items.Remove(item);
        }

        public override T RemoveFirst()
        {
            var first = _items.First;
            _items.RemoveFirst();
            return first;
        }
    }

    sealed class LifoWaitQueue<T> : WaitQueueBase<T>
    {
        readonly ThinLinkedList<T> _items = new ThinLinkedList<T>();

        public override int Count
        {
            get { return _items.Count; }
        }

        public override T First
        {
            get { return _items.First; }
        }

        public override WaitPolicy Policy
        {
            get { return WaitPolicy.LIFO; }
        }

        public override void Add(T item, double priority)
        {
            _items.AddFirst(item);
        }

        public override bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public override bool Remove(T item)
        {
            Debug.Assert(_items.Contains(item));
            return _items.Remove(item);
        }

        public override T RemoveFirst()
        {
            var first = _items.First;
            _items.RemoveFirst();
            return first;
        }
    }

    sealed class PriorityWaitQueue<T> : WaitQueueBase<T>
    {
        readonly OrderedCollection<WaitQueue.Pair<T, double>> _items;

        public PriorityWaitQueue()
        {
            _items = OrderedCollection.New<WaitQueue.Pair<T, double>>(true);
        }

        public override int Count
        {
            get { return _items.Count; }
        }

        public override T First
        {
            get
            {
                Debug.Assert(_items.Count > 0);
                return _items[0].Item1;
            }
        }

        public override WaitPolicy Policy
        {
            get { return WaitPolicy.Priority; }
        }

        public override void Add(T item, double priority)
        {
            _items.Add(WaitQueue.NewPair(item, priority));
        }

        public override bool Contains(T item)
        {
            return _items.Contains(WaitQueue.NewPair(item, 0.0));
        }

        public override IEnumerator<T> GetEnumerator()
        {
// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in _items) {
                yield return item.Item1;
            }
// ReSharper restore LoopCanBeConvertedToQuery
        }

        public override bool Remove(T item)
        {
            var tmpPair = WaitQueue.NewPair(item, 0.0);
            Debug.Assert(_items.Contains(tmpPair));
            return _items.Remove(tmpPair);
        }

        public override T RemoveFirst()
        {
            Debug.Assert(_items.Count > 0);
            var first = _items[0].Item1;
            _items.RemoveAt(0);
            return first;
        }
    }

    sealed class RandomWaitQueue<T> : WaitQueueBase<T>
    {
        readonly OrderedCollection<WaitQueue.Pair<T, int>> _items;
        readonly TRandom _random;

        public RandomWaitQueue(TRandom random)
        {
            _items = OrderedCollection.New<WaitQueue.Pair<T, int>>(true);
            _random = random;
        }

        public override int Count
        {
            get { return _items.Count; }
        }

        public override T First
        {
            get
            {
                Debug.Assert(_items.Count > 0);
                return _items[0].Item1;
            }
        }

        public override WaitPolicy Policy
        {
            get { return WaitPolicy.Random; }
        }

        public override void Add(T item, double priority)
        {
            _items.Add(WaitQueue.NewPair(item, _random.Next()));
        }

        public override bool Contains(T item)
        {
            return _items.Contains(WaitQueue.NewPair(item, 0));
        }

        public override IEnumerator<T> GetEnumerator()
        {
// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in _items) {
                yield return item.Item1;
            }
// ReSharper restore LoopCanBeConvertedToQuery
        }

        public override bool Remove(T item)
        {
            var tmpPair = WaitQueue.NewPair(item, 0);
            Debug.Assert(_items.Contains(tmpPair));
            return _items.Remove(tmpPair);
        }

        public override T RemoveFirst()
        {
            Debug.Assert(_items.Count > 0);
            var first = _items[0].Item1;
            _items.RemoveAt(0);
            return first;
        }
    }
}