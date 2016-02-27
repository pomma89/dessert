'
' EventCallbacks.vb
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
Imports Dessert.Events

Public Module EventCallbacks
    Private Iterator Function DoFail(env As SimEnvironment, ev As SimEvent(Of String)) As IEnumerable(Of SimEvent)
        Yield env.Timeout(5)
        ev.Fail("NO")
    End Function

    Private Sub MyCallback(ev As SimEvent)
        Console.WriteLine("Successo: '{0}'; Valore: '{1}'", ev.Succeeded, ev.Value)
    End Sub

    Private Iterator Function Proc(env As SimEnvironment) As IEnumerable(Of SimEvent)
        Dim ev1 = env.Timeout(7, "SI")
        ev1.Callbacks.Add(AddressOf MyCallback)
        Yield ev1
        Dim ev2 = env.Event (Of String)()
        ev2.Callbacks.Add(AddressOf MyCallback)
        env.Process(DoFail(env, ev2))
        Yield ev2
    End Function

    Sub Run()
        Dim env = Sim.Environment()
        env.Process(Proc(env))
        env.Run()
    End Sub
End Module