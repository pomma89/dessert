namespace Dessert.Examples.CSharp
{
    using System;
    using System.Collections.Generic;
    using Resources;

    public static class PublicToilet
    {
        static readonly double AvgPersonArrival;
        static readonly double AvgTimeInToilet;
        static readonly double SimTime;

        static PublicToilet()
        {
            Sim.CurrentTimeUnit = TimeUnit.Minute;
            AvgPersonArrival = 1.Minutes();
            AvgTimeInToilet = 5.Minutes();
            SimTime = 10.Minutes();
        }

        static IEnumerable<SimEvent> Person(SimEnvironment env, string gender, Resource toilet)
        {
            using (var req = toilet.Request()) {
                yield return req;
                Console.WriteLine("{0:0.00}: {1} --> Bagno", env.Now, gender);
                yield return env.Timeout(env.Random.Exponential(1.0/AvgTimeInToilet));
                Console.WriteLine("{0:0.00}: {1} <-- Bagno", env.Now, gender);
            }
        }

        static IEnumerable<SimEvent> PersonGenerator(SimEnvironment env)
        {
            var womenToilet = Sim.NewResource(env, 1);
            var menToilet = Sim.NewResource(env, 1);
            var count = 0;
            while (true) {
                var rnd = env.Random.NextDouble();
                var gender = ((rnd < 0.5) ? "Donna" : "Uomo") + count++;
                var toilet = (rnd < 0.5) ? womenToilet : menToilet;
                Console.WriteLine("{0:0.00}: {1} in coda", env.Now, gender);
                env.Process(Person(env, gender, toilet));
                yield return env.Timeout(env.Random.Exponential(1.0/AvgPersonArrival));
            }
        }

        public static void Run()
        {
            var env = Sim.NewEnvironment(21);
            env.Process(PersonGenerator(env));
            env.Run(SimTime);
        }
    }
}