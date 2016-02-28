![](http://pomma89.altervista.org/dessert/logo.64.png "Dessert Logo") Dessert
=============================================================================

A discrete event simulation (DES) engine heavily based on the paradigm introduced by Simula and SimPy.

## Summary ##

* Latest release version: `v3.0.11`
* [Thesis (Italian)](http://pomma89.altervista.org/doc/mthesis-essay.pdf) and [Slides (Italian)](http://pomma89.altervista.org/doc/mthesis-slides.pdf)
* Build status on [AppVeyor](https://ci.appveyor.com): [![Build status](https://ci.appveyor.com/api/projects/status/7qgv5o7or96rr8a2?svg=true)](https://ci.appveyor.com/project/pomma89/dessert)
* [Doxygen](http://www.stack.nl/~dimitri/doxygen/index.html) documentation: TODO
* [NuGet](https://www.nuget.org) package(s):
    + [DIBRIS.Dessert](https://www.nuget.org/packages/Dessert/)

Overview
--------

The aim of project Dessert is to bring the "powerful simplicity" of the [SimPy library](https://bitbucket.org/simpy/simpy/) to the .NET/Mono world; what we are trying to do is to keep the concepts introduced by SimPy, while offering much better performance, especially for large scale simulations.

Current maintainers of the project are [Alessio Parma](http://pomma89.altervista.org/) and [Giovanni Lagorio](http://www.disi.unige.it/person/LagorioG/). Since we do not have too much manpower and time to invest in this project, our current goals are to maintain a working "clone" of the release 3.0 of SimPy.

For the same reasons, documentation is pretty short: in any case, please refer to our working examples ([C#](https://github.com/pomma89/Dessert/tree/master/Dessert.Examples.CSharp), [F#](https://github.com/pomma89/Dessert/tree/master/Dessert.Examples.FSharp), [VB.NET](https://github.com/pomma89/Dessert/tree/master/Dessert.Examples.VisualBasic), [Boo](https://github.com/pomma89/Dessert/tree/master/Dessert.Examples.Boo)) to get a better insight of what you can do with Dessert, and how the code really resembles the one you could write with SimPy.

Quick example
-------------

We will start by translating the first example exposed in the [SimPy documentation](https://simpy.readthedocs.org/en/latest/index.html), where we simulate the life of a simple clock. This is the original example:

```py
import simpy

def clock(env, name, tick):
    while True:
        print(name, env.now)
        yield env.timeout(tick)

env = simpy.Environment()
env.process(clock(env, 'fast', 0.5))
env.process(clock(env, 'slow', 1))
env.run(until=2)
```

And this is its output:

```
fast 0.0
slow 0.0
fast 0.5
slow 1.0
fast 1.0
fast 1.5
```

Next sections will show how the example gets translated on Dessert, by using some of the most famous languages for the .NET platform.

### CSharp

Compared to Python, C# is far more verbose and full of unnecessary characters, like semicolons and brackets. However, we tried to mimic SimPy APIs as well as we could, in order to reduce to the minimum the syntactic "noise" coming from the usage of C#.

Let's see how the clock example gets translated into that language:

```cs
using System;
using System.Collections.Generic;
using Dessert;

static class ClockExample
{
    static IEnumerable<IEvent> Clock(IEnvironment env, string name, double tick)
    {
        while (true) {
            Console.WriteLine("{0} {1:0.0}", name, env.Now);
            yield return env.Timeout(tick);
        }
    }

    static void Run()
    {
        var env = Sim.Environment();
        env.Process(Clock(env, "fast", 0.5));
        env.Process(Clock(env, "slow", 1.0));
        env.Run(until: 2);
    }
}
```

### FSharp

F#, thanks to a lean syntax and to its functional principles, lets us write code which is considerably shorter than the one written in C# or in VB.NET. However, there are a few "quirks" caused by the stricter typing, but they do not hamper too much the usage of Dessert.

The following is the translation in F# of the clock example:

```fs
open DIBRIS.Dessert // Yummy :P

let rec clock(env: IEnvironment, name, tick) = seq<IEvent> { 
    printfn "%s %.1f" name env.Now 
    yield upcast env.Timeout(tick)
    yield! clock(env, name, tick)
}

let run() =
    let env = Sim.Environment() 
    env.Process(clock(env, "fast", 0.5)) |> ignore
    env.Process(clock(env, "slow", 1.0)) |> ignore
    env.Run(until = 2)
```

### Visual Basic .NET

VB.NET is _enormously_ more verbose than Python and C#, but the original simplicity of SimPy is preserved:

```vbnet
Module ClockExample
    Iterator Function Clock(env As IEnvironment, name As String, tick As Double) _
    As IEnumerable(Of IEvent)
        While True
            Console.WriteLine("{0} {1:0.0}", name, env.Now)
            Yield env.Timeout(tick)
        End While
    End Function

    Sub Run()
        Dim env = Sim.Environment()
        env.Process(Clock(env, "fast", 0.5))
        env.Process(Clock(env, "slow", 1.0))
        env.Run(until:=2)
    End Sub
End Module
```

### Boo

If this section was not named "Boo", we bet it would be hard for you to distinguish following translation from the original SimPy example. Thanks to Boo, we can obtain a code which is very readable, like the Python one, yet strongly typed and compiled, which allows us to obtain better performance. Here is the code:

```boo
import System
import Dessert

def clock(env as IEnvironment, name, tick):
	while true:
		print "${name}, ${env.Now}"
		yield env.Timeout(tick)

def run():
	env = Sim.Environment()
	env.Process(clock(env, "fast", 0.5))
	env.Process(clock(env, "slow", 1.0))
	env.Run(2)
```

## About this repository and its maintainer ##

Everything done on this repository is freely offered on the terms of the project license. You are free to do everything you want with the code and its related files, as long as you respect the license and use common sense while doing it :-)
