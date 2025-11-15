using System.Net;
using System.Net.Sockets;

namespace socket
{
    /// <summary>
    /// Interface for IserverSocket
    /// </summary>
    public interface IServerSocket
    {
        /// <summary>
        /// Function to bind the socket
        /// </summary>
        /// <param name="local">The local address</param>
        /// <returns></returns>
        Socket Bind(EndPoint local)
        {
            return Bind(local, 0);
        }

        /// <summary>
        /// Function to bind the scoket
        /// </summary>
        /// <param name="local">the local address</param>
        /// <param name="backlog"></param>
        /// <returns></returns>
        Socket Bind(EndPoint local, int backlog);

        /// <summary>
        /// Function to set the blocking state
        /// </summary>
        /// <param name="block">The state to set it</param>
        /// <returns></returns>
        Socket ConfigureBlocking(bool block);

        /// <summary>
        /// Function to receive Accept connections.
        /// </summary>
        /// <returns>The accepted socket</returns>
        ISocket Accept();
    }
}
