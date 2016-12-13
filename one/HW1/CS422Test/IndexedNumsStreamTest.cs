using System;
using NUnit.Framework;
using CS422;

namespace CS422Test
{
	[TestFixture]
	public class IndexedNumsStreamTest
	{
		public IndexedNumsStreamTest ()
		{
		}

		[Test]
		public void testConstructor(){
			IndexedNumsStream ins = new IndexedNumsStream (long.MinValue);
			long expected = 0;
			long actual = ins.Length;

			Assert.AreEqual (expected, actual);

		}

		[Test]
		public void testMaxStreamLength(){
			IndexedNumsStream ins = new IndexedNumsStream (long.MaxValue);
			Assert.AreEqual(long.MaxValue, ins.Length);
		}

		[Test]
		public void testCanSeek(){
			IndexedNumsStream ins = new IndexedNumsStream (600);
			Assert.AreEqual (true, ins.CanSeek);
		}

		[Test]
		public void testSeekNegative(){

		}

		/*
		 * Test cases for begin, current, and end:
		 * 
		 * 1) Does position set to end of stream if offset > streamLength?
		 * 2) Is it stream position set to 0 if returns negative?
		 * 3) Does it set to expected position otherwise
		 */

		[Test]
		public void testSeekBegin(){
			IndexedNumsStream ins = new IndexedNumsStream (600);
			System.IO.SeekOrigin origin = System.IO.SeekOrigin.Begin;

			//1)
			long newPosition = ins.Seek (999, origin);
			Assert.AreEqual (newPosition, 600);


			//2)
			newPosition = ins.Seek(-9999, origin);
			Assert.AreEqual (0, newPosition);

			//3)
			//position should now be at 0.
			newPosition = ins.Seek (50, origin);
			Assert.AreEqual (50, newPosition);
		}

		[Test]
		public void testSeekCurrent(){
			IndexedNumsStream ins = new IndexedNumsStream (600);
			System.IO.SeekOrigin origin = System.IO.SeekOrigin.Current;

			//1)
			long newPosition = ins.Seek (999, origin);
			Assert.AreEqual (newPosition, 600);


			//2)
			newPosition = ins.Seek(-9999, origin);
			Assert.AreEqual (0, newPosition);

			//3)
			//position should now be at 0.
			ins.Seek(50, origin);
			newPosition = ins.Seek (50, origin);
			Assert.AreEqual (100, newPosition);
		}

		[Test]
		public void testSeekEnd(){
			IndexedNumsStream ins = new IndexedNumsStream (600);
			System.IO.SeekOrigin origin = System.IO.SeekOrigin.End;

			//1)
			long newPosition = ins.Seek (999, origin);
			Assert.AreEqual (newPosition, 600);


			//2)
			newPosition = ins.Seek(-9999, origin);
			Assert.AreEqual (0, newPosition);

			//3)
			//position should now be at 0.
			newPosition = ins.Seek(-50, origin);
			Assert.AreEqual (550, newPosition);
		}

		[Test]
		public void testSetLength(){
			IndexedNumsStream ins = new IndexedNumsStream (600);

			ins.SetLength (500);
			Assert.AreEqual (500, ins.Length);
		}

		[Test]
		public void canBufferHoldData(){
			IndexedNumsStream ins = new IndexedNumsStream (600);

			byte[] buffer = new byte[10];

			try{
				ins.Read (buffer, 5, 6);
				Assert.IsTrue (false);
			} catch(ArgumentException){
				Assert.IsTrue (true);
			}

			try{
				ins.Read (buffer, 5, 5);
			} catch(ArgumentException){
				Assert.IsTrue (true);
			}
		}

		[Test]
		public void testNullBuffer(){
			IndexedNumsStream ins = new IndexedNumsStream (600);
			byte[] buffer = null;
			try{
				ins.Read(buffer, 10, 10);
			} catch(NullReferenceException){
				Assert.IsTrue (true);
			}
		}

		[Test]
		public void testArgumentOutOfRangeException(){
			IndexedNumsStream ins = new IndexedNumsStream (600);
			byte[] buffer = new byte[100];
			try{
				ins.Read(buffer, -10, 10);
			} catch(ArgumentOutOfRangeException){
				Assert.IsTrue (true);
			}

			try{
				ins.Read(buffer, 10, -10);
			} catch(ArgumentOutOfRangeException){
				Assert.IsTrue (true);
			}
		}

		[Test]
		public void testEndOfStream(){
			IndexedNumsStream ins = new IndexedNumsStream (600);
			byte[] buffer = new byte[100];

			ins.Seek (0, System.IO.SeekOrigin.End);
			int actual = ins.Read (buffer, 10, 10);
			Assert.AreEqual (0, actual);
		}

		[Test]
		public void testStreamLeftToRead(){
			IndexedNumsStream ins = new IndexedNumsStream (7);
			byte[] buffer = new byte[100];

			ins.Seek (4, System.IO.SeekOrigin.Begin);
			int nBytes = ins.Read (buffer, 0, 7);

			Assert.AreEqual (3, nBytes);
		}

		[Test]
		public void testStreamStructure(){
			IndexedNumsStream ins = new IndexedNumsStream (600);
			byte[] buffer = new byte[500];
			ins.Read (buffer, 0, 500);

			for (int i = 0; i < buffer.Length; i++) {
				Assert.AreEqual (i % 256, buffer [i]);
			}
		}

		[Test]
		public void testSetPositionPastStream(){
			IndexedNumsStream ins = new IndexedNumsStream (10);
			ins.Seek (999, System.IO.SeekOrigin.End);
			Assert.AreEqual (ins.Position, ins.Length);
		}
	}
}

