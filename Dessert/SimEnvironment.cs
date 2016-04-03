// File name: SimEnvironment.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
//
// Copyright (c) 2012-2016 Alessio Parma <alessio.parma@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace DIBRIS.Dessert
{
    using Core;
    using Events;
    using Finsa.CodeServices.Clock;
    using PommaLabs.Thrower;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using Troschuetz.Random;
    using Troschuetz.Random.Generators;

    public sealed class SimEnvironment
    {
        internal readonly SimEvent EndEvent;

        /// <summary>
        /// </summary>
        private readonly OptimizedSkewHeap _events;

        /// <summary>
        ///   Stores an instance of the <see cref="_processes"/> class, which wraps an heap and
        ///   offers methods more specialized for the simulation task. As one can easily expect, the
        ///   agenda is used to schedule processes and events.
        /// </summary>
        private readonly OptimizedSkewHeap _processes;

        private readonly TRandom _random;

        private ulong _highPriority;
        private ulong _lowPriority = 1000000UL;

        /// <summary>
        /// </summary>
        private double _now;

        /// <summary>
        /// </summary>
        private double _prevNow;

        /// <summary>
        /// </summary>
        /// <param name="seed"></param>
        internal SimEnvironment(int seed)
        {
            // We add a dummy timeout event and a dummy process, so that heaps are never empty. They
            // have the maximum priority available, since they never have to be called.
            const double maxPr = double.PositiveInfinity;
            const ulong maxVer = ulong.MaxValue;
            var dummyP = new SimProcess(this, DummyProcess()) { At = maxPr, Version = maxVer };
            var dummyEv = new Dummy(this) { At = maxPr, Version = maxVer };

            _processes = new OptimizedSkewHeap(dummyP);
            _events = new OptimizedSkewHeap(dummyEv);
            _random = TRandom.New(new MT19937Generator(seed));
            EndEvent = new Dummy(this);
        }

        internal SimProcess ScheduleProcess(SimProcess process)
        {
            if (!process.Succeeded && !process.Scheduled)
            {
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

            // Real-time management.
            if (RealTime.Enabled)
            {
                timeout.AtWallClock = RealTime.CurrentUnixTime + (delay * RealTime.ScalingFactor);
            }
        }

        internal void UnscheduleActiveProcess()
        {
            _processes.RemoveMin();
        }

        private void DoSimulate()
        {
            // Real-time management.
            if (RealTime.Enabled)
            {
                // Set the base UNIX time, used to computed the "wall clock" time of timeout events.
                RealTime.SetCurrentUnixTime();
            }

            while (!Ended)
            {
                var minP = _processes.Min;
                var minT = _events.Min;
                if (SimEvent.IsSmaller(minP, minT))
                {
                    // Step a process
                    minP.Step();
                }
                else {
                    // End an event
                    minT.End();
                    _events.RemoveMin();
                }
            }
        }

        private void EndSimulation()
        {
            // If there are other events, time has to be adjusted so that final time will be equal to
            // until event time. However, that is true only if following events are scheduled.
            if (_processes.Count == 1 && _events.Count == 2)
            {
                _now = _prevNow;
            }
            Ended = true;
            Sim.RemoveFromSuspendInfo(this);
        }

        private IEnumerator<SimEvent> DummyProcess()
        {
            Ended = true;
            Sim.RemoveFromSuspendInfo(this);
            yield return Exit(null);
        }

        private IEnumerable<SimEvent> UntilProcess(SimEvent ev)
        {
            yield return ev;
            EndSimulation();
        }

        [Pure]
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

        private IEnumerator<SimEvent> DelayedProcessWrapper(IEnumerator<SimEvent> realGenerator, double delay)
        {
            yield return new Timeout<double>(this, delay, delay);
            yield return new Call<object>(this, realGenerator);
        }

        #endregion Process Construction

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

        #endregion Run Overloads

        #region Object Members

        public override string ToString()
        {
            return string.Format("[Now: {0}]", Now);
        }

        #endregion Object Members

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

        [Pure]
        public bool Ended { get; private set; }

        /// <summary>
        ///   Returns current simulation time.
        /// </summary>
        /// <returns>Current simulation time.</returns>
        [Pure]
        public double Now => _now;

        /// <summary>
        ///   Sets the simulation time.
        /// </summary>
        /// <param name="nextNow">The new simulation time.</param>
        /// <param name="nextWallClock">The new wall clock, used in real-time mode.</param>
        internal void SetNow(double nextNow, double nextWallClock)
        {
            _prevNow = _now;
            _now = nextNow;

            // Real-time management.
            if (RealTime.Enabled)
            {
                double delay;
                if (nextNow < double.MaxValue && (delay = (nextWallClock - RealTime.ScaledUnixTime)) > 0.0)
                {
                    // "delay" is measured in seconds, it must be converted into milliseconds.
#if NET40
                    System.Threading.Thread.Sleep((int) (delay * 1000.0));
#else
                    System.Threading.Tasks.Task.Delay((int) (delay * 1000.0)).Wait();
#endif
                }

                // Update the base UNIX time after having waited.
                RealTime.SetCurrentUnixTime();
            }
        }

        /// <summary>
        ///   Returns the time of the next scheduled event, or <see cref="double.PositiveInfinity"/>
        ///   if there is no further event.
        /// </summary>
        /// <returns>
        ///   The time of the next scheduled event, or <see cref="double.PositiveInfinity"/> if there
        ///   is no further event.
        /// </returns>
        [Pure]
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
        public TRandom Random => _random;

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

        #endregion Event Construction

        #region Exit Construction

        /// <summary>
        ///   Exits from current process or from current call. If called directly from a process
        ///   body, then the process is stopped and the optional exit value can be found on <see
        ///   cref="SimProcess.Value"/>. Otherwise, if this method is called from a procedure body,
        ///   then the procedure is stopped.
        /// </summary>
        /// <returns>The exit event that can be yielded to stop a process or a call.</returns>
        public SimEvent Exit()
        {
            ActiveProcess.SetExitValue(Default.Value);
            return EndEvent;
        }

        /// <summary>
        ///   Exits from current process or from current call. If called directly from a process
        ///   body, then the process is stopped and the optional exit value can be found on <see
        ///   cref="SimProcess.Value"/>. Otherwise, if this method is called from a procedure body,
        ///   then the procedure is stopped and the optional exit value can be found on the event
        ///   returned by <see cref="Call"/>.
        /// </summary>
        /// <param name="value">The optional exit value.</param>
        /// <returns>The exit event that can be yielded to stop a process or a call.</returns>
        public SimEvent Exit(object value)
        {
            ActiveProcess.SetExitValue(value);
            return EndEvent;
        }

        #endregion Exit Construction

        #endregion IEnvironment Members

        #region Real-time

        /// <summary>
        ///   Options for the real-time mode.
        /// </summary>
        public RealTimeOptions RealTime { get; } = new RealTimeOptions();

        /// <summary>
        ///   Available options for the real-time mode.
        /// </summary>
        public sealed class RealTimeOptions
        {
            private double _scalingFactor = DefaultScalingFactor;
            private bool _scalingFactorSet;
            private IClock _wallClock = DefaultWallClock;
            private bool _wallClockSet;

            /// <summary>
            ///   The minimum value which can be assigned to the scaling factor.
            /// </summary>
            public static double MinScalingFactor { get; } = 0.01;

            /// <summary>
            ///   The default scaling factor.
            /// </summary>
            public static double DefaultScalingFactor { get; } = 1.0;

            /// <summary>
            ///   The default "wall clock" instance.
            /// </summary>
            public static IClock DefaultWallClock { get; } = new SystemClock();

            /// <summary>
            ///   Whether the simulation must be run according to "wall clock" time. Default value is <c>false</c>.
            ///
            ///   This flag is automatically set to <c>true</c> when a real-time environment is
            ///   created through <see cref="Sim"/> static factories, such as <see
            ///   cref="Sim.RealTimeEnvironment(RealTimeOptions)"/> and <see
            ///   cref="Sim.RealTimeEnvironment(int, RealTimeOptions)"/>.
            /// </summary>
            [Pure]
            public bool Enabled { get; internal set; } = false;

            /// <summary>
            ///   The real-time scaling factor. Default value is <c>1.0</c>.
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">
            ///   The specified scaling factor is too small (less than <see cref="MinScalingFactor"/>).
            /// </exception>
            /// <exception cref="InvalidOperationException">
            ///   Scaling factor has already been set, it cannot be overwritten.
            /// </exception>
            [Pure]
            public double ScalingFactor
            {
                get
                {
                    var result = _scalingFactor;

                    // Postconditions
                    Debug.Assert(result >= MinScalingFactor);
                    return result;
                }
                set
                {
                    // Preconditions
                    RaiseArgumentOutOfRangeException.IfIsLessOrEqual(value, MinScalingFactor, nameof(value));
                    RaiseInvalidOperationException.If(_scalingFactorSet, ErrorMessages.ScalingFactorNotUpdatable);

                    _scalingFactor = value;
                    _scalingFactorSet = true;
                }
            }

            /// <summary>
            ///   The "wall clock" used for the real-time simulation. Default instance is <see cref="SystemClock"/>.
            /// </summary>
            /// <exception cref="ArgumentNullException">The specified "wall clock" is null.</exception>
            /// <exception cref="InvalidOperationException">
            ///   "Wall clock" has already been set, it cannot be overwritten.
            /// </exception>
            [Pure]
            public IClock WallClock
            {
                get
                {
                    var result = _wallClock;

                    // Postconditions
                    Debug.Assert(!ReferenceEquals(result, null));
                    return result;
                }
                set
                {
                    // Preconditions
                    RaiseArgumentNullException.IfIsNull(value, nameof(value));
                    RaiseInvalidOperationException.If(_wallClockSet, ErrorMessages.WallClockNotUpdatable);

                    _wallClock = value;
                    _wallClockSet = true;
                }
            }

            /// <summary>
            ///   Returns the <see cref="WallClock"/> UNIX time scaled by the specified <see cref="ScalingFactor"/>.
            /// </summary>
            [Pure]
            internal double ScaledUnixTime => WallClock.UnixTime * ScalingFactor;

            /// <summary>
            ///   The current UNIX time, written by the most recent timeout event.
            /// </summary>
            internal double CurrentUnixTime { get; private set; }

            /// <summary>
            ///   Sets the current UNIX time, written by the most recent timeout event.
            /// </summary>
            internal void SetCurrentUnixTime() => CurrentUnixTime = ScaledUnixTime;
        }

        #endregion Real-time

        private sealed class Dummy : SimEvent<Dummy, object>
        {
            internal Dummy(SimEnvironment env) : base(env)
            {
            }

            #region SimEvent Members

            public override object Value
            {
                get { return null; /* IronPython requires this to be null. */ }
            }

            #endregion SimEvent Members
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