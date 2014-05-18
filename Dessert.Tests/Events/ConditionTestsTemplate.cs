// 
// ConditionTests.cs
//  
// Author(s):
//       Alessio Parma <alessio.parma@gmail.com>
// 
// Copyright (c) 2012-2014 Alessio Parma <alessio.parma@gmail.com>
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

namespace Dessert.Tests.Events
{
	using System.Collections.Generic;
	using System.Diagnostics;
    using Dessert.Events;
    using NUnit.Framework;

	partial class ConditionTests
	{
		IEnumerable<SimEvent> AndConditionChecker<T1, T2>(BoolWrapper finished, Condition<T1, T2> cond, T1 ev1, T2 ev2) 
            where T1 : SimEvent
            where T2 : SimEvent
		{
			Debug.Assert(!finished);
			yield return cond;
			Assert.AreEqual(2, cond.Value.Count);
			Assert.AreSame(Env, cond.Env);
			Assert.AreSame(ev1, cond.Ev1);
			Assert.AreSame(Env, cond.Ev1.Env);
			Assert.True(cond.Ev1 && cond.Ev1.Succeeded);
			Assert.AreSame(ev2, cond.Ev2);
			Assert.AreSame(Env, cond.Ev2.Env);
			Assert.True(cond.Ev2 && cond.Ev2.Succeeded);
			finished.Value = true;
		}

		[Test]
		public void AllOf_2SingleTimeouts()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var finished = new BoolWrapper();
			var cond = Env.AllOf(t1, t2);
			Env.Process(AndConditionChecker(finished, cond, t1, t2));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_2SingleTimeouts_Method_LeftToRight()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var finished = new BoolWrapper();
			var cond = t1.And(t2);
			Env.Process(AndConditionChecker(finished, cond, t1, t2));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_2SingleTimeouts_Operator_LeftToRight()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			var finished  = new BoolWrapper();
			var cond = t1 & t2;
			Env.Process(AndConditionChecker(finished, cond, t1, t2));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_2SingleTimeouts_Method_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.And(t2);
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void And_2SingleTimeouts_Operator_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 & t2;
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_2SingleTimeouts_Method_RightToLeft()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var finished = new BoolWrapper();
			var cond = t1.And(t2);
			Env.Process(AndConditionChecker(finished, cond, t1, t2));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_2SingleTimeouts_Operator_RightToLeft()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			var finished  = new BoolWrapper();
			var cond = t1 & (t2);
			Env.Process(AndConditionChecker(finished, cond, t1, t2));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_2SingleTimeouts_Method_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.And(t2);
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void And_2SingleTimeouts_Operator_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 & (t2);
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		IEnumerable<SimEvent> OrConditionChecker<T1, T2>(BoolWrapper finished, Condition<T1, T2> cond, T1 ev1, T2 ev2) 
            where T1 : SimEvent
            where T2 : SimEvent
		{
			Debug.Assert(!finished);
			yield return cond;
			Assert.AreEqual(1, cond.Value.Count);
			Assert.AreSame(Env, cond.Env);
			Assert.AreSame(ev1, cond.Ev1);
			Assert.AreSame(Env, cond.Ev1.Env);
			Assert.True(cond.Ev1 && cond.Ev1.Succeeded);
			Assert.AreSame(ev2, cond.Ev2);
			Assert.AreSame(Env, cond.Ev2.Env);
			Assert.False(cond.Ev2 || cond.Ev2.Succeeded);
			finished.Value = true;
		}

