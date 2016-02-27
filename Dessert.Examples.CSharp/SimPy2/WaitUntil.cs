// 
// WaitUntil.cs
//  
// Author(s):
//       Alessio Parma <alessio.parma@gmail.com>
// 
// Copyright (c) 2012 Alessio Parma <alessio.parma@gmail.com>
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

namespace Dessert.Examples.CSharp.SimPy2
{
    using System;
    using Events;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public sealed class Player
    {
        public Player(int lives = 1, string name = "ImaTarget")
        {
            Name = name;
            Lives = lives;
            Damage = 0;
        }

        public string Name { get; private set; }
        public string Message { get; private set; }
        public SimEvent<object> LifeEvent { get; private set; }
        public int Lives { get; private set; }
        public int Damage { get; set; }

        public SimEvents Life(SimEnvironment env)
        {
            LifeEvent = env.Event();
            Message = "Drat! Some " + Name + " survived Federation attack!";
            while (true) {
                yield return LifeEvent;
                Lives -= 1;
                Damage = 0;
                if (Lives > 0) {
                    continue;
                }
                Message = string.Format("{0} wiped out by Federation at {1}", Name, env.Now);
                //sim.Stop();
            }
        }
    }

    public sealed class Federation
    {
        readonly Random _random = new Random();

        public SimEvents Fight(SimEnvironment env, Player target)
        {
            Console.WriteLine("Three {0} attempting to escape!", target.Name);
            while (true) {
                if (_random.Next(0, 10) < 2) {
                    // Checks for hit on player
                    target.Damage += 1; // Hit! Increment damage to player
                    if (target.Damage <= 5) // Target survives...
                    {
                        Console.WriteLine("Ha! {0} hit! Damage = {1}", target.Name, target.Damage);
                    } else {
                        target.LifeEvent.Succeed();
                        if (target.Lives - 1 == 0) {
                            Console.WriteLine("No more {0} left!", target.Name);
                        } else {
                            Console.WriteLine("Now only {0} {1} left!", target.Lives - 1, target.Name);
                        }
                    }
                }
                yield return env.Timeout(1);
            }
        }
    }

    public static class WaitUntil
    {
        public static void Main()
        {
            var sim = Sim.Environment();
            const int gameOver = 100;
            // Creates a Player object named "Romulans"
            var target = new Player(lives: 3, name: "Romulans");
            sim.Process(target.Life(sim));
            // Creates a Federation object
            var shooter = new Federation();
            sim.Process(shooter.Fight(sim, target));
            sim.Run(until: gameOver);
            Console.WriteLine(target.Message);
        }
    }
}