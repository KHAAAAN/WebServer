using System;
using CS422;

namespace HW1
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			string A = "Hello World!";
			string B = "Hello";
			B = B + " World!";
			string C = "Hello World!";

			Console.WriteLine ("A = " + A);
			Console.WriteLine ("B = " + B);
			Console.WriteLine ("C = " + C);

			Console.WriteLine(A == B);
			Console.WriteLine(A == C);
			Console.WriteLine(B == C);

			object oA = A; // Line 12
			object oB = B; // Line 13
			object oC = C; // Line 14

			Console.WriteLine(oA == oB);
			Console.WriteLine(oA == oC);
			Console.WriteLine(oB == oC);
		}
	}
}
