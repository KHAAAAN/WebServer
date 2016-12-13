using System;
using System.IO;
using CS422;

namespace HW5v2
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            MemoryStream ms = new MemoryStream(10);
            MemoryStream ms2 = new MemoryStream(10);
            byte[] buffer = new byte[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

            ms.Write(buffer, 0, 10);


            ms2.Write(buffer, 0, 10);

            //ConcatStream concatStream = new ConcatStream(ms, ms2);
            byte[] buffer2 = new byte[20];

            ConcatStream concatStream = new ConcatStream(ms, ms2);

            concatStream.Read(buffer2, 0, 15);

            for (int i = 0; i < 15; i++)
            {
                Console.WriteLine(buffer2[i]);
            }
        }
    }
}
