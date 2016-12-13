using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace CS422
{
    public class WebServer
    {
        private static BlockingCollection<Action> coll;
        private static Object addServiceLock;
        private static List<WebService> services;
        private static TcpListener tcpListener;
        private static List<Thread> threads;

        private static readonly int firstThreshold = 2048;
        private static readonly int secondThreshold = 100 * 1024;

        private static void Initialize(int threadCount){
            //initialize static private fields
            coll = new BlockingCollection<Action>();
            addServiceLock = new object();
            services = new List<WebService>();
            threads = new List<Thread>();

            //add our services/applications. Thread-safe.
            AddService(new FilesWebService(
                new StandardFileSystem("/home/jay/422/HW9Test")
                )
            );
            AddService(new DemoService());
        }

        public static bool Start(int port, int threadCount){

            //you need at least one to listen and additional for TCP sockets.
           threadCount = (threadCount <= 0) ? threadCount = 64 : threadCount;
           
            Initialize(threadCount);

            int threadCounter = 0;
            while (threadCounter < threadCount) {
                Thread t = new Thread (
                    new ThreadStart(()=>{
                        ThreadWork(coll);
                    })
                );

                threads.Add(t);

                //start the ThreadWorkerMethod from this thread
                t.Start ();

                threadCounter++;
            }

            //create listen thread
            Thread listenerThread = new Thread(
                new ThreadStart(() =>
                {
                    tcpListener = new TcpListener(IPAddress.Any, port);
                    tcpListener.Start(); // start listening

                    while (true)
                    {
                        try
                        {
                            TcpClient client = tcpListener.AcceptTcpClient(); //******NOTE: must dispose yourself.

                            //create a thread to deal with new TCP socket.
                            coll.Add(() =>
                                { 

                                    WebRequest request = BuildRequest(client); 

                                    if (request != null)
                                    {

                                        //should it be non-null then an appropriate handler is found in the list
                                        //of handlers/services that the web stores

                                        bool flag = false;
                                        
                                        for (int i = 0; i < services.Count; i++)
                                        {
                                            string SURI = services[i].ServiceURI;

                                            //if serviceURI is a prefix of request-target
                                                if (request.RequestTarget.Length >= SURI.Length 
                                                    && SURI == request.RequestTarget.Substring(0, SURI.Length))
                                            {
                                                services[i].Handler(request);
                                                flag = true;
                                                break;
                                            }
                                        }

                                        if (!flag)
                                        {
                                            request.WriteNotFoundResponse(errorHtml);
                                        }

                                    } //else if request was null, then invalid request from client.

                                    //thread will go back to idle if it is null

                                    //close client before we leave.
                                    client.Close();
                                });
                        }
                        catch (SocketException)
                        {
                            
                            /*if (e.SocketErrorCode == SocketError.Interrupted){
                                break;
                            } */

                            break;
                        }

                    }
                    }
                ));

            listenerThread.Start();

            return true;

        }

        private static string errorHtml = "<html>404. That's an error</html>";

        public static void Stop(){

            //make sure to return the worker threads from 
            //their idling state first.
            for (int i = 0; i < threads.Count; i++) {
                coll.Add (null);
            }

            //block till all threads are done 
            //NOTE: it is important to do this
            //after adding null tasks, or
            //else you would be blocked on
            //Take() method forever
            foreach(Thread thread in threads){
                thread.Join();
            }

            //stop accepting client connections.
            tcpListener.Stop();

            //we are done.

        }

        public static void AddService(WebService service){
            //This makes the add service thread-safe.
            lock (addServiceLock){
                services.Add(service);
            }

        }

        public static void ThreadWork(BlockingCollection<Action> coll){
            while (true) {
                //once we invoke, this thread will become busy
                //for however long it sleeps

                Action myTask = coll.Take (); //this is blocking.
                if (myTask == null) {
                    break;
                } else {
                    myTask.Invoke ();
                }
            }
        }

        private static WebRequest BuildRequest(TcpClient client){
            Message message = new Message ();
            WebRequest webRequest = null;

            //make a 1mb buffer
            byte[] buffer = new byte[1024];

            //stop watch that checks if x seconds have passed
            var stopWatch = new System.Diagnostics.Stopwatch();

            //NOTE: client is closed after null is returned
            try {

                NetworkStream stream = client.GetStream (); //NOTE: Must dispose networkstream in webrequest responses.

                //the read will throw an IOException if it takes 2 seconds
                //to stream.Read() a megabyte of data
                stream.ReadTimeout = 2000;

                webRequest = new WebRequest(stream); //reference the stream so we can write response to it, then close.
                //read the networkstream's data into buffer
                //loop to recieve all data sent by the client.

                int bytesRead = 0;
                string content = "";

                int firstThresholdBytes = 0;
                int secondThresholdBytes = 0;

                //we add crlf flag everytime we enter CRLF state
                //when flag is false, first crlf has not been reached
                //when it is true, we have reached first crlf, can do checks
                //then potentially move on to secondThreshold checking
                bool flag = false;

                //needs to exit once FinalState which tells us we finished
                while( !(message.State is FinalState) && stream.DataAvailable) {

                    //starts if first iteration, else resumes.
                    stopWatch.Start();
                    bytesRead = stream.Read (buffer, 0, buffer.Length);

                    /*
                     * The reason we stop the stop watch after every
                     * Read() is we don't want to include the elapsed
                     * time of functionality that doesn't include
                     * reading from the stream like checking
                     * validity and building the HTTP request object.
                     */
                    stopWatch.Stop();

                    /*
                     * If the double line break has not been received
                     * after 10 seconds total, then return null which
                     * will close the connection.
                     * 
                     */
                    if(stopWatch.ElapsedMilliseconds > 10){
                        return null;
                    }

                    if (!flag){ //haven't reached first line break yet
                        firstThresholdBytes += bytesRead;
                    } else {
                        secondThresholdBytes += bytesRead;
                    }

                    content = System.Text.Encoding.ASCII.GetString (buffer, 0, bytesRead);

                    do {

                        content = message.CheckValidity (content, webRequest);
                        if (message.State is ErrorState) {
                            
                            return null;
                        } 

                        if (!flag && message.State is CRLFState){
                            flag = true;
                        }
                    } while (content != "");

                    //check if firstThreshold value has been exceeded
                    if(!flag && firstThresholdBytes >= firstThreshold){
                        return null;
                    } else if(!(message.State is FinalState) && //final state reprsents all the content being read up until body or double CRLF
                        secondThresholdBytes >= secondThreshold){ //we know now we've passed firstThreshold test.
                        return null;
                    }
                        
                };

                //check if we reached finalstate, if no return false.
                if (!(message.State is FinalState)) {
                    return null;
                }

                //else we have the final state.
                string bodyContents = ((FinalState) message.State).BodyContents;
                MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(bodyContents));

                ConcatStream concatStream = null;
                Tuple<string, string> tuple;

                //concatenate the network stream to memorystream
                if(webRequest.Headers.TryGetValue("Content-Length".ToLower(), out tuple)){

                    //if the content-length header is present, make length queryable.
                    //substract bodycontent's length from the queryable content length to get length of actual body.
                    //since the 3rd parameter represents the second stream's "length"
                    concatStream = new ConcatStream(ms, stream, int.Parse(tuple.Item2) - bodyContents.Length); 
                } else {

                    //else, call to Length property will throw an exception
                    concatStream = new ConcatStream(ms, stream);
                }

                webRequest.Body = concatStream; //the body's first byte should be what the value actually is

                //NOTE: don't close client here, close them in responses. since we were successful.
                
            } catch (SocketException) {
                webRequest = null; //make sure it's null.
            }

            return webRequest;
        }

        /********************************
         * Software Design Pattern: State
         * 
         * Reworked from HW3 to a 
         * true Deterministic Finite Automata with proper
         * input feedings and to be able to handle the
         * building of WebRequest object
         ********************************/


        private abstract class State
        {
            protected Message message;
            //context
            protected string content;

            protected HashSet<char> crlfHashSet; 

            public State(){
                crlfHashSet = new HashSet<char> ();
                crlfHashSet.Add ('\r');
                crlfHashSet.Add ('\n');
            }

            public Message Message {
                get { return message; }
                set { message = value; }
            }

            public string Content {
                get { return content; }
                set { content = value; }
            }

            public abstract string CheckValidity (string content, WebRequest webRequest);
        }

        private class EmptyState : State{
            public EmptyState (Message message){
                this.message = message;
                this.content = "";
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {
                this.content += content;
                return StateChangeCheck (webRequest);
            }

            private string StateChangeCheck (WebRequest webRequest)
            {

                if (content.Length >= 3) {

                    //Required single space.
                    if (content.Substring (0, 3) != "GET") {
                        message.State = new ErrorState ();
                    } else {

                        //set webrequest's METHOD field.
                        webRequest.Method = content.Substring(0, 3);

                        //because we have already checked the validity of the method
                        //we can now check everything after that (as well as the SP right after)

                        content = (content.Length > 3) ? content.Substring (3) : "";

                        //change our context's state, we are now done with being in the state of method
                        message.State = new MethodState (this);
                    }
                } else {
                    return ""; //this will let us break out of the caller's loop
                }

                //if the content length is yet to be 3, we stay in this method state.
                return content;
            }

        }

        /*NOTE: Start-line of a request is called request-line
         * 
         * The following is the request-line format:
         * 
         * method SP request-target SP HTTP-version CRLF
         */

        //We are now in the Method state, for this assignment only GET is used.
        private class MethodState : State
        {
            public MethodState (State state) : this (state.Message)
            {
            }

            public MethodState (Message message)
            {
                this.message = message;
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {
                this.content += content;
                return StateChangeCheck ();
            }

            private string StateChangeCheck ()
            {

                if (content [0] != ' ') {
                    message.State = new ErrorState ();
                } else {
                    //content is valid without that space, pass onto the request target state.
                    message.State = new LHSSingleSpaceState(this);
                }

                //"we eat the SP input" if it was there
                return (content.Length > 1) ? content.Substring (1) : "";
            }
        }

        //for making sure to keep our single space constraint.
        private class LHSSingleSpaceState : State
        {

            //if there is a space to the right of our single space, that is an error state.
            //i.e GET  /whatever versus GET /whatever .. where the former is incorrect

            public LHSSingleSpaceState (State state) : this (state.Message)
            {
            }

            //overload instructor will work with passing
            //state and stage.Message.
            public LHSSingleSpaceState (Message message)
            {
                this.message = message;
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {
                this.content = content;
                return StateChangeCheck ();
            }

            private string StateChangeCheck ()
            {
                //if it's another sp or '\r' or '\n', it's wrong.
                if (content [0] == ' ' ||  crlfHashSet.Contains(content[0])) {
                    message.State = new ErrorState ();
                } else {
                    //content is valid without that space, pass onto the request target state.
                    message.State = new RequestTargetState (this);
                }

                return content;
            }

        }

        // SP Request-Target SP 
        private class RequestTargetState : State
        {

            /*here have a default parameter of the state's message.
             * state.Message is a property that belongs to the 
             * parameter we are passing in.
             */
            public RequestTargetState (State state) : this (state.Message)
            {
            }

            //overload instructor will work with passing
            //state and stage.Message.
            public RequestTargetState (Message message)
            {
                this.message = message;
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {
                // = instead of += so we don't have to loop the same content everytime
                this.content = content; 
                return StateChangeCheck (webRequest); 
            }

            private string StateChangeCheck (WebRequest webRequest)
            {
                int i = 0;
                while (i < content.Length) {
                    
                    if (crlfHashSet.Contains (content [i])) {
                        message.State = new ErrorState ();
                        break;
                    } //SP indicates end of our Request-URI
                    else if (content [i] == ' ') {
                        message.State = new RHSSingleSpaceState (this);
                        i++; //so we know we have eaten the SP before entering the new state
                        break;
                    }

                    webRequest.RequestTarget += content[i];

                    i++;
                }

                //if length > i, then we can be at element (i)
                return(content.Length > i) ? content.Substring (i) : "";
            }

        }

        //for making sure to keep our single space constraint.
        private class RHSSingleSpaceState : State
        {

            public RHSSingleSpaceState (State state) : this (state.Message)
            {
            }

            //overload instructor will work with passing
            //state and stage.Message.
            public RHSSingleSpaceState (Message message)
            {
                this.message = message;
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {
                this.content = content;
                return StateChangeCheck ();
            }

            private string StateChangeCheck ()
            {
                //another sp or a cr or an lf?
                if (content [0] == ' ' || crlfHashSet.Contains(content[0])) {
                    message.State = new ErrorState ();
                } else {
                    //keep content as is since we know it's not another sp
                    //we can proceed to checking the http version
                    message.State = new HttpVersionState (this);
                    //content = "";
                }

                return content;
            }

        }

        // HTTP version
        private class HttpVersionState : State
        {

            public HttpVersionState (State state) : this (state.Message)
            {
            }

            public HttpVersionState (Message message)
            {
                this.message = message;
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {

                //we're adding again now because we want the full concatenated http version string with CRLF.
                this.content += content;
                return StateChangeCheck (webRequest);
            }

            private string StateChangeCheck (WebRequest webRequest)
            {
                if (content.Length >= 10) {
                    //Required singple space.
                    if (content.Substring (0, 10) != "HTTP/1.1\r\n") {
                        
                        message.State = new ErrorState ();
                    } else {

                        webRequest.HTTPVersion = content.Substring(0, 10);

                        //because we have already checked the validity of the method
                        //we can now check everything after that (as well as the SP right after)

                        content = (content.Length > 10) ? content.Substring (10) : "";

                        //change our context's state, we are now done with being in the state of method
                        message.State = new CRLFState (this);
                        //content = ""; //you already know you're going to be starting at the above state, dont return current content
                    }
                } else { //because we've already added all of what content is currently, break out of calling loop and call next .Read()
                    return ""; //we have to return "" because we want this state's content to stay the same.
                }

                //if the content length is yet to be 10, we stay in this method state.
                return content;
            }

        }

        //carriage return line feed state i.e \r\n
        private class CRLFState : State
        {

            public CRLFState (State state) : this (state.Message)
            {

            }

            public CRLFState (Message message)
            {
                this.message = message;
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {

                //adding because we want to be able to hit \r\n if it exists
                this.content += content;
                return StateChangeCheck ();
            }

            private string StateChangeCheck ()
            {
                //TODO: ask Evan, check for any header whitespaces not in the form of \r\n? i.e "\n" or " "?
                if (content.Length >= 2) {
                    if (content.Substring (0, 2) == "\r\n") {
                        //another \r\n indicates we immediatly are done, jump to final state.

                        //send any of the body's content found after \r\n.
                        message.State = new FinalState (content.Substring(2));

                        content = "";
                    } else {
                        //Fieldname Could be of form "F:"
                        //if we made it this far.
                        message.State = new FieldNameState (this);
                    }
                } else {
                    return "";
                }

                return content;
            }
        }

        //field-name  =  1*<any CHAR, excluding CTLs, SPACE, and ":">
        private class FieldNameState: State
        {
            public FieldNameState (State state) : this (state.Message)
            {

            }

            public FieldNameState (Message message)
            {
                this.message = message;
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {

                //keeping track of appended content does not matter
                this.content = content;
                return StateChangeCheck ();
            }

            //this is for webRequest
            private string fieldName = "";

            private string StateChangeCheck ()
            {
                int i = 0;
                while (i < content.Length) {
                    if(content[i] == ' '){
                        message.State = new ErrorState ();
                        break;
                    }
                    else if (content [i] == ':') {

                        i++;
                        message.State = new OWSState (this, fieldName);
                        break;
                    }

                    //building up the fieldname to store in webRequest
                    fieldName += content[i];

                    i++;
                }

                return (content.Length > i) ? content.Substring (i) : "";
            }
        }

        private class OWSState: State
        {
            private string fieldName;
            //fieldname to pass to field value state.
            public OWSState (State state, string fieldName) : this (state.Message)
            {
                this.fieldName = fieldName;
            }

            public OWSState (Message message){
                this.message = message;
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {

                //keeping track of appended content does not matter
                this.content = content;
                return StateChangeCheck ();
            }

            private string StateChangeCheck ()
            {
                int i = 0;
                while (i < content.Length) {
                    if(content[i] == ':'){ // : should not be the first value in field-value.
                        message.State = new ErrorState ();
                        break;
                    }
                    else if (content [i] != '\t' && content[i] != ' ') { //not an OWS, proceed.
                        message.State = new FieldValueState (this, this.fieldName, content[i]);
                        i++;
                        break;
                    }
                    i++;
                }

                return (content.Length > i) ? content.Substring (i) : "";
            }
        }

        private class FieldValueState: State
        {
            private string fieldName;
            private string fieldValue;

            public FieldValueState (State state, String fieldName, char fieldValue) : this (state.Message)
            {
                this.fieldName = fieldName;
                this.fieldValue += fieldValue; //first character of field value
            }

            public FieldValueState (Message message){
                this.message = message;
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {

                //keeping track of appended content does not matter
                this.content += content;
                return StateChangeCheck (webRequest);
            }

            private string StateChangeCheck (WebRequest webRequest)
            {
                if (content.Length >= 2) {
                    int i = 0;
                    bool flag = false;

                    while (i < content.Length) {
                        //we don't need a RHSOWSState since we eventually end up at \r\n.
                        //we can have form Test: testing 123 testing \t \t   \t \r\n.
                        //we will just include the RHSOWSState with field value state.
                        if (i + 1 < content.Length && content.Substring (i, 2) == "\r\n") {
                            content = (content.Length > i + 2) ? content.Substring (i + 2) : "";
                            flag = true;
                            message.State = new CRLFState (this);
                            break;
                        }

                        //build up the field value for webRequest.
                        fieldValue += content[i];

                        i++;
                    }

                    if (!flag)
                    {
                        return "";
                    }
                    else
                    {
                        //if flag == true.
                        webRequest.Headers.TryAdd(fieldName.ToLower(), new Tuple<string, string> (this.fieldName, this.fieldValue));
                    }

                } else {
                    return "";
                }

                return content;
            }
        }

        class ErrorState : State
        {

            public ErrorState ()
            {
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {
                return "";
            }
        }

        //this would be a CRLF followed by a CRLF.
        class FinalState : State
        {
            public string BodyContents{ get; set;}
            public FinalState (string bodyContents)
            {
                BodyContents = bodyContents;
            }

            public override string CheckValidity (string content, WebRequest webRequest)
            {
                //this will stay for the remainder of the body read
                //body can be anything according to Evan's specs
                return "";
            }
        }

        /*
         * The 'Context' class
         */
        private class Message
        {
            private State _state;

            public State State {
                get{ return _state; }
                set{ _state = value; }
            }

            public Message ()
            {
                this._state = new EmptyState (this);
            }

            public string CheckValidity (string content, WebRequest webRequest)
            {
                return this._state.CheckValidity (content, webRequest);
            }
        }
    }
}

