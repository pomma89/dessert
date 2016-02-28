'
' EventLatency.vb
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

Imports DIBRIS.Dessert.Resources

Namespace SimPy3
    Public Module EventLatency
        Const SimDuration As Integer = 100

        ''' <summary>
        '''   This class represents the propagation through a cable.
        ''' </summary>
        Private Class Cable
            ReadOnly _env As SimEnvironment
            ReadOnly _delay As Integer
            ReadOnly _store As Store(Of String)

            Public Sub New(env As SimEnvironment, delay As Integer)
                _env = env
                _delay = delay
                _store = Sim.Store(Of String)(env)
            End Sub

            Private Iterator Function Latency(value As String) As IEnumerable(Of SimEvent)
                Yield _env.Timeout(_delay)
                Yield _store.Put(value)
            End Function

            Public Sub Put(value As String)
                _env.Process(Latency(value))
            End Sub

            Public Function Take() As Store(Of String).GetEvent
                Return _store.Get()
            End Function
        End Class

        ''' <summary>
        '''   A process which randomly generates messages.
        ''' </summary>
        Private Iterator Function Sender(env As SimEnvironment, cable As Cable) As IEnumerable(Of SimEvent)
            While True
                ' Waits for next transmission.
                Yield env.Timeout(5)
                cable.Put(String.Format("Sender sent this at {0}", env.Now))
            End While
        End Function

        ''' <summary>
        '''   A process which consumes messages.
        ''' </summary>
        ''' <param name="env"></param>
        ''' <param name="cable"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Iterator Function Receiver(env As SimEnvironment, cable As Cable) As IEnumerable(Of SimEvent)
            While True
                ' Gets the event for the message pipe.
                Dim getEv = cable.Take()
                Yield getEv
                Console.WriteLine("Received this at {0} while {1}", env.Now, getEv.Value)
            End While
        End Function

        Public Sub Run()
            ' Sets up and starts the simulation.
            Console.WriteLine("Event Latency")
            Dim env = Sim.Environment()

            Dim cable = New Cable(env, 10)
            env.Process(Sender(env, cable))
            env.Process(Receiver(env, cable))

            env.Run(until:=SimDuration)
        End Sub
    End Module
End Namespace
