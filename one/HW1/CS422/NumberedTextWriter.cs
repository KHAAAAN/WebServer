using System;
using System.IO;

namespace CS422
{
	public class NumberedTextWriter : TextWriter
	{
		private int currentLineNumber;
		private TextWriter wrappedTextWriter;

		public NumberedTextWriter (TextWriter wrapThis){
			this.currentLineNumber = 1;
			this.wrappedTextWriter = wrapThis;
		}

		public NumberedTextWriter (TextWriter wrapThis, int startingLineNumber){
			this.wrappedTextWriter = wrapThis;
			this.currentLineNumber = startingLineNumber;
		}

		public override void WriteLine (string value)
		{
			string newValue = currentLineNumber.ToString () + ": " + value;

			wrappedTextWriter.WriteLine (newValue);
			currentLineNumber++;
		}

		public override System.Text.Encoding Encoding {
			get {
				return this.wrappedTextWriter.Encoding;
			}
		}
            
	}
}

