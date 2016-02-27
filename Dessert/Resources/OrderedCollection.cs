/*
CodeBits Code Snippets
Copyright (c) 2012 Jeevan James
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

/* Documentation: http://codebits.codeplex.com/wikipage?title=OrderedCollection */

/* SOURCES BELOW HAVE BEEN CUSTOMIZED FOR DESSERT */

namespace Dessert.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using DIBRIS.Hippie;
    using System.Diagnostics;
    using PommaLabs.Thrower.Reflection;
    static class OrderedCollection
    {
        public static OrderedCollection<T> New<T>(bool allowDuplicates = false, bool reverseOrder = false)
            where T : IComparable<T>
        {
            return new OrderedCollection<T>(BetterComparer<T>.Default, allowDuplicates, reverseOrder);
        }
    }

    /// <summary>
    /// Always sorted collection of items.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection</typeparam>
    sealed partial class OrderedCollection<T> : Collection<T>
    {
        const int SimpleAlgorithmThreshold = 10;
        readonly bool _allowDuplicates;
        readonly IComparer<T> _comparer;
        readonly bool _reverseOrder;

        /// <summary>
        ///   Initializes a new instance of the OrderedCollection.
        /// </summary>
        /// <param name="allowDuplicates">True if the collection should allow duplicate values.</param>
        /// <param name="reverseOrder">True to reverse the order in which the items are sorted.</param>
        public OrderedCollection(bool allowDuplicates = false, bool reverseOrder = false)
        {
            var comparableType = typeof(IComparable<>).MakeGenericType(typeof(T));
            Debug.Assert(PortableTypeInfo.IsAssignableFrom(comparableType, typeof(T)), "Generic type should implement IComparable<>");
            _comparer = new ComparableComparer<T>();
            _allowDuplicates = allowDuplicates;
            _reverseOrder = reverseOrder;
        }

        /// <summary>
        /// Initializes a new instance of the OrderedCollection using an IComparer implementation.
        /// </summary>
        /// <param name="comparer">IComparer implementation used for the item comparisons during ordering</param>
        /// <param name="allowDuplicates">True if the collection should allow duplicate values</param>
        /// <param name="reverseOrder">True to reverse the order in which the items are sorted</param>
        public OrderedCollection(IComparer<T> comparer, bool allowDuplicates = false, bool reverseOrder = false)
        {
            Debug.Assert(comparer != null);
            _comparer = comparer;
            _allowDuplicates = allowDuplicates;
            _reverseOrder = reverseOrder;
        }

        /// <summary>
        /// Initializes a new instance of the OrderedCollection class using a Comparison delegate for
        /// the comparison logic.
        /// </summary>
        /// <param name="comparison">The comparison delegate used for the item comparisons during ordering</param>
        /// <param name="allowDuplicates">True if the collection should allow duplicate values</param>
        /// <param name="reverseOrder">True to reverse the order in which the items are sorted</param>
        public OrderedCollection(Comparison<T> comparison, bool allowDuplicates = false, bool reverseOrder = false)
        {
            Debug.Assert(comparison != null);
            _comparer = new ComparisonComparer<T>(comparison);
            _allowDuplicates = allowDuplicates;
            _reverseOrder = reverseOrder;
        }

        /// <summary>
        /// Specifies whether duplicate values are allowed in the collection
        /// </summary>
        public bool AllowDuplicates
        {
            get { return _allowDuplicates; }
        }

        /// <summary>
        /// Specifies whether to sort the items of the collection in reverse order
        /// </summary>
        public bool ReverseOrder
        {
            get { return _reverseOrder; }
        }

        protected override void InsertItem(int index, T item)
        {
            var insertIndex = GetInsertIndex(item);
            Debug.Assert(insertIndex >= 0, "Attempting to set duplicate value in collection");
            base.InsertItem(insertIndex, item);
        }

        protected override void SetItem(int index, T item)
        {
            RemoveItem(index);
            var insertIndex = GetInsertIndex(item);
            Debug.Assert(insertIndex >= 0, "Attempting to set duplicate value in collection");
            base.InsertItem(insertIndex, item);
        }

        /// <summary>
        /// Performs a comparison between two item of type T. By default, this uses the IComparer implementation
        /// or the Comparison delegate specified in the constructor, but derived types can override
        /// this method to specify their own custom logic.
        /// </summary>
        /// <param name="x">The first item to compare</param>
        /// <param name="y">The second item to compare</param>
        /// <returns>A signed integer - zero if the items are equal, less than zero if x is less than y and greater than zero if x is greater than y</returns>
        int Compare(T x, T y)
        {
            return _comparer.Compare(x, y);
        }

        int ReverseComparisonIfNeeded(int comparison)
        {
            return _reverseOrder ? -(comparison) : comparison;
        }

        int GetInsertIndex(T item)
        {
            if (Count == 0) {
                return 0;
            }
            return Count <= SimpleAlgorithmThreshold ? GetInsertIndexSimple(item) : GetInsertIndexComplex(item);
        }

        // Performs a simple left-to-right search for the best location to insert the new item.
        // This algorithm is used while the collection size is small, i.e. less than or equal to the
        // value specified by the SimpleAlgorithmThreshold constant.
        int GetInsertIndexSimple(T item)
        {
            for (var i = 0; i < Items.Count; i++) {
                var existingItem = Items[i];
                var comparison = ReverseComparisonIfNeeded(Compare(existingItem, item));
                if (comparison == 0 && !_allowDuplicates) {
                    return -1;
                }
                if (comparison > 0) {
                    return i;
                }
            }
            return Count;
        }

        // Performs a divide-and-conquer search for the best location to insert the new item.
        // Since the list is already sorted, this is the fastest algorithm after the collection size
        // crosses a certain threshold.
        int GetInsertIndexComplex(T item)
        {
            var minIndex = 0;
            var maxIndex = Count - 1;
            while (minIndex <= maxIndex) {
                var pivotIndex = (maxIndex + minIndex)/2;
                var comparison = ReverseComparisonIfNeeded(Compare(item, Items[pivotIndex]));
                if (comparison == 0 && !_allowDuplicates) {
                    return -1;
                }
                if (comparison < 0) {
                    maxIndex = pivotIndex - 1;
                } else {
                    minIndex = pivotIndex + 1;
                }
            }
            return minIndex;
        }
    }

    public sealed partial class OrderedCollection<T>
    {
        // Comparer that uses the type's IComparable implementation to compare two values.
        sealed class ComparableComparer<TItem> : IComparer<TItem>
        {
            int IComparer<TItem>.Compare(TItem x, TItem y)
            {
                return ((IComparable<TItem>) x).CompareTo(y);
            }
        }
    }

    public sealed partial class OrderedCollection<T>
    {
        // Comparer that uses a Comparison delegate to perform the comparison logic.
        sealed class ComparisonComparer<TItem> : IComparer<TItem>
        {
            readonly Comparison<TItem> _comparison;

            internal ComparisonComparer(Comparison<TItem> comparison)
            {
                _comparison = comparison;
            }

            int IComparer<TItem>.Compare(TItem x, TItem y)
            {
                return _comparison(x, y);
            }
        }
    }
}