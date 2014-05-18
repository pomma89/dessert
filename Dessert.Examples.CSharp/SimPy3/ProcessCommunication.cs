//
// ProcessCommunication.cs
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

#region Original SimPy3 Example

//"""
//Process communication example

//Covers:

//- Resources: Store

//Scenario:
//  This example shows how to interconnect simulation model elements
//  together using "resources.Store" for one-to-one, and many-to-one
//  asynchronous processes. For one-to-many a simple BroadCastPipe class
//  is constructed from Store.

//When Useful:
//  When a consumer process does not always wait on a generating process
//  and these processes run asynchronously. This example shows how to
//  create a buffer and also tell is the consumer process was late
//  yielding to the event from a generating process.

//  This is also useful when some information needs to be broadcast to
//  many receiving processes

//  Finally, using pipes can simplify how processes are interconnected to
//  each other in a simulation model.

//Example By:
//  Keith Smith

//"""
//import random

//import simpy
//import simpy.util

//RANDOM_SEED = 42
//SIM_TIME = 100

//class BroadcastPipe(object):
//    """A Broadcast pipe that allows one process to send messages to many.

//    This construct is useful when message consumers are running at
//    different rates than message generators and provides an event
//    buffering to the consuming processes.

//    The parameters are used to create a new
//    :class:`~simpy.resources.Store` instance each time
//    :meth:`get_output_conn()` is called.

//    """
//    def __init__(self, env, capacity=simpy.core.Infinity,
//                 item_q_type=simpy.resources.queues.FIFO,
//                 event_type=simpy.resources.events.StoreEvent):
//        self.env = env
//        self.capacity = capacity
//        self.item_q_type = item_q_type
//        self.event_type = event_type
//        self.pipes = []

//    def put(self, value):
//        """Broadcast a *value* to all receivers."""
//        if not self.pipes:
//            raise RuntimeError('There are no output pipes.')
//        events = [store.put(value) for store in self.pipes]
//        return simpy.util.all_of(events)  # Condition event for all "events"

//    def get_output_conn(self):
//        """Get a new output connection for this broadcast pipe.

//        The return value is a :class:`~simpy.resources.Store`.

//        """
//        pipe = simpy.Store(self.env, capacity=self.capacity,
//                item_q_type=self.item_q_type, event_type=self.event_type)
//        self.pipes.append(pipe)
//        return pipe

//def message_generator(name, env, out_pipe):
//    """A process which randomly generates messages."""
//    while True:
//        # wait for next transmission
//        yield env.timeout(random.randint(6, 10))

//        # messages are time stamped to later check if the consumer was
//        # late getting them.  Note, using event.triggered to do this may
//        # result in failure due to FIFO nature of simulation yields.
//        # (i.e. if at the same env.now, message_generator puts a message
//        # in the pipe first and then message_consumer gets from pipe,
//        # the event.triggered will be True in the other order it will be
//        # False
//        msg = (env.now, '%s says hello at %d' % (name, env.now))
//        out_pipe.put(msg)

//def message_consumer(name, env, in_pipe):
//    """A process which consumes messages."""
//    while True:
//        # Get event for message pipe
//        msg = yield in_pipe.get()

//        if msg[0] < env.now:
//            # if message was already put into pipe, then
//            # message_consumer was late getting to it. Depending on what
//            # is being modeled this, may, or may not have some
//            # significance
//            print('LATE Getting Message: at time %d: %s received message: %s' %
//                    (env.now, name, msg[1]))

//        else:
//            # message_consumer is synchronized with message_generator
//            print('at time %d: %s received message: %s.' %
//                    (env.now, name, msg[1]))

//        # Process does some other work, which may result in missing messages
//        yield env.timeout(random.randint(4, 8))

//# Setup and start the simulation
//print('Process communication')
//random.seed(RANDOM_SEED)
//env = simpy.Environment()

//# For one-to-one or many-to-one type pipes, use resource.Store
//pipe = simpy.Store(env)
//env.start(message_generator('Generator A', env, pipe))
//env.start(message_consumer('Consumer A', env, pipe))

//print('\nOne-to-one pipe communication\n')
//simpy.simulate(env, until=SIM_TIME)

//# For one-to many use BroadcastPipe
//# (Note: could also be used for one-to-one,many-to-one or many-to-many)
//env = simpy.Environment()
//bc_pipe = BroadcastPipe(env)

//env.start(message_generator('Generator A', env, bc_pipe))
//env.start(message_consumer('Consumer A', env, bc_pipe.get_output_conn()))
//env.start(message_consumer('Consumer B', env, bc_pipe.get_output_conn()))

//print('\nOne-to-many pipe communication\n')
//simpy.simulate(env, until=SIM_TIME)

#endregion

