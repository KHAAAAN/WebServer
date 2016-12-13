using System;
using NUnit.Framework;
using CS422;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CS422Test
{
	[TestFixture]
	public class WebServerTest
	{
		private const string DefaultTemplate =
			"HTTP/1.1 200 OK\r\n" +
			"Content-Type: text/html\r\n" +
			"\r\n\r\n" +
			"<html>ID Number: {0}<br>" +
			"DateTime.Now: {1}<br>" +
			"Requested URL: {2}</html>";
		
		public WebServerTest ()
		{
		}

		[Test]
		public void testRequestWithWeb(){
			bool success = false;

			Thread t = new Thread (new ThreadStart (() => {
				success = WebServer.Start (4220, DefaultTemplate);
				Assert.AreEqual(true, success);
			}));

			Thread t2 = new Thread (new ThreadStart (() => {
				WebRequest request = HttpWebRequest.Create("http://localhost:4220");
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				WebHeaderCollection header = response.Headers;

				var encoding = ASCIIEncoding.ASCII;
				using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
				{
					//string responseText = reader.ReadToEnd();
					//Console.WriteLine(responseText);
				}


			}));

			t.Start ();
			t2.Start ();

			t.Join ();
			t2.Join ();

		}

		[Test]
		public void testRequestWithTCP(){
			bool success = false;

			Thread t = new Thread (new ThreadStart (() => {
				success = WebServer.Start (4221, DefaultTemplate);
				Assert.AreEqual(true, success);
			}));

			Thread t2 = new Thread (new ThreadStart (() => {
				
				byte[] buffer = new byte[1024];
				string header = "GET / HTTP/1.1\r\n" +
					"Host: localhost:4221\r\n" +
					"Connection: keep-alive\r\n" +
					"User-Agent: Mozilla/5.0\r\n" +
					"\r\n";

				//3-way handshake
				var client = new TcpClient("localhost", 4221);

				//send request
				client.Client.Send(System.Text.Encoding.ASCII.GetBytes(header));
				// get response
				var i = client.Client.Receive(buffer);
				var response1 = System.Text.Encoding.UTF8.GetString(buffer, 0, i);

				Console.WriteLine(response1);
			}));

			t.Start ();
			t2.Start ();

			t.Join ();
			t2.Join ();

		}

		[Test]
		public void testMethodState(){
			bool success = false;

			Thread t = new Thread (new ThreadStart (() => {
				success = WebServer.Start (4222, DefaultTemplate);
				Assert.AreEqual(false, success);
			}));

			Thread t2 = new Thread (new ThreadStart (() => {

				byte[] buffer = new byte[1024];
				string msg = "GEO / HTTP/1.1\r\n" +
					"Host: localhost:4222\r\n" +
					"Connection: keep-alive\r\n" +
					"User-Agent: Mozilla/5.0\r\n" +
					"\r\n";

				//3-way handshake
				var client = new TcpClient("localhost", 4222);

				//send request
				client.Client.Send(System.Text.Encoding.ASCII.GetBytes(msg));
				// get response
				var i = client.Client.Receive(buffer);
				var response1 = System.Text.Encoding.UTF8.GetString(buffer, 0, i);

				Console.WriteLine(response1);
			}));

			t.Start ();
			t2.Start ();

			t.Join ();
			t2.Join ();

		}

		[Test]
		public void testRequestTargetState(){
			bool success = false;

			Thread t = new Thread (new ThreadStart (() => {
				success = WebServer.Start (4223, DefaultTemplate);
				Assert.AreEqual(false, success);
			}));

			Thread t2 = new Thread (new ThreadStart (() => {

				byte[] buffer = new byte[1024];
				string msg = "GET  / HTTP/1.1\r\n" +
					"Host: localhost:4223\r\n" +
					"Connection: keep-alive\r\n" +
					"User-Agent: Mozilla/5.0\r\n" +
					"\r\n";

				//3-way handshake
				var client = new TcpClient("localhost", 4223);

				//send request
				client.Client.Send(System.Text.Encoding.ASCII.GetBytes(msg));
				// get response
				var i = client.Client.Receive(buffer);
				var response1 = System.Text.Encoding.UTF8.GetString(buffer, 0, i);

				Console.WriteLine(response1);
			}));

			t.Start ();
			t2.Start ();

			t.Join ();
			t2.Join ();

		}

		[Test]
		public void testRequestWithRHSOWS(){
			bool success = false;

			Thread t = new Thread (new ThreadStart (() => {
				success = WebServer.Start (4224, DefaultTemplate);
				Assert.AreEqual(true, success);
			}));

			Thread t2 = new Thread (new ThreadStart (() => {

				byte[] buffer = new byte[1024];
				string header = "GET / HTTP/1.1\r\n" +
					"Host: localhost:4224 \r\n" +
					"Connection: keep-alive\r\n" +
					"User-Agent: Mozilla/5.0 \r\n" +
					"\r\n";

				//3-way handshake
				var client = new TcpClient("localhost", 4224);

				//send request
				client.Client.Send(System.Text.Encoding.ASCII.GetBytes(header));
				// get response
				var i = client.Client.Receive(buffer);
				var response1 = System.Text.Encoding.UTF8.GetString(buffer, 0, i);

				Console.WriteLine(response1);
			}));

			t.Start ();
			t2.Start ();

			t.Join ();
			t2.Join ();

		}

		[Test]
		public void LHSOWStest(){
			bool success = false;

			Thread t = new Thread (new ThreadStart (() => {
				success = WebServer.Start (4225, DefaultTemplate);
				Assert.AreEqual(true, success);
			}));

			Thread t2 = new Thread (new ThreadStart (() => {

				byte[] buffer = new byte[1024];
				string header = "GET / HTTP/1.1\r\n" +
					"Host:localhost:4224 \r\n" +
					"Connection: keep-alive\r\n" +
					"User-Agent:Mozilla/5.0 \r\n" +
					"\r\n";

				//3-way handshake
				var client = new TcpClient("localhost", 4225);

				//send request
				client.Client.Send(System.Text.Encoding.ASCII.GetBytes(header));
				// get response
				var i = client.Client.Receive(buffer);
				var response1 = System.Text.Encoding.UTF8.GetString(buffer, 0, i);

				Console.WriteLine(response1);
			}));

			t.Start ();
			t2.Start ();

			t.Join ();
			t2.Join ();

		}

		[Test]
		public void ErrorStatetest1(){
			bool success = false;

			Thread t = new Thread (new ThreadStart (() => {
				success = WebServer.Start (4226, DefaultTemplate);
				Assert.AreEqual(false, success);
			}));

			Thread t2 = new Thread (new ThreadStart (() => {

				byte[] buffer = new byte[1024];
				string header = "GET / HTTP/1.1\r\n" +
					"Host:localhost:4224 \r\n" +
					"Connection keep-alive\r\n" +
					"User-Agent:Mozilla/5.0 \r\n" +
					"\r\n";

				//3-way handshake
				var client = new TcpClient("localhost", 4226);

				//send request
				client.Client.Send(System.Text.Encoding.ASCII.GetBytes(header));
				// get response
				var i = client.Client.Receive(buffer);
				var response1 = System.Text.Encoding.UTF8.GetString(buffer, 0, i);

				Console.WriteLine(response1);
			}));

			t.Start ();
			t2.Start ();

			t.Join ();
			t2.Join ();

		}
	}
}