		[Test]
		public void AnyOf_2SingleTimeouts()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var finished = new BoolWrapper();
			var cond = Env.AnyOf(t1, t2);
			Env.Process(OrConditionChecker(finished, cond, t1, t2));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_2SingleTimeouts_Method_LeftToRight()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2);
			Env.Process(OrConditionChecker(finished, cond, t1, t2));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_2SingleTimeouts_Operator_LeftToRight()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			var finished  = new BoolWrapper();
			var cond = t1 | t2;
			Env.Process(OrConditionChecker(finished, cond, t1, t2));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_2SingleTimeouts_Method_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2);
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void Or_2SingleTimeouts_Operator_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 | t2;
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_2SingleTimeouts_Method_RightToLeft()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2);
			Env.Process(OrConditionChecker(finished, cond, t1, t2));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_2SingleTimeouts_Operator_RightToLeft()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			var finished  = new BoolWrapper();
			var cond = t1 | (t2);
			Env.Process(OrConditionChecker(finished, cond, t1, t2));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_2SingleTimeouts_Method_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2);
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void Or_2SingleTimeouts_Operator_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 | (t2);
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		IEnumerable<SimEvent> AndConditionChecker<T1, T2, T3>(BoolWrapper finished, Condition<T1, T2, T3> cond, T1 ev1, T2 ev2, T3 ev3) 
            where T1 : SimEvent
            where T2 : SimEvent
            where T3 : SimEvent
		{
			Debug.Assert(!finished);
			yield return cond;
			Assert.AreEqual(3, cond.Value.Count);
			Assert.AreSame(Env, cond.Env);
			Assert.AreSame(ev1, cond.Ev1);
			Assert.AreSame(Env, cond.Ev1.Env);
			Assert.True(cond.Ev1 && cond.Ev1.Succeeded);
			Assert.AreSame(ev2, cond.Ev2);
			Assert.AreSame(Env, cond.Ev2.Env);
			Assert.True(cond.Ev2 && cond.Ev2.Succeeded);
			Assert.AreSame(ev3, cond.Ev3);
			Assert.AreSame(Env, cond.Ev3.Env);
			Assert.True(cond.Ev3 && cond.Ev3.Succeeded);
			finished.Value = true;
		}

		[Test]
		public void AllOf_3SingleTimeouts()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var finished = new BoolWrapper();
			var cond = Env.AllOf(t1, t2, t3);
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_3SingleTimeouts_Method_LeftToRight()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var finished = new BoolWrapper();
			var cond = t1.And(t2).And(t3);
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_3SingleTimeouts_Operator_LeftToRight()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			var finished  = new BoolWrapper();
			var cond = t1 & t2 & t3;
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_3SingleTimeouts_Method_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.And(t2).And(t3);
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void And_3SingleTimeouts_Operator_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 & t2 & t3;
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_3SingleTimeouts_Method_RightToLeft()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var finished = new BoolWrapper();
			var cond = t1.And(t2.And(t3));
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_3SingleTimeouts_Operator_RightToLeft()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			var finished  = new BoolWrapper();
			var cond = t1 & (t2 & (t3));
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_3SingleTimeouts_Method_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.And(t2.And(t3));
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void And_3SingleTimeouts_Operator_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 & (t2 & (t3));
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		IEnumerable<SimEvent> OrConditionChecker<T1, T2, T3>(BoolWrapper finished, Condition<T1, T2, T3> cond, T1 ev1, T2 ev2, T3 ev3) 
            where T1 : SimEvent
            where T2 : SimEvent
            where T3 : SimEvent
		{
			Debug.Assert(!finished);
			yield return cond;
			Assert.AreEqual(1, cond.Value.Count);
			Assert.AreSame(Env, cond.Env);
			Assert.AreSame(ev1, cond.Ev1);
			Assert.AreSame(Env, cond.Ev1.Env);
			Assert.True(cond.Ev1 && cond.Ev1.Succeeded);
			Assert.AreSame(ev2, cond.Ev2);
			Assert.AreSame(Env, cond.Ev2.Env);
			Assert.False(cond.Ev2 || cond.Ev2.Succeeded);
			Assert.AreSame(ev3, cond.Ev3);
			Assert.AreSame(Env, cond.Ev3.Env);
			Assert.False(cond.Ev3 || cond.Ev3.Succeeded);
			finished.Value = true;
		}

		[Test]
		public void AnyOf_3SingleTimeouts()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var finished = new BoolWrapper();
			var cond = Env.AnyOf(t1, t2, t3);
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_3SingleTimeouts_Method_LeftToRight()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2).Or(t3);
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_3SingleTimeouts_Operator_LeftToRight()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			var finished  = new BoolWrapper();
			var cond = t1 | t2 | t3;
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_3SingleTimeouts_Method_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2).Or(t3);
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void Or_3SingleTimeouts_Operator_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 | t2 | t3;
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_3SingleTimeouts_Method_RightToLeft()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2.Or(t3));
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_3SingleTimeouts_Operator_RightToLeft()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			var finished  = new BoolWrapper();
			var cond = t1 | (t2 | (t3));
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_3SingleTimeouts_Method_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2.Or(t3));
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void Or_3SingleTimeouts_Operator_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 | (t2 | (t3));
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		IEnumerable<SimEvent> AndConditionChecker<T1, T2, T3, T4>(BoolWrapper finished, Condition<T1, T2, T3, T4> cond, T1 ev1, T2 ev2, T3 ev3, T4 ev4) 
            where T1 : SimEvent
            where T2 : SimEvent
            where T3 : SimEvent
            where T4 : SimEvent
		{
			Debug.Assert(!finished);
			yield return cond;
			Assert.AreEqual(4, cond.Value.Count);
			Assert.AreSame(Env, cond.Env);
			Assert.AreSame(ev1, cond.Ev1);
			Assert.AreSame(Env, cond.Ev1.Env);
			Assert.True(cond.Ev1 && cond.Ev1.Succeeded);
			Assert.AreSame(ev2, cond.Ev2);
			Assert.AreSame(Env, cond.Ev2.Env);
			Assert.True(cond.Ev2 && cond.Ev2.Succeeded);
			Assert.AreSame(ev3, cond.Ev3);
			Assert.AreSame(Env, cond.Ev3.Env);
			Assert.True(cond.Ev3 && cond.Ev3.Succeeded);
			Assert.AreSame(ev4, cond.Ev4);
			Assert.AreSame(Env, cond.Ev4.Env);
			Assert.True(cond.Ev4 && cond.Ev4.Succeeded);
			finished.Value = true;
		}

		[Test]
		public void AllOf_4SingleTimeouts()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var finished = new BoolWrapper();
			var cond = Env.AllOf(t1, t2, t3, t4);
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3, t4));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_4SingleTimeouts_Method_LeftToRight()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var finished = new BoolWrapper();
			var cond = t1.And(t2).And(t3).And(t4);
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3, t4));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_4SingleTimeouts_Operator_LeftToRight()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			SimEvent t4 = Env.Timeout(4);
			var finished  = new BoolWrapper();
			var cond = t1 & t2 & t3 & t4;
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3, t4));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_4SingleTimeouts_Method_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.And(t2).And(t3).And(t4);
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void And_4SingleTimeouts_Operator_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 & t2 & t3 & t4;
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_4SingleTimeouts_Method_RightToLeft()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var finished = new BoolWrapper();
			var cond = t1.And(t2.And(t3.And(t4)));
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3, t4));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_4SingleTimeouts_Operator_RightToLeft()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			SimEvent t4 = Env.Timeout(4);
			var finished  = new BoolWrapper();
			var cond = t1 & (t2 & (t3 & (t4)));
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3, t4));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_4SingleTimeouts_Method_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.And(t2.And(t3.And(t4)));
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void And_4SingleTimeouts_Operator_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 & (t2 & (t3 & (t4)));
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		IEnumerable<SimEvent> OrConditionChecker<T1, T2, T3, T4>(BoolWrapper finished, Condition<T1, T2, T3, T4> cond, T1 ev1, T2 ev2, T3 ev3, T4 ev4) 
            where T1 : SimEvent
            where T2 : SimEvent
            where T3 : SimEvent
            where T4 : SimEvent
		{
			Debug.Assert(!finished);
			yield return cond;
			Assert.AreEqual(1, cond.Value.Count);
			Assert.AreSame(Env, cond.Env);
			Assert.AreSame(ev1, cond.Ev1);
			Assert.AreSame(Env, cond.Ev1.Env);
			Assert.True(cond.Ev1 && cond.Ev1.Succeeded);
			Assert.AreSame(ev2, cond.Ev2);
			Assert.AreSame(Env, cond.Ev2.Env);
			Assert.False(cond.Ev2 || cond.Ev2.Succeeded);
			Assert.AreSame(ev3, cond.Ev3);
			Assert.AreSame(Env, cond.Ev3.Env);
			Assert.False(cond.Ev3 || cond.Ev3.Succeeded);
			Assert.AreSame(ev4, cond.Ev4);
			Assert.AreSame(Env, cond.Ev4.Env);
			Assert.False(cond.Ev4 || cond.Ev4.Succeeded);
			finished.Value = true;
		}

		[Test]
		public void AnyOf_4SingleTimeouts()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var finished = new BoolWrapper();
			var cond = Env.AnyOf(t1, t2, t3, t4);
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3, t4));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_4SingleTimeouts_Method_LeftToRight()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2).Or(t3).Or(t4);
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3, t4));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_4SingleTimeouts_Operator_LeftToRight()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			SimEvent t4 = Env.Timeout(4);
			var finished  = new BoolWrapper();
			var cond = t1 | t2 | t3 | t4;
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3, t4));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_4SingleTimeouts_Method_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2).Or(t3).Or(t4);
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void Or_4SingleTimeouts_Operator_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 | t2 | t3 | t4;
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_4SingleTimeouts_Method_RightToLeft()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2.Or(t3.Or(t4)));
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3, t4));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_4SingleTimeouts_Operator_RightToLeft()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			SimEvent t4 = Env.Timeout(4);
			var finished  = new BoolWrapper();
			var cond = t1 | (t2 | (t3 | (t4)));
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3, t4));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_4SingleTimeouts_Method_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2.Or(t3.Or(t4)));
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1));
			Env.Run();
			Assert.True(finished);
		}
		[Test]
		public void Or_4SingleTimeouts_Operator_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 | (t2 | (t3 | (t4)));
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		IEnumerable<SimEvent> AndConditionChecker<T1, T2, T3, T4, T5>(BoolWrapper finished, Condition<T1, T2, T3, T4, T5> cond, T1 ev1, T2 ev2, T3 ev3, T4 ev4, T5 ev5) 
            where T1 : SimEvent
            where T2 : SimEvent
            where T3 : SimEvent
            where T4 : SimEvent
            where T5 : SimEvent
		{
			Debug.Assert(!finished);
			yield return cond;
			Assert.AreEqual(5, cond.Value.Count);
			Assert.AreSame(Env, cond.Env);
			Assert.AreSame(ev1, cond.Ev1);
			Assert.AreSame(Env, cond.Ev1.Env);
			Assert.True(cond.Ev1 && cond.Ev1.Succeeded);
			Assert.AreSame(ev2, cond.Ev2);
			Assert.AreSame(Env, cond.Ev2.Env);
			Assert.True(cond.Ev2 && cond.Ev2.Succeeded);
			Assert.AreSame(ev3, cond.Ev3);
			Assert.AreSame(Env, cond.Ev3.Env);
			Assert.True(cond.Ev3 && cond.Ev3.Succeeded);
			Assert.AreSame(ev4, cond.Ev4);
			Assert.AreSame(Env, cond.Ev4.Env);
			Assert.True(cond.Ev4 && cond.Ev4.Succeeded);
			Assert.AreSame(ev5, cond.Ev5);
			Assert.AreSame(Env, cond.Ev5.Env);
			Assert.True(cond.Ev5 && cond.Ev5.Succeeded);
			finished.Value = true;
		}

		[Test]
		public void AllOf_5SingleTimeouts()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var t5 = Env.Timeout(5);
			var finished = new BoolWrapper();
			var cond = Env.AllOf(t1, t2, t3, t4, t5);
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3, t4, t5));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_5SingleTimeouts_Method_LeftToRight()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var t5 = Env.Timeout(5);
			var finished = new BoolWrapper();
			var cond = t1.And(t2).And(t3).And(t4).And(t5);
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3, t4, t5));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_5SingleTimeouts_Operator_LeftToRight()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			SimEvent t4 = Env.Timeout(4);
			SimEvent t5 = Env.Timeout(5);
			var finished  = new BoolWrapper();
			var cond = t1 & t2 & t3 & t4 & t5;
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3, t4, t5));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_5SingleTimeouts_Operator_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var t5 = Env.Condition<SimEvent>(Env.Timeout(5), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 & t2 & t3 & t4 & t5;
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1, t5.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_5SingleTimeouts_Method_RightToLeft()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var t5 = Env.Timeout(5);
			var finished = new BoolWrapper();
			var cond = t1.And(t2.And(t3.And(t4.And(t5))));
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3, t4, t5));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_5SingleTimeouts_Operator_RightToLeft()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			SimEvent t4 = Env.Timeout(4);
			SimEvent t5 = Env.Timeout(5);
			var finished  = new BoolWrapper();
			var cond = t1 & (t2 & (t3 & (t4 & (t5))));
			Env.Process(AndConditionChecker(finished, cond, t1, t2, t3, t4, t5));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void And_5SingleTimeouts_Operator_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var t5 = Env.Condition<SimEvent>(Env.Timeout(5), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 & (t2 & (t3 & (t4 & (t5))));
			Env.Process(AndConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1, t5.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		IEnumerable<SimEvent> OrConditionChecker<T1, T2, T3, T4, T5>(BoolWrapper finished, Condition<T1, T2, T3, T4, T5> cond, T1 ev1, T2 ev2, T3 ev3, T4 ev4, T5 ev5) 
            where T1 : SimEvent
            where T2 : SimEvent
            where T3 : SimEvent
            where T4 : SimEvent
            where T5 : SimEvent
		{
			Debug.Assert(!finished);
			yield return cond;
			Assert.AreEqual(1, cond.Value.Count);
			Assert.AreSame(Env, cond.Env);
			Assert.AreSame(ev1, cond.Ev1);
			Assert.AreSame(Env, cond.Ev1.Env);
			Assert.True(cond.Ev1 && cond.Ev1.Succeeded);
			Assert.AreSame(ev2, cond.Ev2);
			Assert.AreSame(Env, cond.Ev2.Env);
			Assert.False(cond.Ev2 || cond.Ev2.Succeeded);
			Assert.AreSame(ev3, cond.Ev3);
			Assert.AreSame(Env, cond.Ev3.Env);
			Assert.False(cond.Ev3 || cond.Ev3.Succeeded);
			Assert.AreSame(ev4, cond.Ev4);
			Assert.AreSame(Env, cond.Ev4.Env);
			Assert.False(cond.Ev4 || cond.Ev4.Succeeded);
			Assert.AreSame(ev5, cond.Ev5);
			Assert.AreSame(Env, cond.Ev5.Env);
			Assert.False(cond.Ev5 || cond.Ev5.Succeeded);
			finished.Value = true;
		}

		[Test]
		public void AnyOf_5SingleTimeouts()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var t5 = Env.Timeout(5);
			var finished = new BoolWrapper();
			var cond = Env.AnyOf(t1, t2, t3, t4, t5);
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3, t4, t5));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_5SingleTimeouts_Method_LeftToRight()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var t5 = Env.Timeout(5);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2).Or(t3).Or(t4).Or(t5);
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3, t4, t5));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_5SingleTimeouts_Operator_LeftToRight()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			SimEvent t4 = Env.Timeout(4);
			SimEvent t5 = Env.Timeout(5);
			var finished  = new BoolWrapper();
			var cond = t1 | t2 | t3 | t4 | t5;
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3, t4, t5));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_5SingleTimeouts_Operator_LeftToRight_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var t5 = Env.Condition<SimEvent>(Env.Timeout(5), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 | t2 | t3 | t4 | t5;
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1, t5.Ev1));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_5SingleTimeouts_Method_RightToLeft()
		{
			var t1 = Env.Timeout(1);
			var t2 = Env.Timeout(2);
			var t3 = Env.Timeout(3);
			var t4 = Env.Timeout(4);
			var t5 = Env.Timeout(5);
			var finished = new BoolWrapper();
			var cond = t1.Or(t2.Or(t3.Or(t4.Or(t5))));
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3, t4, t5));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_5SingleTimeouts_Operator_RightToLeft()
		{
			SimEvent t1 = Env.Timeout(1);
			SimEvent t2 = Env.Timeout(2);
			SimEvent t3 = Env.Timeout(3);
			SimEvent t4 = Env.Timeout(4);
			SimEvent t5 = Env.Timeout(5);
			var finished  = new BoolWrapper();
			var cond = t1 | (t2 | (t3 | (t4 | (t5))));
			Env.Process(OrConditionChecker(finished, cond, t1, t2, t3, t4, t5));
			Env.Run();
			Assert.True(finished);
		}

		[Test]
		public void Or_5SingleTimeouts_Operator_RightToLeft_Nested()
		{
			var t1 = Env.Condition<SimEvent>(Env.Timeout(1), c => c.Ev1);
			var t2 = Env.Condition<SimEvent>(Env.Timeout(2), c => c.Ev1);
			var t3 = Env.Condition<SimEvent>(Env.Timeout(3), c => c.Ev1);
			var t4 = Env.Condition<SimEvent>(Env.Timeout(4), c => c.Ev1);
			var t5 = Env.Condition<SimEvent>(Env.Timeout(5), c => c.Ev1);
			var finished  = new BoolWrapper();
			var cond = t1 | (t2 | (t3 | (t4 | (t5))));
			Env.Process(OrConditionChecker(finished, cond, t1.Ev1, t2.Ev1, t3.Ev1, t4.Ev1, t5.Ev1));
			Env.Run();
			Assert.True(finished);
		}


		sealed class BoolWrapper
		{
			public bool Value;

			public static implicit operator bool(BoolWrapper b)
			{
				return b.Value;
			}
		}
	}
}

