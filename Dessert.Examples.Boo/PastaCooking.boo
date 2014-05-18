# 
# PastaCooking.boo
#  
# Author:
#       Alessio Parma <alessio.parma@gmail.com>
# 
# Copyright (c) 2012-2013 Alessio Parma <alessio.parma@gmail.com>
# 
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.

namespace Dessert.Examples.Boo

import System
import Dessert
import Dessert.Sim from Dessert

[Module]
class G:
    public static avgCookTime as double
    public static stdCookTime as double
    public static simTime as double

    static def constructor():
        CurrentTimeUnit = TimeUnit.Minute
        avgCookTime = 10.Minutes()
        stdCookTime = 1.Minutes()
        simTime = 50.Minutes()

def pastaCook(env as SimEnvironment):
    while true:
        cookTime = env.Random.Normal(G.avgCookTime, G.stdCookTime)
        print "Pasta in cottura per ${cookTime} minuti"
        yield env.Timeout(cookTime)
        if cookTime < G.avgCookTime - G.stdCookTime:
            print("Pasta poco cotta!")
        elif cookTime > G.avgCookTime + G.stdCookTime:
            print("Pasta troppo cotta...")
        else:
            print("Pasta ben cotta!!!")

def run():
    env = NewEnvironment()
    env.Process(pastaCook(env))
    env.Run(G.simTime)