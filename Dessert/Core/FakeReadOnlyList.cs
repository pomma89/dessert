//  
// FakeReadOnlyList.cs
//  
// Author(s):
//     Alessio Parma <alessio.parma@gmail.com>
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
    using System.Collections;
    using System.Collections.Generic;

    sealed class FakeReadOnlyList<T> : IList<T>
    {
        const string ReadOnlyCollection = "Collection is readonly";

        T[] _array = new T[1];

        public void ForceAdd(T item)
        {
            if (_array.Length == Count) {
                Array.Resize(ref _array, Count << 1);
            }
            _array[Count++] = item;
        }

        #region IList Members

        public int Count { get; private set; }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public T this[int index]
        {
            get { return _array[index]; }
            set { throw new InvalidOperationException(ReadOnlyCollection); }
        }

        public void Add(T item)
        {
            throw new InvalidOperationException(ReadOnlyCollection);
        }

        public void Clear()
        {
            throw new InvalidOperationException(ReadOnlyCollection);
        }

        public bool Contains(T item)
        {
            for (var i = 0; i < Count; ++i) {
                if (ReferenceEquals(item, _array[i])) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///   Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> 
        ///   to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">
        ///   The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements
        ///   copied from <see cref="T:System.Collections.Generic.ICollection`1"/>.
        ///   The <see cref="T:System.Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        ///   The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="array"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> 
        ///   is greater than the available space from <paramref name="arrayIndex"/>
        ///   to the end of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            for (var i = 0; i < Count; ++i) {
                array[arrayIndex + i] = _array[i];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i) {
                yield return _array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = 0; i < Count; ++i) {
                yield return _array[i];
            }
        }

        /// <summary>
        ///   Determines the index of a specific item in the <see cref="FakeReadOnlyList{T}"/>.
        /// </summary>
        /// <param name="item">
        ///   The object to locate in the <see cref="FakeReadOnlyList{T}"/>.
        /// </param>
        /// <returns>
        ///   The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            for (var i = 0; i < Count; ++i) {
                if (ReferenceEquals(item, _array[i])) {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new InvalidOperationException(ReadOnlyCollection);
        }

        public bool Remove(T item)
        {
            throw new InvalidOperationException(ReadOnlyCollection);
        }

        public void RemoveAt(int index)
        {
            throw new InvalidOperationException(ReadOnlyCollection);
        }

        #endregion
    }
}