//
// Client.cs
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

namespace DIBRIS.Dessert.Examples.CSharp.Galois
{
    using System.Collections.Generic;
    using System.Linq;
    using Events;
    using Resources;

    sealed class Client : Entity
    {
        readonly int _id;
        readonly ServerInfo[] _peers = new ServerInfo[G.MachineCount - 1];
        readonly ICollection<CodePacket> _receivedPackets = new LinkedList<CodePacket>();
        SimEvent<object> _packetConstructed;

        public Client(SimEnvironment env, G g, int id) : base(env, g)
        {
            _id = id;
            for (var i = 1; i < G.MachineCount; ++i) {
                var index = (i + id)%G.MachineCount;
                _peers[i - 1] = new ServerInfo(index);
            }

            _packetConstructed = env.Event();
        }

        public void Receive(IEnumerable<CodePacket> packets)
        {
            foreach (var codePacket in packets) {
                _receivedPackets.Add(codePacket);
            }
            if (_receivedPackets.Count >= G.RequestCount) {
                _packetConstructed.Succeed();
            }
        }

        public IEnumerable<SimEvent> Run()
        {
            while (true) {
                Logger.Info("Deciding wheter to sleep or not");
                if (Env.Random.NextDouble() < G.ClientSleepProb) {
                    var w = Env.Random.Exponential(1.0/G.ClientStopMean);
                    Logger.InfoFormat("Going inactive, holding for {0}", Ts(w));
                    yield return Env.Timeout(w);
                }

                // We reset the state of all peers, generating a new sessionId for the request;
                // after that, we send the request to G.RequestCount + G.ExtraRequestCount 
                // peers starting after this node.
                foreach (var p in _peers) {
                    p.IsUp = true;
                }
                _receivedPackets.Clear();

                var sessionId = string.Format("{0}-{1}", _id, Env.Random.Next(0, 10000));
                var reqCount = G.RequestCount + G.ExtraRequestCount;
                var nextServer = 0;

                // To collect statistics
                var totalReqCount = 0;
                var startTime = Env.Now;

                while (reqCount > G.ExtraRequestCount) {
                    Logger.InfoFormat("Gathered code packets: {0}", _receivedPackets.Count);
                    var counter = 0;
                    for (var s = nextServer; s < G.MachineCount + nextServer - 1; ++s) {
                        var dst = _peers[s%(G.MachineCount - 1)];
                        if (counter < reqCount) {
                            if (!dst.IsUp) {
                                continue;
                            }
                            Logger.InfoFormat("Sending request to {0}", dst);
                            var p = new UdpPacket(sessionId, _id, dst.Id, G.RequestSize, PacketType.Request, 1);
                            G.ClientOSes[_id].Send(p);
                            counter++;
                        } else {
                            nextServer = s%(G.MachineCount - 1);
                            break;
                        }
                    }

                    Logger.Info("Starting timeout");
                    yield return _packetConstructed.Or(Env.Timeout(G.Timeout*counter*8));
                    _packetConstructed = Env.Event();

                    SetPeersDown();
                    reqCount = G.RequestCount + G.ExtraRequestCount - _receivedPackets.Count;
                    totalReqCount += counter;
                }

                // At this point we surely have:
                // _receivedPackets.Count >= G.RequestCount
                // So, we can reconstruct the data.
                G.Stats.AddClientTimeWaited(Env.Now - startTime);
                G.Stats.AddClientRequests(totalReqCount);
                G.Stats.FileReconstructed();
            }
        }

        void SetPeersDown()
        {
            foreach (var peer in _peers.Where(p => p.IsUp && _receivedPackets.Any(r => r.Keeper == p.Id))) {
                peer.IsUp = false;
            }
        }

        sealed class ServerInfo
        {
            public readonly int Id;
            public bool IsUp;

            public ServerInfo(int id)
            {
                Id = id;
                IsUp = true;
            }

            public override string ToString()
            {
                return string.Format("ServerInfo(Id: {0}, IsUp: {1})", Id, IsUp);
            }
        }
    }

    sealed class ClientOS : BaseOS
    {
        readonly ICollection<UdpPacket> _arrivedFrames = new LinkedList<UdpPacket>();
        readonly Store<UdpPacket> _sendRequests;
        object _currSessionId;

        public ClientOS(SimEnvironment env, G g, int osId) : base(env, g, osId)
        {
            _sendRequests = Sim.Store<UdpPacket>(env);
        }

        public void Send(UdpPacket p)
        {
            _currSessionId = p.SessionId;
            _sendRequests.Put(p);
        }

        public IEnumerable<SimEvent> Run()
        {
            var incomingGet = IncomingFrames.Get();
            var sendGet = _sendRequests.Get();
            while (true) {
                yield return incomingGet.Or(sendGet);
                if (incomingGet.Succeeded) {
                    var p = incomingGet.Value;
                    if (p.SessionId == _currSessionId) {
                        Logger.InfoFormat("Received a new frame: {0}", p);
                        _arrivedFrames.Add(p);
                        Logger.InfoFormat("Reconstructing packets from {0} frames", _arrivedFrames.Count);
                        var packets = ReconstructPackets();
                        if (packets.Count != 0) {
                            G.Clients[Id].Receive(packets);
                            Logger.InfoFormat("Reconstructed and sent {0} packets", packets.Count);
                        }
                    }
                    incomingGet = IncomingFrames.Get();
                }
                if (sendGet.Succeeded) {
                    var p = sendGet.Value;
                    var w = G.Latency + p.Len/G.Bandwidth;
                    Logger.InfoFormat("Sending {0}, holding for {1}", p, Ts(w));
                    yield return Env.Timeout(w);
                    G.Switch.Receive(p);
                    sendGet = _sendRequests.Get();
                }
            }
        }

        LinkedList<CodePacket> ReconstructPackets()
        {
            var visited = new HashSet<int>();
            var usedServers = new HashSet<int>();
            var unusedFrames = new LinkedList<UdpPacket>();
            var codePackets = new LinkedList<CodePacket>();

            foreach (var f in _arrivedFrames) {
                if (visited.Contains(f.Src)) {
                    if (!usedServers.Contains(f.Src)) {
                        unusedFrames.AddLast(f);
                    }
                    continue;
                }

                visited.Add(f.Src);
                var frameCount = f.Count;
                var actualCount = _arrivedFrames.Count(p => p.Src == f.Src);

                // If there are enough UDP packets to make a code packet,
                // then we put the new code packet into array.
                if (actualCount >= frameCount) {
                    usedServers.Add(f.Src);
                    var cp = new CodePacket(Id, f.Src, f.Len);
                    codePackets.AddLast(cp);
                } else {
                    unusedFrames.AddLast(f);
                }
            }

            _arrivedFrames.Clear();
            foreach (var f in unusedFrames) {
                _arrivedFrames.Add(f);
            }
            return codePackets;
        }
    }
}