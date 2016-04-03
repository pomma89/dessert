// File name: GasStation.cs
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

namespace DIBRIS.Dessert.Examples.CSharp.SimPy3
{
    using Resources;
    using System;
    using System.Linq;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public static class GasStation
    {
        private const int RandomSeed = 42;
        private const int GasStationSize = 200; // Liters.
        private const int Threshold = 20; // Threshold for calling the tank truck (in %).
        private const int FuelTankSize = 50; // Liters.
        private const int MinFuelTankLevel = 5; // Minimum level of fuel tanks (in liters).
        private const int MaxFuelTankLevel = 25; // Maximum level of fuel tanks (in liters).
        private const int RefuelingSpeed = 2; // Liters / Second.
        private const int TankTruckTime = 300; // Seconds it takes the tank truck to arrive.
        private const int MinInter = 30; // Minimum car creation time, in seconds.
        private const int MaxInter = 300; // Maximum car creation time, in seconds.
        private const int SimTime = 2000; // Simulation time in seconds.

        /// <summary>
        ///   A car arrives at the gas station for refueling. It requests one of the gas station's
        ///   fuel pumps and tries to get the desired amount of gas from it. If the stations
        ///   reservoir is depleted, the car has to wait for the tank truck to arrive.
        /// </summary>
        private static SimEvents Car(string name, SimEnvironment env, Resource gasStation, Container fuelPump)
        {
            var fuelTankLevel = env.Random.Next(MinFuelTankLevel, MaxFuelTankLevel);
            Console.WriteLine("{0} arriving at gas station at {1:.0}", name, env.Now);
            using (var req = gasStation.Request())
            {
                var start = env.Now;

                // Requests one of the gas pumps.
                yield return req;

                // Gets the required amount of fuel.
                var litersRequired = FuelTankSize - fuelTankLevel;
                yield return fuelPump.Get(litersRequired);

                // The "actual" refueling process takes some time
                yield return env.Timeout(litersRequired / (double) RefuelingSpeed);
                Console.WriteLine("{0} finished refueling in {1:.0} seconds.", name, env.Now - start);
            }
        }

        /// <summary>
        ///   Periodically checks the level of the <see cref="fuelPump"/> and calls the tank truck if
        ///   the level falls below a threshold.
        /// </summary>
        private static SimEvents GasStationControl(SimEnvironment env, Container fuelPump)
        {
            while (true)
            {
                if (fuelPump.Level / fuelPump.Capacity * 100 < Threshold)
                {
                    // We need to call the tank truck now!
                    Console.WriteLine("Calling tank truck at {0}", env.Now);

                    // Waits for the tank truck to arrive and refuel the station.
                    yield return env.Process(TankTruck(env, fuelPump));
                }
                yield return env.Timeout(10); // Checks every 10 seconds.
            }
        }

        /// <summary>
        ///   Arrives at the gas station after a certain delay and refuels it.
        /// </summary>
        private static SimEvents TankTruck(SimEnvironment env, Container fuelPump)
        {
            yield return env.Timeout(TankTruckTime);
            Console.WriteLine("Tank truck arriving at time {0}", env.Now);

            var amount = fuelPump.Capacity - fuelPump.Level;
            Console.WriteLine("Tank truck refueling {0:.0} liters.", amount);
            yield return fuelPump.Put(amount);
        }

        /// <summary>
        ///   Generates new cars that arrive at the gas station.
        /// </summary>
        private static SimEvents CarGenerator(SimEnvironment env, Resource gasStation, Container fuelPump)
        {
            foreach (var i in Enumerable.Range(0, int.MaxValue))
            {
                yield return env.Timeout(env.Random.Next(MinInter, MaxInter));
                env.Process(Car("Car " + i, env, gasStation, fuelPump));
            }
        }

        /// <summary>
        ///   Sets up and starts the simulation.
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("Gas Station refueling");

            // Creates the environment and starts processes.
            var env = Sim.Environment(RandomSeed);
            var gasStation = Sim.Resource(env, 2);
            var fuelPump = Sim.Container(env, GasStationSize, level: GasStationSize);
            env.Process(GasStationControl(env, fuelPump));
            env.Process(CarGenerator(env, gasStation, fuelPump));

            // Run!
            env.Run(until: SimTime);
        }
    }
}