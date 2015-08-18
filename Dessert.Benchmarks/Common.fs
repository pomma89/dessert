// 
// Common.fs
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

namespace Dessert.Benchmarks

open System
open System.Diagnostics
open System.Globalization
open System.Threading
open Dessert
open Dessert.Recording
open Troschuetz.Random
open Troschuetz.Random.Generators

module Common =
    let simTime = 1000.0
    let memRecFreq = simTime/5.0
    let minTimeout = double simTime/100.0
    let maxTimeout = double simTime/20.0
    let repetitionCount = 21
    let currProc = Process.GetCurrentProcess()

    let tag = match Environment.OSVersion.Platform with
              | PlatformID.Unix -> "dessert-linux"
              | _ -> "dessert-windows"

    let processCounts = [for i in 1..40 do yield 500*i]

    Thread.CurrentThread.CurrentCulture <- CultureInfo("en-GB") // To avoid commas in decimal values.

    let memoryRecorder(env: SimEnvironment, tally: Tally) = seq<SimEvent> {
        while true do
            yield upcast env.Timeout(memRecFreq)
            currProc.Refresh()
            let procMemInMb = currProc.WorkingSet64/(1024L*1024L)
            tally.Observe(float procMemInMb);
    }

    type Counter() =
        let random = TRandom(NR3Generator())
        let mutable total = 0UL
        member c.Total = total
        member c.Increment() = total <- total + 1UL
        member c.RandomDelay = random.NextDouble(minTimeout, maxTimeout)

    type Result(eventCount: double, avgMem: int) =
        let eventCount = eventCount
        let avgMem = avgMem
        member r.EventCount = eventCount
        member r.AverageMemUsage = avgMem
    
    let toEventsPerSec(eventCount: uint64, timeInMillisec: int64) =
        let seconds = (float timeInMillisec/1000.0)
        (float eventCount/seconds)
    
    let cleanUp() =
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking = true)
        GC.WaitForPendingFinalizers()