//
// Server.cs
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
    using System;
    using System.Collections.Generic;
    using Resources;

    sealed class Server : Entity
    {
        readonly IDictionary<int, CodePacket> _cache = new Dictionary<int, CodePacket>();
        readonly int _id;
        readonly IDictionary<int, CodePacket> _packets = new Dictionary<int, CodePacket>();
        readonly Store<RequestPacket> _requests;
        bool _isUp;

        public Server(SimEnvironment env, G g, int id) : base(env, g)
        {
            _id = id;
            _isUp = true;
            _requests = Sim.Store<RequestPacket>(env);
        }

        public void Receive(RequestPacket newRequest)
        {
            if (!_isUp) {
                return;
            }
            _requests.Put(newRequest);
        }

        public IEnumerable<SimEvent> Run()
        {
            while (true) {
                var reqGet = _requests.Get();
                yield return reqGet;

                var rp = reqGet.Value;
                Logger.InfoFormat("Handling request {0}", rp);

                // We gather the requested code packet
                CodePacket cp;
                if (_cache.TryGetValue(rp.Owner, out cp)) {
                    Logger.InfoFormat("Gathering {0} from cache, holding for {1}", cp, Ts(G.CacheAccessTime));
                    yield return Env.Timeout(G.CacheAccessTime);
                } else {
                    // Not in cache, packet must be recovered from disk
                    _cache.Add(rp.Owner, cp = _packets[rp.Owner]);
                    var w = Env.Random.NextDouble(0, G.MaxAccessTime);
                    Logger.InfoFormat("Gathering {0} from disk and caching it, holding for {1}", cp, Ts(w));
                    yield return Env.Timeout(w);
                }

                G.ServerOSes[_id].Send(cp, rp.SessionId);

                // With some probability, the server goes down for some time
                if (Env.Random.NextDouble() >= G.ServerDownProb) {
                    continue;
                }
                _isUp = false;
                var wait = Env.Random.Exponential(1.0/G.ServerStopMean);
                Logger.InfoFormat("Going offline, holding for {0}", Ts(wait));
                yield return Env.Timeout(wait);
                _cache.Clear();
                _isUp = true;
            }
        }

        public void StoreCodePacket(CodePacket cp)
        {
            _packets.Add(cp.Owner, cp);
        }
    }

    sealed class ServerOS : BaseOS
    {
        readonly Store<AnswerInfo> _sendRequests;

        public ServerOS(SimEnvironment env, G g, int osId) : base(env, g, osId)
        {
            _sendRequests = Sim.Store<AnswerInfo>(env);
        }

        public IEnumerable<SimEvent> Run()
        {
            var incomingGet = IncomingFrames.Get();
            var sendGet = _sendRequests.Get();
            while (true) {
                yield return incomingGet.Or(sendGet);
                if (incomingGet.Succeeded) {
                    var packet = incomingGet.Value;
                    var reqPacket = new RequestPacket(packet.Src, packet.SessionId);
                    Logger.InfoFormat("Sending packet {0} to server", reqPacket);
                    G.Servers[Id].Receive(reqPacket);
                    incomingGet = IncomingFrames.Get();
                }
                if (!sendGet.Succeeded) {
                    continue;
                }

                var sr = sendGet.Value;
                var cp = sr.Packet;
                var sessionId = sr.SessionId;

                // At this point we need to break the code packet into some UDP packets and send them
                var nUdp = (int) Math.Ceiling(cp.Len/(double) G.MTU);
                var lastPacketSize = cp.Len - nUdp*G.MTU;
                if (lastPacketSize != 0) {
                    nUdp++;
                }
                Logger.InfoFormat("Breaking packet into {0} UDP packets", nUdp);

                UdpPacket p;
                double w;
                for (var i = 0; i < nUdp - 1; ++i) {
                    p = new UdpPacket(sessionId, Id, cp.Owner, G.MTU, PacketType.Answer, nUdp);
                    w = G.Latency + p.Len/G.Bandwidth;
                    Logger.InfoFormat("Sending {0}, holding for {1}", p, Ts(w));
                    yield return Env.Timeout(w);
                    G.Switch.Receive(p);
                }
                // Must send the remaining part of the packet
                if (lastPacketSize == 0) {
                    p = new UdpPacket(sessionId, Id, cp.Owner, G.MTU, PacketType.Answer, nUdp);
                } else {
                    p = new UdpPacket(sessionId, Id, cp.Owner, lastPacketSize, PacketType.Answer, nUdp);
                }
                w = G.Latency + p.Len/G.Bandwidth;
                Logger.InfoFormat("Sending {0}, holding for {1}", p, Ts(w));
                yield return Env.Timeout(w);
                G.Switch.Receive(p);
                sendGet = _sendRequests.Get();
            }
        }

        public void Send(CodePacket cp, object sessionId)
        {
            _sendRequests.Put(new AnswerInfo(cp, sessionId));
        }

        struct AnswerInfo
        {
            public readonly CodePacket Packet;
            public readonly object SessionId;

            public AnswerInfo(CodePacket cp, object sessionId)
            {
                Packet = cp;
                SessionId = sessionId;
            }
        }
    }
}