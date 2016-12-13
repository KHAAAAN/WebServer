using System;
using System.IO;
using System.Collections.Generic;
using CS422;

namespace HW8
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //string dir = @"/home/jay/422";

            byte[] buffer = new byte[1024];

            buffer[0] = 1;
            buffer[1] = 2;
            buffer[2] = 3;
            buffer[3] = 1;
            buffer[4] = 2;
            buffer[5] = 3;


            MemoryStream ms = new MemoryStream(buffer, 0, 1024);
            ms.Write(new byte[]{ 4, 5, 6 }, 0, 3);
            Console.WriteLine(buffer[0]);

            List<Item> list = new List<Item>();
            Item item = new Item();

            for(int i = 0; i < 2; i++){
                list.Add(new Item());
            }

            list.Add(item);

            for(int i = 0; i < 2; i++){
                list.Add(new Item());
            }

            list[2].test();
            list.RemoveAt(2);
            list[2].test();

            Console.WriteLine( ((char)('a' + 1)).ToString());
        }

        public class Item{
            public Item(){}
            public void test(){
                Console.WriteLine("test");
            }
        }

      
    }
}
