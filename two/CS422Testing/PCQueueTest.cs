using System;
using System.Threading;
using CS422;
using NUnit.Framework;

namespace CS422Testing
{
	[TestFixture]
	public class PCQueueTest
	{

		[Test]
		public void multiThreadTest ()
		{
			PCQueue pCQueue = new PCQueue ();

			Thread producer;
			Thread consumer;

			pCQueue.Enqueue (1);

			producer = new Thread (new ThreadStart (() => {

				pCQueue.Enqueue(2);

			}
			));
			consumer = new Thread (new ThreadStart(()=>{
				//int out_value = 0;
				//bool success = pCQueue.Dequeue (ref out_value);
				//Console.WriteLine ("success = " + success + "\nout_value = " + out_value);

				//out_value = 0;
				//success = pCQueue.Dequeue (ref out_value);
				//Console.WriteLine ("success = " + success + "\nout_value = " + out_value);
					
			}));
				
			consumer.Start ();
			producer.Start ();

		
		}
	}
}

