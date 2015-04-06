//
// Resource.cs
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
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using Collections;
    using Core;
    using Events;

    public sealed class Resource : SimEntity
    {
        readonly int _capacity;
        readonly IWaitQueue<RequestEvent> _requestQueue;

        /// <summary>
        ///   Stores the users which own this resource.
        /// </summary>
        readonly ThinLinkedList<RequestEvent> _users = new ThinLinkedList<RequestEvent>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="capacity"></param>
        /// <param name="requestPolicy"></param>
        internal Resource(SimEnvironment env, int capacity, WaitPolicy requestPolicy) : base(env)
        {
            _capacity = capacity;
            _requestQueue = WaitQueue.New<RequestEvent>(requestPolicy, Env);
        }

        void BalanceQueues()
        {
            if (_requestQueue.Count > 0 && _requestQueue.First.TrySchedule()) {
                _requestQueue.RemoveFirst();
            }
        }

        #region IResource Members

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
            get { return _requestQueue.Policy; }
        }

        public IEnumerable<RequestEvent> RequestQueue
        {
            get { return _requestQueue; }
        }

        public IEnumerable<RequestEvent> Users
        {
            get { return _users; }
        }

        public ReleaseEvent Release(RequestEvent request)
        {
            Contract.Requires<ArgumentNullException>(request != null, ErrorMessages.NullRequest);
            Contract.Requires<ArgumentException>(ReferenceEquals(this, request.Resource), ErrorMessages.DifferentResource);
            return new ReleaseEvent(this, request);
        }

        public RequestEvent Request()
        {
            return new RequestEvent(this, Default.Priority);
        }

        public RequestEvent Request(double priority)
        {
            return new RequestEvent(this, priority);
        }

        #endregion

        public sealed class ReleaseEvent : ResourceEvent<ReleaseEvent, object>
        {
            readonly RequestEvent _request;

            internal ReleaseEvent(Resource resource, RequestEvent request) : base(resource.Env, Default.Priority)
            {
                _request = request;
                _request.Dispose();
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

            #endregion
        }

        public sealed class RequestEvent : ResourceEvent<RequestEvent, object>
        {
            readonly Resource _resource;

            internal RequestEvent(Resource resource, double priority) : base(resource.Env, priority)
            {
                _resource = resource;
                if (_resource._requestQueue.Count > 0 || !TrySchedule()) {
                    _resource._requestQueue.Add(this, priority);
                }
            }

            internal bool TrySchedule()
            {
                if (_resource._users.Count < _resource.Capacity) {
                    _resource._users.Add(this);
                    Env.ScheduleEvent(this);
                    return true;
                }
                return false;
            }

            #region Public Members

            public Resource Resource
            {
                get { return _resource; }
            }

            public override void Dispose()
            {
                if (Disposed) {
                    return;
                }
                if (Succeeded || !_resource._requestQueue.Contains(this)) {
                    Debug.Assert(_resource._users.Contains(this));
                    _resource._users.Remove(this);
                } else {
                    Debug.Assert(_resource._requestQueue.Contains(this));
                    _resource._requestQueue.Remove(this);
                }
                _resource.BalanceQueues();
                // Marks event as disposed, so that the request event
                // cannot be disposed two or more times.
                Disposed = true;
                Debug.Assert(!_resource._users.Contains(this));
                Debug.Assert(!_resource._requestQueue.Contains(this));
            }

            #endregion

            #region SimEvent Members

            public override object Value
            {
                get { return Default.NoValue; }
            }

            #endregion
        }
    }
}