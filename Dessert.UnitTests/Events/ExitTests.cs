// 
// ExitTests.cs
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

namespace DIBRIS.Dessert.Tests.Events
{
    using NUnit.Framework;
    using SimEvents = System.Collections.Generic.IEnumerable<SimEvent>;

    sealed class ExitTests : TestBase
    {
        SimEvents ExitYielder(object value)
        {
            yield return Env.Exit(value);
            Assert.Fail();
        }

        SimEvents ExitYielder_InnerProcess(object value)
        {
            var process = Env.Process(ExitYielder(value));
            yield return process;
            Assert.AreEqual(value, process.Value);
            yield return Env.Exit(process.Value);
            Assert.Fail();
        }

        [Test]
        public void Exit_CommonYielder()
        {
            const string exitValue = "PINO";
            var process = Env.Process(ExitYielder(exitValue));
            Env.Run();
            Assert.AreEqual(exitValue, process.Value);
        }

        [Test]
        public void Exit_InnerYielder()
        {
            const string value = "PINO";
            var process = Env.Process(ExitYielder_InnerProcess(value));
            Env.Run();
            Assert.AreEqual(value, process.Value);
        }
    }
}