using packetparser;
using socket;
using System.Collections.Concurrent;
using System.Net;

namespace socketry
{
    /// <summary>
    /// Class to create a new link.
    /// </summary>
    public class Link
    {
        ISocket _clientChannel;
        private MemoryStream _leftOverBuffer;

        public SocketPacket _currentSocketPacket;

        private ConcurrentQueue<Packet> _packets;

        /// <summary>
        /// Constructor for Link Class
        /// </summary>
        /// <param name="port">the port to start the link at</param>
        public Link(int port)
        {
            _clientChannel = new SocketTCP();
            _clientChannel.ConfigureBlocking(true); // block till connection is established
            _clientChannel.Connect(new IPEndPoint(IPAddress.Loopback, port));
            _clientChannel.ConfigureBlocking(false);
            _packets = new ConcurrentQueue<Packet>();
        }

        /// <summary>
        /// Constructor for Link class
        /// </summary>
        /// <param name="_connectedChannel">the socket to start the link on</param>
        public Link(ISocket _connectedChannel)
        {
            _clientChannel = _connectedChannel;
            _clientChannel.ConfigureBlocking(false);
            _packets = new ConcurrentQueue<Packet>();
        }

        /// <summary>
        /// Function to set the blocking state of the socket.
        /// </summary>
        /// <param name="to_block">the state to set to</param>
        public void ConfigureBlocking(bool to_block)
        {
            _clientChannel.ConfigureBlocking(to_block);
        }

        /// <summary>
        /// Function to register the socket to the selector.
        /// </summary>
        /// <param name="selector">The selector to register with</param>
        public void Register(Selector selector)
        {
            if (_clientChannel != null)
            {
                _clientChannel.Register(selector, SelectionKey.OP_READ, this);
            }
        }

        /// <summary>
        /// Function to read data from the sockets
        /// </summary>
        /// <returns>The stream containing the data read</returns>
        private MemoryStream ReadData()
        {
            int bufferLength = 1024;
            if (_leftOverBuffer != null)
            {
                bufferLength = Math.Max(1024, _leftOverBuffer.Capacity + 1024);
            }
            MemoryStream readBuffer = new MemoryStream(bufferLength);
            if (_leftOverBuffer != null)
            {
                readBuffer.Write(_leftOverBuffer.ToArray());
            }
            int dataRead = _clientChannel.Read(readBuffer);
            if (dataRead == 0)
            {
                // Raise runtime Exception
                Console.WriteLine("no data received...");
            }
            readBuffer.Position = 0;
            return readBuffer;
        }

        /// <summary>
        /// Function to get all the received packets.
        /// </summary>
        /// <returns>the list of all packets</returns>
        public List<Packet> GetPackets()
        {
            try
            {
                ReadAndParseAllPackets();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                return new List<Packet>();
            }
            List<Packet> packets = new List<Packet>(_packets);
            _packets.Clear();
            return packets;
        }

        /// <summary>
        /// Function to read and parse all packets
        /// </summary>
        void ReadAndParseAllPackets()
        {
            bool initialState = _clientChannel.IsBlocking();
            int dataRead = ReadAndParsePackets();
            Console.WriteLine("Read packets...");
            _clientChannel.ConfigureBlocking(false);

            while (dataRead > 0)
            {
                dataRead = ReadAndParsePackets();
            }
            _clientChannel.ConfigureBlocking(initialState);
        }

        /// <summary>
        /// Function to get the length.
        /// </summary>
        /// <param name="data">the data to read</param>
        /// <param name="pos">the position of data</param>
        /// <returns></returns>
        static int ReadInt(byte[] data, int pos)
        {
            if (data.Length < pos)
            {
                throw new ArgumentException("Expected more data...");
            }
            return ((data[pos] & 0xFF) << 24) |
                ((data[pos + 1] & 0xFF) << 16) |
                ((data[pos + 2] & 0xFF) << 8) |
                (data[pos + 3] & 0xFF);
        }

        /// <summary>
        /// Function to parse packets
        /// </summary>
        /// <returns>the length of data parsed</returns>
        int ReadAndParsePackets()
        {
            MemoryStream buffer = ReadData();
            Console.WriteLine("Read from socket...");
            int remaining = (int)(buffer.Length - buffer.Position);
            byte[] data = new byte[remaining];
            buffer.Read(data, 0, remaining);

            int currDatapos = 0;
            int len = data.Length;
            while (currDatapos < len)
            {
                if (_currentSocketPacket == null)
                {
                    int left = len - currDatapos;
                    if (left < 4)
                    {
                        _leftOverBuffer = new MemoryStream(left);
                        _leftOverBuffer.Write(data, currDatapos, left);
                        _leftOverBuffer.Position = 0;
                        break;
                    }
                    int contentLength = ReadInt(data, currDatapos);
                    _currentSocketPacket = new SocketPacket(contentLength, new MemoryStream());
                    currDatapos += 4;
                }

                int toRead = Math.Min(_currentSocketPacket.BytesLeft, data.Length - currDatapos);
                _currentSocketPacket.Content.Write(data, currDatapos, toRead);
                _currentSocketPacket.BytesLeft -= toRead;
                currDatapos += toRead;

                if (_currentSocketPacket.BytesLeft == 0)
                {
                    _packets.Enqueue(Packet.Parse(_currentSocketPacket.Content.ToArray()));
                    _currentSocketPacket = null;
                }

            }
            return data.Length;
        }

        /// <summary>
        /// Function to get all packets
        /// </summary>
        /// <returns>The received packet</returns>
        public Packet GetPacket()
        {
            try
            {
                ReadAndParseAllPackets();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            Packet packet = _packets.TryDequeue(out var p) ? p : null;
            Console.WriteLine($"packet {packet.ToString()}");
            return packet;
        }

        /// <summary>
        /// Function to send packet.
        /// </summary>
        /// <param name="packet">The packet to send to</param>
        /// <returns>The state of the packet</returns>
        public bool SendPacket(Packet packet)
        {
            byte[] packetData = Packet.Serialize(packet).ToArray();
            MemoryStream socketData = new MemoryStream(packetData.Length + 4);
            BinaryWriter writer = new BinaryWriter(socketData);
            writer.Write((byte)((packetData.Length >> 24) & 0xFF));
            writer.Write((byte)((packetData.Length >> 16) & 0xFF));
            writer.Write((byte)((packetData.Length >> 8) & 0xFF));
            writer.Write((byte)(packetData.Length & 0xFF));
            writer.Write(packetData);
            socketData.Position = 0;
            Console.WriteLine($"Sent packet {socketData.Length}");
            if (_clientChannel != null)
            {
                try
                {
                    _clientChannel.Write(socketData);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return false;
        }
    }
}
