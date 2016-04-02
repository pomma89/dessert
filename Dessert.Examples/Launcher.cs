// 
// File name: Launcher.cs
//  
// Author(s):
//       Alessio Parma <alessio.parma@gmail.com>
// 
// Copyright (c) 2012-2016 Alessio Parma <alessio.parma@gmail.com>
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

namespace DIBRIS.Dessert.Examples
{
    using System;
    using CSharp;
    using CSharp.Galois;
    using CSharp.SimPy2;
    using CSharp.SimPy3;
    using FSharp;
    using FSharp.SimPy2;
    using FSharp.SimPy3;
    using System.Globalization;
    using System.Threading;
    using VisualBasic;
    using VisualBasic.SimPy3;
    using BankRenege = CSharp.SimPy3.BankRenege;
    using HelloWorld = CSharp.HelloWorld;
    using Message = CSharp.SimPy2.Message;
    using CSharp.RealTime;

    static class Launcher
    {
        const string GaloisWithDessertFlag = "-gd";

        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == GaloisWithDessertFlag) {
                RunExample("Dessert - Galois fields", () => Starter.Run());
                return;
            }

            // Force en-US culture.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            RunCSharpExamples();
            RunFSharpExamples();
            RunVisualBasicExamples();

            Console.Write("Press any key to exit...");
            Console.Read();
        }

        static void RunCSharpExamples()
        {
            PrintHeader("CSHARP");

            // Expected output:
            RunExample("Dessert - My first simulation", MyFirstSimulation.Run);

            // Expected output:
            RunExample("Dessert - My second simulation", MySecondSimulation.Run);

            RunExample("Dessert - Machine sensors monitoring", MachineSensorsMonitoring.Run);

            // Expected output:
            // 7
            // True
            // True
            // True
            // 10
            // False
            // False       
            // True       
            RunExample("Dessert - Condition operators", ConditionOperators.Run);

            // Expected output:
            // ALL: {T: VAL_T, P: VAL_P}
            // ANY: {T: VAL_T}
            // CUSTOM: {T: VAL_T, P: VAL_P}
            RunExample("Dessert - Condition tester", ConditionTester.Run);

            // Expected output:
            // 7
            // True
            // True
            // 10
            // True
            // False
            RunExample("Dessert - Condition usage", ConditionUsage.Run);

            // Expected output:
            // Total clients: 196
            // Average wait: 4.8
            RunExample("Dessert - Data recording", DataRecording.Run);

            // Expected output:
            // B: 0
            // A: 7
            RunExample("Dessert - Delayed start", DelayedStart.Run);

            // Expected output:
            // SOMETHING BAD HAPPENED
            RunExample("Dessert - Event failure example", EventFailure.Run);

            // Expected output:
            // SI :)
            // NO :(
            RunExample("Dessert - Event trigger", EventTrigger.Run);

            // Expected output:
            // 0
            // 1
            // 1
            // 2
            // 3
            // 5
            // 8
            // 13
            // 21
            // 34
            RunExample("Dessert - Fibonacci producer consumer", FibonacciProducerConsumer.Run);

            // Expected output:
            // Hello World simulation :)
            // Hello World at 2.1!
            // Hello World at 4.2!
            // Hello World at 6.3!
            // Hello World at 8.4!
            RunExample("Dessert - Hello World", HelloWorld.Run);

            // Expected output:
            // Hello World real - time simulation:)
            // A - Sleeping at 0, real 28/02/2016 08:12:37...
            // B - Sleeping at 1, real 28/02/2016 08:12:38...
            // A - Awake at 3, real 28/02/2016 08:12:40
            // A - Sleeping at 3, real 28/02/2016 08:12:40...
            // B - Awake at 4, real 28/02/2016 08:12:41
            // B - Sleeping at 4, real 28/02/2016 08:12:41...
            // A - Awake at 6, real 28/02/2016 08:12:43
            // A - Sleeping at 6, real 28/02/2016 08:12:43...
            // B - Awake at 7, real 28/02/2016 08:12:44
            // B - Sleeping at 7, real 28/02/2016 08:12:44...
            // A - Awake at 9, real 28/02/2016 08:12:46
            // A - Sleeping at 9, real 28/02/2016 08:12:46...
            RunExample("Dessert (RT) - Say hello in real time", SayHelloInRealTime.Run);

            // Expected output:
            // Interupted at: NOW
            RunExample("Dessert - Interrupt handling", InterruptHandling.Run);

            // Expected output:
            // 0: Carico la macchina...
            // 5: Eseguo il comando A
            // 30: Carico la macchina...
            // 35: Eseguo il comando C
            // 60: Carico la macchina...
            // 65: Eseguo il comando A
            // 90: Carico la macchina...
            // 95: Eseguo il comando C
            RunExample("Dessert - Machine load", MachineLoad.Run);

            // Expected output:
            // 5
            // A
            // B
            RunExample("Dessert - Named events", TimeoutsWithValues.Run);

            // Expected output:
            // Pasta in cottura per 8,83107802885584 minuti
            // Pasta poco cotta!
            // Pasta in cottura per 10,996828351594 minuti
            // Pasta ben cotta!!!
            // Pasta in cottura per 9,02596303707764 minuti
            // Pasta ben cotta!!!
            // Pasta in cottura per 10,6547286665566 minuti
            // Pasta ben cotta!!!
            // Pasta in cottura per 9,5279653628264 minuti
            // Pasta ben cotta!!!
            // Pasta in cottura per 10,8264511630765 minuti
            RunExample("Dessert - Pasta cooking", PastaCooking.Run);

            // Expected output:
            // 0,00: Donna0 in coda
            // 0,00: Donna0 --> Bagno
            // 1,61: Uomo1 in coda
            // 1,61: Uomo1 --> Bagno
            // 1,71: Donna0 <-- Bagno
            // 1,79: Uomo1 <-- Bagno
            // 2,88: Donna2 in coda
            // 2,88: Donna2 --> Bagno
            // 4,04: Donna2 <-- Bagno
            // 5,91: Uomo3 in coda
            // ,91: Uomo3 --> Bagno
            // 5,96: Donna4 in coda
            // 5,96: Donna4 --> Bagno
            // 6,76: Uomo3 <-- Bagno
            // 7,83: Donna5 in coda
            // 8,20: Uomo6 in coda
            // 8,20: Uomo6 --> Bagno
            // 9,08: Donna7 in coda
            // 9,16: Uomo6 <-- Bagno
            // 9,70: Uomo8 in coda
            // 9,70: Uomo8 --> Bagno
            // 9,80: Uomo9 in coda
            RunExample("Dessert - Public toilet", PublicToilet.Run);

            // Expected output:
            // A: 6
            // B: 5
            // C: 0
            // D: 8
            // E: 2
            // F: 3
            // G: 9
            // H: 4
            // I: 1
            // J: 7
            RunExample("Dessert - Resource policy", ResourcePolicy.Run);

            // Expected output:
            // 1: Unicorno colpito, si!
            // 15: Unicorno colpito, si!
            // 16: Unicorno colpito, si!
            // 30: Alieno colpito, si!
            // 37: Unicorno colpito, si!
            // 44: Alieno mancato, no...
            // 56: Alieno colpito, si!
            // 66: Unicorno colpito, si!
            // 69: Unicorno colpito, si!
            // 77: Pollo colpito, si!
            // 95: Unicorno mancato, no...
            RunExample("Dessert - Target shooting", TargetShooting.Run);

            // Expected output:
            // Treno in viaggio per 1,00 minuti
            // Arrivo in stazione, attesa passeggeri
            // Treno in viaggio per 32,12 minuti
            // Arrivo in stazione, attesa passeggeri
            // Treno in viaggio per 6,82 minuti
            // Arrivo in stazione, attesa passeggeri
            // Treno in viaggio per 22,78 minuti
            // Al minuto 50,00: FRENO EMERGENZA
            RunExample("Dessert - Train interrupt", TrainInterrupt.Run);

            // Expected output:
            RunExample("SimPy3 - Bank renege", BankRenege.Run);

            // Expected output:
            // Start parking and charging at 0
            // Was interrupted. Hope, the battery is full enough ...
            // Start driving at 3
            // Start parking and charging at 5
            // Start driving at 10
            // Start parking and charging at 12
            RunExample("SimPy3 - Car interrupt", CarDriver.Run);

            RunExample("SimPy3 - Gas station", GasStation.Run);

            RunExample("SimPy3 - Movie renege", MovieRenege.Run);

            RunExample("SimPy3 - Process communication", ProcessCommunication.Run);

            Console.WriteLine("--> Bus breakdown example ###");
            BusBreakdown.Main();
            Console.WriteLine();

            Console.WriteLine("--> Customer example ###");
            Customer.Main();
            Console.WriteLine();

            Console.WriteLine("--> Message example ###");
            Message.Run();
            Console.WriteLine();

            Console.WriteLine("--> Source example ###");
            Source.Main();
            Console.WriteLine();

            Console.WriteLine("--> Wait until example <--");
            //WaitUntil.Main(); TODO Broken...
            Console.WriteLine();
        }

        static void RunFSharpExamples()
        {
            PrintHeader("FSHARP");

            // Expected output:
            // Finanze totali al tempo 300.00: 7207
            // Clienti entrati: 112
            // Clienti serviti: 76
            // Tempo medio di attesa: 31.31
            // Tempo medio di servizio: 9.33
            RunExample("Dessert - Bank example", BankExample.run);

            // Expected output:
            // Hello World simulation :)
            // Hello World at 2.1!
            // Hello World at 4.2!
            // Hello World at 6.3!
            // Hello World at 8.4!
            RunExample("Dessert - Hello World", FSharp.HelloWorld.run);

            // 5.000000: 0 ha bevuto!
            // 10.000000: 1 ha bevuto!
            // 15.000000: 2 ha bevuto!
            // 20.000000: 3 ha bevuto!
            // 25.000000: 4 chiama tecnico
            // 25.000000: 4 ha bevuto!
            // 30.000000: 5 ha bevuto!
            RunExample("Dessert - Water drinkers", WaterDrinkers.run);

            // Expected output:
            // c1 requests 1 unit at time = 0
            // c2 requests 1 unit at time = 0
            // c3 requests 1 unit at time = 0
            // c4 requests 1 unit at time = 0
            // c5 requests 1 unit at time = 0
            // c6 requests 1 unit at time = 0
            // c1 done at time = 100
            // c2 done at time = 100
            // c3 done at time = 200
            // c4 done at time = 200
            // c5 done at time = 300
            // c6 done at time = 300
            // Request order: ['c1', 'c2', 'c3', 'c4', 'c5', 'c6']
            // Service order: ['c1', 'c2', 'c3', 'c4', 'c5', 'c6']
            RunExample("SimPy2 - Client", Client.run);

            // Expected output:
            // c1 requests 1 unit at time = 0
            // c2 requests 1 unit at time = 0
            // c3 requests 1 unit at time = 0
            // c4 requests 1 unit at time = 0
            // c5 requests 1 unit at time = 0
            // c6 requests 1 unit at time = 0
            // c1 done at time = 100
            // c2 done at time = 100
            // c6 done at time = 200
            // c5 done at time = 200
            // c4 done at time = 300
            // c3 done at time = 300
            // Request order: ['c1', 'c2', 'c3', 'c4', 'c5', 'c6']
            // Service order: ['c1', 'c2', 'c6', 'c5', 'c4', 'c3']
            RunExample("SimPy2 - Client priority", ClientPriority.run);

            // Expected output:
            // Starting simulation
            // 0 1 Starting
            // 6 2 Starting
            // 100 1 Arrived
            // 106 2 Arrived
            // Current time is 106
            RunExample("Message", FSharp.SimPy2.Message.run);

            // Expected output:
            RunExample("Bank renege", FSharp.SimPy3.BankRenege.run);

            // Expected output:
            // Start parking at 0
            // Start driving at 5
            // Start parking at 7
            // Start driving at 12
            // Start parking at 14
            RunExample("Car", Car.run);

            // Expected output:
            // Start parking and charging at 0
            // Start driving at 5
            // Start parking and charging at 7
            // Start driving at 12
            // Start parking and charging at 14
            RunExample("Car charge", CarCharge.run);
        }

        static void RunVisualBasicExamples()
        {
            PrintHeader("VISUAL BASIC");

            // Expected output:
            // Successo: 'True'; Valore: 'SI'
            // Successo: 'False'; Valore: 'NO'
            RunExample("Dessert - Event callbacks", EventCallbacks.Run);

            // Expected output:
            // Hello World simulation :)
            // Hello World at 2.1!
            // Hello World at 4.2!
            // Hello World at 6.3!
            // Hello World at 8.4!
            RunExample("Dessert - Hello World", VisualBasic.HelloWorld.Run);

            // Expected output:
            // Pino viene curato...
            // Gino viene curato...
            // Tino viene curato...
            // Dino viene curato...
            // Nino viene curato...
            RunExample("Dessert - Hospital", Hospital.Run);

            // Expected output:
            // Pino viene curato...
            // Gino viene curato...
            // Tino viene curato...
            // Gino scavalcato da Tino
            // Cure finite per Pino
            // Dino viene curato...
            // Cure finite per Tino
            // Nino viene curato...
            // Cure finite per Dino
            // Cure finite per Nino
            RunExample("Dessert - Hospital preemption", HospitalPreemption.Run);

            // Expected output:
            // 1: Prodotto un 13
            // 6: Consumato un 13
            // 15: Prodotto un 1
            // 15: Consumato un 1
            // 16: Prodotto un 14
            // 17: Prodotto un 3
            // 19: Consumato un 14
            // 23: Prodotto un 13
            // 36: Consumato un 3
            // 36: Prodotto un 6
            // 48: Consumato un 13
            // 48: Prodotto un 16
            // 57: Consumato un 6
            // 57: Prodotto un 2
            // 59: Consumato un 16
            RunExample("Dessert - Producer consumer", ProducerConsumer.Run);

            // Expected output:
            // 1: Prodotto un 14
            // 2: Prodotto un 1
            // 6: PARI, consumato un 14
            // 13: DISPARI, consumato un 1
            // 16: Prodotto un 1
            // 19: Prodotto un 6
            // 19: PARI, consumato un 6
            // 21: Prodotto un 7
            // 27: DISPARI, consumato un 1
            // 27: Prodotto un 16
            // 32: PARI, consumato un 16
            // 32: Prodotto un 2
            // 39: DISPARI, consumato un 7
            // 39: Prodotto un 2
            // 41: PARI, consumato un 2
            // 45: PARI, consumato un 2
            // 49: Prodotto un 10
            // 53: PARI, consumato un 10
            // 54: Prodotto un 19
            // 54: DISPARI, consumato un 19
            // 58: Prodotto un 19
            RunExample("Dessert - Producer filtered consumer", ProducerFilteredConsumer.Run);

            RunExample("Dessert - Value usage", ValueUsage.Run);

            RunExample("SimPy3 - Event latency", EventLatency.Run);
        }

        static void PrintHeader(string langName)
        {
            Console.WriteLine("### {0} EXAMPLES ###", langName);
            Console.WriteLine();
        }

        static void RunExample(string name, Action example)
        {
            Console.WriteLine("--> {0} <--", name);
            example();
            Console.WriteLine();
        }
    }
}