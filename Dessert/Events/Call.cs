//
// Call.cs
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
    using System.Collections.Generic;

    public class Call<T> : StandaloneEvent<Call<T>, T>, IInternalCall
    {
        /// <summary>
        /// 
        /// </summary>
        readonly SimProcess _process;

        /// <summary>
        /// 
        /// </summary>
        T _value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="generator"></param>
        internal Call(SimEnvironment env, IEnumerator<SimEvent> generator) : base(env)
        {
            (this as IInternalCall).Steps = generator;
            _process = env.ActiveProcess;
            env.ScheduleEvent(this);
        }

        #region IInternalCall Members

        IInternalCall IInternalCall.PreviousCall { get; set; }

        IEnumerator<SimEvent> IInternalCall.Steps { get; set; }

        void IInternalCall.SetValue(object val)
        {
            _value = (T) val;
        }

        #endregion

        #region SimEvent Members

        public override T Value
        {
            get { return _value; }
        }

        protected override void OnEnd()
        {
            _process.PushCall(this);
        }

        protected override State ValidStatesMask()
        {
            return base.ValidStatesMask() | State.Succeeded;
        }

        #endregion
    }

    public sealed class Call : Call<object>
    {
        internal Call(SimEnvironment env, IEnumerator<SimEvent> generator) : base(env, generator)
        {
        }
    }

    interface IInternalCall
    {
        IInternalCall PreviousCall { get; set; }

        IEnumerator<SimEvent> Steps { get; set; }

        void SetValue(object value);
    }
}