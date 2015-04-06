'
' HospitalPreemption.vb
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

Public Module HospitalPreemption
    Const Red = 0
    Const Yellow = 1
    Const Green = 2

    Private ReadOnly Names As Dictionary(Of SimProcess, String) = New Dictionary(Of SimProcess, String)()

    Private Iterator Function Person(env As SimEnvironment, code As Integer, hospital As PreemptiveResource,
                                     delay As Double, name As String) As IEnumerable(Of SimEvent)
        Names.Add(env.ActiveProcess, name)
        Yield env.Timeout(delay)
        Using req = hospital.Request(code, preempt := (code = Red))
            Yield req
            Console.WriteLine("{0} viene curato...", name)
            Yield env.Timeout(7)
            Dim info As PreemptionInfo = Nothing
            If env.ActiveProcess.Preempted(info) Then
                Console.WriteLine("{0} scavalcato da {1}", name, Names(info.By))
            Else
                Console.WriteLine("Cure finite per {0}", name)
            End If
        End Using
    End Function

    Sub Run()
        Dim env = Sim.Environment()
        Dim hospital = Sim.PreemptiveResource(env, capacity := 2)
        env.Process(Person(env, Yellow, hospital, 0, "Pino"))
        env.Process(Person(env, Green, hospital, 0, "Gino"))
        env.Process(Person(env, Green, hospital, 1, "Nino"))
        env.Process(Person(env, Yellow, hospital, 1, "Dino"))
        env.Process(Person(env, Red, hospital, 2, "Tino"))
        env.Run()
    End Sub
End Module