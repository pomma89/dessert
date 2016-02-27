// 
// ClientPriority.fs
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

module Dessert.Examples.FSharp.SimPy2.ClientPriority

open System.Collections.Generic
open Dessert
open Dessert.Examples.FSharp.Utilities
open Dessert.Resources

let inClients = List<string>()  // List with the clients ordered by their requests.
let outClients = List<string>() // List with the clients ordered by completion of service.
let servTime = 100.0

let getServed (env: SimEnvironment) name priority (myServer: Resource) = seq<SimEvent> {
    inClients.Add name
    printfn "%s requests 1 unit at time = %g" name env.Now
    let req = myServer.Request(priority)
    yield upcast req
    yield upcast env.Timeout servTime
    req.Dispose()
    printfn "%s done at time = %g" name env.Now
    outClients.Add name
}   

// Expected output:
// c1 requests 1 unit at time = 0
// c2 requests 1 unit at time = 0
// c3 requests 1 unit at time = 0
// c4 requests 1 unit at time = 0
// c5 requests 1 unit at time = 0
// c6 requests 1 unit at time = 0
// c1 done at time = 100
// c2 done at time = 100
// c6 done at time = 200
// c5 done at time = 200
// c4 done at time = 300
// c3 done at time = 300
// Request order: ['c1', 'c2', 'c3', 'c4', 'c5', 'c6']
// Service order: ['c1', 'c2', 'c6', 'c5', 'c4', 'c3']
let run() =
    let env = Sim.Environment()
    let server = Sim.Resource(env, 2, WaitPolicy.Priority)  
    // Six client processes are created and started.
    for i = 1 to 6 do
        let id = i.ToString()
        let priority = float(6-i)
        let name = "c" + id
        env.Process((getServed env name priority server)) |> ignore
    env.Run (until = 500.0)
    printfn "Request order: %s" (listToString inClients)
    printfn "Service order: %s" (listToString outClients)