using System.Net;
using System.Net.Sockets;

/**
 ServerSocketChannel -> Socket
 SocketChannel -> Socket
 SelectableChannel -> Socket
 */
namespace socket
{
    /// <summary>
    /// Interface for Isocket
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// Function to read for a given stream.
        /// </summary>
        /// <param name="dst">the stream to read from</param>
        /// <returns>the number of bytes read</returns>
        int Read(MemoryStream dst);

        /// <summary>
        /// Function to write data from a stream.
        /// </summary>
        /// <param name="src">The stream to write from</param>
        /// <returns>The number of bytes written</returns>
        int Write(MemoryStream src);

        /// <summary>
        /// Function to set the blocking state
        /// </summary>
        /// <param name="block">The state to set</param>
        /// <returns>The Socket after setting the state</returns>
        Socket ConfigureBlocking(bool block);

        /// <summary>
        /// Function to connect to the remote address.
        /// </summary>
        /// <param name="remote">The remote address</param>
        /// <returns>Status of connection</returns>
        bool Connect(EndPoint remote);

        /// <summary>
        /// Checks if socket is connected.
        /// </summary>
        /// <returns>the connection state</returns>
        bool IsConnected();

        /// <summary>
        /// Checks if socket is blocking.
        /// </summary>
        /// <returns>the blocking state</returns>
        bool IsBlocking();

        /// <summary>
        /// Function to register a socket to the selector.
        /// </summary>
        /// <param name="sel">the selctor to register to</param>
        /// <param name="ops">the operation to set to</param>
        /// <param name="att">the attribute of the object</param>
        /// <returns>The key of the object</returns>
        SelectionKey Register(Selector sel, int ops, Object att);

        /// <summary>
        /// Function to register a socket to the selector.
        /// </summary>
        /// <param name="sel">the selctor to register to</param>
        /// <param name="ops">the operation to set to</param>
        /// <returns>The key of the object</returns>
        SelectionKey Register(Selector sel, int ops)
        {
            return Register(sel, ops, null);
        }
    }
}
