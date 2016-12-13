using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

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

					Console.WriteLine (message.State);

					do {

						bytesRead = stream.Read (buffer, 0, buffer.Length);
						dataRead = System.Text.Encoding.ASCII.GetString (buffer, 0, bytesRead);

						content = message.CheckValidity (dataRead);
						Console.WriteLine (message.State);


						while (content != "") {

							content = message.CheckValidity (content);
							Console.WriteLine (message.State);
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
					Console.WriteLine (sb.ToString ());
					requestedURL = WebServer.ParseHttpRequest (sb.ToString ());

					//NOTE: We are safe to do this parse and check because we've reach an accepting state.

					if (requestedURL == "") {
						//No Host: was specified.
						success = false;
					} else {
						string formattedResponse = string.Format (responseTemplate, "11346814", DateTime.Now, requestedURL);
						byte[] msg = System.Text.Encoding.ASCII.GetBytes (formattedResponse);

						stream.Write (msg, 0, msg.Length);
						//Console.WriteLine(String.Format("Sent: {0}", formattedResponse));
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
			Regex regex = new Regex ("\r\nHost: *.*\r\n");
			Match match = regex.Match (data);
			string value = match.Value;

			if (value.Length > 7 && value [7] == ' ') {
                    
				value = (value.Length > 8) ? value.Substring (8) : "";
			} else {
				value = (value.Length > 7) ? value.Substring (7) : "";
			}
			
			//if value = "" this means we did not get a valid template with Host:

			//starts after sp and ends right before cr
			return value;
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

		/*NOTE: Start-line of a request is called request-line
		 * 
		 * The following is the request-line format:
		 * 
		 * method SP request-target SP HTTP-version CRLF
		 */

		// Method SP, for this assignment only GET is used.
		private class MethodState : State
		{
			
			public MethodState (Message message)
			{
				this.message = message;
				Initialize ();
			}

			private void Initialize ()
			{
				content = "";
			}

			public override string CheckValidity (string content)
			{
				this.content += content;
				return StateChangeCheck ();
			}

			private string StateChangeCheck ()
			{

				if (content.Length >= 4) {

					//Required single space.
					if (content.Substring (0, 4) != "GET ") {
						message.State = new ErrorState ();
					} else {

						//because we have already checked the validity of the method
						//we can now check everything after that (as well as the SP right after)

						content = (content.Length > 4) ? content.Substring (4) : "";
						
						//change our context's state, we are now done with being in the state of method
						message.State = new LHSSingleSpaceState (this);
					}
				} else {
					return "";
				}
					
				//if the content length is yet to be 4, we stay in this method state.
				return content;
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
				
				if (content.Length >= 1) {

					//need to check for other whitespace characters? ***Ask Evan
					if (content [0] == ' ') {
						message.State = new ErrorState ();
					} else {
						//content is valid without that space, pass onto the request target state.
						message.State = new RequestTargetState (this);
					}
				}

				return content;
			}

		}

		// Request-Target SP
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
				
				//TODO: have better uri validation, ask Evan for more details.
				//need to check for other whitespace characters? ***Ask Evan

				int i = 0;
				bool flag = false;
				while (i < content.Length) {
					//space indicates end of our uri
					if (content [i] == ' ') {
						flag = true;
						break;
					}
					i++;
				}

				//if length > i + 1, then we can be at element (i+ 1)
				content = (content.Length > i + 1) ? content.Substring (i + 1) : "";
				if (flag) {
					message.State = new RHSSingleSpaceState (this);
				}
				return content;
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
				
				if (content.Length >= 1) {

					//need to check for other whitespace characters? ***Ask Evan
					if (content [0] == ' ') {
						message.State = new ErrorState ();
					} else {
						//keep content as is since we know it's not another sp
						//we can proceed to checking the version
						message.State = new HttpVersionState (this);
						//content = "";
					}
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
				} else {
					return "";
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
						Console.WriteLine (content);
						//if we immediatly are done, jump to final state.
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
				if (content.Length >= 1) {
					int i = 0;
					bool flag = false;
					while (i < content.Length) {
							if (content [i] == ':') {

							flag = true;
							break;
						}
						i++;
					}

					//if length > i + 1, then we can be at element (i+ 1)
					content = (content.Length > i + 1) ? content.Substring (i + 1) : "";
					if (flag) {
						message.State = new LHSOWSState (this);
					}
				}
				return content;
			}
		}

		private class LHSOWSState: State
		{
			public LHSOWSState (State state) : this (state.Message)
			{

			}

			public LHSOWSState (Message message)
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
				if (content.Length >= 1) {
					int i = 0;
					while (i < content.Length) {

						//this means that we are done with OWS's
						//OWS = *(SP or HTAB), move on to next state
						if (content [i] != ' ' && content [i] != '\t') {
							message.State = new FieldValueState (this);
							break;
						}
						i++;
					}

					//if length > i + 1, then we can be at element (i+ 1)
					content = (content.Length > i + 1) ? content.Substring (i + 1) : "";
				}

				return content;
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
						if (i + 1 < content.Length && content.Substring(i, 2) == "\r\n") {
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
				this._state = new MethodState (this);
			}

			public string CheckValidity (string content)
			{
				return this._state.CheckValidity (content);
			}
		}
	}
}

