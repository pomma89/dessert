//
// Globals.cs
//  
// Author(s):
//       Alessio Parma <alessio.parma@gmail.com>
//       Artur Tolstenco <tartur88@gmail.com>
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

namespace Dessert.Examples.CSharp.Galois
{
    using System.Collections.Generic;
    using Resources;
    using log4net;
    using log4net.Config;

    enum PacketType
    {
        Answer,
        Request
    }

    sealed class G
    {
        // Quantities
        public const short SimCount = 20;

        // Sizes (in bytes)
        public const int FileSize = 64*1024;
        public const int MTU = 1024;
        public const int RequestSize = 64;

        // Probabilities (between 0 and 1)
        public const double ClientSleepProb = 0.3;
        public const double ServerDownProb = 0.6;
        public static short RequestCount = 8;
        public static short MachineCount = 16;
        public static short FrameCount = 128;

        // Times
        public static readonly double MaxSimTime;
        public static readonly double MemoryRecordingFrequency;
        public static readonly double ClientStopMean;
        public static readonly double ServerStopMean;
        public static readonly double Timeout;
        public static readonly double CacheAccessTime;
        public static readonly double MaxAccessTime;
        public static readonly double Latency;
        public static readonly double Bandwidth;

        public static int ExtraRequestCount; // Between 0 and (MachineCount - RequestCount - 1)

        public readonly ClientOS[] ClientOSes = new ClientOS[MachineCount];
        public readonly Client[] Clients = new Client[MachineCount];
        public readonly ServerOS[] ServerOSes = new ServerOS[MachineCount];
        public readonly Server[] Servers = new Server[MachineCount];
        public readonly Stats Stats = new Stats();
        public Switch Switch;

        static G()
        {
            XmlConfigurator.Configure();
            Sim.CurrentTimeUnit = TimeUnit.Microsecond;
            MaxSimTime = 10.Seconds();
            MemoryRecordingFrequency = MaxSimTime/10;
            ClientStopMean = 25.Milliseconds(); // Exponential
            ServerStopMean = 15.Milliseconds(); // Exponential
            Timeout = 1.5.Milliseconds(); // Fixed
            CacheAccessTime = 100.Microseconds(); // Fixed
            MaxAccessTime = 1.Microseconds(); // Fixed
            Latency = 100.Nanoseconds(); // Fixed
            Bandwidth = 11*1024*1024/1.Seconds(); // Bandwidth MB/s = 11 * 1024^2 B / 1 sec
        }

        public static void SetParameters(short machineCount, short frameCount)
        {
            MachineCount = machineCount;
            RequestCount = (short) (machineCount/2);
            FrameCount = frameCount;
        }
    }

    abstract class Entity
    {
        protected readonly SimEnvironment Env;
        protected readonly G G;
        protected ILog Logger;

        protected Entity(SimEnvironment env, G g)
        {
            Env = env;
            G = g;
        }

        public void SetLogger(string entityName)
        {
            Logger = LogManager.GetLogger(entityName);
        }

        public static string Ts(double waitTime)
        {
            return string.Format("{0:.###}", waitTime);
        }

        protected IEnumerable<SimEvent> WaitForSend(object packet, int packetLen)
        {
            var w = G.Latency + packetLen/G.Bandwidth;
            Logger.InfoFormat("Sending {0}, waiting for {1}", packet, Ts(w));
            yield return Env.Timeout(w);
        }
    }

    abstract class BaseOS : Entity
    {
        protected readonly int Id;
        protected readonly Store<UdpPacket> IncomingFrames;

        protected BaseOS(SimEnvironment env, G g, int osId) : base(env, g)
        {
            Id = osId;
            IncomingFrames = Sim.NewStore<UdpPacket>(env);
        }

        public void Receive(UdpPacket p)
        {
            IncomingFrames.Put(p);
        }
    }
}