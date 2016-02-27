// 
// ExamplesTests.cs
//  
// Author(s):
//       Alessio Parma <alessio.parma@gmail.com>
//       Giovanni Lagorio <giovanni.lagorio@gmail.com>
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

#if NET40
    // Empty, used because examples are not included in the .NET 4 solution.
#else

namespace Dessert.Tests
{


    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Examples.CSharp;
    using Examples.CSharp.SimPy2;
    using Examples.CSharp.SimPy3;
    using Examples.FSharp;
    using Examples.FSharp.SimPy2;
    using Examples.FSharp.SimPy3;
    using Examples.VisualBasic;
    using Examples.VisualBasic.SimPy3;
    using NUnit.Framework;
    using BankRenege = Examples.CSharp.SimPy3.BankRenege;
    using ClockExample = Examples.CSharp.SimPy3.ClockExample;
    using HelloWorld = Examples.CSharp.HelloWorld;
    using Message = Examples.CSharp.SimPy2.Message;

    [TestFixture]
    sealed class ExamplesTests
    {
        [SetUp]
        public void SetUp()
        {
            _loggedStream = new MemoryStream();
            _loggedWriter = new FormattedWriter(_loggedStream);
            _originalOutput = Console.Out;
            Console.SetOut(_loggedWriter);
        }

        [TearDown]
        public void TearDown()
        {
            _loggedWriter.Close();
            _lines = null;
            Console.SetOut(_originalOutput);
        }

        readonly Encoding _encoding = Encoding.UTF8;
        MemoryStream _loggedStream;
        FormattedWriter _loggedWriter;
        IEnumerator _lines;
        TextWriter _originalOutput;

        void AssertNoMoreLines()
        {
            Assert.False(_lines.MoveNext());
        }

        void AssertRightLine(string expected)
        {
            Assert.True(_lines.MoveNext());
            Assert.AreEqual(expected, _lines.Current as string);
        }

        void ReadLoggedStream()
        {
            _loggedWriter.Flush();
            var length = (int) _loggedStream.Length;
            var bytes = new Byte[length];

            _loggedStream.Seek(0, SeekOrigin.Begin);
            _loggedStream.Read(bytes, 0, length);

            var text = _encoding.GetString(bytes, 0, length);
            _lines = text.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries).GetEnumerator();
        }

        sealed class FormattedWriter : StreamWriter
        {
            static readonly IFormatProvider CultureInfo = new CultureInfo("en-GB");

            public FormattedWriter(Stream stream) : base(stream)
            {
            }

            public override IFormatProvider FormatProvider
            {
                get { return CultureInfo; }
            }
        }

