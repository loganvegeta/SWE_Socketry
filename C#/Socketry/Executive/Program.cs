using socketry;

namespace Executive
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool isServer = false;
            if (!isServer)
            {
                Dictionary<String, Func<byte[], byte[]>> procedures = new Dictionary<string, Func<byte[], byte[]>>();

                DummyClass dummy = new DummyClass();
                procedures["Add"] = dummy.AddWrapper;

                SocketryClient client = new SocketryClient(new byte[] { 1, 1, 1 }, 60000, procedures);
                Console.WriteLine("Client Started...");
                //client.GetRemoteProceduresNames();

                Thread handler = new Thread(() => client.ListenLoop());
                handler.Start();
                byte fnid = client.GetRemoteProceduresId("hello");
                Console.WriteLine($"{fnid}");
                byte[] response = client.MakeRemoteCall(fnid, new byte[0], 1).Task.Result;
                Console.WriteLine("Main thread running...");

                handler.Join();
            }
            else
            {
                Dictionary<String, Func<byte[], byte[]>> procedures = new Dictionary<string, Func<byte[], byte[]>>();

                DummyClass2 dummy2 = new DummyClass2();

                SocketryServer server = new SocketryServer(60000, procedures);
                Console.WriteLine("Server started...");

                Thread handler = new Thread(() => server.ListenLoop());
                handler.Start();

                byte[] result = server.MakeRemoteCall(server.GetRemoteProceduresId("Add"), dummy2.AddSerialize(4, 3), 2).Task.Result;
                int sum = BitConverter.ToInt32(result.Take(4).Reverse().ToArray(), 0);
                Console.WriteLine($"4 + 3 = {sum}");

                handler.Join();
            }
        }
    }
}
