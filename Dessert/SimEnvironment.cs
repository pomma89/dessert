//
// SimEnvironment.cs
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

namespace Dessert
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using Core;
    using Events;
    using Troschuetz.Random;
    using Troschuetz.Random.Generators;

    public sealed class SimEnvironment
    {
        internal readonly SimEvent EndEvent;

        /// <summary>
        /// 
        /// </summary>
        readonly OptimizedSkewHeap _events;

        /// <summary>
        ///   Stores an instance of the <see cref="_processes"/> class, which wraps an heap
        ///   and offers methods more specialized for the simulation task. As one can
        ///   easily expect, the agenda is used to schedule processes and events.
        /// </summary>
        readonly OptimizedSkewHeap _processes;

        readonly TRandom _random;

        ulong _highPriority;
        ulong _lowPriority = 1000000UL;

        /// <summary>
        /// 
        /// </summary>
        double _now;

        /// <summary>
        /// 
        /// </summary>
        double _prevNow;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        internal SimEnvironment(int seed)
        {           
            // We add a dummy timeout event and a dummy process, so that heaps are never empty.
            // They have the maximum priority available, since they never have to be called.
            const double maxPr = double.PositiveInfinity;
            const ulong maxVer = ulong.MaxValue;
            var dummyP = new SimProcess(this, DummyProcess()) {At = maxPr, Version = maxVer};
            var dummyEv = new Dummy(this) {At = maxPr, Version = maxVer};

            _processes = new OptimizedSkewHeap(dummyP);
            _events = new OptimizedSkewHeap(dummyEv);
            _random = TRandom.New(new MT19937Generator(seed));
            EndEvent = new Dummy(this);         
        }

        internal SimProcess ScheduleProcess(SimProcess process)
        {
            if (!process.Succeeded && !process.Scheduled) {
                process.At = Now;
                process.Version = _lowPriority++;
                _processes.Add(process);
            }
            return process;
        }

        internal void ScheduleEvent(SimEvent ev)
        {
            Debug.Assert(!ev.Scheduled && !ev.Succeeded && !ev.Failed);
            ev.At = Now;
            ev.Version = _lowPriority++;
            _events.Add(ev);
        }

        internal void ScheduleInterrupt(Interrupt interrupt)
        {
            Debug.Assert(!interrupt.Scheduled && !interrupt.Succeeded && !interrupt.Failed);
            interrupt.At = Now;
            interrupt.Version = _highPriority++;
            _events.Add(interrupt);
        }

        internal void ScheduleTimeout<TVal>(Timeout<TVal> timeout, double delay)
        {
            Debug.Assert(!timeout.Scheduled && !timeout.Succeeded && !timeout.Failed);
            timeout.At = Now + delay;
            timeout.Version = _lowPriority++;
            _events.Add(timeout);
        }

        internal void UnscheduleActiveProcess()
        {
            _processes.RemoveMin();
        }

        void DoSimulate()
        {
            while (!Ended) {
                var minP = _processes.Min;
                var minT = _events.Min;
                if (SimEvent.IsSmaller(minP, minT)) {
                    // Step a process
                    minP.Step();
                } else {
                    // End an event
                    minT.End();
                    _events.RemoveMin();
                }
            }
        }

        void EndSimulation()
        {
            // If there are other events, time has to be adjusted
            // so that final time will be equal to until event time.
            // However, that is true only if following events are scheduled.
            if (_processes.Count == 1 && _events.Count == 2) {
                Now = _prevNow;
            }
            Ended = true;
            Sim.RemoveFromSuspendInfo(this);
        }

        IEnumerator<SimEvent> DummyProcess()
        {
            Ended = true;
            Sim.RemoveFromSuspendInfo(this);
            yield return Exit(null);
        }

        IEnumerable<SimEvent> UntilProcess(SimEvent ev)
        {
            yield return ev;
            EndSimulation();
        }

        [System.Diagnostics.Contracts.Pure]
        public bool IsValidDelay(double delay)
        {
            return delay >= 0 && (Now + delay) <= double.MaxValue;
        }

        #region Process Construction

        public SimProcess Process(IEnumerable<SimEvent> generator)
        {
// ReSharper disable PossibleMultipleEnumeration
            Contract.Requires<ArgumentNullException>(generator != null, ErrorMessages.NullGenerator);
            Contract.Requires<ArgumentNullException>(generator.GetEnumerator() != null, ErrorMessages.NullGenerator);
            Contract.Ensures(Contract.Result<SimProcess>() != null);
            Contract.Ensures(ReferenceEquals(Contract.Result<SimProcess>().Env, this));
            Contract.Ensures(ReferenceEquals(Contract.Result<SimProcess>().Value, null));
            return ScheduleProcess(new SimProcess(this, generator.GetEnumerator()));
// ReSharper restore PossibleMultipleEnumeration
        }

        public SimProcess DelayedProcess(IEnumerable<SimEvent> generator, double delay)
        {
// ReSharper disable PossibleMultipleEnumeration
            Contract.Requires<ArgumentNullException>(generator != null, ErrorMessages.NullGenerator);
            Contract.Requires<ArgumentNullException>(generator.GetEnumerator() != null, ErrorMessages.NullGenerator);
            Contract.Requires<ArgumentOutOfRangeException>(IsValidDelay(delay), ErrorMessages.InvalidDelay);
            Contract.Ensures(Contract.Result<SimProcess>() != null);
            Contract.Ensures(ReferenceEquals(Contract.Result<SimProcess>().Env, this));
            Contract.Ensures(ReferenceEquals(Contract.Result<SimProcess>().Value, null));
            return ScheduleProcess(new SimProcess(this, DelayedProcessWrapper(generator.GetEnumerator(), delay)));
// ReSharper restore PossibleMultipleEnumeration
        }

        IEnumerator<SimEvent> DelayedProcessWrapper(IEnumerator<SimEvent> realGenerator, double delay)
        {
            yield return new Timeout<double>(this, delay, delay);
            yield return new Call<object>(this, realGenerator);
        }

        #endregion

        #region Run Overloads

        public void Run()
        {
            Contract.Requires<InvalidOperationException>(!Ended);
            Contract.Requires<InvalidOperationException>(!double.IsPositiveInfinity(Peek));
            Contract.Ensures(Ended);

            this.Timeout(double.MaxValue).Callbacks.Add(e => EndSimulation());
            DoSimulate();
        }

        public void Run(double until)
        {
            Contract.Requires<InvalidOperationException>(!Ended);
            Contract.Requires<InvalidOperationException>(!double.IsPositiveInfinity(Peek));
            Contract.Requires<ArgumentOutOfRangeException>(IsValidDelay(until), ErrorMessages.InvalidDelay);
            Contract.Ensures(Ended);

            this.Timeout(until).Callbacks.Add(e => EndSimulation());
            DoSimulate();
        }

        public void Run(int until)
        {
            Contract.Requires<InvalidOperationException>(!Ended);
            Contract.Requires<InvalidOperationException>(!double.IsPositiveInfinity(Peek));
            Contract.Requires<ArgumentOutOfRangeException>(IsValidDelay(until), ErrorMessages.InvalidDelay);
            Contract.Ensures(Ended);

            this.Timeout(until).Callbacks.Add(e => EndSimulation());
            DoSimulate();
        }

        public void Run(SimEvent until)
        {
            Contract.Requires<InvalidOperationException>(!Ended);
            Contract.Requires<InvalidOperationException>(!double.IsPositiveInfinity(Peek));
            Contract.Requires<ArgumentNullException>(until != null, ErrorMessages.NullEvent);
            Contract.Requires<ArgumentException>(ReferenceEquals(this, until.Env), ErrorMessages.DifferentEnvironment);
            Contract.Requires<ArgumentException>(!until.Failed);
            Contract.Ensures(Ended);

            // TODO Fix this, since when until is triggered UntilProcess is not immediately triggered.
            Process(UntilProcess(until));
            DoSimulate();
        }

        #endregion

        #region Timeout Construction

        #endregion

        #region Object Members

        public override string ToString()
        {
            return string.Format("[Now: {0}]", Now);
        }

        #endregion

        #region IEnvironment Members

        /// <summary>
        ///   The process that is currently running in the simulation.
        /// </summary>
        [Pure]
        public SimProcess ActiveProcess
        {
            get
            {
                Debug.Assert(_processes.Min is SimProcess);
                return _processes.Min as SimProcess;
            }
        }

        [System.Diagnostics.Contracts.Pure]
        public bool Ended { get; private set; }

        /// <summary>
        ///   Returns current simulation time.
        /// </summary>
        /// <returns>Current simulation time.</returns>
        [System.Diagnostics.Contracts.Pure]
        public double Now
        {
            get { return _now; }
            internal set
            {
                _prevNow = _now;
                _now = value;
            }
        }

        /// <summary>
        ///   Returns the time of the next scheduled event, or <see cref="double.PositiveInfinity"/> 
        ///   if there is no further event.
        /// </summary>
        /// <returns>
        ///   The time of the next scheduled event, or <see cref="double.PositiveInfinity"/> 
        ///   if there is no further event.
        /// </returns>
        [System.Diagnostics.Contracts.Pure]
        public double Peek
        {
            get
            {
                var minP = _processes.Min;
                var minT = _events.Min;
                return SimEvent.IsSmaller(minP, minT) ? minP.At : minT.At;
            }
        }

        /// <summary>
        ///   A random numbers generator which can be used inside simulations.
        /// </summary>
        [Pure]
        public TRandom Random
        {
            get { return _random; }
        }

        #region Event Construction

        /// <summary>
        ///   Returns a new generic event.
        /// </summary>
        /// <returns>A new generic event.</returns>
        public SimEvent<object> Event()
        {
            Contract.Ensures(Contract.Result<SimEvent<object>>() != null);
            Contract.Ensures(ReferenceEquals(Contract.Result<SimEvent<object>>().Env, this));
            Contract.Ensures(ReferenceEquals(Contract.Result<SimEvent<object>>().Value, null));
            return new SimEvent<object>(this);
        }

        /// <summary>
        ///   Returns a new generic event.
        /// </summary>
        /// <returns>A new generic event.</returns>
        public SimEvent<TVal> Event<TVal>()
        {
            Contract.Ensures(Contract.Result<SimEvent<TVal>>() != null);
            Contract.Ensures(ReferenceEquals(Contract.Result<SimEvent<TVal>>().Env, this));
            Contract.Ensures(ReferenceEquals(Contract.Result<SimEvent<TVal>>().Value, null) ||
                             Contract.Result<SimEvent<TVal>>().Value.Equals(default(TVal)));
            return new SimEvent<TVal>(this);
        }

        #endregion

        #region Exit Construction

        /// <summary>
        ///   Exits from current process or from current call. If called directly from
        ///   a process body, then the process is stopped and the optional exit value
        ///   can be found on <see cref="SimProcess.Value"/>. Otherwise, if this method
        ///   is called from a procedure body, then the procedure is stopped.
        /// </summary>
        /// <returns>The exit event that can be yielded to stop a process or a call.</returns>
        public SimEvent Exit()
        {
            ActiveProcess.SetExitValue(Default.Value);
            return EndEvent;
        }

        /// <summary>
        ///   Exits from current process or from current call. If called directly from
        ///   a process body, then the process is stopped and the optional exit value
        ///   can be found on <see cref="SimProcess.Value"/>. Otherwise, if this method
        ///   is called from a procedure body, then the procedure is stopped and the
        ///   optional exit value can be found on the event returned by <see cref="Call"/>.
        /// </summary>
        /// <param name="value">The optional exit value.</param>
        /// <returns>The exit event that can be yielded to stop a process or a call.</returns>
        public SimEvent Exit(object value)
        {
            ActiveProcess.SetExitValue(value);
            return EndEvent;
        }

        #endregion

        #endregion

        sealed class Dummy : SimEvent<Dummy, object>
        {
            internal Dummy(SimEnvironment env) : base(env)
            {
            }

            #region SimEvent Members

            public override object Value
            {
                get { return null; /* IronPython requires this to be null. */ }
            }

            #endregion
        }
    }

    // Call creator
    public static partial class Sim
    {
        /// <summary>
        ///   Returns a new call event.
        /// </summary>
        /// <returns>A new call event.</returns>
        [Pure]
        public static Call Call(this SimEnvironment env, IEnumerable<SimEvent> gen)
        {
// ReSharper disable PossibleMultipleEnumeration
            Contract.Requires<ArgumentNullException>(env != null, ErrorMessages.NullEnvironment);
            Contract.Requires<ArgumentNullException>(gen != null && gen.GetEnumerator() != null, ErrorMessages.NullGenerator);
            Contract.Ensures(Contract.Result<Call>() != null);
            return new Call(env, gen.GetEnumerator());
// ReSharper restore PossibleMultipleEnumeration
        }

        /// <summary>
        ///   Returns a new call event.
        /// </summary>
        /// <returns>A new call event.</returns>
        [Pure]
        public static Call<T> Call<T>(this SimEnvironment env, IEnumerable<SimEvent> gen)
        {
            // ReSharper disable PossibleMultipleEnumeration
            Contract.Requires<ArgumentNullException>(env != null, ErrorMessages.NullEnvironment);
            Contract.Requires<ArgumentNullException>(gen != null && gen.GetEnumerator() != null, ErrorMessages.NullGenerator);
            Contract.Ensures(Contract.Result<Call<T>>() != null);
            return new Call<T>(env, gen.GetEnumerator());
            // ReSharper restore PossibleMultipleEnumeration
        }       
    }

    // Timeout creator
    public static partial class Sim
    {
        [Pure]
        public static Timeout Timeout(this SimEnvironment env, double delay)
        {
            Contract.Requires<ArgumentNullException>(env != null, ErrorMessages.NullEnvironment);
            Contract.Requires<ArgumentOutOfRangeException>(env.IsValidDelay(delay), ErrorMessages.InvalidDelay);
            Contract.Ensures(Contract.Result<Timeout>() != null);
            Contract.Ensures(ReferenceEquals(Contract.Result<Timeout>().Env, env));
            Contract.Ensures(Contract.Result<Timeout>().Value.Equals(delay));
            Contract.Ensures(Contract.Result<Timeout>().Delay.Equals(delay));
            return new Timeout(env, delay);
        }

        [Pure]
        public static Timeout<T> Timeout<T>(this SimEnvironment env, double delay, T value)
        {
            Contract.Requires<ArgumentNullException>(env != null, ErrorMessages.NullEnvironment);
            Contract.Requires<ArgumentOutOfRangeException>(env.IsValidDelay(delay), ErrorMessages.InvalidDelay);
            Contract.Ensures(Contract.Result<Timeout<T>>() != null);
            Contract.Ensures(ReferenceEquals(Contract.Result<Timeout<T>>().Env, env));
            Contract.Ensures(Contract.Result<Timeout<T>>().Value.Equals(value));
            Contract.Ensures(Contract.Result<Timeout<T>>().Delay.Equals(delay));
            return new Timeout<T>(env, delay, value);
        }
    }
}