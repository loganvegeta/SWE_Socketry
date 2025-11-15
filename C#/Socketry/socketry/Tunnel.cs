using packetparser;
using socket;
using System.Collections.Concurrent;

namespace socketry
{
    public class Tunnel
    {
        private static byte _NO_CALL_IDS_AVAILABLE = 127;
        IDictionary<CallIdentifier, TaskCompletionSource<byte[]>> _packets;

        List<Link> _links;
        Selector _selector;

        private readonly object _lock = new object();

        /// <summary>
        /// Function to initialize a tunnel
        /// </summary>
        private void Initialize()
        {
            _selector = Selector.Open();
            _packets = new ConcurrentDictionary<CallIdentifier, TaskCompletionSource<byte[]>>();
        }

        /// <summary>
        /// Constructor for tunnel class
        /// </summary>
        /// <param name="LinkPorts"></param>
        public Tunnel(short[] LinkPorts)
        {
            Initialize();
            this._links = new List<Link>();
            foreach (short linkPort in LinkPorts)
            {
                Link link = new Link(linkPort);
                link.Register(_selector);
                _links.Add(link);
            }
            CallIdentifier call = new CallIdentifier(2, 3);
            TaskCompletionSource<byte[]> resFuture = new TaskCompletionSource<byte[]>();
            Console.WriteLine("packet adding...");
            _packets.Add(call, resFuture);
            Console.WriteLine("packet added...");
        }

        /// <summary>
        /// Constructor for tunnel class
        /// </summary>
        /// <param name="sockets"></param>
        public Tunnel(ISocket[] sockets)
        {
            Initialize();
            _links = new List<Link>();
            foreach (ISocket socket in sockets)
            {
                Link link = new Link(socket);
                link.Register(_selector);
                _links.Add(link);
            }
        }

        /// <summary>
        /// Function to send packets
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <returns>The response packet</returns>
        private Packet FeedPacket(Packet packet)
        {
            Console.WriteLine($"Feeding packet {packet.ToString()}");
            switch (packet)
            {
                case Packet.Result resPacket:
                    {
                        byte[] result = resPacket.response;
                        var callIdentifier = new CallIdentifier(resPacket.callId, resPacket.fnId);
                        if (_packets.TryGetValue(callIdentifier, out var resFuture))
                        {
                            resFuture.TrySetResult(result);
                        }
                        return null;
                    }
                case Packet.Error errorPacket:
                    {
                        byte[] error = errorPacket.error;
                        var callIdentifier = new CallIdentifier(errorPacket.callId, errorPacket.fnId);
                        if (_packets.TryGetValue(callIdentifier, out var resFuture))
                        {
                            resFuture.TrySetException(new Exception(error.ToString()));
                        }
                        return null;
                    }

                default:
                    {
                        return packet;
                    }
            }
        }

        /// <summary>
        /// Function to selecta Link
        /// </summary>
        /// <returns>the selected link</returns>
        private Link SelectLink()
        {
            var rand = new Random();
            int linkId = Math.Max(0, (int)(rand.NextDouble() * _links.Count) - 1);
            return _links[linkId];
        }

        /// <summary>
        /// Function to get the call identifier
        /// </summary>
        /// <param name="fnId">the function id</param>
        /// <returns>the call identifier</returns>
        CallIdentifier GetCallIdentifier(byte fnId)
        {
            lock (_lock)
            {
                byte callId = _NO_CALL_IDS_AVAILABLE;
                while (true)
                {
                    sbyte i = -127;
                    for (; i < 127; i++)
                    {
                        if (!_packets.ContainsKey(new CallIdentifier((byte)i, fnId)))
                        {
                            callId = (byte)i;
                            break;
                        }
                    }
                    if (callId != _NO_CALL_IDS_AVAILABLE)
                    {
                        break;
                    }
                    try
                    {
                        Thread.Sleep(1000);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                CallIdentifier callIdentifier = new CallIdentifier(callId, fnId);
                _packets.Add(callIdentifier, null);
                return callIdentifier;
            }
        }

        /// <summary>
        /// Function to send packet.
        /// </summary>
        /// <param name="packet">the packet to send </param>
        public void SendPacket(Packet packet)
        {
            Link link = SelectLink();
            link.SendPacket(packet);
        }

        /// <summary>
        /// Function to listen for packets
        /// </summary>
        /// <returns>the list of packets received</returns>
        public List<Packet> Listen()
        {
            List<Packet> packets = new List<Packet>();
            try
            {
                int readyChannels = _selector.Select();
                if (readyChannels == 0)
                {
                    return new List<Packet>();
                }

                ISet<SelectionKey> selectedKeys = _selector.SelectedKeys();
                foreach (SelectionKey selectedkey in selectedKeys)
                {
                    SelectionKey key = selectedkey;
                    selectedKeys.Remove(selectedkey);
                    if (!key.IsValid()) continue;

                    if (key.IsReadable())
                    {
                        Link link = (Link)key.Attachment();
                        foreach (Packet packet in link.GetPackets())
                        {
                            packets.Add(packet);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to listen {e.ToString()}");
            }

            List<Packet> packetsToReturn = new List<Packet>();
            foreach (Packet packet in packets)
            {
                Packet feededPacket = FeedPacket(packet);
                if (feededPacket != null)
                {
                    packetsToReturn.Add(feededPacket);
                }
            }

            return packetsToReturn;
        }

        /// <summary>
        /// Function to call the function
        /// </summary>
        /// <param name="fnId">the fnuction id</param>
        /// <param name="arguments">the arguments of function</param>
        /// <returns>the task completion</returns>
        public TaskCompletionSource<byte[]> CallFn(byte fnId, byte[] arguments)
        {
            CallIdentifier callIdentifier = GetCallIdentifier(fnId);
            TaskCompletionSource<byte[]> resFuture = new TaskCompletionSource<byte[]>();
            Packet.Call packet = new Packet.Call(fnId, callIdentifier.callId, arguments);
            SendPacket(packet);
            _packets[callIdentifier] = resFuture;
            return resFuture;
        }
    }
}
