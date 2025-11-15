namespace socket
{
    public class SelectionKey
    {
        public static int OP_READ = 1;
        public static int OP_WRITE = 2;
        public static int OP_CONNECT = 4;
        private Object att;
        private bool isValid = true;
        private bool isReadable = false;
        private bool isWritable = false;
        private bool isConnectable = false;

        /// <summary>
        /// Function to set the attribute.
        /// </summary>
        /// <param name="att">the attribute to set</param>
        public void setAtt(Object att)
        {
            this.att = att;
        }

        /// <summary>
        /// Function to set the readable state.
        /// </summary>
        /// <param name="val">the val to set the state</param>
        public void setReadable(bool val)
        {
            isReadable = val;
        }

        /// <summary>
        /// Function to set the writable state.
        /// </summary>
        /// <param name="val">the val to set the state</param>
        public void setWritable(bool val)
        {
            isWritable = val;
        }

        /// <summary>
        /// Function to set the connectable state.
        /// </summary>
        /// <param name="val">the val to set the state</param>
        public void setConnectable(bool val)
        {
            isConnectable = val;
        }

        /// <summary>
        /// Function to check if key is valid
        /// </summary>
        /// <returns>the state of key</returns>
        public bool IsValid()
        {
            return isValid;
        }

        /// <summary>
        /// Function to check if key is readable
        /// </summary>
        /// <returns>the state of key</returns>
        public bool IsReadable()
        {
            return isReadable;
        }

        /// <summary>
        /// Function to check if key is writable
        /// </summary>
        /// <returns>the state of key</returns>
        public bool IsWritable()
        {
            return isWritable;
        }

        /// <summary>
        /// Function to check if key is cpnnectable
        /// </summary>
        /// <returns>the state of key</returns>
        public bool IsConnectable()
        {
            return isConnectable;
        }

        /// <summary>
        /// Function to give the attachment
        /// </summary>
        /// <returns>the attachement of key</returns>
        public Object Attachment()
        {
            return att;
        }
    }
}
