using System;
using System.IO;
using NUnit.Framework;

namespace CS422
{
    [TestFixture]
    public class NoSeekMemoryStreamTest
    {
        public NoSeekMemoryStreamTest()
        {
        }

        [Test]
        public void SeekMethodExceptionTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[1]);
            try{
                ms.Seek(5, SeekOrigin.Begin);
                Assert.Fail();
            } catch(NotSupportedException){
            }
        }

        [Test]
        public void SeekWithPositionTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[1]);
            try{
                ms.Position = 5;
                Assert.Fail();
            } catch(NotSupportedException){
            }
        }

        [Test]
        public void CanReadTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[1]);
            if (!ms.CanRead)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void CanWriteTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[1]);
            if (!ms.CanRead)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void CanSeekTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[1]);
            if (ms.CanSeek)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Constructor1BufferNullTest(){
            try{
                NoSeekMemoryStream ms = new NoSeekMemoryStream(null);
                Assert.Fail();
            } catch (ArgumentNullException) {

            }
        }

        [Test]
        public void Constructor2BufferNullTest(){
            try{
                NoSeekMemoryStream ms = new NoSeekMemoryStream(null, 0, 10);
                Assert.Fail();
            } catch (ArgumentNullException) {

            }
        }

        [Test]
        public void Constructor2NegativeOffsetOrCountTest(){
            try{
                NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[1], -1, 1);
                Assert.Fail();
            } catch(ArgumentOutOfRangeException){

            }

            try{
                NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[1], 0, -1);
                Assert.Fail();
            } catch(ArgumentOutOfRangeException){

            }
        }

        [Test]
        public void Constructor2StreamStartTest(){
            try{
                NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[10], 5, 6);
                Assert.Fail();
            } catch(ArgumentException){

            }
        }

        [Test]
        public void ReadBufferNullTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[10]);

            try{
                ms.Read(null, 0, 4);
                Assert.Fail();
            } catch(ArgumentNullException){

            }
        }

        [Test]
        public void ReadNegativeOffsetOrCountTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[10]);
            byte[]buffer = new byte[10];
            try{
                ms.Read(buffer, -1, 1);
                Assert.Fail();
            } catch(ArgumentOutOfRangeException){

            }

            try{
                ms.Read(buffer, 0, -1);
                Assert.Fail();
            } catch(ArgumentOutOfRangeException){

            }
        }

        [Test]
        public void ReadMoreThanWeCanTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[10]);
            byte[]buffer = new byte[10];
            try {
                ms.Read(buffer, 5, 6);
                Assert.Fail();
            } catch (ArgumentException){

            }
        }

        [Test]
        public void ReadTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[10]);
            byte[]buffer = new byte[10];
            int read = ms.Read(buffer, 0, 10);
            if (read != 10)
            {
                Assert.Fail();
            }

        }

        [Test]
        public void ReadZeroTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[10]);
            byte[]buffer = new byte[10];
            int read = ms.Read(buffer, 0, 10);

            read = ms.Read(buffer, 0, 10);

            if (read != 0)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void ReadMovesPositionTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[10]);
            byte[]buffer = new byte[10];
            int read = ms.Read(buffer, 3, 5);
            if (ms.Position != 5)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void WriteBufferNullTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[10]);
            byte[]buffer = new byte[10];

            try{
                ms.Write(null, 0, 10);
            } catch(ArgumentNullException){
            }
        }

        [Test]
        public void WriteNegativeOffsetOrCountTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[10]);
            byte[]buffer = new byte[10];
            try{
                ms.Write(buffer, -1, 1);
                Assert.Fail();
            } catch(ArgumentOutOfRangeException){

            }

            try{
                ms.Write(buffer, 0, -1);
                Assert.Fail();
            } catch(ArgumentOutOfRangeException){

            }
        }

        [Test]
        public void WriteMovesPositionTest(){
            NoSeekMemoryStream ms = new NoSeekMemoryStream(new byte[10]);
            byte[]buffer = new byte[10];

            Assert.AreEqual(0, ms.Position);

            ms.Write(buffer, 0, 3);

            Assert.AreEqual(3, ms.Position);
        }
    }
}

