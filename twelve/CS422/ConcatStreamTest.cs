using System;
using System.IO;
using NUnit.Framework;

namespace CS422
{
    [TestFixture]
    public class ConcatStreamTest
    {
        [Test]
        public void ReadTwoMemoryStreamTest(){
            MemoryStream ms1 = new MemoryStream(new byte[]{ 0, 1, 2, 3 });
            MemoryStream ms2 = new MemoryStream(new byte[]{ 4, 5, 6, 7, 8, 9 });

            ConcatStream concatStream = new ConcatStream(ms1, ms2);

            byte[] buffer = new byte[10];
            int read = 0;
            int count = 1;
            int bufferPosition = 0;
            byte[] originalData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };


            /****NOTE: The count changing is the "Reading data in random chunks"*********/

            Console.WriteLine("bufferPosition = {0}, count = {1}", bufferPosition,
                count);
            while( (read = concatStream.Read(buffer, bufferPosition, count)) > 0 ){


                if (bufferPosition < 9)
                {
                    bufferPosition += read;
                } 

                if (count + 1 + bufferPosition < buffer.Length)
                {
                    count++;
                }
                else
                {
                    count = 1;
                }

                Console.WriteLine("bufferPosition = {0}, count = {1}, read = {2}",
                    bufferPosition,
                    count, read);
            }

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(buffer[i]);
                Assert.AreEqual(originalData[i], buffer[i]);
            }
        }

        [Test]
        public void MsWithNoSeekMsTest(){
            MemoryStream ms1 = new MemoryStream(new byte[]{ 0, 1, 2, 3 });
            NoSeekMemoryStream ms2 = new NoSeekMemoryStream(new byte[]{ 4, 5, 6, 7, 8, 9 });

            ConcatStream concatStream = new ConcatStream(ms1, ms2);

            byte[] buffer = new byte[10];
            int read = 0;
            int count = 1;
            int bufferPosition = 0;
            byte[] originalData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };


            /****NOTE: The count changing is the "Reading data in random chunks"*********/

            Console.Error.WriteLine("bufferPosition = {0}, count = {1}", bufferPosition,
                count);
            while( (read = concatStream.Read(buffer, bufferPosition, count)) > 0 ){


                if (bufferPosition < 9)
                {
                    bufferPosition += read;
                } 

                if (count + 1 + bufferPosition < buffer.Length)
                {
                    count++;
                }
                else
                {
                    count = 1;
                }

                Console.Error.WriteLine("bufferPosition = {0}, count = {1}, read = {2}",
                    bufferPosition,
                    count, read);
            }

            for (int i = 0; i < 10; i++)
            {
                Console.Error.WriteLine(buffer[i]);
                if (buffer[i] != originalData[i])
                {
                    Assert.Fail();
                }
            }

        }

        [Test]
        public void LengthExceptionTest(){
            try{
                MemoryStream ms1 = new MemoryStream(new byte[]{ 0, 1, 2, 3 });
                NoSeekMemoryStream ms2 = new NoSeekMemoryStream(new byte[]{ 4, 5, 6, 7, 8, 9 });

                ConcatStream concatStream = new ConcatStream(ms2, ms1);
                Assert.Fail();
            } catch(NotSupportedException){
            }
        }

        [Test]
        public void LengthNonExceptionTest(){
            try{
                MemoryStream ms1 = new MemoryStream(new byte[]{ 0, 1, 2, 3 });
                NoSeekMemoryStream ms2 = new NoSeekMemoryStream(new byte[]{ 4, 5, 6, 7, 8, 9 });

                ConcatStream concatStream = new ConcatStream(ms1, ms2);

            } catch(NotSupportedException){
                Assert.Fail();
            }
        }

        [Test]
        public void WriteSecondConstructorTest(){
            MemoryStream ms1 = new MemoryStream(new byte[5]);
            MemoryStream ms2 = new MemoryStream(new byte[5]);

            ConcatStream concatStream = new ConcatStream(ms1, ms2, 5);

            byte[] buffer = new byte[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            concatStream.Write(buffer, 0, 10);

            concatStream.Seek(0, SeekOrigin.Begin);

            byte[] buffer2 = new byte[10];

            concatStream.Read(buffer2, 0, 10);

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(buffer2[i]);
                Assert.AreEqual(buffer[i], buffer2[i]); 
            }
        }

        [Test]
        public void CheckStreamLengthTest(){
            MemoryStream ms1 = new MemoryStream(new byte[5]);
            MemoryStream ms2 = new MemoryStream(new byte[5]);

            ConcatStream concatStream = new ConcatStream(ms1, ms2);

            Assert.AreEqual(10, concatStream.Length);
        }

        /*The ConcatStream must also be able to expand provided the following two things
        are true:
        1. The second of the two streams supports expanding
        2. The 2-parameter constructor was used to instantiate the ConcatStream 
        (i.e. the stream is not
        fixed length due to the use of the 3-parameter constructor)
        Do NOT let the stream expand if the 3-parameter constructor was used, 
        even if the second of the two
        streams supports expanding.
        */

        [Test]
        public void ExpandableWriteTest(){
            MemoryStream ms1 = new MemoryStream(new byte[5]);
            MemoryStream ms2 = new MemoryStream();

            ConcatStream concatStream = new ConcatStream(ms1, ms2);
            Console.WriteLine("Length = {0}", concatStream.Length);

            byte[] buffer = new byte[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            concatStream.Write(buffer, 0, 10);

            concatStream.Seek(0, SeekOrigin.Begin);

            byte[] buffer2 = new byte[10];

            concatStream.Read(buffer2, 0, 10);

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(buffer2[i]);
                Assert.AreEqual(buffer[i], buffer2[i]); 
            }

            Console.WriteLine("Length = {0}", concatStream.Length);
        }
    }
}

