using System;

namespace CS422
{
	public class IndexedNumsStream : System.IO.Stream
	{
		private long streamLength;
		public override long Length { get{return this.streamLength;} }

		private long position;
		public override long Position { get{ return this.position; } 
			set{ 
				this.position = value;

				if (this.position < 0) {
					this.position = 0;
				} else if (this.position > streamLength) { 
					//our position can't be past streamLength, i.e the EOF
					//would be 1 past the last element

					this.position = streamLength;
				}
			}
		}
			
		public IndexedNumsStream (long streamLength)
		{
			this.streamLength = (streamLength < 0) ? 0 : streamLength;
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanWrite {
			get {
				return false;
			}
		}

		public override bool CanSeek {
			get {
				return true;
			}
		}

		// position < 0, == 0, and > 0 cases accounted for
		public override long Seek (long offset, System.IO.SeekOrigin origin)
		{
			long originLong = 0;

			//Begin case is accounted for with default originLong = 0 value.
			if (origin == System.IO.SeekOrigin.Current) {
				originLong = Position;
			} else if (origin == System.IO.SeekOrigin.End) {
				originLong = streamLength;
			}

			/*
			 * if offset is negative, it'll go to an element preceeded by SeekOrigin
			 * Also, if the sum stored in Position is negative, our Property's set
			 * will put it back to 0 as per Evan's specs.
			*/
			Position = offset + originLong;

			return Position;
		}

		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		public override void SetLength (long value)
		{
			this.streamLength = (value < 0) ? 0 : value;
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			/* 1)
			 * now from the offset to count has to be less than or equal
			 * to buffer, therefore another case
			 * could be that the buffer size might 
			 * not be enough to read up to offset + count data
			 * 
			 * NOTE: The offset only takes the buffer
			 * into account, (it has nothing to do with the stream)
			 */
			if (offset + count > buffer.Length) {
				throw new ArgumentException();
			}

			// 2) buffer wasn't initialized
			/* NOTE: This was specified in docs but
			 * if buffer is a null parameter it will
			 * throw a NullReferenceException before entering
			 * this method. In the docs it says if the buffer
			 * is null then throw ArgumentNullException()
  			 */
			/*if (buffer == null) {
				throw new ArgumentNullException();
			}*/

			// 3)
			if (offset < 0 || count < 0) {
				throw new ArgumentOutOfRangeException();
			}

			/* 4)
			 * If our position is == streamLength
			 * that means our position is at the end 
			 * of stream and we have nothing more to read.
			 * 
			 * NOTE: Position will never be past streamLength
			 */
			if (Position == this.streamLength) {
				//0 elements will have been have been read.
				count = 0;
			}

			/* 5)
			 * we can't read past the stream's length
			 * also if count is greater than streamLength - Position
			 * (also seen as stream elements left to read)
			 * we won't have to worry about overflow and
			 * setting it to a negative integer.
			 */
			if (count > streamLength - Position) {
				count = (int) (streamLength - Position);
			}

			for (int i = 0; i < count; i++) {
				buffer [i + offset] = (byte)(Position++ % 256);
			}

			return count;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}

	}
}

