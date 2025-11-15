namespace packetparser
{
    /// <summary>
    /// Record to store the packet and its length
    /// </summary>
    /// <param name="packet">The packet record</param>
    /// <param name="bytesConsumed">The size of packet</param>
    public record ParseResult(Packet packet, int bytesConsumed) { }
}