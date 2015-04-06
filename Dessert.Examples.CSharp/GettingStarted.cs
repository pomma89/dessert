//
// GettingStarted.cs
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

namespace Dessert.Examples.CSharp
{
    using System;
    using Resources;
    using Troschuetz.Random;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public static class MyFirstSimulation
    {
        const int RandomSeed = 42; // The seed of the random numbers generator.
        const int WaitTime = 5; // Minutes, or whatever our "unit of time" is.

        static readonly string[] Names = {"Pino", "Gino", "Nino", "Dino"};

        static SimEvents Person(SimEnvironment env, string name)
        {
            Console.WriteLine("Hi, I'm {0} and I will wait for {1} minutes!", name, WaitTime);
            // The process will stop its execution for WaitTime time units.
            yield return env.Timeout(WaitTime);
            Console.WriteLine("Ok, {0}'s wait has finished at {1}. Bye :)", name, env.Now);
        }

        public static void Run()
        {
            // We create the environment into which
            // our first simulation will run.
            var env = Sim.Environment(RandomSeed);

            // We start two "Person" processes,
            // which will have a random name picked from Names.
            env.Process(Person(env, env.Random.Choice(Names)));
            env.DelayedProcess(Person(env, env.Random.Choice(Names)), delay: 3);

            // After that, we finally start our simulation.
            // We expect an output similar to the following:
            // Hi, I'm Pino and I will wait for 5 minutes!
            // Hi, I'm Dino and I will wait for 5 minutes!
            // Ok, Pino's wait has finished at 5. Bye :)
            // Ok, Dino's wait has finished at 8. Bye :)
            env.Run();
        }
    }

    public static class MySecondSimulation
    {
        const int RandomSeed = 42; // The seed of the random numbers generator.
        const int NeededTime = 4; // Given in minutes, average time needed by a person.
        const int EmployeeCount = 2; // The number of employees the post office has.
        const int SpawnFrequency = 2; // Given in minutes, average time at which people arrive.

        static readonly string[] Names = {"Pino", "Gino", "Nino", "Dino", "Bobb", "John"};
        static readonly TRandom Random = new TRandom(RandomSeed);

        static SimEvents PersonSpawner(SimEnvironment env, Resource postOffice)
        {
            while (true) {
                // We first start a new "Person" process...
                env.Process(Person(env, postOffice, Random.Choice(Names)));
                // And then we sleep for nearly SpawnFrequency minutes.
                var waitTime = Random.Exponential(1.0/SpawnFrequency);
                yield return env.Timeout(waitTime);
            }
        }

        static SimEvents Person(SimEnvironment env, Resource postOffice, string name)
        {
            var arrivedAt = env.Now;
            Console.WriteLine("Hi, I'm {0} and I entered the office at {1:0.00}", name, arrivedAt);
            // The person adds herself to the queue at the office.
            using (var req = postOffice.Request()) {
                yield return req;
                Console.WriteLine("Finally it's {0}'s turn! I waited {1:0.00} minutes", name, env.Now - arrivedAt);
                // If we got here, then it's our turn.
                // Therefore, we simulate the fulfillment
                // of a service with a timeout.
                var waitTime = Random.Exponential(1.0/NeededTime);
                Console.WriteLine("{0}'s job will take {1:0.00} minutes", name, waitTime);
                yield return env.Timeout(waitTime);
            }
            Console.WriteLine("Ok, {0} leaves the office at {1:0.00}. Bye :)", name, env.Now);
        }

        public static void Run()
        {
            // We create the environment into which
            // our first simulation will run.
            var env = Sim.Environment(RandomSeed);

            // The postOffice can be represented as a resource,
            // whose "capacity" is given by the number of employees
            // the simulated post office has.
            var postOffice = Sim.Resource(env, EmployeeCount);

            // We start the "PersonSpawner" processes,
            // which will start new "Person" processes at random intervals.
            env.Process(PersonSpawner(env, postOffice));

            // After that, we finally start our simulation.
            // We expect an output similar to the following:
            // Hi, I'm Pino and I entered the office at 0,00
            // Finally it's Pino's turn! I waited 0,00 minutes
            // Pino's job will take 1,43 minutes
            // Ok, Pino leaves the office at 1,43. Bye :)
            // Hi, I'm Pino and I entered the office at 2,86
            // Finally it's Pino's turn! I waited 0,00 minutes
            // Pino's job will take 1,60 minutes
            // Hi, I'm Bobb and I entered the office at 3,50
            // Finally it's Bobb's turn! I waited 0,00 minutes
            // Bobb's job will take 1,28 minutes
            // Ok, Pino leaves the office at 4,46. Bye :)
            // Ok, Bobb leaves the office at 4,78. Bye :)
            // Hi, I'm Pino and I entered the office at 9,49
            // Finally it's Pino's turn! I waited 0,00 minutes
            // Pino's job will take 5,48 minutes
            // Hi, I'm Dino and I entered the office at 9,67
            // Finally it's Dino's turn! I waited 0,00 minutes
            // Dino's job will take 0,64 minutes
            // Ok, Dino leaves the office at 10,31. Bye :)
            env.Run(until: 20);
        }
    }
}