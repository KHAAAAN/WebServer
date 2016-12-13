using System;
using CS422;
using System.IO;
using System.Text.RegularExpressions;

namespace HW9
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            WebServer.Start(4220, 10);

        }
    }
}