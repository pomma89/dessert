// 
// CarCharge.fs
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

module Dessert.Examples.FSharp.SimPy3.CarCharge

open Dessert

type Car (env: SimEnvironment) as x =
    // Starts the "run" process every time an instance is created.
    do env.Process (x.run) |> ignore

    member x.run = seq<SimEvent> {
        printfn "Start parking and charging at %g" env.Now
        let chargeDuration = 5.0
        // We yield the process that start() returns to wait for it to finish.
        yield upcast env.Process (x.charge chargeDuration)

        // The charge process has finished and we can start driving again.
        printfn "Start driving at %g" env.Now
        let tripDuration = 2.0
        yield upcast env.Timeout tripDuration
            
        // Runs the same process again. Tail recursion optimization
        // makes following instruction safe and efficient.
        yield! x.run
    }

    member x.charge duration = seq<SimEvent> {
        yield upcast env.Timeout(duration)
    }

// Expected output:
// Start parking and charging at 0
// Start driving at 5
// Start parking and charging at 7
// Start driving at 12
// Start parking and charging at 14
let run() =
    let env = Sim.Environment()
    let car = Car env
    env.Run (until = 15.0)