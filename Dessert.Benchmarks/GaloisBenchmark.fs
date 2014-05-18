// 
// GaloisBenchmark.fs
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

namespace Dessert.Benchmarks

open System
open System.Diagnostics
open System.IO
open Dessert.Benchmarks.Common
open Dessert.Examples.CSharp

module GaloisBenchmark =
    
    let parameters = [(4, 64); (8, 96); (16, 128); (32, 256)]

    let run() =
        let outputName = String.Format("galois-benchmark-{0}.csv", tag)
        let output = new StreamWriter(outputName)
        for (machineCount, frameCount) in parameters do
            printfn "### Running Galois with (mc = %d, fc = %d) ###" machineCount frameCount
            let stopwatch = Stopwatch()
            stopwatch.Start()
            let usedMem = Galois.Starter.Run(int16 machineCount, int16 frameCount)
            stopwatch.Stop()
            let execTime = stopwatch.Elapsed.TotalMinutes
            output.WriteLine("{0};{1};{2}", machineCount, execTime, usedMem)
            output.Flush()
            printfn "### Execution time: %A minutes ###" execTime
            printfn "### Used memory: %A MB ###" usedMem
            printfn ""
        output.Close()