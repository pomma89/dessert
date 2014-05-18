//
// MovieRenege.cs
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
//Movie renege example

//Covers:

//- Resources: Resource
//- Condition events
//- Shared events

//Scenario:
//  A movie theatre has one ticket counter selling tickets for three
//  movies (next show only). When a movie is sold out, all people waiting
//  to buy tickets for that movie renege (leave queue).

//"""
//import collections
//import random

//import simpy

//RANDOM_SEED = 42
//TICKETS = 50  # Number of tickets per movie
//SIM_TIME = 120  # Run until

//def moviegoer(env, movie, num_tickets, theater):
//    """A moviegoer tries to by a number of tickets (*num_tickets*) for
//    a certain *movie* in a *theater*.

//    If the movie becomes sold out, she leaves the theater. If she gets
//    to the counter, she tries to buy a number of tickets. If not enough
//    tickets are left, she argues with the teller and leaves.

//    If at most one ticket is left after the moviegoer bought her
//    tickets, the *sold out* event for this movie is triggered causing
//    all remaining moviegoers to leave.

//    """
//    with theater.counter.request() as my_turn:
//        # Wait until its our turn or until the movie is sold out
//        result = yield my_turn | theater.sold_out[movie]

//        # Check if it's our turn of if movie is sold out
//        if my_turn not in result:
//            theater.num_renegers[movie] += 1
//            env.exit()

//        # Check if enough tickets left.
//        if theater.available[movie] < num_tickets:
//            # Moviegoer leaves after some discussion
//            yield env.timeout(0.5)
//            env.exit()

//        # Buy tickets
//        theater.available[movie] -= num_tickets
//        if theater.available[movie] < 2:
//            # Trigger the "sold out" event for the movie
//            theater.sold_out[movie].succeed()
//            theater.when_sold_out[movie] = env.now
//            theater.available[movie] = 0
//        yield env.timeout(1)

//def customer_arrivals(env, theater):
//    """Create new *moviegoers* until the sim time reaches 120."""
//    while True:
//        yield env.timeout(random.expovariate(1 / 0.5))

//        movie = random.choice(theater.movies)
//        num_tickets = random.randint(1, 6)
//        if theater.available[movie]:
//            env.start(moviegoer(env, movie, num_tickets, theater))

//Theater = collections.namedtuple('Theater', 'counter, movies, available, '
//                                            'sold_out, when_sold_out, '
//                                            'num_renegers')

//# Setup and start the simulation
//print('Movie renege')
//random.seed(RANDOM_SEED)
//env = simpy.Environment()

//# Create movie theater
//counter = simpy.Resource(env, capacity=1)
//movies = ['Python Unchained', 'Kill Process', 'Pulp Implementation']
//available = {movie: TICKETS for movie in movies}
//sold_out = {movie: env.event() for movie in movies}
//when_sold_out = {movie: None for movie in movies}
//num_renegers = {movie: 0 for movie in movies}
//theater = Theater(counter, movies, available, sold_out, when_sold_out,
//                  num_renegers)

//# Start process and simulate
//env.start(customer_arrivals(env, theater))
//simpy.simulate(env, until=SIM_TIME)

//# Analysis/results
//for movie in movies:
//    if theater.sold_out[movie]:
//        print('Movie "%s" sold out %.1f minutes after ticket counter '
//              'opening.' % (movie, theater.when_sold_out[movie]))
//        print('  Number of people leaving queue when film sold out: %s' %
//              theater.num_renegers[movie])

#endregion

namespace Dessert.Examples.CSharp.SimPy3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Events;
    using Resources;
    using Troschuetz.Random;
    using IEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public static class MovieRenege
    {
        const int RandomSeed = 63;
        const int Tickets = 50;
        const int SimTime = 120;

        static SimEnvironment _env;
        static Theater _theater;

        /// <summary>
        ///   A moviegoer tries to by a number of tickets (<paramref name="ticketCount"/>)
        ///   for a certain <paramref name="movie"/> in a <see cref="_theater"/>.
        ///   If the movie becomes sold out, she leaves the theater. If she gets
        ///   to the counter, she tries to buy a number of tickets. If not enough
        ///   tickets are left, she argues with the teller and leaves.
        ///   If at most one ticket is left after the moviegoer bought her
        ///   tickets, the sold out event for this movie is triggered causing
        ///   all remaining moviegoers to leave.
        /// </summary>
        static IEvents MovieGoer(MovieInfo movie, int ticketCount)
        {
            using (var myTurn = _theater.Counter.Request()) {
                // Wait until its our turn or until the movie is sold out
                yield return myTurn.Or(movie.SoldOut);

                // Check if it's our turn or if movie is sold out
                if (!myTurn.Succeeded) {
                    movie.RenegerCount++;
                    yield break;
                }

                // Check if there are enough tickets left
                if (movie.Available < ticketCount) {
                    // Moviegoer leaves after some discussion
                    yield return _env.Timeout(0.5);
                    yield break;
                }

                // Buy tickets
                movie.Available -= ticketCount;
                if (movie.Available < 2 && !movie.SoldOut.Succeeded) {
                    // Trigger the "sold out" event for the movie
                    movie.SoldOut.Succeed();
                    movie.WhenSoldOut = _env.Now;
                    movie.Available = 0;
                }
                yield return _env.Timeout(1);
            }
        }

        /// <summary>
        ///   Creates new <see cref="MovieGoer"/> processes
        ///   until the simulation time reaches <see cref="SimTime"/>.
        /// </summary>
        static IEvents CustomerArrivals()
        {
            while (true) {
                yield return _env.Timeout(_env.Random.Exponential(1.0/0.5));
                var movie = _env.Random.Choice(_theater.Movies);
                if (movie.Available > 0) {
                    var ticketCount = _env.Random.Next(1, 7); // Possible outputs: 1, 2, 3, 4, 5, 6
                    _env.Process(MovieGoer(movie, ticketCount));
                }
            }
        }

        public static void Run()
        {
            // Sets up and starts simulation
            Console.WriteLine("Movie renege");
            _env = Sim.NewEnvironment(RandomSeed);

            // Creates movie theater
            var counter = Sim.NewResource(_env, 1);
            var titles = new[] {".NET Unchained", "Kill Process", "Pulp Implementation"};
            var movies = new List<MovieInfo>(titles.Length);
            movies.AddRange(titles.Select(t => new MovieInfo(t, _env.Event())));
            _theater = new Theater(counter, movies);

            // Starts process and simulates
            _env.Process(CustomerArrivals());
            _env.Run(until: SimTime);

            // Analysis and results
            foreach (var movie in movies.Where(m => m.SoldOut.Succeeded)) {
                Console.WriteLine("Movie \"{0}\" sold out {1:.0} minutes after ticket counter opening.", movie.Title,
                                  movie.WhenSoldOut);
                Console.WriteLine("  Number of people leaving queue when film sold out: {0}", movie.RenegerCount);
            }
        }

        sealed class MovieInfo
        {
            public readonly SimEvent<object> SoldOut;
            public readonly string Title;
            public int Available;
            public int RenegerCount;
            public double WhenSoldOut;

            public MovieInfo(string title, SimEvent<object> soldOut)
            {
                Title = title;
                Available = Tickets;
                SoldOut = soldOut;
            }
        }

        sealed class Theater
        {
            public readonly Resource Counter;
            public readonly IList<MovieInfo> Movies;

            public Theater(Resource counter, IList<MovieInfo> movies)
            {
                Counter = counter;
                Movies = movies;
            }
        }
    }
}