namespace socketry
{
    public record CallIdentifier(byte callId, byte fnId)
    {
        /// <summary>
        /// Function to hash the call and functionId.
        /// </summary>
        /// <returns>the new hashed value</returns>
        public int HashCode()
        {
            return callId << 8 | fnId;
        }
    }
}
