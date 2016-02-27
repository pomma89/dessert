//
// Switch.cs
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
    using System.Collections.Generic;
    using Resources;

    sealed class Switch : Entity
    {
        readonly Store<UdpPacket> _frames;

        public Switch(SimEnvironment env, G g) : base(env, g)
        {
            _frames = Sim.Store<UdpPacket>(env, G.FrameCount);
        }

        public IEnumerable<SimEvent> Run()
        {
            while (true) {
                var getFrame = _frames.Get();
                yield return getFrame;
                // Switch is awakened again by the Receive method.
                var p = getFrame.Value;
                yield return Env.Call(WaitForSend(p, p.Len));
                Send(p);
            }
        }

        public void Receive(UdpPacket packet)
        {
            if (_frames.Count == G.FrameCount) {
                Logger.Info("Lost a packet");
                G.Stats.MessageLost();
            } else {
                Logger.InfoFormat("Packet {0} incoming", packet);
                _frames.Put(packet);
            }
        }

        public override string ToString()
        {
            return string.Format("Switch(UsedFrames: {0})", _frames.Count);
        }

        void Send(UdpPacket packet)
        {
            if (packet.Type == PacketType.Request) {
                G.ServerOSes[packet.Dst].Receive(packet);
            } else {
                G.ClientOSes[packet.Dst].Receive(packet);
            }
        }
    }
}