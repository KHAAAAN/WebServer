using System;
using CS422;

namespace HW13
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var x = StandardFileSystem.Create("/home/jay/422/HW9Test");
            var y = new FilesWebService(x);
            WebServer.AddService(y);
            WebServer.Start(4220, 10);
        }
    }
}
