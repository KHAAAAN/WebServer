using System;
using System.IO;

namespace CS422
{
    public class ConcatStream : Stream
    {
        private Stream _first;
        private Stream _second;

        public override bool CanRead{ get{return _first.CanRead && _second.CanRead;} }

        public override bool CanSeek{ get{return _first.CanSeek && _second.CanSeek;} }

        public override bool CanWrite{ get{return _first.CanWrite && _second.CanWrite;} }

        private long _position;

        public override long Position { get{ return _position; } 
            set{
                _position = value;

                if (_position < 0) {
                    _position = 0;
                } else if (_position > Length) { 
                    //our position can't be past streamLength, i.e the EOF
                    //would be 1 past the last element

                    _position = Length;
                }
            }
        }


        public override long Length{ 
            get{
                if (_fixedLength != null) //second constructor was used.
                {
                    return _first.Length + (long)_fixedLength;
                }
                else
                {
                    //can let exception bubble up if length
                    //not supported
                    return _first.Length + _second.Length;
                }
            }
        
        }

        public ConcatStream(Stream first, Stream second){
            long firstLength = first.Length; //will throw exception if no length exists

            _position = 0;

            _first = first;
            _second = second;
        }

        private long? _fixedLength = null; //the second stream's length for ctor below

        public ConcatStream(Stream first, Stream second, long fixedLength){
            
            //will throw exception if no length exists for first constructor.
            long firstLength = first.Length;
            _fixedLength = fixedLength;

            _position = 0;

            _first = first;
            _second = second;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        // position < 0, == 0, and > 0 cases accounted for
        public override long Seek (long offset, System.IO.SeekOrigin origin)
        {
            if (!CanSeek)
            {
                //If both can't seek, not supported.
                throw new NotSupportedException();
            }

            long originLong = 0;

            //Begin case is accounted for with default originLong = 0 value.
            if (origin == System.IO.SeekOrigin.Current) {
                originLong = Position;
            } else if (origin == System.IO.SeekOrigin.End) {
                originLong = Length;
            }

            /*
             * if offset is negative, it'll go to an element preceeded by SeekOrigin
             * Also, if the sum stored in Position is negative, our Property's set
             * will put it back to 0 as per Evan's specs.
            */
            _position = offset + originLong;

            return Position;
        }


        public override void SetLength(long value)
        {
            throw new NotSupportedException();

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //0)
            if (!CanRead)
            {
                throw new NotSupportedException();
            }

             /* 1)
             * now from the offset to count has to be less than or equal
             * to buffer, therefore another case
             * could be that the buffer size might 
             * not be enough to read up to offset + count data
             * 
             * NOTE: The offset only takes the buffer
             * into account, (it has nothing to do with the stream)
             */
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException();
            }

            // 2) buffer wasn't initialized
            /* NOTE: This was specified in docs but
             * if buffer is a null parameter it will
             * throw a NullReferenceException before entering
             * this method. In the docs it says if the buffer
             * is null then throw ArgumentNullException()
             */
            if (buffer == null) {
                throw new ArgumentNullException();
            }

            // 3)
            if (offset < 0 || count < 0) {
                throw new ArgumentOutOfRangeException();
            }

            int totalRead = 0; //how much bytes we have read.
            int read = 0;

            //we are at the first stream. and count is not 0 so not to unneccesarilly go down here.
            if (_position < _first.Length && count != 0)
            {
                if (CanSeek)
                {
                    //we can seek first else, forward read only functionality.
                    _first.Seek(_position, SeekOrigin.Begin);
                }

                read = _first.Read(buffer, offset, count);
                _position += read;
                totalRead += read;
                //we have this much more to read.
                count = count - read;
            }

            //we are at the second stream. and we have more to read.
            if (_position >= _first.Length && count != 0)
            {
                if (CanSeek)
                {
                    //we can seek first else, forward read only functionality.

                    /*for example
                     * if _first was of length 2, and our Position was 2
                     * at this point, we'd have to start at the 0th
                     * position of _second, so subtract length from Position
                     */
                    _second.Seek(_position - _first.Length, SeekOrigin.Begin);
                }

                //offset + read, where read is an additional offset from what we read in 
                //first stream, that is if we read anything.

                if (_second.GetType() == typeof(System.Net.Sockets.NetworkStream))
                {
                    if (((System.Net.Sockets.NetworkStream)_second).DataAvailable)
                    {
                        read = _second.Read(buffer, offset + read, count);
                    }
                    else
                    {
                        read = 0;
                    }
                }
                else
                {
                    read = _second.Read(buffer, offset + read, count);
                }
                totalRead += read;
                _position += read;
            }


            return totalRead;

        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            //1
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException();
            }

            //2
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            //3
            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            //4
            if (!CanWrite)
            {
                throw new NotSupportedException();
            }
                
            int firstCount = 0;
            //we are the first stream
            if (_position < _first.Length)
            {
                //don't seek unneedlessly.
                if (_position != _first.Position && CanSeek)
                {
                    //we will start writing from this position.
                    _first.Seek(_position, SeekOrigin.Begin);
                }

                //only if we are at the position we need to be. we can write.
                if (_position == _first.Position)
                {
                    firstCount = count;

                    //count cannot exceed what we are able to write to
                    //in the first stream.
                    long ableToWrite = _first.Length - _first.Position;

                    if (count >= ableToWrite){
                        firstCount = (int) ableToWrite;

                        //subtract what we are able to write
                        //from count, now count 
                        //is what will be left after we write.
                        count -= (int)ableToWrite;
                    }

                    //else count was less, and we need not worry about
                    //writing more than we can to first stream.

                    _first.Write(buffer, offset, firstCount);

                    //write moves the position
                    _position += firstCount;
                    
                }
            }

            if (_position >= _first.Length)
            {
                if ( (_position - _first.Length) != _second.Position && CanSeek)
                {
                    _second.Seek(_position - _first.Length, SeekOrigin.Begin);
                } 

                //We HAVE to be at the proper position to write.
                if ((_position - _first.Length) == _second.Position)
                {
                    /********
                     * Before we write, make sure to limit count
                     * to something second stream can write
                     * to. AKA truncate if needed
                     */

                    if (_fixedLength != null && count > _fixedLength - _second.Position )
                    {
                        count = (int)_fixedLength - (int)_second.Position;
                    }

                    long prevPosition = _second.Position;

                    _second.Write(buffer, offset + firstCount, count);

                    long finalPositionCount = _second.Position - prevPosition;

                    //write moves the position.
                    _position += finalPositionCount; 
                }
                else
                {
                    //weren't able to write successfully.
                    throw new Exception();
                }
            }

        }
    }
}

