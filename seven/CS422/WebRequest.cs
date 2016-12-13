using System;
using System.Collections.Generic;
//using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;
using System.Text;

namespace CS422
{
    public class WebRequest
    {
        public Stream Body {get; set;} //will often be a ConcatStream object

        //key is lowercase header, value is [0] original header case [1] header value

        private ConcurrentDictionary<string, Tuple<string, string> > _headers;
        public ConcurrentDictionary<string, Tuple<string, string> > Headers { get {return _headers;} 
            set{ _headers = value;}
        }

        public string Method { get; set; }
        public string RequestTarget { get; set; }
        public string HTTPVersion{ get; set; }

        public WebRequest(NetworkStream response){
            _response = response;
            _headers = new ConcurrentDictionary<string, Tuple<string, string>>();
        }

        /*public long GetContentLengthOrDefault(long defaultValue)
        {
            // ??
        }
        public Tuple<long,long> GetRangeHeader()
        {
            // ??
        }*/

        private NetworkStream _response; //private reference we use to write to.

        public void WriteNotFoundResponse(string pageHTML){
            StringBuilder sb = new StringBuilder();

            sb.Append("HTTP/1.1 404 Not Found\r\n");
            sb.Append("Content-Length: " + pageHTML.Length + "\r\n");
            sb.Append("Content-Type: text/html\r\n");

            sb.Append("\r\n");

            sb.Append(pageHTML);

            byte[] buffer = ASCIIEncoding.ASCII.GetBytes(sb.ToString());
            _response.Write(buffer, 0, buffer.Length);

            //NOTE: must dispose
            _response.Dispose();
        }

        public bool WriteHTMLResponse(string htmlString){
            StringBuilder sb = new StringBuilder();

            sb.Append("HTTP/1.1 200 Success\r\n");
            sb.Append("Content-Length: " + htmlString.Length + "\r\n");
            sb.Append("Content-Type: text/html\r\n");

            sb.Append("\r\n");

            sb.Append(htmlString);

            byte[] buffer = ASCIIEncoding.ASCII.GetBytes(sb.ToString());
            _response.Write(buffer, 0, buffer.Length);

            //NOTE: must dispose
            _response.Dispose();

            return true;
        }
    }
}

