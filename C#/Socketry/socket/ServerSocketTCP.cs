using System.Net;
using System.Net.Sockets;

namespace socket
{
    /// <summary>
    /// Class to create the serverSocket.
    /// </summary>
    public class ServerSocketTCP : IServerSocket
    {
        private Socket _osServerSocket;

        /// <summary>
        /// Constructor for the server socket.
        /// </summary>
        public ServerSocketTCP()
        {
            _osServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _osServerSocket.Blocking = false;
        }

        /// <summary>
        /// Function to bind the scoket
        /// </summary>
        /// <param name="local">the local address</param>
        /// <param name="backlog"></param>
        /// <returns></returns>
        public Socket Bind(EndPoint local, int backlog)
        {
            _osServerSocket.Bind(local);
            _osServerSocket.Listen(backlog);
            return _osServerSocket;
        }

        /// <summary>
        /// Function to set the blocking state
        /// </summary>
        /// <param name="block">The state to set it</param>
        /// <returns></returns>
        public Socket ConfigureBlocking(bool block)
        {
            _osServerSocket.Blocking = block;
            return _osServerSocket;
        }

        /// <summary>
        /// Function to receive Accept connections.
        /// </summary>
        /// <returns>the accepted socket</returns>
        public ISocket Accept()
        {
            return new SocketTCP(_osServerSocket.Accept());
        }


    }
}
