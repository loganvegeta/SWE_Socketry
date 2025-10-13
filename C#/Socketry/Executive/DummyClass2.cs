using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Executive
{
    public class DummyClass2
    {
        public byte[] hello(byte[] args)
        {
            return System.Text.Encoding.UTF8.GetBytes("Hello World\n");
        }

        public byte[] AddSerialize(int a,int b)
        {
            byte[] bytes = new byte[8];
            byte[] aBytes = BitConverter.GetBytes(a);
            byte[] bBytes = BitConverter.GetBytes(b);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(aBytes);
                Array.Reverse(bBytes);
            }

            Buffer.BlockCopy(aBytes, 0, bytes, 0, 4);
            Buffer.BlockCopy(bBytes, 0, bytes, 4, 4);
            return bytes;
        }
    }
}
