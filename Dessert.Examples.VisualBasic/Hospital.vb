'
' Hospital.vb
'  
' Author(s):
'       Alessio Parma <alessio.parma@gmail.com>
' 
' Copyright (c) 2012-2014 Alessio Parma <alessio.parma@gmail.com>
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

Public Module Hospital
    Const Red = 0
    Const Yellow = 1
    Const Green = 2

    Private Iterator Function Person(env As SimEnvironment, name As String, code As Integer, hospital As Resource) _
        As IEnumerable(Of SimEvent)
        Using req = hospital.Request(code)
            Yield req
            Console.WriteLine("{0} viene curato...", name)
            Yield env.Timeout(5)
        End Using
    End Function

    Sub Run()
        Dim env = Sim.Environment()
        Dim hospital = Sim.Resource(env, capacity := 2, requestPolicy := WaitPolicy.Priority)
        env.Process(Person(env, "Pino", Yellow, hospital))
        env.Process(Person(env, "Gino", Green, hospital))
        env.Process(Person(env, "Nino", Green, hospital))
        env.Process(Person(env, "Dino", Yellow, hospital))
        env.Process(Person(env, "Tino", Red, hospital))
        env.Run()
    End Sub
End Module