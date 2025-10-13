using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executive
{
    public class DummyClass
    {
        public int Add(int a,int b)
        {
            Console.WriteLine($"{a} + {b} = {a+b}");
            return a + b;
        }

        public byte[] AddWrapper(byte[] args) 
        {
            int a = BitConverter.ToInt32(args.Take(4).Reverse().ToArray(), 0);
            int b = BitConverter.ToInt32(args.Skip(4).Take(4).Reverse().ToArray(), 0);
            int res = Add(a,b);

            return BitConverter.GetBytes(res);
        }
    }
}
