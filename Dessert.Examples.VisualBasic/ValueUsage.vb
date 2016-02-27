'
' ValueUsage.vb
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

Public Module ValueUsage
    Private Iterator Function Process(env As SimEnvironment) As IEnumerable(Of SimEvent)
        Dim t = env.Timeout(5, value := "A BORING EXAMPLE")
        Yield t
        ' Since t.Value is a string, we can call
        ' string methods on it.
        Console.WriteLine(t.Value.Substring(2, 6))

        Dim intStore = Sim.Store (Of Double)(env)
        intStore.Put(t.Delay)
        Dim getEv = intStore.Get()
        Yield getEv
        ' Since getEv.Value is a double, we can
        ' multiply it by 2.5, as expected.
        Console.WriteLine(getEv.Value*2.5)
    End Function

    Sub Run()
        Dim env = Sim.Environment()
        env.Process(Process(env))
        env.Run()
    End Sub
End Module