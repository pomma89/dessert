![](https://googledrive.com/host/0B8v0ikF4z2BiR29YQmxfSlE1Sms/Progetti/Dessert/logo-64.png "Dessert Logo") Dessert
==================================================================================================================

*A discrete event simulation (DES) engine heavily based on the paradigm introduced by Simula and SimPy.*

## Summary ##

* Latest release version: `v3.0.12`
* [Thesis (Italian)](https://googledrive.com/host/0B8v0ikF4z2BiR29YQmxfSlE1Sms/Progetti/Dessert/mthesis-essay.pdf) and [Slides (Italian)](https://googledrive.com/host/0B8v0ikF4z2BiR29YQmxfSlE1Sms/Progetti/Dessert/mthesis-slides.pdf)
* Build status on [AppVeyor](https://ci.appveyor.com): [![Build status](https://ci.appveyor.com/api/projects/status/w1wmaefjipfv8cfu?svg=true)](https://ci.appveyor.com/project/pomma89/dessert)
* [Doxygen](http://www.stack.nl/~dimitri/doxygen/index.html) documentation: https://goo.gl/f3MgrO
* [NuGet](https://www.nuget.org) package(s):
    + [DIBRIS.Dessert](https://www.nuget.org/packages/Dessert/)

## Overview ##

The aim of project Dessert is to bring the "powerful simplicity" of the [SimPy library](https://bitbucket.org/simpy/simpy/) to the .NET/Mono world; what we are trying to do is to keep the concepts introduced by SimPy, while offering much better performance, especially for large scale simulations.

Current maintainers of the project are [Alessio Parma](http://pomma89.altervista.org/) and [Giovanni Lagorio](http://www.disi.unige.it/person/LagorioG/). Since we do not have too much manpower and time to invest in this project, our current goals are to maintain a working "clone" of the release 3.0 of SimPy.

For the same reasons, documentation is pretty short: in any case, please refer to our working examples ([C#](https://github.com/pomma89/Dessert/tree/master/Dessert.Examples.CSharp), [F#](https://github.com/pomma89/Dessert/tree/master/Dessert.Examples.FSharp), [VB.NET](https://github.com/pomma89/Dessert/tree/master/Dessert.Examples.VisualBasic), [Boo](https://github.com/pomma89/Dessert/tree/master/Dessert.Examples.Boo)) to get a better insight of what you can do with Dessert, and how the code really resembles the one you could write with SimPy.

### Dessert or SimPy? ###

This question often pops up in messages we receive, so we think its better to answer it here. First of all, let's start with a quick side to side comparison of the two projects:

|               | Dessert                          | SimPy                            |
|---------------|----------------------------------|----------------------------------|
| License       | MIT                              | MIT                              |
| Language      | C#, VB.NET, F#, IronPython       | Python                           |
| Status        | Working and tested               | Production ready                 |
| Documentation | Examples, tests                  | Examples, tests and proper docs  |
| OS            | Windows (.NET), GNU/Linux (Mono) | Windows, GNU/Linux               |
| Performance   | Fast, especially on Windows      | Can be vastly improved with PyPy |
| Support       | Very limited, best effort        | Not known, never tried           |

If you need a stable project, documented and maintained, then SimPy is the right choice, at least as of April 2016. When coupled with PyPy, SimPy can offer more than decent performance and its readability is unmatched.

Dessert, on the other hand, is a working project, but, as of April 2016, it has been nearly 30 months since the last "important" commits. Project has received minor fixes during those months, but no work has been done to ensure that functionality is still aligned to the one offered by SimPy. That might, and probably will, happen, but there is not a proper timeline.

Summing up, you can use Dessert for practical purposes, but be prepared to face some issues. Should you find them, please report them through GitHub, so that they will be handled as soon as possible. However, since both maintainers can work on Dessert only during their spare time, answers or fixes might take a few days to be prepared.

## How to build ##

In order to build Dessert, the following development environment is required:

* Visual Studio 2015, the Community edition is more than enough.
* Visual Studio must be installed with ["Shared projects" support](http://www.c-sharpcorner.com/UploadFile/7ca517/shared-project-an-impressive-features-of-visual-studio-201/).
* The [Code Contracts extension](https://visualstudiogallery.msdn.microsoft.com/1ec7db13-3363-46c9-851f-1ce455f66970) must also be installed.

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

## About this repository and its maintainers ##

Everything done on this repository is freely offered on the terms of the project license. You are free to do everything you want with the code and its related files, as long as you respect the license and use common sense while doing it :-)
