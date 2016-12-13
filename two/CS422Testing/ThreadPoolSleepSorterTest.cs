using System;
using CS422;
using NUnit.Framework;

namespace CS422Testing
{
	[TestFixture]
	public class ThreadPoolSleepSorterTest
	{
		public ThreadPoolSleepSorterTest ()
		{
		}

		[Test]
		public void testSort(){
			ThreadPoolSleepSorter tpss = new ThreadPoolSleepSorter(Console.Out, 10);
			tpss.Sort (new byte[]{ 2, 1, 3 });
		}
	}
}

