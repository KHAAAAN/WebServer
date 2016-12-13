using System;
using CS422;

namespace HW2
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			
			PCQueue pCQueue = new PCQueue ();
			int out_value = -1;
			bool success = false;

			for (int i = 0; i < 10; i++) {
				pCQueue.Enqueue (i);
			}

			//pCQueue.printQueue ();

			for (int i = 0; i < 11; i++) {
				success = pCQueue.Dequeue (ref out_value);
				Console.WriteLine ("success = " + success);
				Console.WriteLine ("out_value = " + out_value);
				out_value = -1;
			}



			/*using (ThreadPoolSleepSorter tpss = 
				new ThreadPoolSleepSorter (Console.Out, 30)) {
				tpss.Sort (new byte[]{ 4, 2, 9, 3 , 5, 8, 7, 10, 1, 6});
				tpss.Sort (new byte[]{ 3, 0, 2 });
				tpss.Sort (new byte[]{ 0, 0, 0});

				for (int i = 14; i > 0; i--) {
					tpss.Sort (new byte[]{ (byte)i });
				}

				//Console.WriteLine ("Press ENTER to cancel wait.");
				//Console.ReadLine ();
			}*/
		}
	}
}
