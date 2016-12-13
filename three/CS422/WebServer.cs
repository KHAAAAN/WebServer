using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace CS422
{
	public class WebServer
	{
		

		public WebServer ()
		{
		}

		public static bool Start (int port, string responseTemplate)
		{
			Message message = new Message ();
			bool success = true;

			//make a 1mb buffer
			byte[] buffer = new byte[1024];
		
			//translated to string form from data recieved from client socket.
			StringBuilder sb = new StringBuilder ();

			//the HOST field in the Request line of the HTTP request
			string requestedURL;

			try {
				//listens for connection on specified port parameter on all
				//network interfaces. I.e wired and wireless assigned IP addresses
				TcpListener tcpListener = new TcpListener (IPAddress.Any, port);

				//start listening 
				tcpListener.Start ();

				/********NOTE: These using statements call Dispose() at the end of the block implicitly ******/
				using (TcpClient tcpClient = tcpListener.AcceptTcpClient ())
				using (NetworkStream stream = tcpClient.GetStream ()) {

					//read the networkstream's data into buffer
					//loop to recieve all data sent by the client.

					int bytesRead = 0;
					string dataRead = "";
					string content = "";

					do {

						bytesRead = stream.Read (buffer, 0, buffer.Length);
						dataRead = System.Text.Encoding.ASCII.GetString (buffer, 0, bytesRead);

						content = message.CheckValidity (dataRead);

						while (content != "") {

							content = message.CheckValidity (content);
							if (message.State is ErrorState) {
								return false;
							}
						}



						sb.Append (dataRead);

					} while(stream.DataAvailable);


					//check if we reached finalstate, if no return false.
					if (!(message.State is FinalState)) {
						return false;
					}

					//at this point we will have the full data.
					requestedURL = WebServer.ParseHttpRequest (sb.ToString ());

					//NOTE: We are safe to do this parse and check because we've reach an accepting state.

					if (requestedURL == "") {
						//No Host: was specified.
						success = false;
					} else {
						string formattedResponse = string.Format (responseTemplate, "11346814", DateTime.Now, requestedURL);
						byte[] msg = System.Text.Encoding.ASCII.GetBytes (formattedResponse);

						stream.Write (msg, 0, msg.Length);
					}

					//close the tcp connection with the client since we are done.
					tcpClient.Close ();
				}
			} catch (SocketException) {
				success = false;
			}

			return success;
		}

		private static string ParseHttpRequest (string data)
        {
            Regex regex = new Regex ("\r\nHost:*.*\r\n");
            Match match = regex.Match (data);
            string value = match.Value;

            return value.Substring(7).Trim();
        }

		/********************************
		 * Software Design Pattern: State
		 * 
		 * NOTE: I used this Design Pattern, mostly
		 * for practice and to futureproof assignments
		 * 
		 * Will need rework, was not able to allocate
		 * time for this assignment due to exams. Many bugs persist. 
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

			public abstract string CheckValidity (string content);
		}

		private class EmptyState : State{
			public EmptyState (Message message){
				this.message = message;
				this.content = "";
			}

			public override string CheckValidity (string content)
			{
				this.content += content;
				return StateChangeCheck ();
			}

			private string StateChangeCheck ()
			{

				if (content.Length >= 3) {

					//Required single space.
					if (content.Substring (0, 3) != "GET") {
						message.State = new ErrorState ();
					} else {

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

			public override string CheckValidity (string content)
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
					message.State = new RequestTargetState (this);
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

			public override string CheckValidity (string content)
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

			public override string CheckValidity (string content)
			{
				// = instead of += so we don't have to loop the same content everytime
				this.content = content; 
				return StateChangeCheck (); 
			}

			private string StateChangeCheck ()
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

			public override string CheckValidity (string content)
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

			public override string CheckValidity (string content)
			{

				//we're adding again now because we want the full concatenated http version string with CRLF.
				this.content += content;
				return StateChangeCheck ();
			}

			private string StateChangeCheck ()
			{
				if (content.Length >= 10) {
					//Required singple space.
					if (content.Substring (0, 10) != "HTTP/1.1\r\n") {
						message.State = new ErrorState ();
					} else {

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

				//if the content length is yet to be 4, we stay in this method state.
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

			public override string CheckValidity (string content)
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
						message.State = new FinalState ();
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

            public override string CheckValidity (string content)
            {

                //keeping track of appended content does not matter
                this.content = content;
                return StateChangeCheck ();
            }

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
                        message.State = new OWSState (this);
                        break;
                    }
                    i++;
                }

                return (content.Length > i) ? content.Substring (i) : "";
            }
        }

        private class OWSState: State
        {
            public OWSState (State state) : this (state.Message)
            {

            }

            public OWSState (Message message)
            {
                this.message = message;
            }

            public override string CheckValidity (string content)
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
                        i++;
                        message.State = new FieldValueState (this);
                        break;
                    }
                    i++;
                }

                return (content.Length > i) ? content.Substring (i) : "";
            }
        }

		private class FieldValueState: State
		{
			public FieldValueState (State state) : this (state.Message)
			{

			}

			public FieldValueState (Message message)
			{
				this.message = message;
			}

			public override string CheckValidity (string content)
			{

				//keeping track of appended content does not matter
				this.content += content;
				return StateChangeCheck ();
			}

			private string StateChangeCheck ()
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
						i++;
					}

					if (!flag) {
						return "";
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

			public override string CheckValidity (string content)
			{
				return "";
			}
		}

		//this would be a CRLF followed by a CRLF.
		class FinalState : State
		{
			
			public FinalState ()
			{
			}

			public override string CheckValidity (string content)
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

			public string CheckValidity (string content)
			{
				return this._state.CheckValidity (content);
			}
		}
	}
}

