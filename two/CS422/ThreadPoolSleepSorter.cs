using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CS422
{
	public class ThreadPoolSleepSorter : IDisposable
	{
		private delegate void MyTask ();

		TextWriter output;
		BlockingCollection<MyTask> coll;
		private int threadCount;

		public ThreadPoolSleepSorter(TextWriter output, ushort threadCount){
			this.output = output;
			this.threadCount = threadCount;
			if (threadCount == 0) {
				threadCount = 64;
			} 

		
			coll = new BlockingCollection<MyTask> ();
			coll.ToArray ().Length;
			ushort threadCounter = 0;
			while (threadCounter < threadCount) {
				Thread t = new Thread (
					new ThreadStart(()=>{
						ThreadWorkerMethod(coll);
					})
				);

				//start the ThreadWorkerMethod from this thread
				t.Start ();

				threadCounter++;
			}
		}

		//have multiple threads from threadpool start this method
		//basically work to get available tasks done
		private void ThreadWorkerMethod(BlockingCollection<MyTask> coll){
			//This will loop to seek available tasks that haven't been taken by other threads.
			while (true) {
				//once we invoke, this thread will become busy
				//for however long it sleeps

				MyTask myTask = coll.Take ();
				if (myTask == null) {
					break;
				} else {
					myTask.Invoke ();
				}
			}
		}

		public void Sort(byte[] values){

			foreach (byte b in values) {
				//by adding, you're queueing tasks.
				coll.Add (()=>{

					//this sleeps on whichever thread is not currently sleeping
					//and available.
					Thread.Sleep ((int)b * 1000);
					output.WriteLine (b);
				});
			}
		}
			
		public void Dispose(){
			for (int i = 0; i < threadCount; i++) {
				coll.Add (null);
			}
		}
	}
}

