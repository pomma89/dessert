'
' ProducerConsumer.vb
'  
' Author(s):
'       Alessio Parma <alessio.parma@gmail.com>
' 
' Copyright (c) 2012-2016 Alessio Parma <alessio.parma@gmail.com>
' 
' Permission is hereby granted, free of charge, to any person obtaining a copy
' of this software and associated documentation files (the "Software"), to deal
' in the Software without restriction, including without limitation the rights
' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
' copies of the Software, and to permit persons to whom the Software is
' furnished to do so, subject to the following conditions:
' 
' The above copyright notice and this permission notice shall be included in
' all copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
' THE SOFTWARE.
Imports Dessert.Resources

Public Module ProducerConsumer
    Private Iterator Function Producer(env As SimEnvironment, store As Store(Of Integer)) As IEnumerable(Of SimEvent)
        While True
            Yield env.Timeout(env.Random.Next(1, 20))
            Dim item = env.Random.Next(1, 20)
            Yield store.Put(item)
            Console.WriteLine("{0}: Prodotto un {1}", env.Now, item)
        End While
    End Function

    Private Iterator Function Consumer(env As SimEnvironment, store As Store(Of Integer)) As IEnumerable(Of SimEvent)
        While True
            Yield env.Timeout(env.Random.Next(1, 20))
            Dim getEv = store.Get()
            Yield getEv
            Console.WriteLine("{0}: Consumato un {1}", env.Now, getEv.Value)
        End While
    End Function

    Sub Run()
        Dim env = Sim.Environment(21)
        Dim store = Sim.Store (Of Integer)(env, capacity := 2)
        env.Process(Producer(env, store))
        env.Process(Producer(env, store))
        env.Process(Consumer(env, store))
        env.Run(until := 60)
    End Sub
End Module