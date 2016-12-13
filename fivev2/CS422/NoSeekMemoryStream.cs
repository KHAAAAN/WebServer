using System;
using System.IO;
namespace CS422
{
    /// <summary>
    /// Represents a memory stream that does not support seeking, but otherwise has
    /// functionality identical to the MemoryStream class.
    /// </summary>
    /// 
    /// A class derived from Stream does not support seeking. Length is not supported
    /// according to ms docs.
    public class NoSeekMemoryStream : MemoryStream
    {
        public override bool CanRead{ get{return true;} }

        public override bool CanSeek{ get{return false;} }

        public override bool CanWrite{ get{return true;} }

        private long _position;
        public override long Position { get{ return _position; } set{ throw new NotSupportedException(); } } 

        public override int Capacity{ get; set; }

        public override long Length
        {
            get {throw new NotSupportedException(); }
        }

        //underlying buffer
        private byte[] _buffer;

        public NoSeekMemoryStream(byte[] buffer){
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }
            _buffer = buffer;
            Capacity = _buffer.Length;
            _position = 0;
        }

        //offset is index to start from
        //count is the length of the stream in bytes.
        public NoSeekMemoryStream(byte[] buffer, int offset, int count){
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException();
            }

            _buffer = buffer;
            Capacity = count;
            _position = offset;
        }

        // implement
        // Override necessary properties and methods to ensure that this stream functions
        // just like the MemoryStream class, but throws a NotSupportedException when seeking
        // is attempted (you'll have to override more than just the Seek function!)

        public override long Seek(long offset, SeekOrigin loc)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException();
            }

            //Console.WriteLine("Length = {0}, Capacity = {1}", Length, Capacity);
            if(Position == Capacity){
                count = 0;
            }

            int read = 0; //how many bytes have been read.

            for(int i = 0; i < count && Position < Capacity; i++){
                
                buffer[i + offset] = _buffer[_position++];
                read++;
            }

            return read;
        }
            

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException();
            }

            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < count && _position < Capacity; i++)
            {
                _buffer[_position++] = buffer[i + offset];
            }

        }
    }
}

