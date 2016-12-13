using System;

namespace CS422
{
    internal class DemoService : WebService
    {
        private const string c_template =
            "<html>This is the response to the request:<br>" +
            "Method: {0}<br>Request-Target/URI: {1}<br>" +
            "Request body size, in bytes: {2}<br><br>" +
            "Student ID: {3}</html>";
        
        public override string ServiceURI
        {
            get
            {
                return "/";
            }
        }

        public override void Handler(WebRequest req)
        {
            Tuple<string, string> t;

            //see if content length was provided.
            bool success = req.Headers.TryGetValue("Content-Length".ToLower(), out t);

            string requestBodySize = "0"; //no content-length provided.

            if (success)
            {
                requestBodySize = t.Item2;
            }

            string formattedString = String.Format(c_template, 
                req.Method, req.RequestTarget, requestBodySize, "11346814");
            
            req.WriteHTMLResponse(formattedString);
        }
    }
}