        [Test]
        public void CSharp_Dessert_ConditionOperators()
        {
            ConditionOperators.Run();
            ReadLoggedStream();
            AssertRightLine("7");
            AssertRightLine("True");
            AssertRightLine("True");
            AssertRightLine("True");
            AssertRightLine("10");
            AssertRightLine("False");
            AssertRightLine("False");
            AssertRightLine("True");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_ConditionTester()
        {
            ConditionTester.Run();
            ReadLoggedStream();
            AssertRightLine("ALL: VAL_T, VAL_P");
            AssertRightLine("ANY: VAL_T");
            AssertRightLine("CUSTOM: VAL_T, VAL_P");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_ConditionUsage()
        {
            ConditionUsage.Run();
            ReadLoggedStream();
            AssertRightLine("7");
            AssertRightLine("True");
            AssertRightLine("True");
            AssertRightLine("10");
            AssertRightLine("True");
            AssertRightLine("False");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_DataRecording()
        {
            DataRecording.Run();
            ReadLoggedStream();
            AssertRightLine("Total clients: 196");
            AssertRightLine("Average wait: 4.8");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_DelayedStart()
        {
            DelayedStart.Run();
            ReadLoggedStream();
            AssertRightLine("B: 0");
            AssertRightLine("A: 7");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_EventFailure()
        {
            EventFailure.Run();
            ReadLoggedStream();
            AssertRightLine("SOMETHING BAD HAPPENED");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_EventTrigger()
        {
            EventTrigger.Run();
            ReadLoggedStream();
            AssertRightLine("SI :)");
            AssertRightLine("NO :(");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_FibonacciProducerConsumer()
        {
            FibonacciProducerConsumer.Run();
            ReadLoggedStream();
            AssertRightLine("0");
            AssertRightLine("1");
            AssertRightLine("1");
            AssertRightLine("2");
            AssertRightLine("3");
            AssertRightLine("5");
            AssertRightLine("8");
            AssertRightLine("13");
            AssertRightLine("21");
            AssertRightLine("34");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_HelloWorld()
        {
            HelloWorld.Run();
            ReadLoggedStream();
            AssertRightLine("Hello World simulation :)");
            AssertRightLine("Hello World at 2.1!");
            AssertRightLine("Hello World at 4.2!");
            AssertRightLine("Hello World at 6.3!");
            AssertRightLine("Hello World at 8.4!");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_InterruptHandling()
        {
            InterruptHandling.Run();
            ReadLoggedStream();
            AssertRightLine("Interrupted at: NOW");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_MachineLoad()
        {
            MachineLoad.Run();
            ReadLoggedStream();
            AssertRightLine("0: Carico la macchina...");
            AssertRightLine("5: Eseguo il comando A");
            AssertRightLine("30: Carico la macchina...");
            AssertRightLine("35: Eseguo il comando C");
            AssertRightLine("60: Carico la macchina...");
            AssertRightLine("65: Eseguo il comando A");
            AssertRightLine("90: Carico la macchina...");
            AssertRightLine("95: Eseguo il comando C");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_MyFirstSimulation()
        {
            MyFirstSimulation.Run();
            ReadLoggedStream();
            AssertRightLine("Hi, I'm Gino and I will wait for 5 minutes!");
            AssertRightLine("Hi, I'm Dino and I will wait for 5 minutes!");
            AssertRightLine("Ok, Gino's wait has finished at 5. Bye :)");
            AssertRightLine("Ok, Dino's wait has finished at 8. Bye :)");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_MySecondSimulation()
        {
            MySecondSimulation.Run();
            ReadLoggedStream();
            AssertRightLine("Hi, I'm Pino and I entered the office at 0.00");
            AssertRightLine("Finally it's Pino's turn! I waited 0.00 minutes");
            AssertRightLine("Pino's job will take 1.43 minutes");
            AssertRightLine("Ok, Pino leaves the office at 1.43. Bye :)");
            AssertRightLine("Hi, I'm Pino and I entered the office at 2.86");
            AssertRightLine("Finally it's Pino's turn! I waited 0.00 minutes");
            AssertRightLine("Pino's job will take 1.60 minutes");
            AssertRightLine("Hi, I'm Bobb and I entered the office at 3.50");
            AssertRightLine("Finally it's Bobb's turn! I waited 0.00 minutes");
            AssertRightLine("Bobb's job will take 1.28 minutes");
            AssertRightLine("Ok, Pino leaves the office at 4.46. Bye :)");
            AssertRightLine("Ok, Bobb leaves the office at 4.78. Bye :)");
            AssertRightLine("Hi, I'm Pino and I entered the office at 9.49");
            AssertRightLine("Finally it's Pino's turn! I waited 0.00 minutes");
            AssertRightLine("Pino's job will take 5.48 minutes");
            AssertRightLine("Hi, I'm Dino and I entered the office at 9.67");
            AssertRightLine("Finally it's Dino's turn! I waited 0.00 minutes");
            AssertRightLine("Dino's job will take 0.64 minutes");
            AssertRightLine("Ok, Dino leaves the office at 10.31. Bye :)");
            AssertRightLine("Hi, I'm John and I entered the office at 12.07");
            AssertRightLine("Finally it's John's turn! I waited 0.00 minutes");
            AssertRightLine("John's job will take 1.07 minutes");
            AssertRightLine("Hi, I'm Bobb and I entered the office at 12.17");
            AssertRightLine("Ok, John leaves the office at 13.14. Bye :)");
            AssertRightLine("Finally it's Bobb's turn! I waited 0.97 minutes");
            AssertRightLine("Bobb's job will take 9.16 minutes");
            AssertRightLine("Hi, I'm Gino and I entered the office at 13.61");
            AssertRightLine("Hi, I'm Bobb and I entered the office at 13.74");
            AssertRightLine("Ok, Pino leaves the office at 14.97. Bye :)");
            AssertRightLine("Finally it's Gino's turn! I waited 1.36 minutes");
            AssertRightLine("Gino's job will take 1.22 minutes");
            AssertRightLine("Ok, Gino leaves the office at 16.19. Bye :)");
            AssertRightLine("Finally it's Bobb's turn! I waited 2.45 minutes");
            AssertRightLine("Bobb's job will take 2.81 minutes");
            AssertRightLine("Ok, Bobb leaves the office at 19.00. Bye :)");
            AssertRightLine("Hi, I'm Bobb and I entered the office at 19.66");
            AssertRightLine("Finally it's Bobb's turn! I waited 0.00 minutes");
            AssertRightLine("Bobb's job will take 12.30 minutes");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_NamedEvents()
        {
            TimeoutsWithValues.Run();
            ReadLoggedStream();
            AssertRightLine("5");
            AssertRightLine("A");
            AssertRightLine("B");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_PastaCooking()
        {
            PastaCooking.Run();
            ReadLoggedStream();
            AssertRightLine("Pasta in cottura per 10.996828351594 minuti");
            AssertRightLine("Pasta ben cotta!!!");
            AssertRightLine("Pasta in cottura per 9.75045532943448 minuti");
            AssertRightLine("Pasta ben cotta!!!");
            AssertRightLine("Pasta in cottura per 9.51803635506927 minuti");
            AssertRightLine("Pasta ben cotta!!!");
            AssertRightLine("Pasta in cottura per 10.4418199182862 minuti");
            AssertRightLine("Pasta ben cotta!!!");
            AssertRightLine("Pasta in cottura per 8.93382209344506 minuti");
            AssertRightLine("Pasta poco cotta!");
            AssertRightLine("Pasta in cottura per 11.2258433501376 minuti");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_PublicToilet()
        {
            PublicToilet.Run();
            ReadLoggedStream();
            AssertRightLine("0.00: Donna0 in coda");
            AssertRightLine("0.00: Donna0 --> Bagno");
            AssertRightLine("1.61: Uomo1 in coda");
            AssertRightLine("1.61: Uomo1 --> Bagno");
            AssertRightLine("1.71: Donna0 <-- Bagno");
            AssertRightLine("1.79: Uomo1 <-- Bagno");
            AssertRightLine("2.88: Donna2 in coda");
            AssertRightLine("2.88: Donna2 --> Bagno");
            AssertRightLine("4.04: Donna2 <-- Bagno");
            AssertRightLine("5.91: Uomo3 in coda");
            AssertRightLine("5.91: Uomo3 --> Bagno");
            AssertRightLine("5.96: Donna4 in coda");
            AssertRightLine("5.96: Donna4 --> Bagno");
            AssertRightLine("6.76: Uomo3 <-- Bagno");
            AssertRightLine("7.83: Donna5 in coda");
            AssertRightLine("8.20: Uomo6 in coda");
            AssertRightLine("8.20: Uomo6 --> Bagno");
            AssertRightLine("9.08: Donna7 in coda");
            AssertRightLine("9.16: Uomo6 <-- Bagno");
            AssertRightLine("9.70: Uomo8 in coda");
            AssertRightLine("9.70: Uomo8 --> Bagno");
            AssertRightLine("9.80: Uomo9 in coda");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_ResourcePolicy()
        {
            ResourcePolicy.Run();
            ReadLoggedStream();
            AssertRightLine("A: 6");
            AssertRightLine("B: 5");
            AssertRightLine("C: 0");
            AssertRightLine("D: 8");
            AssertRightLine("E: 2");
            AssertRightLine("F: 3");
            AssertRightLine("G: 9");
            AssertRightLine("H: 4");
            AssertRightLine("I: 1");
            AssertRightLine("J: 7");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_TargetShooting()
        {
            TargetShooting.Run();
            ReadLoggedStream();
            AssertRightLine("1: Unicorno colpito, si!");
            AssertRightLine("15: Unicorno colpito, si!");
            AssertRightLine("16: Unicorno colpito, si!");
            AssertRightLine("30: Alieno colpito, si!");
            AssertRightLine("37: Unicorno colpito, si!");
            AssertRightLine("44: Alieno mancato, no...");
            AssertRightLine("56: Alieno colpito, si!");
            AssertRightLine("66: Unicorno colpito, si!");
            AssertRightLine("69: Unicorno colpito, si!");
            AssertRightLine("77: Pollo colpito, si!");
            AssertRightLine("95: Unicorno mancato, no...");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_Dessert_TrainInterrupt()
        {
            TrainInterrupt.Run();
            ReadLoggedStream();
            AssertRightLine("Treno in viaggio per 1.00 minuti");
            AssertRightLine("Arrivo in stazione, attesa passeggeri");
            AssertRightLine("Treno in viaggio per 32.12 minuti");
            AssertRightLine("Arrivo in stazione, attesa passeggeri");
            AssertRightLine("Treno in viaggio per 6.82 minuti");
            AssertRightLine("Arrivo in stazione, attesa passeggeri");
            AssertRightLine("Treno in viaggio per 22.78 minuti");
            AssertRightLine("Al minuto 50.00: FRENO EMERGENZA");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_SimPy2_BusBreakdown()
        {
            BusBreakdown.Main();
            ReadLoggedStream();
            AssertRightLine("Breakdown Bus at 300");
            AssertRightLine("Bus repaired at 320");
            AssertRightLine("Breakdown Bus at 620");
            AssertRightLine("Bus repaired at 640");
            AssertRightLine("Breakdown Bus at 940");
            AssertRightLine("Bus repaired at 960");
            AssertRightLine("Bus has arrived at 1060");
            AssertRightLine("Dessert: No more events at time 1260");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_SimPy2_Customer()
        {
            Customer.Main();
            ReadLoggedStream();
            AssertRightLine("Starting simulation");
            AssertRightLine("Here I am at the shop Marta");
            AssertRightLine("I just bought something Marta");
            AssertRightLine("I just bought something Marta");
            AssertRightLine("I just bought something Marta");
            AssertRightLine("I just bought something Marta");
            AssertRightLine("All I have left is 60 I am going home Marta");
            AssertRightLine("Current time is 30");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_SimPy2_Message()
        {
            Message.Run();
            ReadLoggedStream();
            AssertRightLine("Starting simulation");
            AssertRightLine("0 1 Starting");
            AssertRightLine("6 2 Starting");
            AssertRightLine("100 1 Arrived");
            AssertRightLine("106 2 Arrived");
            AssertRightLine("Current time is 106");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_SimPy3_BankRenege()
        {
            BankRenege.Run();
            ReadLoggedStream();
            AssertRightLine("Bank renege");
            AssertRightLine("00.0000 Customer00: Here I am");
            AssertRightLine("00.0000 Customer00: Waited 0.000");
            AssertRightLine("00.6461 Customer01: Here I am");
            AssertRightLine("02.8439 Customer01: RENEGED after 2.198");
            AssertRightLine("16.8235 Customer02: Here I am");
            AssertRightLine("18.5958 Customer00: Finished");
            AssertRightLine("18.5958 Customer02: Waited 1.772");
            AssertRightLine("32.3050 Customer02: Finished");
            AssertRightLine("38.0942 Customer03: Here I am");
            AssertRightLine("38.0942 Customer03: Waited 0.000");
            AssertRightLine("43.8030 Customer03: Finished");
            AssertRightLine("46.8195 Customer04: Here I am");
            AssertRightLine("46.8195 Customer04: Waited 0.000");
            AssertRightLine("50.7801 Customer04: Finished");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_SimPy3_CarDriver()
        {
            CarDriver.Run();
            ReadLoggedStream();
            AssertRightLine("Start parking and charging at 0");
            AssertRightLine("Was interrupted. Hope, the battery is full enough ...");
            AssertRightLine("Start driving at 3");
            AssertRightLine("Start parking and charging at 5");
            AssertRightLine("Start driving at 10");
            AssertRightLine("Start parking and charging at 12");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_SimPy3_Clock()
        {
            ClockExample.Run();
            ReadLoggedStream();
            AssertRightLine("fast 0.0");
            AssertRightLine("slow 0.0");
            AssertRightLine("fast 0.5");
            AssertRightLine("slow 1.0");
            AssertRightLine("fast 1.0");
            AssertRightLine("fast 1.5");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_SimPy3_GasStation()
        {
            GasStation.Run();
            ReadLoggedStream();
            AssertRightLine("Gas Station refueling");
            AssertRightLine("Car 0 arriving at gas station at 35.0");
            AssertRightLine("Car 0 finished refueling in 19.5 seconds.");
            AssertRightLine("Car 1 arriving at gas station at 270.0");
            AssertRightLine("Car 1 finished refueling in 20.0 seconds.");
            AssertRightLine("Car 2 arriving at gas station at 301.0");
            AssertRightLine("Car 2 finished refueling in 15.0 seconds.");
            AssertRightLine("Car 3 arriving at gas station at 420.0");
            AssertRightLine("Car 3 finished refueling in 20.0 seconds.");
            AssertRightLine("Car 4 arriving at gas station at 706.0");
            AssertRightLine("Calling tank truck at 710");
            AssertRightLine("Car 4 finished refueling in 22.0 seconds.");
            AssertRightLine("Car 5 arriving at gas station at 754.0");
            AssertRightLine("Car 6 arriving at gas station at 985.0");
            AssertRightLine("Tank truck arriving at time 1010");
            AssertRightLine("Tank truck refueling 193.0 liters.");
            AssertRightLine("Car 5 finished refueling in 272.5 seconds.");
            AssertRightLine("Car 6 finished refueling in 46.5 seconds.");
            AssertRightLine("Car 7 arriving at gas station at 1203.0");
            AssertRightLine("Car 7 finished refueling in 22.0 seconds.");
            AssertRightLine("Car 8 arriving at gas station at 1463.0");
            AssertRightLine("Car 8 finished refueling in 15.0 seconds.");
            AssertRightLine("Car 9 arriving at gas station at 1556.0");
            AssertRightLine("Calling tank truck at 1560");
            AssertRightLine("Car 9 finished refueling in 14.0 seconds.");
            AssertRightLine("Car 10 arriving at gas station at 1724.0");
            AssertRightLine("Car 11 arriving at gas station at 1828.0");
            AssertRightLine("Tank truck arriving at time 1860");
            AssertRightLine("Tank truck refueling 178.0 liters.");
            AssertRightLine("Car 11 finished refueling in 45.5 seconds.");
            AssertRightLine("Car 10 finished refueling in 158.0 seconds.");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_SimPy3_MovieRenege()
        {
            MovieRenege.Run();
            ReadLoggedStream();
            AssertRightLine("Movie renege");
            AssertRightLine("Movie \".NET Unchained\" sold out 45.4 minutes after ticket counter opening.");
            AssertRightLine("  Number of people leaving queue when film sold out: 7");
            AssertRightLine("Movie \"Kill Process\" sold out 48.4 minutes after ticket counter opening.");
            AssertRightLine("  Number of people leaving queue when film sold out: 6");
            AssertRightLine("Movie \"Pulp Implementation\" sold out 38.9 minutes after ticket counter opening.");
            AssertRightLine("  Number of people leaving queue when film sold out: 7");
            AssertNoMoreLines();
        }

        [Test]
        public void FSharp_Dessert_BankExample()
        {
            BankExample.run();
            ReadLoggedStream();
            AssertRightLine("Finanze totali al tempo 300.00: 7207");
            AssertRightLine("Clienti entrati: 112");
            AssertRightLine("Clienti serviti: 76");
            AssertRightLine("Tempo medio di attesa: 31.31");
            AssertRightLine("Tempo medio di servizio: 9.33");
            AssertNoMoreLines();
        }

        [Test]
        public void FSharp_Dessert_HelloWorld()
        {
            Examples.FSharp.HelloWorld.run();
            ReadLoggedStream();
            AssertRightLine("Hello World simulation :)");
            AssertRightLine("Hello World at 2.1!");
            AssertRightLine("Hello World at 4.2!");
            AssertRightLine("Hello World at 6.3!");
            AssertRightLine("Hello World at 8.4!");
            AssertNoMoreLines();
        }

        [Test]
        public void FSharp_Dessert_WaterDrinkers()
        {
            WaterDrinkers.run();
            ReadLoggedStream();
            AssertRightLine("5.000000: 0 ha bevuto!");
            AssertRightLine("10.000000: 1 ha bevuto!");
            AssertRightLine("15.000000: 2 ha bevuto!");
            AssertRightLine("20.000000: 3 ha bevuto!");
            AssertRightLine("25.000000: 4 chiama tecnico");
            AssertRightLine("25.000000: 4 ha bevuto!");
            AssertRightLine("30.000000: 5 ha bevuto!");
            AssertNoMoreLines();
        }

        [Test]
        public void FSharp_SimPy2_Client()
        {
            Client.run();
            ReadLoggedStream();
            AssertRightLine("c1 requests 1 unit at time = 0");
            AssertRightLine("c2 requests 1 unit at time = 0");
            AssertRightLine("c3 requests 1 unit at time = 0");
            AssertRightLine("c4 requests 1 unit at time = 0");
            AssertRightLine("c5 requests 1 unit at time = 0");
            AssertRightLine("c6 requests 1 unit at time = 0");
            AssertRightLine("c1 done at time = 100");
            AssertRightLine("c2 done at time = 100");
            AssertRightLine("c3 done at time = 200");
            AssertRightLine("c4 done at time = 200");
            AssertRightLine("c5 done at time = 300");
            AssertRightLine("c6 done at time = 300");
            AssertRightLine("Request order: ['c1', 'c2', 'c3', 'c4', 'c5', 'c6']");
            AssertRightLine("Service order: ['c1', 'c2', 'c3', 'c4', 'c5', 'c6']");
            AssertNoMoreLines();
        }

        [Test]
        public void FSharp_SimPy2_ClientPriority()
        {
            ClientPriority.run();
            ReadLoggedStream();
            AssertRightLine("c1 requests 1 unit at time = 0");
            AssertRightLine("c2 requests 1 unit at time = 0");
            AssertRightLine("c3 requests 1 unit at time = 0");
            AssertRightLine("c4 requests 1 unit at time = 0");
            AssertRightLine("c5 requests 1 unit at time = 0");
            AssertRightLine("c6 requests 1 unit at time = 0");
            AssertRightLine("c1 done at time = 100");
            AssertRightLine("c2 done at time = 100");
            AssertRightLine("c6 done at time = 200");
            AssertRightLine("c5 done at time = 200");
            AssertRightLine("c4 done at time = 300");
            AssertRightLine("c3 done at time = 300");
            AssertRightLine("Request order: ['c1', 'c2', 'c3', 'c4', 'c5', 'c6']");
            AssertRightLine("Service order: ['c1', 'c2', 'c6', 'c5', 'c4', 'c3']");
            AssertNoMoreLines();
        }

        [Test]
        public void FSharp_SimPy2_Message()
        {
            Examples.FSharp.SimPy2.Message.run();
            ReadLoggedStream();
            AssertRightLine("Starting simulation");
            AssertRightLine("0 1 Starting");
            AssertRightLine("6 2 Starting");
            AssertRightLine("100 1 Arrived");
            AssertRightLine("106 2 Arrived");
            AssertRightLine("Current time is 106");
            AssertNoMoreLines();
        }

        [Test]
        public void FSharp_SimPy3_BankRenege()
        {
            Examples.FSharp.SimPy3.BankRenege.run();
            ReadLoggedStream();
            AssertRightLine("Bank renege");
            AssertRightLine("00.0000 Customer00: Here I am");
            AssertRightLine("00.0000 Customer00: Waited 0.000");
            AssertRightLine("00.6461 Customer01: Here I am");
            AssertRightLine("02.8439 Customer01: RENEGED after 2.198");
            AssertRightLine("16.8235 Customer02: Here I am");
            AssertRightLine("18.5958 Customer00: Finished");
            AssertRightLine("18.5958 Customer02: Waited 1.772");
            AssertRightLine("32.3050 Customer02: Finished");
            AssertRightLine("38.0942 Customer03: Here I am");
            AssertRightLine("38.0942 Customer03: Waited 0.000");
            AssertRightLine("43.8030 Customer03: Finished");
            AssertRightLine("46.8195 Customer04: Here I am");
            AssertRightLine("46.8195 Customer04: Waited 0.000");
            AssertRightLine("50.7801 Customer04: Finished");
            AssertNoMoreLines();
        }

        [Test]
        public void FSharp_SimPy3_Car()
        {
            Car.run();
            ReadLoggedStream();
            AssertRightLine("Start parking at 0");
            AssertRightLine("Start driving at 5");
            AssertRightLine("Start parking at 7");
            AssertRightLine("Start driving at 12");
            AssertRightLine("Start parking at 14");
            AssertNoMoreLines();
        }

        [Test]
        public void FSharp_SimPy3_CarCharge()
        {
            CarCharge.run();
            ReadLoggedStream();
            AssertRightLine("Start parking and charging at 0");
            AssertRightLine("Start driving at 5");
            AssertRightLine("Start parking and charging at 7");
            AssertRightLine("Start driving at 12");
            AssertRightLine("Start parking and charging at 14");
            AssertNoMoreLines();
        }

        [Test]
        public void FSharp_SimPy3_Clock()
        {
            Clock.run();
            ReadLoggedStream();
            AssertRightLine("fast 0.0");
            AssertRightLine("slow 0.0");
            AssertRightLine("fast 0.5");
            AssertRightLine("slow 1.0");
            AssertRightLine("fast 1.0");
            AssertRightLine("fast 1.5");
            AssertNoMoreLines();
        }

        [Test]
        public void VisualBasic_Dessert_EventCallbacks()
        {
            EventCallbacks.Run();
            ReadLoggedStream();
            AssertRightLine("Successo: 'True'; Valore: 'SI'");
            AssertRightLine("Successo: 'False'; Valore: 'NO'");
            AssertNoMoreLines();
        }

        [Test]
        public void VisualBasic_Dessert_HelloWorld()
        {
            Examples.VisualBasic.HelloWorld.Run();
            ReadLoggedStream();
            AssertRightLine("Hello World simulation :)");
            AssertRightLine("Hello World at 2.1!");
            AssertRightLine("Hello World at 4.2!");
            AssertRightLine("Hello World at 6.3!");
            AssertRightLine("Hello World at 8.4!");
            AssertNoMoreLines();
        }

        [Test]
        public void VisualBasic_Dessert_Hospital()
        {
            Hospital.Run();
            ReadLoggedStream();
            AssertRightLine("Pino viene curato...");
            AssertRightLine("Gino viene curato...");
            AssertRightLine("Tino viene curato...");
            AssertRightLine("Dino viene curato...");
            AssertRightLine("Nino viene curato...");
            AssertNoMoreLines();
        }

        [Test]
        public void VisualBasic_Dessert_HospitalPreemption()
        {
            HospitalPreemption.Run();
            ReadLoggedStream();
            AssertRightLine("Pino viene curato...");
            AssertRightLine("Gino viene curato...");
            AssertRightLine("Gino scavalcato da Tino");
            AssertRightLine("Tino viene curato...");
            AssertRightLine("Cure finite per Pino");
            AssertRightLine("Dino viene curato...");
            AssertRightLine("Cure finite per Tino");
            AssertRightLine("Nino viene curato...");
            AssertRightLine("Cure finite per Dino");
            AssertRightLine("Cure finite per Nino");
            AssertNoMoreLines();
        }

        [Test]
        public void VisualBasic_Dessert_ProducerConsumer()
        {
            ProducerConsumer.Run();
            ReadLoggedStream();
            AssertRightLine("1: Prodotto un 13");
            AssertRightLine("6: Consumato un 13");
            AssertRightLine("15: Prodotto un 1");
            AssertRightLine("15: Consumato un 1");
            AssertRightLine("16: Prodotto un 14");
            AssertRightLine("17: Prodotto un 3");
            AssertRightLine("19: Consumato un 14");
            AssertRightLine("23: Prodotto un 13");
            AssertRightLine("36: Consumato un 3");
            AssertRightLine("36: Prodotto un 6");
            AssertRightLine("48: Consumato un 13");
            AssertRightLine("48: Prodotto un 16");
            AssertRightLine("57: Consumato un 6");
            AssertRightLine("57: Prodotto un 2");
            AssertRightLine("59: Consumato un 16");
            AssertNoMoreLines();
        }

        [Test]
        public void VisualBasic_Dessert_ProducerFilteredConsumer()
        {
            ProducerFilteredConsumer.Run();
            ReadLoggedStream();
            AssertRightLine("1: Prodotto un 14");
            AssertRightLine("2: Prodotto un 1");
            AssertRightLine("6: PARI, consumato un 14");
            AssertRightLine("13: DISPARI, consumato un 1");
            AssertRightLine("16: Prodotto un 1");
            AssertRightLine("19: Prodotto un 6");
            AssertRightLine("19: PARI, consumato un 6");
            AssertRightLine("21: Prodotto un 7");
            AssertRightLine("27: DISPARI, consumato un 1");
            AssertRightLine("27: Prodotto un 16");
            AssertRightLine("32: PARI, consumato un 16");
            AssertRightLine("32: Prodotto un 2");
            AssertRightLine("39: DISPARI, consumato un 7");
            AssertRightLine("39: Prodotto un 2");
            AssertRightLine("41: PARI, consumato un 2");
            AssertRightLine("45: PARI, consumato un 2");
            AssertRightLine("49: Prodotto un 10");
            AssertRightLine("53: PARI, consumato un 10");
            AssertRightLine("54: Prodotto un 19");
            AssertRightLine("54: DISPARI, consumato un 19");
            AssertRightLine("58: Prodotto un 19");
            AssertNoMoreLines();
        }

        [Test]
        public void VisualBasic_Dessert_ValueUsage()
        {
            ValueUsage.Run();
            ReadLoggedStream();
            AssertRightLine("BORING");
            AssertRightLine("12.5");
            AssertNoMoreLines();
        }

        [Test]
        public void VisualBasic_SimPy3_Clock()
        {
            Examples.VisualBasic.SimPy3.ClockExample.Run();
            ReadLoggedStream();
            AssertRightLine("fast 0.0");
            AssertRightLine("slow 0.0");
            AssertRightLine("fast 0.5");
            AssertRightLine("slow 1.0");
            AssertRightLine("fast 1.0");
            AssertRightLine("fast 1.5");
            AssertNoMoreLines();
        }

        [Test]
        public void VisualBasic_SimPy3_EventLatency()
        {
            EventLatency.Run();
            ReadLoggedStream();
            AssertRightLine("Event Latency");
            AssertRightLine("Received this at 15 while Sender sent this at 5");
            AssertRightLine("Received this at 20 while Sender sent this at 10");
            AssertRightLine("Received this at 25 while Sender sent this at 15");
            AssertRightLine("Received this at 30 while Sender sent this at 20");
            AssertRightLine("Received this at 35 while Sender sent this at 25");
            AssertRightLine("Received this at 40 while Sender sent this at 30");
            AssertRightLine("Received this at 45 while Sender sent this at 35");
            AssertRightLine("Received this at 50 while Sender sent this at 40");
            AssertRightLine("Received this at 55 while Sender sent this at 45");
            AssertRightLine("Received this at 60 while Sender sent this at 50");
            AssertRightLine("Received this at 65 while Sender sent this at 55");
            AssertRightLine("Received this at 70 while Sender sent this at 60");
            AssertRightLine("Received this at 75 while Sender sent this at 65");
            AssertRightLine("Received this at 80 while Sender sent this at 70");
            AssertRightLine("Received this at 85 while Sender sent this at 75");
            AssertRightLine("Received this at 90 while Sender sent this at 80");
            AssertRightLine("Received this at 95 while Sender sent this at 85");
            AssertNoMoreLines();
        }
    }
}

#endif