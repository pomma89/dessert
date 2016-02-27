// 
// Customer.cs
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
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    public static class Customer
    {
        public static SimEvents Buy(SimEnvironment env, string name, double budget = 40)
        {
            Console.WriteLine("Here I am at the shop {0}", name);
            for (var i = 0; i < 4; ++i) {
                yield return env.Timeout(5); // Executed 4 times at intervals of 5 time units
                Console.WriteLine("I just bought something {0}", name);
                budget -= 10;
            }
            Console.WriteLine("All I have left is {0} I am going home {1}", budget, name);
        }

        // Expected output:
        // Starting simulation
        // Here I am at the shop Marta
        // I just bought something Marta
        // I just bought something Marta
        // I just bought something Marta
        // I just bought something Marta
        // All I have left is 60 I am going home Marta
        // Current time is 30
        public static void Main()
        {
            var env = Sim.Environment();
            env.DelayedProcess(Buy(env, "Marta", 100), delay: 10);
            Console.WriteLine("Starting simulation");
            env.Run(until: 100);
            Console.WriteLine("Current time is {0}", env.Now);
        }
    }
}