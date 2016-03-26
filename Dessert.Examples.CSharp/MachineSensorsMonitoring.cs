// File name: MachineSensorsMonitoring.cs
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

using DIBRIS.Dessert.Recording;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DIBRIS.Dessert.Examples.CSharp
{
    /// <summary>
    ///   Simulates a series of machines in a manufacturing process that have several sensors that
    ///   generate a data point every minute.
    /// </summary>
    public static class MachineSensorsMonitoring
    {
        static readonly double MonitoringTime;
        static readonly double WorkTime;
        static readonly double SimTime;

        static MachineSensorsMonitoring()
        {
            Sim.CurrentTimeUnit = TimeUnit.Minute;

            // Sensors generate a data point every minute.
            MonitoringTime = 1.Minutes();

            // Machines perform work cycles of 3 minutes.
            WorkTime = 3.Minutes();

            // Simulation will last 30 minutes.
            SimTime = 30.Minutes();
        }

        static IEnumerable<SimEvent> Machine(SimEnvironment env, char tag, IRecorder pressureRecorder, IRecorder temperatureRecorder)
        {
            Console.WriteLine($"Machine {tag} has powered up");

            // Start sensor processes.
            env.Process(PressureSensor(env, tag, pressureRecorder));
            env.Process(TemperatureSensor(env, tag, temperatureRecorder));
            Console.WriteLine($"All sensors for machine {tag} are active");

            for (var i = 1; ; ++i)
            {
                // Perform machine work.
                Console.WriteLine($"Machine {tag} has started work cycle {i}");
                yield return env.Timeout(WorkTime);
                Console.WriteLine($"Machine {tag} has ended work cycle {i}");
            }
        }

        static IEnumerable<SimEvent> PressureSensor(SimEnvironment env, char tag, IRecorder pressureRecorder)
        {
            while (true)
            {
                yield return env.Timeout(MonitoringTime);

                // Read the pressure value and record it.
                var pressure = env.Random.Normal(1000, 50);
                pressureRecorder.Observe(pressure);
                Console.WriteLine($"Pressure sensor for machine {tag} has recorded a pressure of {pressure:.00} bar");
            }
        }

        static IEnumerable<SimEvent> TemperatureSensor(SimEnvironment env, char tag, IRecorder temperatureRecorder)
        {
            while (true)
            {
                yield return env.Timeout(MonitoringTime);

                // Read the temperature value and record it.
                var temperature = env.Random.Poisson(100);
                temperatureRecorder.Observe(temperature);
                Console.WriteLine($"Temperature sensor for machine {tag} has recorded a temperature of {temperature} °F");
            }
        }

        public static void Run()
        {
            // We specify the seed in order to have fixed results in all examples; however, in a
            // production environment, it should be different among all simulations, so that each
            // simulations produces unique results.
            const int seed = 21;
            var env = Sim.Environment(seed);

            // Here we record values using a "tally", which keeps tracks only of results (median,
            // mode, mean, ...) and not of the data itself. Tallies have a very low memory footprint.
            var aPressureRecorder = Sim.Tally(env);
            var aTemperatureRecorder = Sim.Tally(env);
            env.Process(Machine(env, 'A', aPressureRecorder, aTemperatureRecorder));

            // Here we record values using a "monitor", which keeps tracks of results (median, mode,
            // mean, ...) and also of the observed data. Of course, monitors use more memory than tallies.
            var bPressureRecorder = Sim.Monitor(env);
            var bTemperatureRecorder = Sim.Monitor(env);
            env.Process(Machine(env, 'B', bPressureRecorder, bTemperatureRecorder));

            // Run the simulation.
            env.Run(SimTime);

            Console.WriteLine();
            Console.WriteLine($"Machine A average pressure: {aPressureRecorder.Mean():.00} bar");
            Console.WriteLine($"Machine A average temperature: {aTemperatureRecorder.Mean():.00} °F");
            Console.WriteLine();
            Console.WriteLine($"Machine B average pressure: {bPressureRecorder.Mean():.00} bar");
            Console.WriteLine($"Machine B average temperature: {bTemperatureRecorder.Mean():.00} °F");

            // Since we used two monitors for machine B, we can also print the recorded data.
            var pressureValues = string.Join(", ", bPressureRecorder.Samples.Select(s => s.Sample.ToString(".00")));
            var temperatureValues = string.Join(", ", bTemperatureRecorder.Samples.Select(s => s.Sample.ToString()));
            Console.WriteLine();
            Console.WriteLine($"Machine B pressure values: {pressureValues}");
            Console.WriteLine($"Machine B temperature values: {temperatureValues}");
        }
    }
}
