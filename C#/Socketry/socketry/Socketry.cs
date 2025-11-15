using packetparser;
using System.Text;

namespace socketry
{
    public abstract class Socketry
    {
        Dictionary<String, Func<byte[], byte[]>> _procedures;
        String[] _procedureNames;
        String[] _remoteprocedureNames;

        public Tunnel[] _tunnels;

        /// <summary>
        /// Function to get all procedures.
        /// </summary>
        /// <returns>the list of all procedures</returns>
        public byte[] GetProcedures()
        {
            MemoryStream buffer = new MemoryStream(1024);
            BinaryWriter binaryWriter = new BinaryWriter(buffer);
            foreach (String procedureName in _procedureNames)
            {
                Console.WriteLine(procedureName);
                binaryWriter.Write(Encoding.UTF8.GetBytes(procedureName));
                binaryWriter.Write(0);
            }
            buffer.Position = 0;
            byte[] result = buffer.ToArray();
            return result;
        }

        /// <summary>
        /// Function to set all the procedures.
        /// </summary>
        /// <param name="procedures">the list of all procedures</param>
        public void SetProcedures(Dictionary<String, Func<byte[], byte[]>> procedures)
        {
            _procedures = procedures;
            _procedures.Add("GetProcedures", i => GetProcedures());
            _procedureNames = new String[_procedures.Count];
            _procedureNames[0] = "GetProcedures";

            int index = 1;
            foreach (String key in _procedures.Keys)
            {
                if (!key.Equals("GetProcedures"))
                {
                    Console.WriteLine($"Function {key} set at {index}");
                    _procedureNames[index++] = key;
                }
            }
        }

        /// <summary>
        /// Function to get all remote procedures names
        /// </summary>
        public void GetRemoteProceduresNames()
        {
            byte[] initResponse = null;
            try
            {
                Console.WriteLine($"1");
                initResponse = MakeRemoteCall((byte)0, new byte[0], 0).Task.Result;
                Console.WriteLine($"2");
            }
            catch (AggregateException ex)
            {
                Console.WriteLine($"3");
                Console.WriteLine($"Aggregrate exception {ex.ToString()}");
                Console.WriteLine($"4");
            }
            Console.WriteLine($"init response {initResponse.Length}");
            List<String> remoteProceduresNameList = new List<String>();
            StringBuilder currentName = new StringBuilder();
            foreach (byte b in initResponse)
            {
                if (b == 0)
                {
                    remoteProceduresNameList.Add(currentName.ToString());
                    currentName = new StringBuilder();
                }
                else
                {
                    currentName.Append((char)b);
                }
            }
            _remoteprocedureNames = remoteProceduresNameList.ToArray();
        }

        /// <summary>
        /// Function to listen continuously
        /// </summary>
        public void ListenLoop()
        {
            try
            {
                StartListening();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Function to listen for all packets.
        /// </summary>
        public void StartListening()
        {
            while (true)
            {
                foreach (Tunnel tunnel in _tunnels)
                {
                    List<Packet> unhandledPackets = tunnel.Listen();
                    unhandledPackets.ForEach(packet =>
                    {
                        HandlePacket(packet, tunnel);
                    });
                }
            }
        }

        /// <summary>
        /// Function to handle each packets.
        /// </summary>
        /// <param name="packet">The packet to be handled</param>
        /// <param name="tunnel">The tunnel to send it </param>
        public void HandlePacket(Packet packet, Tunnel tunnel)
        {
            switch (packet)
            {
                case Packet.Call callPacket:
                    {
                        Packet responsePacket;
                        try
                        {
                            Console.WriteLine($"Packet: {packet.ToString()}");
                            byte[] response = HandleRemoteCall(callPacket.fnId, callPacket.arguments);
                            responsePacket = new Packet.Result(callPacket.fnId, callPacket.callId, response);
                        }
                        catch (Exception e)
                        {
                            responsePacket = new Packet.Error(callPacket.fnId, callPacket.callId, Encoding.UTF8.GetBytes(e.Message));
                        }
                        tunnel.SendPacket(responsePacket);
                        break;
                    }
                case Packet.Ping ignored:
                    {
                        tunnel.SendPacket(Packet.Ping.Instance);
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"Unhandled packet {packet}");
                        break;
                    }
            }
        }

        /// <summary>
        /// Function to calla remote procedure.
        /// </summary>
        /// <param name="fnId">the function id to call</param>
        /// <param name="data">the data to send</param>
        /// <returns>the result of the data</returns>
        public byte[] HandleRemoteCall(byte fnId, byte[] data)
        {
            Func<byte[], byte[]> procedure = _procedures[_procedureNames[fnId]];
            Console.WriteLine($"Fn: {procedure.ToString()} {data}");
            if (procedure == null)
            {
                Console.WriteLine("Required procedure does not exists...");
            }
            return (byte[])procedure(data);
        }

        /// <summary>
        /// Function to get remote procedures id
        /// </summary>
        /// <param name="name">the name of preocdure</param>
        /// <returns>the function id</returns>
        public byte GetRemoteProceduresId(String name)
        {
            if (_remoteprocedureNames == null)
            {
                Console.WriteLine("Fetching remote procedures names...");
                try
                {
                    GetRemoteProceduresNames();
                }
                catch (Exception e) { }

                Console.WriteLine(_remoteprocedureNames[0]);
            }

            for (byte i = 0; i < _remoteprocedureNames.Length; i++)
            {
                if (_remoteprocedureNames[i].Equals(name))
                {
                    return i;
                }
            }

            return 0;
            // in java throws error
        }

        /// <summary>
        /// Function to make a function call
        /// </summary>
        /// <param name="fnId">the function id</param>
        /// <param name="data">the data to send</param>
        /// <param name="tunnnelId">the tunnel to send to</param>
        /// <returns></returns>
        public TaskCompletionSource<byte[]> MakeRemoteCall(byte fnId, byte[] data, int tunnnelId)
        {
            if (tunnnelId < 0 || tunnnelId >= _tunnels.Length)
            {
                throw new Exception($"Unexpected tunnel id {tunnnelId}...");
            }

            Tunnel tnl = _tunnels[tunnnelId];
            return tnl.CallFn(fnId, data);
        }
    }
}
