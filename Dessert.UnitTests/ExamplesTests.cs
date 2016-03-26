// ExamplesTests.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com> Giovanni Lagorio <giovanni.lagorio@gmail.com>
//
// Copyright (c) 2012-2016 Alessio Parma <alessio.parma@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if NET40
    // Empty, used because examples are not included in the .NET 4 solution.
#else

namespace DIBRIS.Dessert.Tests
{
    using Examples.CSharp;
    using Examples.CSharp.SimPy2;
    using Examples.CSharp.SimPy3;
    using Examples.FSharp;
    using Examples.FSharp.SimPy2;
    using Examples.FSharp.SimPy3;
    using Examples.VisualBasic;
    using Examples.VisualBasic.SimPy3;
    using NUnit.Framework;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using BankRenege = Examples.CSharp.SimPy3.BankRenege;
    using ClockExample = Examples.CSharp.SimPy3.ClockExample;
    using HelloWorld = Examples.CSharp.HelloWorld;
    using Message = Examples.CSharp.SimPy2.Message;

    [TestFixture]
    sealed class ExamplesTests
    {
        private static readonly CultureInfo EnUsCulture = new CultureInfo("en-US");

        [SetUp]
        public void SetUp()
        {
            // Force en-US culture.
            Thread.CurrentThread.CurrentCulture = EnUsCulture;
            Thread.CurrentThread.CurrentUICulture = EnUsCulture;

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
            _lines = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).GetEnumerator();
        }

        sealed class FormattedWriter : StreamWriter
        {
            public FormattedWriter(Stream stream) : base(stream)
            {
            }

