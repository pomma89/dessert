// 
// WaterDrinkers.fs
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

module Dessert.Examples.FSharp.WaterDrinkers

open Dessert
open Dessert.Events
open Dessert.Resources

let boxCapacity = 1.0 // Liters
let glassCapacity = 0.25 // Liters
let mutable fillBox: SimEvent<int> = null

let rec filler(env: SimEnvironment, box: Container) = seq<SimEvent> {
    yield upcast box.Put(boxCapacity - box.Level)
    fillBox <- env.Event<int>()
    yield upcast fillBox
    let id = fillBox.Value
    printfn "%f: %d chiama tecnico" env.Now id
    yield! filler(env, box)
}

let drinker(env: SimEnvironment, id, box: Container) = seq<SimEvent> {
    // Occorre controllare che l'evento fillBox non sia gia'
    // stato attivato, perche' attivarlo nuovamente
    // risulterebbe in una eccezione da parte di SimPy.
    if box.Level < glassCapacity && not fillBox.Succeeded then
        fillBox.Succeed(id)    
    yield upcast box.Get(glassCapacity)
    printfn "%f: %d ha bevuto!" env.Now id
}

let rec spawner(env: SimEnvironment, box, nextId) = seq<SimEvent> {
    yield upcast env.Timeout(5.0)
    env.Process(drinker(env, nextId, box)) |> ignore
    yield! spawner(env, box, nextId+1)
}

let run() =
    let env = Sim.Environment()
    let box = Sim.Container(env, capacity=boxCapacity)
    env.Process(filler(env, box)) |> ignore
    env.Process(spawner(env, box, 0)) |> ignore
    env.Run(until=31)