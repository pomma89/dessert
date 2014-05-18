// 
// BankExample.fs
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

module Dessert.Examples.FSharp.BankExample

open Dessert
open Dessert.Resources
open MoreLinq // Espone MinBy, usato dentro Spawner

Sim.CurrentTimeUnit <- TimeUnit.Minute
let avgIncomingT, avgServiceT = (3).Minutes(), (10).Minutes()
let queueCount = 3 // Numero sportelli
let bankCap, bankLvl = 20000.0, 2000.0 // Euro
let waitTally, servTally = Sim.NewTally(), Sim.NewTally()
let mutable totClients = 0

let client(env: SimEnvironment, queue: Resource, bank: Container, amount, get) = seq<SimEvent> {
    use req = queue.Request()
    let s1 = env.Now
    yield upcast req
    waitTally.Observe(env.Now - s1)
    let s2 = env.Now
    yield upcast env.Timeout(env.Random.Exponential(1.0/avgServiceT))
    if get then yield upcast bank.Get(amount)
    else yield upcast bank.Put(amount)
    servTally.Observe(env.Now - s2)
}

let rec spawner(env: SimEnvironment, queues: Resource list, bank) = seq<SimEvent> {
    yield upcast env.Timeout(env.Random.Exponential(1.0/avgIncomingT))
    let queue = queues.MinBy(fun q -> q.Count)
    let amount = float(env.Random.Next(50, 500))
    let get = env.Random.NextDouble() < 0.4
    env.Process(client(env, queue, bank, amount, get)) |> ignore
    totClients <- totClients + 1
    yield! spawner(env, queues, bank)
}

let run() =
    let env = Sim.NewEnvironment(seed = 21)
    let queues = [for x in 1 .. queueCount do yield Sim.NewResource(env, 1)]
    let bank = Sim.NewContainer(env, bankCap, bankLvl)

    // Avvio della simulazione
    env.Process(spawner(env, queues, bank)) |> ignore
    env.Run(until = (5).Hours())

    // Raccolta dati statistici
    printfn "Finanze totali al tempo %.2f: %g" env.Now bank.Level
    printfn "Clienti entrati: %d" totClients
    printfn "Clienti serviti: %d" servTally.Count
    printfn "Tempo medio di attesa: %.2f" (waitTally.Mean())
    printfn "Tempo medio di servizio: %.2f" (servTally.Mean())
