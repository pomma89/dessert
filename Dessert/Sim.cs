// File name: Sim.cs
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
    using PommaLabs.Thrower;
    using Recording;
    using Resources;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;

    public static partial class Sim
    {
        private static readonly Dictionary<TimeUnit, double> SecondToUnit = new Dictionary<TimeUnit, double> {
            {TimeUnit.Nanosecond, 0.000000001},
            {TimeUnit.Microsecond, 0.000001},
            {TimeUnit.Millisecond, 0.001},
            {TimeUnit.Second, 1},
            {TimeUnit.Minute, 60},
            {TimeUnit.Hour, 3600},
            {TimeUnit.Day, 60*60*24}
        };

        /// <summary>
        ///   Stores a reference to a dummy environment used by unbound instances of <see
        ///   cref="Recording.Monitor"/> and <see cref="Recording.Tally"/>.
        /// </summary>
        private static readonly SimEnvironment DummyEnv = new SimEnvironment(0);

        #region Container Construction

        public static Container Container(SimEnvironment env)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Ensures(ReferenceEquals(Contract.Result<Container>().Env, env));
            Contract.Ensures(Contract.Result<Container>().Capacity.Equals(Default.Capacity));
            Contract.Ensures(Contract.Result<Container>().Level.Equals(Default.Level));
            Contract.Ensures(Contract.Result<Container>().GetPolicy == Default.Policy);
            Contract.Ensures(Contract.Result<Container>().PutPolicy == Default.Policy);
            Contract.Ensures(!Contract.Result<Container>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<Container>().PutQueue.Any());
            return new Container(env, Default.Capacity, Default.Level, Default.Policy, Default.Policy);
        }

        public static Container Container(SimEnvironment env, double capacity)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Ensures(ReferenceEquals(Contract.Result<Container>().Env, env));
            Contract.Ensures(Contract.Result<Container>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<Container>().Level.Equals(Default.Level));
            Contract.Ensures(Contract.Result<Container>().GetPolicy == Default.Policy);
            Contract.Ensures(Contract.Result<Container>().PutPolicy == Default.Policy);
            Contract.Ensures(!Contract.Result<Container>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<Container>().PutQueue.Any());
            return new Container(env, capacity, Default.Level, Default.Policy, Default.Policy);
        }

        public static Container Container(SimEnvironment env, double capacity, double level)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Requires<ArgumentOutOfRangeException>(level >= 0 && level <= capacity);
            Contract.Ensures(ReferenceEquals(Contract.Result<Container>().Env, env));
            Contract.Ensures(Contract.Result<Container>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<Container>().Level.Equals(level));
            Contract.Ensures(Contract.Result<Container>().GetPolicy == Default.Policy);
            Contract.Ensures(Contract.Result<Container>().PutPolicy == Default.Policy);
            Contract.Ensures(!Contract.Result<Container>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<Container>().PutQueue.Any());
            return new Container(env, capacity, level, Default.Policy, Default.Policy);
        }

        public static Container Container(SimEnvironment env, double capacity, double level, WaitPolicy getPolicy,
                                             WaitPolicy putPolicy)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Requires<ArgumentOutOfRangeException>(level >= 0 && level <= capacity);
            Contract.Requires<ArgumentException>(Enum.IsDefined(typeof(WaitPolicy), getPolicy));
            Contract.Requires<ArgumentException>(Enum.IsDefined(typeof(WaitPolicy), putPolicy));
            Contract.Ensures(ReferenceEquals(Contract.Result<Container>().Env, env));
            Contract.Ensures(Contract.Result<Container>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<Container>().Level.Equals(level));
            Contract.Ensures(Contract.Result<Container>().GetPolicy == getPolicy);
            Contract.Ensures(Contract.Result<Container>().PutPolicy == putPolicy);
            Contract.Ensures(!Contract.Result<Container>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<Container>().PutQueue.Any());
            return new Container(env, capacity, level, getPolicy, putPolicy);
        }

        #endregion Container Construction

        #region Environment Construction

        /// <summary>
        ///   Creates a new environment.
        /// </summary>
        /// <returns>A new simulation environment.</returns>
        public static SimEnvironment Environment() => Environment(System.Environment.TickCount);

        /// <summary>
        ///   Creates a new environment with a custom seed.
        /// </summary>
        /// <param name="seed">The seed used by the exposed random generator.</param>
        /// <returns>A new simulation environment.</returns>
        public static SimEnvironment Environment(int seed)
        {
            var env = new SimEnvironment(seed);
            env.RealTime.Locked = true;
            Debug.Assert(env.Now.Equals(0));
            Debug.Assert(env.Random.Seed == seed);
            lock (SuspendInfo)
            {
                SuspendInfo[env] = new Dictionary<SimProcess, SimEvent<object>>();
            }
            return env;
        }

        /// <summary>
        ///   Creates a new real-time environment with the default options.
        /// </summary>
        /// <returns>A new real-time simulation environment.</returns>
        public static SimEnvironment RealTimeEnvironment()
        {
            var env = Environment(System.Environment.TickCount);
            env.RealTime.Enabled = true;
            return env;
        }

        /// <summary>
        ///   Creates a new real-time environment with a custom seed and the default options.
        /// </summary>
        /// <param name="seed">The seed used by the exposed random generator.</param>
        /// <returns>A new real-time simulation environment.</returns>
        public static SimEnvironment RealTimeEnvironment(int seed)
        {
            var env = Environment(seed);
            env.RealTime.Enabled = true;
            return env;
        }

        /// <summary>
        ///   Creates a new real-time environment with custom options.
        /// </summary>
        /// <param name="realTimeOptions">The custom real-time options.</param>
        /// <returns>A new real-time simulation environment.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="realTimeOptions"/> is null, or the specified "wall clock" is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   The specified scaling factor is too small (less than <see cref="SimEnvironment.RealTimeOptions.MinScalingFactor"/>).
        /// </exception>
        public static SimEnvironment RealTimeEnvironment(SimEnvironment.RealTimeOptions realTimeOptions)
        {
            // Preconditions
            RaiseArgumentNullException.IfIsNull(realTimeOptions, nameof(realTimeOptions));
            RaiseArgumentNullException.IfIsNull(realTimeOptions.WallClock, nameof(realTimeOptions.WallClock));
            RaiseArgumentOutOfRangeException.IfIsLessOrEqual(realTimeOptions.ScalingFactor, SimEnvironment.RealTimeOptions.MinScalingFactor, nameof(realTimeOptions.ScalingFactor));

            var env = Environment(System.Environment.TickCount);
            env.RealTime.Enabled = true;
            env.RealTime.Locked = false;
            env.RealTime.WallClock = realTimeOptions.WallClock;
            env.RealTime.ScalingFactor = realTimeOptions.ScalingFactor;
            env.RealTime.Locked = true;
            return env;
        }

        /// <summary>
        ///   Creates a new real-time environment with a custom seed and custom options.
        /// </summary>
        /// <param name="seed">The seed used by the exposed random generator.</param>
        /// <param name="realTimeOptions">The custom real-time options.</param>
        /// <returns>A new real-time simulation environment.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="realTimeOptions"/> is null, or the specified "wall clock" is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   The specified scaling factor is too small (less than <see cref="SimEnvironment.RealTimeOptions.MinScalingFactor"/>).
        /// </exception>
        public static SimEnvironment RealTimeEnvironment(int seed, SimEnvironment.RealTimeOptions realTimeOptions)
        {
            // Preconditions
            RaiseArgumentNullException.IfIsNull(realTimeOptions, nameof(realTimeOptions));
            RaiseArgumentNullException.IfIsNull(realTimeOptions.WallClock, nameof(realTimeOptions.WallClock));
            RaiseArgumentOutOfRangeException.IfIsLessOrEqual(realTimeOptions.ScalingFactor, SimEnvironment.RealTimeOptions.MinScalingFactor, nameof(realTimeOptions.ScalingFactor));

            var env = Environment(seed);
            env.RealTime.Enabled = true;
            env.RealTime.Locked = false;
            env.RealTime.WallClock = realTimeOptions.WallClock;
            env.RealTime.ScalingFactor = realTimeOptions.ScalingFactor;
            env.RealTime.Locked = true;
            return env;
        }

        #endregion Environment Construction

        #region FilterStore Construction

        public static FilterStore<T> FilterStore<T>(SimEnvironment env)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Ensures(ReferenceEquals(Contract.Result<FilterStore<T>>().Env, env));
            Contract.Ensures(Contract.Result<FilterStore<T>>().Capacity.Equals(Default.Capacity));
            Contract.Ensures(Contract.Result<FilterStore<T>>().Count.Equals(0));
            Contract.Ensures(Contract.Result<FilterStore<T>>().GetPolicy == Default.Policy);
            Contract.Ensures(Contract.Result<FilterStore<T>>().PutPolicy == Default.Policy);
            Contract.Ensures(Contract.Result<FilterStore<T>>().ItemPolicy == Default.Policy);
            Contract.Ensures(!Contract.Result<FilterStore<T>>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<FilterStore<T>>().PutQueue.Any());
            Contract.Ensures(!Contract.Result<FilterStore<T>>().ItemQueue.Any());
            return new FilterStore<T>(env, Default.Capacity, Default.Policy, Default.Policy, Default.Policy);
        }

        public static FilterStore<T> FilterStore<T>(SimEnvironment env, int capacity)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Ensures(ReferenceEquals(Contract.Result<FilterStore<T>>().Env, env));
            Contract.Ensures(Contract.Result<FilterStore<T>>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<FilterStore<T>>().Count.Equals(0));
            Contract.Ensures(Contract.Result<FilterStore<T>>().GetPolicy == Default.Policy);
            Contract.Ensures(Contract.Result<FilterStore<T>>().PutPolicy == Default.Policy);
            Contract.Ensures(Contract.Result<FilterStore<T>>().ItemPolicy == Default.Policy);
            Contract.Ensures(!Contract.Result<FilterStore<T>>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<FilterStore<T>>().PutQueue.Any());
            Contract.Ensures(!Contract.Result<FilterStore<T>>().ItemQueue.Any());
            return new FilterStore<T>(env, capacity, Default.Policy, Default.Policy, Default.Policy);
        }

        public static FilterStore<T> FilterStore<T>(SimEnvironment env, int capacity, WaitPolicy getPolicy,
                                                               WaitPolicy putPolicy)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(WaitPolicy), getPolicy));
            Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(WaitPolicy), putPolicy));
            Contract.Ensures(ReferenceEquals(Contract.Result<FilterStore<T>>().Env, env));
            Contract.Ensures(Contract.Result<FilterStore<T>>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<FilterStore<T>>().Count.Equals(0));
            Contract.Ensures(Contract.Result<FilterStore<T>>().GetPolicy == getPolicy);
            Contract.Ensures(Contract.Result<FilterStore<T>>().PutPolicy == putPolicy);
            Contract.Ensures(Contract.Result<FilterStore<T>>().ItemPolicy == Default.Policy);
            Contract.Ensures(!Contract.Result<FilterStore<T>>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<FilterStore<T>>().PutQueue.Any());
            Contract.Ensures(!Contract.Result<FilterStore<T>>().ItemQueue.Any());
            return new FilterStore<T>(env, capacity, getPolicy, putPolicy, Default.Policy);
        }

        public static FilterStore<T> FilterStore<T>(SimEnvironment env, int capacity, WaitPolicy getPolicy,
                                                               WaitPolicy putPolicy, WaitPolicy itemPolicy)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(WaitPolicy), getPolicy));
            Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(WaitPolicy), putPolicy));
            Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(WaitPolicy), itemPolicy));
            Contract.Ensures(ReferenceEquals(Contract.Result<FilterStore<T>>().Env, env));
            Contract.Ensures(Contract.Result<FilterStore<T>>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<FilterStore<T>>().Count.Equals(0));
            Contract.Ensures(Contract.Result<FilterStore<T>>().GetPolicy == getPolicy);
            Contract.Ensures(Contract.Result<FilterStore<T>>().PutPolicy == putPolicy);
            Contract.Ensures(Contract.Result<FilterStore<T>>().ItemPolicy == itemPolicy);
            Contract.Ensures(!Contract.Result<FilterStore<T>>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<FilterStore<T>>().PutQueue.Any());
            Contract.Ensures(!Contract.Result<FilterStore<T>>().ItemQueue.Any());
            return new FilterStore<T>(env, capacity, getPolicy, putPolicy, itemPolicy);
        }

        #endregion FilterStore Construction

        #region Monitor Construction

        public static Monitor Monitor(SimEnvironment env)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            //Debug.Assert(monitor.Env.Equals(env));
            //Debug.Assert(monitor.Name == (name ?? Default.NameFor.Monitor));
            //Debug.Assert(monitor.Count == 0);
            return new Monitor(env);
        }

        /// <summary>
        ///   Returns a new monitor which is not bound to a specific environment.
        /// </summary>
        /// <returns>A new monitor which is not bound to a specific environment.</returns>
        public static Monitor Monitor()
        {
            return new Monitor(DummyEnv);
        }

        #endregion Monitor Construction

        #region PreemptiveResource Construction

        public static PreemptiveResource PreemptiveResource(SimEnvironment env, int capacity)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Ensures(ReferenceEquals(Contract.Result<PreemptiveResource>().Env, env));
            Contract.Ensures(Contract.Result<PreemptiveResource>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<PreemptiveResource>().Count.Equals(0));
            Contract.Ensures(Contract.Result<PreemptiveResource>().RequestPolicy == WaitPolicy.Priority);
            Contract.Ensures(!Contract.Result<PreemptiveResource>().RequestQueue.Any());
            Contract.Ensures(!Contract.Result<PreemptiveResource>().Users.Any());
            return new PreemptiveResource(env, capacity);
        }

        #endregion PreemptiveResource Construction

        #region Resource Construction

        public static Resource Resource(SimEnvironment env, int capacity)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Ensures(ReferenceEquals(Contract.Result<Resource>().Env, env));
            Contract.Ensures(Contract.Result<Resource>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<Resource>().Count.Equals(0));
            Contract.Ensures(Contract.Result<Resource>().RequestPolicy == Default.Policy);
            Contract.Ensures(!Contract.Result<Resource>().RequestQueue.Any());
            Contract.Ensures(!Contract.Result<Resource>().Users.Any());
            return new Resource(env, capacity, Default.Policy);
        }

        public static Resource Resource(SimEnvironment env, int capacity, WaitPolicy requestPolicy)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(WaitPolicy), requestPolicy));
            Contract.Ensures(ReferenceEquals(Contract.Result<Resource>().Env, env));
            Contract.Ensures(Contract.Result<Resource>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<Resource>().Count.Equals(0));
            Contract.Ensures(Contract.Result<Resource>().RequestPolicy == requestPolicy);
            Contract.Ensures(!Contract.Result<Resource>().RequestQueue.Any());
            Contract.Ensures(!Contract.Result<Resource>().Users.Any());
            return new Resource(env, capacity, requestPolicy);
        }

        #endregion Resource Construction

        #region Store Construction

        public static Store<T> Store<T>(SimEnvironment env)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Ensures(ReferenceEquals(Contract.Result<Store<T>>().Env, env));
            Contract.Ensures(Contract.Result<Store<T>>().Capacity.Equals(Default.Capacity));
            Contract.Ensures(Contract.Result<Store<T>>().Count.Equals(0));
            Contract.Ensures(Contract.Result<Store<T>>().GetPolicy == Default.Policy);
            Contract.Ensures(Contract.Result<Store<T>>().PutPolicy == Default.Policy);
            Contract.Ensures(Contract.Result<Store<T>>().ItemPolicy == Default.Policy);
            Contract.Ensures(!Contract.Result<Store<T>>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<Store<T>>().PutQueue.Any());
            Contract.Ensures(!Contract.Result<Store<T>>().ItemQueue.Any());
            return new Store<T>(env, Default.Capacity, Default.Policy, Default.Policy, Default.Policy);
        }

        public static Store<T> Store<T>(SimEnvironment env, int capacity)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Ensures(ReferenceEquals(Contract.Result<Store<T>>().Env, env));
            Contract.Ensures(Contract.Result<Store<T>>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<Store<T>>().Count.Equals(0));
            Contract.Ensures(Contract.Result<Store<T>>().GetPolicy == Default.Policy);
            Contract.Ensures(Contract.Result<Store<T>>().PutPolicy == Default.Policy);
            Contract.Ensures(Contract.Result<Store<T>>().ItemPolicy == Default.Policy);
            Contract.Ensures(!Contract.Result<Store<T>>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<Store<T>>().PutQueue.Any());
            Contract.Ensures(!Contract.Result<Store<T>>().ItemQueue.Any());
            return new Store<T>(env, capacity, Default.Policy, Default.Policy, Default.Policy);
        }

        public static Store<T> Store<T>(SimEnvironment env, int capacity, WaitPolicy getPolicy, WaitPolicy putPolicy)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(WaitPolicy), getPolicy));
            Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(WaitPolicy), putPolicy));
            Contract.Ensures(ReferenceEquals(Contract.Result<Store<T>>().Env, env));
            Contract.Ensures(Contract.Result<Store<T>>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<Store<T>>().Count.Equals(0));
            Contract.Ensures(Contract.Result<Store<T>>().GetPolicy == getPolicy);
            Contract.Ensures(Contract.Result<Store<T>>().PutPolicy == putPolicy);
            Contract.Ensures(Contract.Result<Store<T>>().ItemPolicy == Default.Policy);
            Contract.Ensures(!Contract.Result<Store<T>>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<Store<T>>().PutQueue.Any());
            Contract.Ensures(!Contract.Result<Store<T>>().ItemQueue.Any());
            return new Store<T>(env, capacity, getPolicy, putPolicy, Default.Policy);
        }

        public static Store<T> Store<T>(SimEnvironment env, int capacity, WaitPolicy getPolicy, WaitPolicy putPolicy,
                                           WaitPolicy itemPolicy)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            Contract.Requires<ArgumentOutOfRangeException>(capacity > 0);
            Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(WaitPolicy), getPolicy));
            Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(WaitPolicy), putPolicy));
            Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(WaitPolicy), itemPolicy));
            Contract.Ensures(ReferenceEquals(Contract.Result<Store<T>>().Env, env));
            Contract.Ensures(Contract.Result<Store<T>>().Capacity.Equals(capacity));
            Contract.Ensures(Contract.Result<Store<T>>().Count.Equals(0));
            Contract.Ensures(Contract.Result<Store<T>>().GetPolicy == getPolicy);
            Contract.Ensures(Contract.Result<Store<T>>().PutPolicy == putPolicy);
            Contract.Ensures(Contract.Result<Store<T>>().ItemPolicy == itemPolicy);
            Contract.Ensures(!Contract.Result<Store<T>>().GetQueue.Any());
            Contract.Ensures(!Contract.Result<Store<T>>().PutQueue.Any());
            Contract.Ensures(!Contract.Result<Store<T>>().ItemQueue.Any());
            return new Store<T>(env, capacity, getPolicy, putPolicy, itemPolicy);
        }

        #endregion Store Construction

        #region Tally Construction

        public static Tally Tally(SimEnvironment env)
        {
            Contract.Requires<ArgumentNullException>(env != null);
            //Debug.Assert(tally.Env.Equals(env));
            //Debug.Assert(tally.Name == (name ?? Default.NameFor.Tally));
            //Debug.Assert(tally.Count == 0);
            return new Tally(env);
        }

        /// <summary>
        ///   Returns a new tally which is not bound to a specific environment.
        /// </summary>
        /// <returns>A new tally which is not bound to a specific environment.</returns>
        public static Tally Tally()
        {
            return new Tally(DummyEnv);
        }

        #endregion Tally Construction

        #region Dessert Extensions

        private static readonly IDictionary<SimEnvironment, IDictionary<SimProcess, SimEvent<object>>> SuspendInfo =
            new Dictionary<SimEnvironment, IDictionary<SimProcess, SimEvent<object>>>();

        public static void Resume(this SimProcess process)
        {
            SimEvent<object> suspend;
            if (SuspendInfo[process.Env].TryGetValue(process, out suspend))
            {
                suspend.TrySucceed();
            }
        }

        public static void Resume(this SimProcess process, double delay)
        {
            SimEvent<object> suspend;
            if (SuspendInfo[process.Env].TryGetValue(process, out suspend))
            {
                process.Env.Process(ResumeDelayed(suspend, delay));
            }
        }

        public static SimEvent Suspend(this SimEnvironment env)
        {
            return SuspendInfo[env][env.ActiveProcess] = env.Event<object>();
        }

        internal static void RemoveFromSuspendInfo(SimEnvironment env)
        {
            Debug.Assert(SuspendInfo.ContainsKey(env));
            lock (SuspendInfo)
            {
                SuspendInfo.Remove(env);
            }
        }

        private static IEnumerable<SimEvent> ResumeDelayed(SimEvent<object> suspend, double delay)
        {
            yield return suspend.Env.Timeout(delay);
            suspend.TrySucceed();
        }

        #endregion Dessert Extensions

        #region Time Utilities

        public static TimeUnit CurrentTimeUnit { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Nanoseconds(this double time)
        {
            return ConvertTime(time, TimeUnit.Nanosecond);
        }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Nanoseconds(this int time)
        {
            return ConvertTime(time, TimeUnit.Nanosecond);
        }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Microseconds(this double time)
        {
            return ConvertTime(time, TimeUnit.Microsecond);
        }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Microseconds(this int time)
        {
            return ConvertTime(time, TimeUnit.Microsecond);
        }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Milliseconds(this double time)
        {
            return ConvertTime(time, TimeUnit.Millisecond);
        }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Milliseconds(this int time)
        {
            return ConvertTime(time, TimeUnit.Millisecond);
        }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Seconds(this double time)
        {
            return ConvertTime(time, TimeUnit.Second);
        }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Seconds(this int time)
        {
            return ConvertTime(time, TimeUnit.Second);
        }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Minutes(this double time)
        {
            return ConvertTime(time, TimeUnit.Minute);
        }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Minutes(this int time)
        {
            return ConvertTime(time, TimeUnit.Minute);
        }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Hours(this double time)
        {
            return ConvertTime(time, TimeUnit.Hour);
        }

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>Design inspired by Humanizer library: http://www.mehdi-khalili.com/humanizer-v0-5</remarks>
        public static double Hours(this int time)
        {
            return ConvertTime(time, TimeUnit.Hour);
        }

        private static double ConvertTime(double time, TimeUnit unit)
        {
            return time * (SecondToUnit[unit] / SecondToUnit[CurrentTimeUnit]);
        }

        #endregion Time Utilities
    }

    public enum TimeUnit : byte
    {
        Nanosecond,
        Microsecond,
        Millisecond,
        Second,
        Minute,
        Hour,
        Day
    }

    /// <summary>
    /// </summary>
    public sealed class InterruptUncaughtException : Exception
    {
        internal InterruptUncaughtException() : base(ErrorMessages.InterruptUncaught)
        {
        }
    }
}