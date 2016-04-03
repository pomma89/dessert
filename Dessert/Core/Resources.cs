// File name: ErrorMessages.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
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

namespace DIBRIS.Dessert.Core
{
    using Resources;

    static partial class ErrorMessages
    {
        public const string ContractClass = "A contract class cannot be instanced";
        public const string DifferentEnvironment = "Given event belongs to a different environment";
        public const string DifferentResource = "Given request belongs to a different resource";
        public const string EndedProcess = "Given process has ended its lifecycle.";
        public const string ExcessiveQuantity = "Given quantity is greater than container capacity.";
        public const string InternalError = "???";
        public const string InterruptSameProcess = "A process cannot interrupt itself.";
        public const string InterruptedDifferentProcess = "A process cannot query another process for interrupts.";
        public const string InterruptUncaught = "Process was interrupted but it did not check for it.";
        public const string InvalidDelay = "Delay should not be negative and Now+delay must not cause overflow.";
        public const string InvalidEventCount = "No more than five events can take part in a condition.";
        public const string InvalidMethod = "Given method should not be used.";
        public const string InvalidRecordingTime = "Given time is less than start time.";
        public const string NegativeQuantity = "Quantity should not be negative.";
        public const string NoObservations = "Tally or monitor has no observations.";
        public const string NullEnvironment = "Given simulation environment cannot be null.";
        public const string NullEvaluator = "Condition evaluator cannot be null.";
        public const string NullEvent = "Yielded events, or condition events, cannot be null.";
        public const string NullGenerator = "Generator used by process or call event cannot be null.";
        public const string NullRequest = "A null request cannot be released.";
        public const string ScalingFactorNotUpdatable = "Scaling factor can be set only once and it cannot be overwritten.";
        public const string WallClockNotUpdatable = "Wall clock can be set only once and it cannot be overwritten.";

        public static string InvalidEnum<TEnum>() where TEnum : struct
        {
            return string.Format("Invalid value for {0}.", typeof(TEnum).Name);
        }
    }

    internal static class Default
    {
        public const int Capacity = int.MaxValue;
        public const int Level = 0;
        public const int NoDelay = 0;
        public const WaitPolicy Policy = WaitPolicy.FIFO;
        public const bool Preempt = true;
        public const double Priority = 0.0;

        public static readonly object NoValue = new object();
        public static readonly object Value = null;
    }
}