using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

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

        private NetworkStream _response; //private reference we use to write to.

        public WebRequest(NetworkStream response){
            _response = response;
            _headers = new ConcurrentDictionary<string, Tuple<string, string>>();
        }

        /*public long GetContentLengthOrDefault(long defaultValue)
        {
            // ??
        }*/

        //support start byte to end byte
        //and supprt start byte to no end given
        public Tuple<long,long> GetRangeHeader(long resourceLength)
        {
            Tuple<long, long> t = null;
            Tuple<string, string> rangeHeaderValue = null;

            if (_headers.TryGetValue("range", out rangeHeaderValue))
            {
                string range = rangeHeaderValue.Item2.ToString();

                //get rid of all whitespace
                range = Regex.Replace(range, @"\s+", string.Empty);

                //get rid of bytes=
                range = Regex.Replace(range, "bytes=",string.Empty);

                //start-end, or start-
                string[] ranges = range.Split(new char[]{'-'},
                    StringSplitOptions.RemoveEmptyEntries);

                //x- format
                if (ranges.Length == 1)
                {
                    long firstByte = long.Parse(ranges[0]);
                    long lastByte = resourceLength - 1;

                    if (firstByte >= resourceLength)
                    {
                        return null; //our firstByte is out of range.
                    }

                    t = new Tuple<long, long>(firstByte, lastByte);
                }
                else if (ranges.Length == 2) //x-y format
                {
                    long firstByte = long.Parse(ranges[0]);
                    long lastByte = long.Parse(ranges[1]);

                    if (firstByte >= resourceLength || lastByte >= resourceLength
                        || firstByte > lastByte)
                    {
                        return null; //invalid range
                    }

                    t = new Tuple<long, long>(firstByte, lastByte);

                }
            }

            return t;
        }



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

        public void WriteRangeNotSatisfiableResponse(string pageHTML, string fileLength){
            StringBuilder sb = new StringBuilder();
            DateTime date = DateTime.Now;
            string HttpDate = date.ToUniversalTime().ToString("r");

            sb.Append("HTTP/1.1 416 Range Not Satisfiable\r\n");
            sb.Append("Date:" + HttpDate + "\r\n");
            sb.Append("Content-Range: bytes */" + fileLength + "\r\n");

            sb.Append("\r\n");

            sb.Append(pageHTML);

            byte[] buffer = ASCIIEncoding.ASCII.GetBytes(sb.ToString());
            _response.Write(buffer, 0, buffer.Length);

            //NOTE: must dispose
            _response.Dispose();
        }

        public void WriteResponse(byte[] buffer){
            _response.Write(buffer, 0, buffer.Length);
        }

        public void WriteResponse(byte[] buffer, int length){
            _response.Write(buffer, 0, length);
        }

        public void CloseResponse(){
            _response.Dispose();
        }
    }
}

