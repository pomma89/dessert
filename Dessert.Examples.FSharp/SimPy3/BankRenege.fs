// 
// BankRenege.fs
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

module Dessert.Examples.FSharp.SimPy3.BankRenege

open Dessert
open Dessert.Resources
open Troschuetz.Random
open Troschuetz.Random.Generators

let randomSeed = 42
let newCustomers = 5          // Total number of customers
let intervalCustomers = 10.0  // Generate new customers roughly every x seconds
let minPatience = 1.0         // Min. customer patience
let maxPatience = 3.0         // Max. customer patience

let random = TRandom(ALFGenerator(randomSeed))

// Customer arrives, is served and leaves
let customer (env: SimEnvironment) name (counter: Resource) timeInBank = seq<SimEvent> {
    let arrive = env.Now
    printfn "%07.4f %s: Here I am" arrive name

    use req = counter.Request()
    let patience = random.NextDouble(minPatience, maxPatience)
    // Wait for the counter or abort at the end of our tether
    yield upcast Sim.Or(req, env.Timeout(patience))

    let wait = env.Now - arrive

    if req.Succeeded then
        // We got to the counter
        printfn "%07.4f %s: Waited %.3f" env.Now name wait

        let tib = random.Exponential(1.0 / timeInBank)
        yield upcast env.Timeout(tib)
        printfn "%07.4f %s: Finished" env.Now name
    else
        // We reneged
        printfn "%07.4f %s: RENEGED after %.3f" env.Now name wait
}

// Source generates customers randomly
let source (env: SimEnvironment) number interval (counter: Resource) = seq<SimEvent> {  
    for i = 0 to number-1 do
        let c = customer env (sprintf "Customer%02d" i) counter 12.0
        env.Process(c) |> ignore
        let t = random.Exponential(1.0 / interval)
        yield upcast env.Timeout(t)
}

let run() =
    // Setup and start the simulation
    printfn "Bank renege"
    let env = Sim.Environment()

    // Start processes and simulate
    let counter = Sim.Resource(env, capacity = 1)
    env.Process(source env newCustomers intervalCustomers counter) |> ignore
    env.Run()