using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CS422
{
    public class FilesWebService : WebService
    {
        private FileSys422 _fs;
        private bool _allowUploads;

        //I.e If we make a request to http://localhost:4220/files
        //it will show root directory, this is the root of
        //the standard file system we are showing access to
        private const string _route = "/files";
        private const string errorHtml = "<html>404. That's an error</html>";

        public FilesWebService(FileSys422 fs){
            _fs = fs;
            _allowUploads = true;
        }

        public override string ServiceURI
        {
            get
            {
                return _route;
            }
        }

        public override void Handler(WebRequest req) //this is for get request.
        {
            if (req.Method == "GET")
            {
                GetHandler(req);
            }
            else if (req.Method == "PUT")
            {
                PutHandler(req);
            }
            else
            {
                throw new Exception("Method: " + req.Method + " not supported.");
            }
        }

        private void GetHandler(WebRequest req){

            //NOTE use Uri.UnescapeDataString to convert the escaped string
            //to it's unescaped representation.
            //i.e if we get http://localhost:4220/%20test/
            //would become http://localhost:4220/ test/
            string[] names = Uri.UnescapeDataString(req.RequestTarget).Split(new char[]{'/'}, 
                StringSplitOptions.RemoveEmptyEntries);

            //first name is files.
            string currString = "";

            Dir422 currNode = _fs.GetRoot(); //root is what we chose, the client knows it as "files".
            Dir422 nextNode = null;
            File422 file = null;

            int uriCase = 1;

            for(int i = 1; i < names.Length; i++){
                currString = names[i];
                if ((nextNode = currNode.GetDir(currString)) != null)
                {
                    //go to next dir to repeat iteration.
                    currNode = nextNode;
                }
                else if ((file = currNode.GetFile(currString)) != null)
                {
                    uriCase = 2;
                    break;
                }
                else
                {
                    uriCase = 3;
                    break;
                }
            }

            switch (uriCase)
            {
                //URI maps to an existing directory in the file system.
                case 1:
                    string htmlString = BuildDirHTML(currNode);
                    req.WriteHTMLResponse(htmlString);
                    break;

                    //The URI maps to an existing file in the file system
                case 2:
                    SendFileContent(file, req);
                    break;

                    //The URI maps to something that doesn’t exist in the file system
                case 3:
                    req.WriteNotFoundResponse(errorHtml);
                    break;
            }
        }

        private void PutHandler(WebRequest req){
            //NOTE use Uri.UnescapeDataString to convert the escaped string
            //to it's unescaped representation.
            //i.e if we get http://localhost:4220/%20test/
            //would become http://localhost:4220/ test/
            string[] names = Uri.UnescapeDataString(req.RequestTarget).Split(new char[]{'/'}, 
                StringSplitOptions.RemoveEmptyEntries);

            //first name is files.
            string currString = "";

            Dir422 currNode = _fs.GetRoot(); //root is what we chose, the client knows it as "files".
            Dir422 nextNode = null;

            for(int i = 1; i < names.Length; i++){
                currString = names[i];
                if(i == names.Length - 1){ // the c in /a/b/c
                    //put the file.
                    byte[] buffer = new byte[1024];
                    Stream concatStream = req.Body;
                    int read = 0;

                    string path = _fs.GetRoot().Name + BuildPath(currNode) + "/" + names[i];

                    //Don't allow overwriting existing files or files with \ character
                    if (currNode.ContainsFile(names[i], false) || names[i].Contains("\\"))
                    {
                        return;
                    }

                    using (FileStream newFile = File.OpenWrite(path))
                    {

                        while ((read = concatStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            newFile.Write(buffer, 0, read);
                            Array.Clear(buffer, 0, buffer.Length); //reset buffer for next read.
                        }
                    }
                }
                else if ((nextNode = currNode.GetDir(currString)) != null)
                {
                    //go to next dir to repeat iteration.
                    currNode = nextNode;
                }
                else
                {
                    return;
                }
            }

            string htmlString = BuildDirHTML(currNode);
            //req.WriteHTMLResponse(htmlString);
            req.WriteRegularResponse(htmlString);
        }

        private string BuildPath(Dir422 dir){
            Dir422 currNode = dir;
            string currString = "";

            //note: our root dir name is replaced with whatever _route's value is.
            while (currNode.Parent != null)
            {
                string currNodeName = currNode.Name.Replace("#", "%23");
                currString = "/" + currNodeName + currString;
                currNode = currNode.Parent;
            }

            return currString;
        }

        private string BuildDirHTML(Dir422 directory){
            StringBuilder foldersBuilder = new StringBuilder();
            StringBuilder filesBuilder = new StringBuilder();

            var dirs = directory.GetDirs(); 
            var files = directory.GetFiles();

            const string nodeTemplate = "<a href=\"" + _route + "{0}\">{1}</a><br>";
            string path = "";

            if (dirs.Count > 0)
            {
                path = BuildPath(directory);
                foldersBuilder.Append("<h1>Folders</h1>");
                foreach (Dir422 dir in dirs)
                {
                    string dirName = dir.Name.Replace("#", "%23");
                    foldersBuilder.Append(
                        String.Format(nodeTemplate,
                            path + "/" + dirName, dir.Name)
                    );
                }
            }

            if (files.Count > 0)
            {
                path = BuildPath(directory);
                filesBuilder.Append("<h1>Files</h1>");
                foreach (File422 file in files)
                {
                    string fileName = file.Name.Replace("#", "%23");
                    filesBuilder.Append(
                        String.Format(nodeTemplate,
                            path + "/" + fileName, file.Name)
                    );
                }
            }

            StringBuilder sb = new StringBuilder("<html>");

            if (_allowUploads)
            {
                sb.AppendLine(uploadJS);
            }
            
            if (files.Count == 0 && dirs.Count == 0)
            {
                sb.Append("<h1>Empty Directory.</h1>");
            }
            else
            {
                sb.AppendFormat("{0}{1}", 
                    foldersBuilder.ToString(), filesBuilder.ToString());
            }

            if(_allowUploads){
                sb.AppendFormat(uploadJS2, _route + BuildPath(directory)); //this second parameter is GetHREF in Evan's hw description
            }

            sb.Append("</html>");
                
            return sb.ToString();
        }

        private Dictionary<string, string> exts = new Dictionary<string, string>()
        {
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".pdf", "application/pdf" },
            { ".mp4", "video/mp4" },
            { ".txt", "text/plain" },
            { ".html", "text/html" },
            { ".xml", "application/xml" }
        };

        private void SendFileContent(File422 file, WebRequest req){
            byte[] buffer = new byte[1024]; //1kb buffer
            StringBuilder sb = new StringBuilder();
            string contentType; //default contentType
            string status = "200 Success"; //default status
            int initialPos;
            long contentLength, startByte = 0;

            //find extension
            if (!exts.TryGetValue(Path.GetExtension(file.Name).ToLower(),
                out contentType))
            {
                //default
                contentType = "application/octet-stream";
            }

            using(Stream fs = file.OpenReadOnly()){
                
                contentLength = fs.Length;
                //if client sent a range request.
                if (req.Headers.ContainsKey("range"))
                {
                    var t = req.GetRangeHeader(fs.Length);
                    //find range
                    if (t == null)
                    {
                        string pageHTML = "<html><h1>416 REQUESTED RANGE NOT SATISFIABLE</h1></html>";
                        req.WriteRangeNotSatisfiableResponse(pageHTML,
                            fs.Length.ToString());
                        return;
                    }
                    status = "206 Partial Content";
                    startByte = t.Item1; //start offset byte
                    contentLength = (t.Item2 - t.Item1) + 1; //because contentLength is the length, not last byte.
                
                }

                sb.Append("HTTP/1.1 " + status + "\r\n");
                sb.Append("Content-Length: " + contentLength + "\r\n");
                sb.Append("Content-Type: " + contentType + "\r\n");

                //we need this so that the file downloads, instead
                //of trying to switch views.
                /*sb.Append("Content-Disposition: attachment; filename=\"" +
                    file.Name + "\"\r\n");*/

                sb.Append("\r\n");
                initialPos = sb.Length;

                ASCIIEncoding.ASCII.GetBytes(sb.ToString()).CopyTo(buffer, 0);

                if (req.Headers.ContainsKey("range"))
                {
                    int totalBytesRead;
                    int bytesRead = 0;

                    //seek to startbyte.
                    fs.Seek(startByte, SeekOrigin.Begin);

                    //our initial read has to be the smaller of one of these 2.
                    int initialRead = ((buffer.Length - initialPos) < contentLength)
                        ? buffer.Length - initialPos
                        : (int)contentLength; // if (buffer.Length - initialPos) >= cL
                    
                    totalBytesRead = fs.Read(buffer, initialPos, initialRead);

                    //Console.WriteLine(ASCIIEncoding.ASCII.GetString(buffer));

                    //has to be what we had initially plus what we just read.
                    req.WriteResponse(buffer, initialPos + initialRead);
                    Array.Clear(buffer, 0, buffer.Length);

                    //if we still have not read up to content length, keep reading.
                    if (totalBytesRead < contentLength)
                    {
                        int subsequentRead = (buffer.Length < contentLength)
                            ? buffer.Length
                            : (int)contentLength; // if (buffer.Length - initialPos) >= cL

                        //keep track of previous total bytes
                        int prevTotalBytesRead = totalBytesRead;

                        while ((bytesRead = fs.Read(buffer, 0, subsequentRead)) != 0 &&
                           (totalBytesRead += bytesRead) < contentLength)
                        {
                            prevTotalBytesRead = totalBytesRead;

                            req.WriteResponse(buffer, bytesRead);
                            Array.Clear(buffer, 0, buffer.Length);
                        }

                        if (totalBytesRead >= contentLength)
                        {
                            //we subtract the value of totalBytes right before it was more than contentLength,
                            //from content length (contentLength - prevTotalBytesRead)
                            //this gives us the last bit we need to write to achieve the range requested's length.
                            req.WriteResponse(buffer, (int)contentLength - prevTotalBytesRead);
                        }
                    }

                }
                else
                {

                    fs.Read(buffer, initialPos, buffer.Length - initialPos);
                    req.WriteResponse(buffer);

                    while (fs.Read(buffer, 0, buffer.Length) != 0)
                    {
                        req.WriteResponse(buffer);
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                }

                req.CloseResponse();
            }
        }

        private const string uploadJS = @"<script>
        function selectedFileChanged(fileInput, urlPrefix){
         document.getElementById('uploadHdr').innerText = 'Uploading ' + fileInput.files[0].name + '...';
         
         // Need XMLHttpRequest to do the upload
         if (!window.XMLHttpRequest)
         {
            alert('Your browser does not support XMLHttpRequest. Please update your browser.');
            return;
         }

         // Hide the file selection controls while we upload
         var uploadControl = document.getElementById('uploader');
         if (uploadControl)
         {
            uploadControl.style.visibility = 'hidden';
         }
         // Build a URL for the request
         if (urlPrefix.lastIndexOf('/') != urlPrefix.length - 1)
         {
            urlPrefix += '/';
         }
         
         var uploadURL = urlPrefix + fileInput.files[0].name;

         // Create the service request object
         var req = new XMLHttpRequest();
         req.open('PUT', uploadURL);
         console.log(uploadURL);

         req.onreadystatechange = function()
         {
            console.log(req);
            console.log(req.status);
            console.log(document.getElementById('uploadHdr'))
            document.getElementById('uploadHdr').innerText = 'Upload (request status == ' + req.status + ')';
            // Un-comment the line below and comment-out the line above if you want the page to
             // refresh after the upload
            location.reload();
         };
         req.send(fileInput.files[0]);
        }
        </script>
        ";

        private const string uploadJS2 = "<hr><h3 id='uploadHdr'>Upload</h3><br>" +
            "<input id=\"uploader\" type='file' " +
            "onchange='selectedFileChanged(this,\"{0}\")' /><hr>";
    }
}