namespace Dessert.Examples.CSharp.SimPy3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Resources;
    using Troschuetz.Random;
    using IEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public static class ProcessCommunication
    {
        const int RandomSeed = 42;
        const int SimTime = 100;

        static readonly TRandom Rand = new TRandom(RandomSeed);

        /// <summary>
        ///   A process which randomly generates messages.
        /// </summary>
        static IEvents MessageGenerator(string name, SimEnvironment env, Store<Message> outPipe)
        {
            while (true) {
                // Waits for next transmission.
                yield return env.Timeout(Rand.Next(6, 10));
                // Messages are time stamped to later check
                // if the consumer was late getting them.
                var content = string.Format("{0} says hello at {1:.0}", name, env.Now);
                yield return outPipe.Put(new Message(env.Now, content));
            }
        }

        /// <summary>
        ///   A process which randomly generates messages.
        /// </summary>
        static IEvents MessageGenerator(string name, SimEnvironment env, BroadcastPipe<Message> outPipe)
        {
            while (true) {
                // Waits for next transmission.
                yield return env.Timeout(Rand.Next(6, 10));
                // Messages are time stamped to later check
                // if the consumer was late getting them.
                var content = string.Format("{0} says hello at {1:.0}", name, env.Now);
                yield return outPipe.Put(new Message(env.Now, content));
            }
        }

        /// <summary>
        ///   A process which consumes messages.
        /// </summary>
        static IEvents MessageConsumer(string name, SimEnvironment env, Store<Message> inPipe)
        {
            while (true) {
                // Gets event for message pipe.
                var getEv = inPipe.Get();
                yield return getEv;
                var msg = getEv.Value;

                if (msg.Timestamp < env.Now) {
                    // If message was already put into pipe, then
                    // MessageConsumer was late getting to it.
                    // Depending on what is being modeled, this may, or may not,
                    // have some significance.
                    Console.WriteLine("LATE Getting Message: at time {0:.0}, \"{1}\" received message \"{2}\"", env.Now,
                                      name, msg.Content);
                } else {
                    // MessageConsumer is synchronized with MessageGenerator.
                    Console.WriteLine("At time {0:.0}, \"{1}\" received message \"{2}\"", env.Now, name, msg.Content);
                }

                // Process does some other work, which may result in missing messages.
                yield return env.Timeout(Rand.Next(4, 8));
            }
        }

        public static void Run()
        {
            // Sets up and starts the simulation.
            Console.WriteLine("Process communication");
            var env = Sim.NewEnvironment();

            // For one-to-one or many-to-one type pipes, use Resources.Store.
            var pipe = Sim.NewStore<Message>(env);
            env.Process(MessageGenerator("Generator A", env, pipe));
            env.Process(MessageConsumer("Consumer A", env, pipe));

            Console.WriteLine();
            Console.WriteLine("One-to-one pipe communication");
            Console.WriteLine();
            env.Run(until: SimTime);

            // For one-to many, instead, use BroadcastPipe.
            // (Note: it could also be used for one-to-one, many-to-one or many-to-many)
            env = Sim.NewEnvironment();
            var bcPipe = new BroadcastPipe<Message>(env);

            env.Process(MessageGenerator("Generator A", env, bcPipe));
            env.Process(MessageConsumer("Consumer A", env, bcPipe.OutputConn()));
            env.Process(MessageConsumer("Consumer B", env, bcPipe.OutputConn()));

            Console.WriteLine();
            Console.WriteLine("One-to-many pipe communication");
            Console.WriteLine();
            env.Run(until: SimTime);
        }

        /// <summary>
        ///   A Broadcast pipe that allows one process to send messages to many.
        /// 
        ///   This construct is useful when message consumers are running at
        ///   different rates than message generators and provides an event
        ///   buffering to the consuming processes.
        ///
        ///   The parameters are used to create a new <see cref="Store{TItem}"/>
        ///   instance each time <see cref="OutputConn"/> is called.
        /// </summary>
        sealed class BroadcastPipe<TItem>
        {
            readonly int _capacity;
            readonly SimEnvironment _env;
            readonly IList<Store<TItem>> _pipes = new List<Store<TItem>>();

            public BroadcastPipe(SimEnvironment env, int capacity = int.MaxValue)
            {
                _env = env;
                _capacity = capacity;
            }

            /// <summary>
            ///   Broadcasts a <paramref name="value"/> to all receivers.
            /// </summary>
            public SimEvent Put(TItem value)
            {
                if (_pipes.Count == 0) {
                    throw new InvalidOperationException("There are no output pipes.");
                }
                var events = _pipes.Select(p => p.Put(value)).ToArray();
                switch (events.Length) {
                    case 1:
                        return _env.AllOf(events[0]);
                    case 2:
                        return _env.AllOf(events[0], events[1]);
                    case 3:
                        return _env.AllOf(events[0], events[1], events[2]);
                    case 4:
                        return _env.AllOf(events[0], events[1], events[2], events[3]);
                    case 5:
                        return _env.AllOf(events[0], events[1], events[2], events[3], events[4]);
                }
                throw new Exception("Too many pipes!");
            }

            /// <summary>
            ///   Get a new output connection for this broadcast pipe.
            /// </summary>
            public Store<TItem> OutputConn()
            {
                var pipe = Sim.NewStore<TItem>(_env, _capacity);
                _pipes.Add(pipe);
                return pipe;
            }
        }

        struct Message
        {
            public readonly string Content;
            public readonly double Timestamp;

            public Message(double timestamp, string content)
            {
                Timestamp = timestamp;
                Content = content;
            }
        }
    }
}