            public override IFormatProvider FormatProvider => EnUsCulture;
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
        public void CSharp_Dessert_MachineSensorsMonitoring()
        {
            MachineSensorsMonitoring.Run();
            ReadLoggedStream();
            AssertRightLine("Machine A has powered up");
            AssertRightLine("All sensors for machine A are active");
            AssertRightLine("Machine A has started work cycle 1");
            AssertRightLine("Machine B has powered up");
            AssertRightLine("All sensors for machine B are active");
            AssertRightLine("Machine B has started work cycle 1");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1049.84 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 116 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 967.83 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 108 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1098.67 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 98 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 955.66 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 97 °F");
            AssertRightLine("Machine A has ended work cycle 1");
            AssertRightLine("Machine A has started work cycle 2");
            AssertRightLine("Machine B has ended work cycle 1");
            AssertRightLine("Machine B has started work cycle 2");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1041.94 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 101 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 1020.73 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 107 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 970.67 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 90 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 1044.08 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 110 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1012.59 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 106 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 960.82 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 107 °F");
            AssertRightLine("Machine A has ended work cycle 2");
            AssertRightLine("Machine A has started work cycle 3");
            AssertRightLine("Machine B has ended work cycle 2");
            AssertRightLine("Machine B has started work cycle 3");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1012.51 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 101 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 1051.31 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 100 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1041.60 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 96 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 974.64 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 92 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 989.34 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 103 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 998.58 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 91 °F");
            AssertRightLine("Machine A has ended work cycle 3");
            AssertRightLine("Machine A has started work cycle 4");
            AssertRightLine("Machine B has ended work cycle 3");
            AssertRightLine("Machine B has started work cycle 4");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1001.04 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 98 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 889.92 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 104 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 974.86 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 105 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 1034.73 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 89 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 992.05 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 117 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 1021.72 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 102 °F");
            AssertRightLine("Machine A has ended work cycle 4");
            AssertRightLine("Machine A has started work cycle 5");
            AssertRightLine("Machine B has ended work cycle 4");
            AssertRightLine("Machine B has started work cycle 5");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 971.61 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 95 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 979.54 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 100 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1072.05 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 91 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 968.02 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 103 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1042.59 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 97 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 1047.13 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 105 °F");
            AssertRightLine("Machine A has ended work cycle 5");
            AssertRightLine("Machine A has started work cycle 6");
            AssertRightLine("Machine B has ended work cycle 5");
            AssertRightLine("Machine B has started work cycle 6");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1005.22 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 100 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 990.29 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 102 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1085.14 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 94 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 959.36 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 100 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1038.38 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 93 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 1025.65 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 114 °F");
            AssertRightLine("Machine A has ended work cycle 6");
            AssertRightLine("Machine A has started work cycle 7");
            AssertRightLine("Machine B has ended work cycle 6");
            AssertRightLine("Machine B has started work cycle 7");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1050.31 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 107 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 981.72 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 104 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1007.29 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 98 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 943.18 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 103 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 988.34 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 104 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 914.24 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 84 °F");
            AssertRightLine("Machine A has ended work cycle 7");
            AssertRightLine("Machine A has started work cycle 8");
            AssertRightLine("Machine B has ended work cycle 7");
            AssertRightLine("Machine B has started work cycle 8");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 980.92 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 103 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 927.73 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 96 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1043.72 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 99 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 974.55 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 101 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1016.03 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 99 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 993.43 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 108 °F");
            AssertRightLine("Machine A has ended work cycle 8");
            AssertRightLine("Machine A has started work cycle 9");
            AssertRightLine("Machine B has ended work cycle 8");
            AssertRightLine("Machine B has started work cycle 9");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1004.31 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 85 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 1042.83 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 86 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 993.43 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 96 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 1063.74 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 102 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 921.31 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 116 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 952.20 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 92 °F");
            AssertRightLine("Machine A has ended work cycle 9");
            AssertRightLine("Machine A has started work cycle 10");
            AssertRightLine("Machine B has ended work cycle 9");
            AssertRightLine("Machine B has started work cycle 10");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 969.27 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 96 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 957.86 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 97 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 1121.96 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 111 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 1052.07 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 99 °F");
            AssertRightLine("Pressure sensor for machine A has recorded a pressure of 921.35 bar");
            AssertRightLine("Temperature sensor for machine A has recorded a temperature of 90 °F");
            AssertRightLine("Pressure sensor for machine B has recorded a pressure of 1036.28 bar");
            AssertRightLine("Temperature sensor for machine B has recorded a temperature of 123 °F");
            AssertRightLine("Machine A average pressure: 1014.43 bar");
            AssertRightLine("Machine A average temperature: 100.17 °F");
            AssertRightLine("Machine B average pressure: 990.68 bar");
            AssertRightLine("Machine B average temperature: 100.90 °F");
            AssertRightLine("Machine B pressure values: 967.83, 955.66, 1020.73, 1044.08, 960.82, 1051.31, 974.64, 998.58, 889.92, 1034.73, 1021.72, 979.54, 968.02, 1047.13, 990.29, 959.36, 1025.65, 981.72, 943.18, 914.24, 927.73, 974.55, 993.43, 1042.83, 1063.74, 952.20, 957.86, 1052.07, 1036.28");
            AssertRightLine("Machine B temperature values: 108, 97, 107, 110, 107, 100, 92, 91, 104, 89, 102, 100, 103, 105, 102, 100, 114, 104, 103, 84, 96, 101, 108, 86, 102, 92, 97, 99, 123");
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
            AssertRightLine("Hi, I'm Nino and I entered the office at 0.00");
            AssertRightLine("Finally it's Nino's turn! I waited 0.00 minutes");
            AssertRightLine("Nino's job will take 3.58 minutes");
            AssertRightLine("Hi, I'm Dino and I entered the office at 0.28");
            AssertRightLine("Finally it's Dino's turn! I waited 0.00 minutes");
            AssertRightLine("Dino's job will take 1.77 minutes");
            AssertRightLine("Hi, I'm Gino and I entered the office at 0.52");
            AssertRightLine("Ok, Dino leaves the office at 2.05. Bye :)");
            AssertRightLine("Finally it's Gino's turn! I waited 1.53 minutes");
            AssertRightLine("Gino's job will take 13.31 minutes");
            AssertRightLine("Hi, I'm Pino and I entered the office at 3.27");
            AssertRightLine("Ok, Nino leaves the office at 3.58. Bye :)");
            AssertRightLine("Finally it's Pino's turn! I waited 0.31 minutes");
            AssertRightLine("Pino's job will take 1.01 minutes");
            AssertRightLine("Hi, I'm Gino and I entered the office at 4.55");
            AssertRightLine("Ok, Pino leaves the office at 4.59. Bye :)");
            AssertRightLine("Finally it's Gino's turn! I waited 0.03 minutes");
            AssertRightLine("Gino's job will take 2.65 minutes");
            AssertRightLine("Ok, Gino leaves the office at 7.24. Bye :)");
            AssertRightLine("Hi, I'm Nino and I entered the office at 7.34");
            AssertRightLine("Finally it's Nino's turn! I waited 0.00 minutes");
            AssertRightLine("Nino's job will take 14.47 minutes");
            AssertRightLine("Hi, I'm Nino and I entered the office at 7.60");
            AssertRightLine("Hi, I'm John and I entered the office at 8.41");
            AssertRightLine("Hi, I'm John and I entered the office at 14.27");
            AssertRightLine("Ok, Gino leaves the office at 15.36. Bye :)");
            AssertRightLine("Finally it's Nino's turn! I waited 7.76 minutes");
            AssertRightLine("Nino's job will take 1.09 minutes");
            AssertRightLine("Ok, Nino leaves the office at 16.45. Bye :)");
            AssertRightLine("Finally it's John's turn! I waited 8.04 minutes");
            AssertRightLine("John's job will take 2.61 minutes");
            AssertRightLine("Hi, I'm Gino and I entered the office at 17.89");
            AssertRightLine("Ok, John leaves the office at 19.06. Bye :)");
            AssertRightLine("Finally it's John's turn! I waited 4.79 minutes");
            AssertRightLine("John's job will take 0.86 minutes");
            AssertRightLine("Ok, John leaves the office at 19.91. Bye :)");
            AssertRightLine("Finally it's Gino's turn! I waited 2.03 minutes");
            AssertRightLine("Gino's job will take 2.86 minutes");
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
            AssertRightLine("0.22: Uomo1 in coda");
            AssertRightLine("0.22: Uomo1 --> Bagno");
            AssertRightLine("0.55: Donna2 in coda");
            AssertRightLine("0.60: Donna3 in coda");
            AssertRightLine("0.96: Donna4 in coda");
            AssertRightLine("2.81: Donna5 in coda");
            AssertRightLine("2.98: Uomo6 in coda");
            AssertRightLine("4.06: Donna7 in coda");
            AssertRightLine("4.25: Uomo8 in coda");
            AssertRightLine("6.00: Donna9 in coda");
            AssertRightLine("6.20: Donna0 <-- Bagno");
            AssertRightLine("6.20: Donna2 --> Bagno");
            AssertRightLine("6.77: Donna10 in coda");
            AssertRightLine("6.92: Donna2 <-- Bagno");
            AssertRightLine("6.92: Donna3 --> Bagno");
            AssertRightLine("8.21: Donna3 <-- Bagno");
            AssertRightLine("8.21: Donna4 --> Bagno");
            AssertRightLine("8.78: Donna11 in coda");
            AssertRightLine("9.48: Donna12 in coda");
            AssertRightLine("9.63: Uomo13 in coda");
            AssertRightLine("9.91: Donna14 in coda");
            AssertRightLine("9.94: Uomo15 in coda");
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
            AssertRightLine("Treno in viaggio per 30.22 minuti");
            AssertRightLine("Arrivo in stazione, attesa passeggeri");
            AssertRightLine("Treno in viaggio per 2.24 minuti");
            AssertRightLine("Arrivo in stazione, attesa passeggeri");
            AssertRightLine("Treno in viaggio per 12.41 minuti");
            AssertRightLine("Arrivo in stazione, attesa passeggeri");
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
            AssertRightLine("02.8640 Customer00: Finished");
            AssertRightLine("27.7147 Customer01: Here I am");
            AssertRightLine("27.7147 Customer01: Waited 0.000");
            AssertRightLine("29.2376 Customer01: Finished");
            AssertRightLine("29.9255 Customer02: Here I am");
            AssertRightLine("29.9255 Customer02: Waited 0.000");
            AssertRightLine("36.4186 Customer02: Finished");
            AssertRightLine("38.2005 Customer03: Here I am");
            AssertRightLine("38.2005 Customer03: Waited 0.000");
            AssertRightLine("57.3872 Customer04: Here I am");
            AssertRightLine("58.9494 Customer04: RENEGED after 1.562");
            AssertRightLine("59.2723 Customer03: Finished");
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
            AssertRightLine("Car 0 arriving at gas station at 150.0");
            AssertRightLine("Car 0 finished refueling in 18.5 seconds.");
            AssertRightLine("Car 1 arriving at gas station at 414.0");
            AssertRightLine("Car 1 finished refueling in 14.0 seconds.");
            AssertRightLine("Car 2 arriving at gas station at 587.0");
            AssertRightLine("Car 2 finished refueling in 20.0 seconds.");
            AssertRightLine("Car 3 arriving at gas station at 790.0");
            AssertRightLine("Car 3 finished refueling in 22.5 seconds.");
            AssertRightLine("Car 4 arriving at gas station at 888.0");
            AssertRightLine("Calling tank truck at 890");
            AssertRightLine("Car 4 finished refueling in 17.5 seconds.");
            AssertRightLine("Car 5 arriving at gas station at 942.0");
            AssertRightLine("Car 6 arriving at gas station at 1181.0");
            AssertRightLine("Tank truck arriving at time 1190");
            AssertRightLine("Tank truck refueling 185.0 liters.");
            AssertRightLine("Car 6 finished refueling in 26.5 seconds.");
            AssertRightLine("Car 5 finished refueling in 268.0 seconds.");
            AssertRightLine("Car 7 arriving at gas station at 1278.0");
            AssertRightLine("Car 7 finished refueling in 14.0 seconds.");
            AssertRightLine("Car 8 arriving at gas station at 1403.0");
            AssertRightLine("Car 8 finished refueling in 18.0 seconds.");
            AssertRightLine("Car 9 arriving at gas station at 1440.0");
            AssertRightLine("Calling tank truck at 1450");
            AssertRightLine("Car 9 finished refueling in 13.0 seconds.");
            AssertRightLine("Car 10 arriving at gas station at 1650.0");
            AssertRightLine("Car 10 finished refueling in 13.0 seconds.");
            AssertRightLine("Car 11 arriving at gas station at 1694.0");
            AssertRightLine("Tank truck arriving at time 1750");
            AssertRightLine("Tank truck refueling 191.0 liters.");
            AssertRightLine("Car 11 finished refueling in 71.0 seconds.");
            AssertRightLine("Car 12 arriving at gas station at 1768.0");
            AssertRightLine("Car 12 finished refueling in 20.5 seconds.");
            AssertRightLine("Car 13 arriving at gas station at 1938.0");
            AssertRightLine("Car 13 finished refueling in 14.5 seconds.");
            AssertNoMoreLines();
        }

