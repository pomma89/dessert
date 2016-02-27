//
// GasStation.cs
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

#region Original SimPy3 Example

//"""
//Gas Station Refueling example

//Covers:

//- Resources: Resource
//- Resources: Container
//- Waiting for other processes

//Scenario:
//  A gas station has a limited number of gas pumps that share a common
//  fuel reservoir. Cars randomly arrive at the gas station, request one
//  of the fuel pumps and start refueling from that reservoir.

//  A gas station control process observes the gas station's fuel level
//  and calls a tank truck for refueling if the station's level drops
//  below a threshold.

//"""
//import itertools
//import random

//import simpy

//RANDOM_SEED = 42
//GAS_STATION_SIZE = 200     # liters
//THRESHOLD = 10             # Threshold for calling the tank truck (in %)
//FUEL_TANK_SIZE = 50        # liters
//FUEL_TANK_LEVEL = [5, 25]  # Min/max levels of fuel tanks (in liters)
//REFUELING_SPEED = 2        # liters / second
//TANK_TRUCK_TIME = 300      # Seconds it takes the tank truck to arrive
//T_INTER = [30, 300]        # Create a car every [min, max] seconds
//SIM_TIME = 1000            # Simulation time in seconds

//def car(name, env, gas_station, fuel_pump):
//    """A car arrives at the gas station for refueling.

//    It requests one of the gas station's fuel pumps and tries to get the
//    desired amount of gas from it. If the stations reservoir is
//    depleted, the car has to wait for the tank truck to arrive.

//    """
//    fuel_tank_level = random.randint(*FUEL_TANK_LEVEL)
//    print('%s arriving at gas station at %.1f' % (name, env.now))
//    with gas_station.request() as req:
//        start = env.now
//        # Request one of the gas pumps
//        yield req

//        # Get the required amount of fuel
//        liters_required = FUEL_TANK_SIZE - fuel_tank_level
//        yield fuel_pump.get(liters_required)

//        # The "actual" refueling process takes some time
//        yield env.timeout(liters_required / REFUELING_SPEED)

//        print('%s finished refueling in %.1f seconds.' % (name,
//                                                          env.now - start))

//def gas_station_control(env, fuel_pump):
//    """Periodically check the level of the *fuel_pump* and call the tank
//    truck if the level falls below a threshold."""
//    while True:
//        if fuel_pump.level / fuel_pump.capacity * 100 < THRESHOLD:
//            # We need to call the tank truck now!
//            print('Calling tank truck at %d' % env.now)
//            # Wait for the tank truck to arrive and refuel the station
//            yield env.start(tank_truck(env, fuel_pump))

//        yield env.timeout(10)  # Check every 10 seconds

//def tank_truck(env, fuel_pump):
//    """Arrives at the gas station after a certain delay and refuels it."""
//    yield env.timeout(TANK_TRUCK_TIME)
//    print('Tank truck arriving at time %d' % env.now)
//    ammount = fuel_pump.capacity - fuel_pump.level
//    print('Tank truck refuelling %.1f liters.' % ammount)
//    yield fuel_pump.put(ammount)

//def car_generator(env, gas_station, fuel_pump):
//    """Generate new cars that arrive at the gas station."""
//    for i in itertools.count():
//        yield env.timeout(random.randint(*T_INTER))
//        env.start(car('Car %d' % i, env, gas_station, fuel_pump))

//# Setup and start the simulation
//print('Gas Station refuelling')
//random.seed(RANDOM_SEED)

//# Create environment and start processes
//env = simpy.Environment()
//gas_station = simpy.Resource(env, 2)
//fuel_pump = simpy.Container(env, GAS_STATION_SIZE, init=GAS_STATION_SIZE)
//env.start(gas_station_control(env, fuel_pump))
//env.start(car_generator(env, gas_station, fuel_pump))

//# Run!
//simpy.simulate(env, until=SIM_TIME)

#endregion

namespace DIBRIS.Dessert.Examples.CSharp.SimPy3
{
    using System;
    using System.Linq;
    using Resources;
    using Troschuetz.Random;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public static class GasStation
    {
        const int RandomSeed = 42;
        const int GasStationSize = 200; // Liters.
        const int Threshold = 20; // Threshold for calling the tank truck (in %).
        const int FuelTankSize = 50; // Liters.
        const int MinFuelTankLevel = 5; // Minimum level of fuel tanks (in liters).
        const int MaxFuelTankLevel = 25; // Maximum level of fuel tanks (in liters).
        const int RefuelingSpeed = 2; // Liters / Second.
        const int TankTruckTime = 300; // Seconds it takes the tank truck to arrive.
        const int MinInter = 30; // Minimum car creation time, in seconds.
        const int MaxInter = 300; // Maximum car creation time, in seconds.
        const int SimTime = 2000; // Simulation time in seconds.

        static readonly TRandom Rand = new TRandom(RandomSeed);

        /// <summary>
        ///   A car arrives at the gas station for refueling.
        ///   It requests one of the gas station's fuel pumps and tries to get the
        ///   desired amount of gas from it. If the stations reservoir is
        ///   depleted, the car has to wait for the tank truck to arrive.
        /// </summary>
        static SimEvents Car(string name, SimEnvironment env, Resource gasStation, Container fuelPump)
        {
            var fuelTankLevel = Rand.Next(MinFuelTankLevel, MaxFuelTankLevel);
            Console.WriteLine("{0} arriving at gas station at {1:.0}", name, env.Now);
            using (var req = gasStation.Request()) {
                var start = env.Now;
                // Requests one of the gas pumps.
                yield return req;
                // Gets the required amount of fuel.
                var litersRequired = FuelTankSize - fuelTankLevel;
                yield return fuelPump.Get(litersRequired);
                // The "actual" refueling process takes some time
                yield return env.Timeout(litersRequired/(double) RefuelingSpeed);
                Console.WriteLine("{0} finished refueling in {1:.0} seconds.", name, env.Now - start);
            }
        }

        /// <summary>
        ///   Periodically checks the level of the <see cref="fuelPump"/> and
        ///   calls the tank truck if the level falls below a threshold.
        /// </summary>
        static SimEvents GasStationControl(SimEnvironment env, Container fuelPump)
        {
            while (true) {
                if (fuelPump.Level/fuelPump.Capacity*100 < Threshold) {
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
        static SimEvents TankTruck(SimEnvironment env, Container fuelPump)
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
        static SimEvents CarGenerator(SimEnvironment env, Resource gasStation, Container fuelPump)
        {
            foreach (var i in Enumerable.Range(0, int.MaxValue)) {
                yield return env.Timeout(Rand.Next(MinInter, MaxInter));
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
            var env = Sim.Environment();
            var gasStation = Sim.Resource(env, 2);
            var fuelPump = Sim.Container(env, GasStationSize, level: GasStationSize);
            env.Process(GasStationControl(env, fuelPump));
            env.Process(CarGenerator(env, gasStation, fuelPump));

            // Run!
            env.Run(until: SimTime);
        }
    }
}