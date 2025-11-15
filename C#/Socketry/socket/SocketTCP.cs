using System.Net;
using System.Net.Sockets;

namespace socket
{
    public class SocketTCP : ISocket
    {
        private Socket _osSocket;

        public SocketTCP()
        {
            _osSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _osSocket.Blocking = false;
        }

        public SocketTCP(Socket _socket)
        {
            _osSocket = _socket;
        }

        /// <summary>
        /// Function to read for a given stream.
        /// </summary>
        /// <param name="dst">the stream to read from</param>
        /// <returns>the number of bytes read</returns>
        public int Read(MemoryStream dst)
        {
            byte[] temp = new byte[1024];
            int r = 0;
            try
            {
                r = _osSocket.Receive(temp);
                dst.Write(temp, 0, r);
            }
            catch (SocketException e)
            {
            }
            Console.WriteLine($"Received message ({r} bytes):");
            return r;
        }

        /// <summary>
        /// Function to read from a given stream
        /// </summary>
        /// <param name="dst">the stream to read from</param>
        /// <param name="offset">the offset to start</param>
        /// <param name="length">the length to read</param>
        /// <returns>number of bytes received</returns>
        public long Read(MemoryStream dst, int offset, int length)
        {
            return _osSocket.Receive(dst.GetBuffer(), offset, length, SocketFlags.None);
        }

        /// <summary>
        /// Function to write data from a stream.
        /// </summary>
        /// <param name="src">The stream to write from</param>
        /// <returns>The number of bytes written</returns>
        public int Write(MemoryStream src)
        {
            return _osSocket.Send(src.GetBuffer());
        }

        /// <summary>
        /// Function to write from a given stream
        /// </summary>
        /// <param name="src">the stream to wriet from</param>
        /// <param name="offset">the offset to start from</param>
        /// <param name="length">the number of bytes to write</param>
        /// <returns>number of bytes written</returns>
        public long Write(MemoryStream src, int offset, int length)
        {
            return _osSocket.Send(src.GetBuffer(), offset, length, SocketFlags.None);
        }

        /// <summary>
        /// Function to set the blocking state
        /// </summary>
        /// <param name="block">The state to set</param>
        /// <returns>The Socket after setting the state</returns>
        public Socket ConfigureBlocking(bool block)
        {
            _osSocket.Blocking = block;
            return _osSocket;
        }

        /// <summary>
        /// Function to connect to the remote address.
        /// </summary>
        /// <param name="remote">The remote address</param>
        /// <returns>Status of connection</returns>
        public bool Connect(EndPoint remote)
        {
            _osSocket.Connect(remote);
            return _osSocket.Connected;
        }

        /// <summary>
        /// Checks if socket is connected.
        /// </summary>
        /// <returns>the connection state</returns>
        public bool IsConnected()
        {
            return _osSocket.Connected;
        }

        /// <summary>
        /// Checks if socket is blocking.
        /// </summary>
        /// <returns>the blocking state</returns>
        public bool IsBlocking()
        {
            return _osSocket.Blocking;
        }

        /// <summary>
        /// Function to register a socket to the selector.
        /// </summary>
        /// <param name="sel">the selctor to register to</param>
        /// <param name="ops">the operation to set to</param>
        /// <param name="att">the attribute of the object</param>
        /// <returns>The key of the object</returns>
        public SelectionKey Register(Selector sel, int ops, Object att)
        {
            return sel.register(_osSocket, ops, att);
        }

    }
}
