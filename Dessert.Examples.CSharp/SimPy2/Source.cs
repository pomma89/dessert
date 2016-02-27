//
// Source.cs
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

    public static class Source
    {
        static SimEvents Execute(SimEnvironment env, int finish)
        {
            while (env.Now < finish) {
                // Creates a new Customer and activates it (with default parameters).
                env.Process(Customer.Buy(env, "Marta"));
                Console.WriteLine("{0} {1}", env.Now, "Marta");
                yield return env.Timeout(10);
            }
        }

        public static void Main()
        {
            var env = Sim.Environment();
            env.Process(Execute(env, 33));
            env.Run(until: 100);
        }
    }
}