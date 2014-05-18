//
// OptimizedSkewHeap.cs
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
    using System.Diagnostics;

    sealed class OptimizedSkewHeap
    {
        public OptimizedSkewHeap(SimEvent initialRoot)
        {
            Min = initialRoot;
            Count = 1;
        }

        public int Count { get; private set; }

        public SimEvent Min { get; private set; }

        public void Add(SimEvent ev)
        {
            Debug.Assert(Min != null && ev != null);
            Debug.Assert(ev.Left == null && ev.Right == null);
            ev.Scheduled = true;
            if (SimEvent.IsSmaller(ev, Min)) {
                ev.Left = Min;
                Min = ev;
            } else {
                var x = Min;
                var y = x;
                x = x.Right;
                y.Right = y.Left;
                while (x != null) {
                    if (SimEvent.IsSmaller(ev, x)) {
                        y.Left = ev;
                        y = ev;
                        ev = x;
                    } else {
                        y.Left = x;
                        y = x;
                    }
                    x = y.Right;
                    y.Right = y.Left;
                }
                y.Left = ev;
            }
            Count++;
        }

        public void RemoveMin()
        {
            Debug.Assert(Count > 0);
            Debug.Assert(Min != null);
            var min = Min;
            var h1 = Min.Left;
            var h2 = Min.Right;
            if (h1 == null) {
                Min = h2;
            } else if (h2 == null) {
                Min = h1;
            } else {
                if (SimEvent.IsSmaller(h2, h1)) {
                    var tmp = h2;
                    h2 = h1;
                    h1 = tmp;
                }
                Min = h1;
                var y = h1;
                h1 = h1.Right;
                y.Right = y.Left;
                while (h1 != null) {
                    if (SimEvent.IsSmaller(h2, h1)) {
                        y.Left = h2;
                        y = h2;
                        h2 = h1;
                    } else {
                        y.Left = h1;
                        y = h1;                      
                    }
                    h1 = y.Right;
                    y.Right = y.Left;
                }
                y.Left = h2;
            }
            min.Scheduled = false;
            min.Left = min.Right = null;
            Count--;
        }
    }
}