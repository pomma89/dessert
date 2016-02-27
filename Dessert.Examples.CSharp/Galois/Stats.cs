//
// Stats.cs
//  
// Author(s):
//       Alessio Parma <alessio.parma@gmail.com>
//       Artur Tolstenco <tartur88@gmail.com>
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

namespace Dessert.Examples.CSharp.Galois
{
    using Recording;

    sealed class Stats
    {
        static readonly Tally ClientRequestsTally = Sim.Tally();
        static readonly Tally ClientTimeWaitedTally = Sim.Tally();
        static readonly Tally MemoryUsedTally = Sim.Tally();
        static readonly Tally ReconstructedFilesTally = Sim.Tally();
        static readonly Tally LostSwitchMessagesTally = Sim.Tally();
        readonly Tally _clientRequestsTmpTally = Sim.Tally();
        readonly Tally _clientTimeWaitedTmpTally = Sim.Tally();
        readonly Tally _memoryUsedTmpTally = Sim.Tally();
        int _lostSwitchMessages;
        int _reconstructedFiles;

        public double ClientRequestsAvg()
        {
            return _clientRequestsTmpTally.Mean();
        }

        public static double ClientRequestsTotalAvg()
        {
            return ClientRequestsTally.Mean();
        }

        public void AddClientRequests(int n)
        {
            ClientRequestsTally.Observe(n);
            _clientRequestsTmpTally.Observe(n);
        }

        public double ClientTimeWaitedAvg()
        {
            return _clientTimeWaitedTmpTally.Mean();
        }

        public static double ClientTimeWaitedTotalAvg()
        {
            return ClientTimeWaitedTally.Mean();
        }

        public void AddClientTimeWaited(double t)
        {
            ClientTimeWaitedTally.Observe(t);
            _clientTimeWaitedTmpTally.Observe(t);
        }

        public int MemoryUsedAvg()
        {
            return (int) _memoryUsedTmpTally.Mean();
        }

        public static int MemoryUsedTotalAvg()
        {
            var res = (int) MemoryUsedTally.Mean();
            MemoryUsedTally.Reset();
            return res;
        }

        public void AddMemoryUsed(double m)
        {
            MemoryUsedTally.Observe(m);
            _memoryUsedTmpTally.Observe(m);
        }

        public int ReconstructedFiles()
        {
            return _reconstructedFiles;
        }

        public int LostSwitchMessages()
        {
            return _lostSwitchMessages;
        }

        public static int ReconstructedFilesAvg()
        {
            return (int) ReconstructedFilesTally.Mean();
        }

        public static int LostSwitchMessagesAvg()
        {
            return (int) LostSwitchMessagesTally.Mean();
        }

        public void FileReconstructed()
        {
            _reconstructedFiles++;
        }

        public void MessageLost()
        {
            _lostSwitchMessages++;
        }

        /// <summary>
        ///   Called when each simulation has ended.
        /// </summary>
        public void Reset()
        {
            ReconstructedFilesTally.Observe(_reconstructedFiles);
            LostSwitchMessagesTally.Observe(_lostSwitchMessages);
        }

        /// <summary>
        ///   Called when all simulations have ended.
        /// </summary>
        public static void ResetMon()
        {
            ClientRequestsTally.Reset();
            ClientTimeWaitedTally.Reset();
            // MemoryUsedTally.Reset();
            // It has not to be reset, since it collects data for all simulations.
            ReconstructedFilesTally.Reset();
            LostSwitchMessagesTally.Reset();
        }
    }
}