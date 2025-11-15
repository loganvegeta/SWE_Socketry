namespace socketry
{
    public class SocketPacket
    {
        public int BytesLeft;
        public MemoryStream Content;

        /// <summary>
        /// Constructor for SocketPacket class.
        /// </summary>
        /// <param name="contentLength">The length of the packet</param>
        /// <param name="content">the data to read from</param>
        public SocketPacket(int contentLength, MemoryStream content)
        {
            this.BytesLeft = contentLength;
            this.Content = content;
        }
    }
}
