//
// ConditionEvaluators.cs
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

namespace Dessert.Core
{
    using Events;

    static class ConditionEvaluators
    {
        #region All Evaluators

        public static bool AllEvents<T1>(Condition<T1> c) where T1 : SimEvent
        {
            return c.Value.Count == 1;
        }

        public static bool AllEvents<T1, T2>(Condition<T1, T2> c) where T1 : SimEvent where T2 : SimEvent
        {
            return c.Value.Count == 2;
        }

        public static bool AllEvents<T1, T2, T3>(Condition<T1, T2, T3> c) where T1 : SimEvent where T2 : SimEvent
            where T3 : SimEvent
        {
            return c.Value.Count == 3;
        }

        public static bool AllEvents<T1, T2, T3, T4>(Condition<T1, T2, T3, T4> c) where T1 : SimEvent
            where T2 : SimEvent where T3 : SimEvent where T4 : SimEvent
        {
            return c.Value.Count == 4;
        }

        public static bool AllEvents<T1, T2, T3, T4, T5>(Condition<T1, T2, T3, T4, T5> c) where T1 : SimEvent
            where T2 : SimEvent where T3 : SimEvent where T4 : SimEvent where T5 : SimEvent
        {
            return c.Value.Count == 5;
        }

        #endregion

        #region Any Evaluators

        public static bool AnyEvent<T1>(Condition<T1> c) where T1 : SimEvent
        {
            return c.Value.Count >= 1;
        }

        public static bool AnyEvent<T1, T2>(Condition<T1, T2> c) where T1 : SimEvent where T2 : SimEvent
        {
            return c.Value.Count >= 1;
        }

        public static bool AnyEvent<T1, T2, T3>(Condition<T1, T2, T3> c) where T1 : SimEvent where T2 : SimEvent
            where T3 : SimEvent
        {
            return c.Value.Count >= 1;
        }

        public static bool AnyEvent<T1, T2, T3, T4>(Condition<T1, T2, T3, T4> c) where T1 : SimEvent where T2 : SimEvent
            where T3 : SimEvent where T4 : SimEvent
        {
            return c.Value.Count >= 1;
        }

        public static bool AnyEvent<T1, T2, T3, T4, T5>(Condition<T1, T2, T3, T4, T5> c) where T1 : SimEvent
            where T2 : SimEvent where T3 : SimEvent where T4 : SimEvent where T5 : SimEvent
        {
            return c.Value.Count >= 1;
        }

        #endregion
    }
}