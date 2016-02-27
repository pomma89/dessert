//
// Resource.cs
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
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using Core;
    using DIBRIS.Hippie;
    using Events;

    public sealed class PreemptiveResource : SimEntity
    {
        readonly int _capacity;

        /// <summary>
        ///   Stores the requests waiting for this resource.
        /// </summary>
        readonly ArrayHeap<RequestEvent, ReqPriority> _requestQueue;

        /// <summary>
        ///   Stores the users which own this resource.
        /// </summary>
        readonly ArrayHeap<RequestEvent, ReqPriority> _users;

        ulong _nextVersion;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="capacity"></param>
        internal PreemptiveResource(SimEnvironment env, int capacity) : base(env)
        {
            _capacity = capacity;
            _requestQueue = HeapFactory.NewRawBinaryHeap<RequestEvent, ReqPriority>(ReqPriority.Comparer);
            _users = HeapFactory.NewRawBinaryHeap<RequestEvent, ReqPriority>(ReqPriority.ReverseComparer);
        }

        void BalanceQueues()
        {
            if (_requestQueue.Count > 0 && _requestQueue.Min.Value.TrySchedule()) {
                _requestQueue.RemoveMin();
            }
        }

        #region IPreemptiveResource Members

        public int Capacity
        {
            get { return _capacity; }
        }

        public int Count
        {
            get { return _users.Count; }
        }

        public WaitPolicy RequestPolicy
        {
            get { return WaitPolicy.Priority; }
        }

        public IEnumerable<RequestEvent> RequestQueue
        {
            get
            {
// ReSharper disable LoopCanBeConvertedToQuery
                foreach (var req in _requestQueue) {
                    yield return req.Value;
                }
// ReSharper restore LoopCanBeConvertedToQuery
            }
        }

        public IEnumerable<RequestEvent> Users
        {
            get
            {
// ReSharper disable LoopCanBeConvertedToQuery
                foreach (var user in _users) {
                    yield return user.Value;
                }
// ReSharper restore LoopCanBeConvertedToQuery
            }
        }

        public ReleaseEvent Release(RequestEvent request)
        {
            Contract.Requires<ArgumentNullException>(request != null, ErrorMessages.NullRequest);
            Contract.Requires<ArgumentException>(ReferenceEquals(this, request.Resource), ErrorMessages.DifferentResource);
            return new ReleaseEvent(this, request);
        }

        public RequestEvent Request()
        {
            return new RequestEvent(this, Default.Priority, Default.Preempt);
        }

        public RequestEvent Request(double priority)
        {
            return new RequestEvent(this, priority, Default.Preempt);
        }

        public RequestEvent Request(double priority, bool preempt)
        {
            return new RequestEvent(this, priority, preempt);
        }

        #endregion

        #region SimPy3 Helpers

#if SIMPY3
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

    //public IResourceRequest request()
    //{
    //    return Request(Default.Priority);
    //}

// ReSharper restore UnusedMember.Global
// ReSharper restore InconsistentNaming
#endif

        #endregion

        sealed class ReqPriority
        {
            public static readonly IComparer<ReqPriority> Comparer = new CustomComparer();
            public static readonly IComparer<ReqPriority> ReverseComparer = new ReverseCustomComparer();

            public readonly bool Preempt;
            public readonly double Priority;
            public readonly double Time;
            readonly ulong _version;

            public ReqPriority(double priority, double time, bool preempt, ulong version)
            {
                Priority = priority;
                Time = time;
                Preempt = preempt;
                _version = version;
            }

            sealed class CustomComparer : IComparer<ReqPriority>
            {
                public int Compare(ReqPriority x, ReqPriority y)
                {
                    var prCmp = x.Priority.CompareTo(y.Priority);
                    if (prCmp != 0) {
                        return prCmp;
                    }
                    var timeCmp = x.Time.CompareTo(y.Time);
                    if (timeCmp != 0) {
                        return timeCmp;
                    }
                    // A true preempt has a greater priority than a false one.
                    var preemptCmp = y.Preempt.CompareTo(x.Preempt);
                    return (preemptCmp != 0) ? preemptCmp : x._version.CompareTo(y._version);
                }
            }

            sealed class ReverseCustomComparer : IComparer<ReqPriority>
            {
                public int Compare(ReqPriority x, ReqPriority y)
                {
                    return -1*Comparer.Compare(x, y);
                }
            }
        }

        public sealed class RequestEvent : ResourceEvent<RequestEvent, object>
        {
            readonly PreemptiveResource _resource;
            readonly ReqPriority _priority;
            readonly SimProcess _process;

            IHeapHandle<RequestEvent, ReqPriority> _handle;

            internal RequestEvent(PreemptiveResource resource, double priority, bool preempt)
                : base(resource.Env, priority)
            {
                _resource = resource;
                _priority = new ReqPriority(priority, Env.Now, preempt, resource._nextVersion++);
                _process = Env.ActiveProcess;

                if (_resource._requestQueue.Count == 0 && TrySchedule()) {
                    return;
                }
                _handle = _resource._requestQueue.Add(this, _priority);
                if (preempt && _resource._requestQueue.Min.Value.Equals(this)) {
                    var toPreempt = _resource._users.Min.Value;
                    if (ReqPriority.Comparer.Compare(toPreempt._priority, _priority) <= 0) {
                        return;
                    }
                    toPreempt.Dispose();
                    toPreempt._process.Interrupt(new PreemptionInfo(_process, toPreempt.Time));
                }
            }

            internal bool TrySchedule()
            {
                if (_resource._users.Count < _resource.Capacity) {
                    _handle = _resource._users.Add(this, _priority);
                    Env.ScheduleEvent(this);
                    return true;
                }
                return false;
            }

            #region Public Members

            public bool Preempt
            {
                get { return _priority.Preempt; }
            }

            public PreemptiveResource Resource
            {
                get { return _resource; }
            }

            public double Time
            {
                get { return _priority.Time; }
            }

            public override void Dispose()
            {
                if (Disposed) {
                    return;
                }
                if (Succeeded || !_resource._requestQueue.Contains(_handle)) {
                    _resource._users.Remove(_handle);
                } else {
                    _resource._requestQueue.Remove(_handle);
                }
                _resource.BalanceQueues();
                // Marks event as disposed, so that the request event
                // cannot be disposed two or more times.
                Disposed = true;
                Debug.Assert(!_resource._users.Contains(_handle));
                Debug.Assert(!_resource._requestQueue.Contains(_handle));
            }

            #endregion

            #region SimEvent Members

            public override object Value
            {
                get { return Default.NoValue; }
            }

            #endregion
        }

        public sealed class ReleaseEvent : ResourceEvent<ReleaseEvent, object>
        {
            readonly RequestEvent _request;

            internal ReleaseEvent(PreemptiveResource resource, RequestEvent request)
                : base(resource.Env, Default.Priority)
            {
                _request = request;
                Env.ScheduleEvent(this);
            }

            #region Public Members

            public RequestEvent Request
            {
                get { return _request; }
            }

            public override void Dispose()
            {
                // Do nothing...
            }

            #endregion

            #region SimEvent Members

            public override object Value
            {
                get { return Default.NoValue; }
            }

            protected override void OnEnd()
            {
                _request.Dispose();
            }

            #endregion
        }
    }
}