using packetparser;

namespace socketry
{
    public class SocketryClient : Socketry
    {
        /// <summary>
        /// Constructor for SocketryClient class.
        /// </summary>
        /// <param name="socketPerTunnel">te list of sockets per tunnel</param>
        /// <param name="serverPort">the server port</param>
        /// <param name="_procedures">the list of procedures</param>
        /// <exception cref="Exception">the exception occured</exception>
        public SocketryClient(byte[] socketPerTunnel, int serverPort, Dictionary<String, Func<byte[], byte[]>> _procedures)
        {
            this.SetProcedures(_procedures);

            Link link = new Link(serverPort);
            link.ConfigureBlocking(true);
            Packet initPacket = new Packet.Init(socketPerTunnel);
            link.SendPacket(initPacket);

            Console.WriteLine("Sent init...");
            Packet acceptPacket = link.GetPacket() as Packet.Accept;

            if (!(acceptPacket != null))
            {
                throw new Exception("Expected accept packet");
            }

            short[] ports = ((Packet.Accept)acceptPacket).ports;
            Console.WriteLine("Reached here...");

            this.SetTunnelsFromPorts(ports, socketPerTunnel);
        }

        /// <summary>
        /// The function to create tunnel form ports
        /// </summary>
        /// <param name="ports">the list of ports</param>
        /// <param name="socketsPerTunnel">the list of tunnels</param>
        public void SetTunnelsFromPorts(short[] ports, byte[] socketsPerTunnel)
        {
            List<Tunnel> tunnels = new List<Tunnel>();
            byte LastSocketNum = 0;
            foreach (byte socketNum in socketsPerTunnel)
            {
                List<short> portsForTunnel = new List<short>();
                for (int i = LastSocketNum; i < LastSocketNum + socketNum; i++)
                {
                    portsForTunnel.Add(ports[i]);
                }
                LastSocketNum += socketNum;
                try
                {
                    Tunnel tunnel = new Tunnel(portsForTunnel.ToArray());
                    tunnels.Add(tunnel);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            this._tunnels = tunnels.ToArray();
        }
    }
}