        [Test]
        public void CSharp_SimPy3_MovieRenege()
        {
            MovieRenege.Run();
            ReadLoggedStream();
            AssertRightLine("Movie renege");
            AssertRightLine("Movie \".NET Unchained\" sold out 46.3 minutes after ticket counter opening.");
            AssertRightLine("  Number of people leaving queue when film sold out: 15");
            AssertRightLine("Movie \"Kill Process\" sold out 49.3 minutes after ticket counter opening.");
            AssertRightLine("  Number of people leaving queue when film sold out: 18");
            AssertRightLine("Movie \"Pulp Implementation\" sold out 39.8 minutes after ticket counter opening.");
            AssertRightLine("  Number of people leaving queue when film sold out: 12");
            AssertNoMoreLines();
        }

        [Test]
        public void FSharp_Dessert_BankExample()
        {
            BankExample.run();
            ReadLoggedStream();
            AssertRightLine("Finanze totali al tempo 300.00: 8684");
            AssertRightLine("Clienti entrati: 108");
            AssertRightLine("Clienti serviti: 79");
            AssertRightLine("Tempo medio di attesa: 17.19");
            AssertRightLine("Tempo medio di servizio: 9.01");
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
            AssertRightLine("02.8640 Customer00: Finished");
            AssertRightLine("27.7147 Customer01: Here I am");
            AssertRightLine("27.7147 Customer01: Waited 0.000");
            AssertRightLine("29.2376 Customer01: Finished");
            AssertRightLine("29.9255 Customer02: Here I am");
            AssertRightLine("29.9255 Customer02: Waited 0.000");
            AssertRightLine("36.4186 Customer02: Finished");
            AssertRightLine("38.2005 Customer03: Here I am");
            AssertRightLine("38.2005 Customer03: Waited 0.000");
            AssertRightLine("57.3872 Customer04: Here I am");
            AssertRightLine("58.9494 Customer04: RENEGED after 1.562");
            AssertRightLine("59.2723 Customer03: Finished");
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
