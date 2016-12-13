// Create a PDF file with the answers to the 10 questions listed
// Put your name and ID number at the top, followed by the answers to the 10 questions, in order
// Each question is worth 2 points
// A single paragraph should suffice for each question
// Do all work independently

// To compile you will need to add a reference to Microsoft.CSharp.dll (NOT a 'using' 
// statement, but an actual project DLL reference). There may be others as well.

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace HW14Questions
{
    class MainClass
    {
        private static int s_sharedValue = 0;

        private static readonly Random sr_rand = new Random();

        public static void Main(string[] args)
        {
            // Right now all function calls are made here in main
            // You may need to comment-out some of the calls while testing particular implementations
            /*Q1();
            Q2();
            Q3();
            Q4();
            Q5();
            Q6();
            Q7();
            Q8();
            Q9();
            Q10();*/
            Q10();
        }

        // What's wrong with the insertion sort algorithm implementation in this function?
        // Describe the problem and list the solution
        private static void Q1()
        {
            // Generate an array of random values
            int[] nums = new int[50];
            for (int i = 0; i < nums.Length; i++)
            {
                nums[i] = sr_rand.Next(100);
            }

            // Sort using insertion sort
            for (int i = 1; i < nums.Length; i++)
            {
                int j = i;
                while (j > 0 && nums[j] < nums[j - 1])
                {
                    int temp = nums[j];
                    nums[j] = nums[j - 1];
                    nums[j - 1] = temp;
                    j -= 1;
                }
            }

            // Display the results
            Console.Write("Q1: ");
            foreach (int num in nums) { Console.Write(num + " "); }
            Console.WriteLine();
        }

        // The code below attempts to use reflection to invoke the member function named 
        // "Function" with an instance of the class Q2Class (declared below). Despite Q2Class 
        // having this member function, and it taking 0 parameters, which seems to match up 
        // with the mi.Invoke(...) call, this code produces a null reference exception. Explain 
        // why this exception occurs and what the simple fix is in order to allow the function 
        // to be invoked.
        class Q2Class { public int X = 0; void Function() { X++; } }
        private static void Q2()
        {
            Q2Class q1 = new Q2Class();
            MethodInfo mi = typeof(Q2Class).GetMethod("Function");
            mi.Invoke(q1, new object[]{});
            Console.WriteLine("Q2: X=" + q1.X);
        }

        // For each of the 4 declaration lines in the function below, describe the operation as 
        // either lossless or lossy, depending on whether or not some manner of data loss is 
        // occuring with the assignment. You do not have to comment about the Console.WriteLine 
        // call, but you will want to look at its output. 
        // Make sure you have 4 comments, in order, for the 4 assignment lines in this function.
        private static void Q3()
        {
            float f1 = 4.25f;
            double d1 = f1;
            float f2 = 1.4f;
            double d2 = f2;
            Console.WriteLine("Q3: {0}, {1}, {2}, {3}", f1, d1, f2, d2);
        }

        // You should be aware that the "object" class is a reference type. On the 2nd line of 
        // this function, we create a reference type from an integer value. Explain what happens 
        // on this line and why the displayed values for num and o are what they are.
        private static void Q4()
        {
            int num = 41;
            object o = num;
            num++;
            Console.WriteLine("Q4: num={0}, o={1}", num, o);
        }

        // (question to be answered is inside function)
        private static void Q5()
        {
            // Suppose this string was a function parameter (as opposed to something we're just 
            // declaring here locally) and was known to be the contents received from the 
            // first line sent by a client to an HTTP server.
            string firstLine = "!@#$%^&*()";

            // Suppose the remaining code is in the server logic and is attempting to recognize 
            // whether or not the first line of an HTTP request has a valid method. The intention 
            // of the code is to have the bool variable "isValidHTTPMethod" set to true if the 
            // request has a valid HTTP/1.1 method, and false if it does not. You may assume that 
            // the string 'firstLine' was the contents received from the socket up to the first 
            // line break. The bytes "\r\n" were receive by the socket reading code, but are not 
            // included in the firstLine string. 
            // Q: Explain what's wrong with the logic and why it doesn't fully achieve this goal.

            // Declare all HTTP 1.1 methods
            string[] validMethods = {
                "GET", "HEAD", "POST", "PUT", "DELETE", "CONNECT",
                "OPTIONS", "TRACE", "PATH"
            };
            bool isValidHTTPMethod = false;
            foreach (string valid in validMethods)
            {
                if (firstLine.StartsWith(valid))
                {
                    isValidHTTPMethod = true;
                    break;
                }
            }
        }

        // Assume that the local variables 'a' and 'b' could be reassigned to ANY integer in the Int32 
        // range. Ignore the hard-coded values that are used as a simple illustration and assume they 
        // could be anything in the range [int.MinValue,int.MaxValue]. 
        // Under this assumption, which of the 4 arithmetic operations are lossless? Explain why.
        private static void Q6()
        {
            int a = int.MaxValue, b = 2;
            int w = a + b;
            int x = a - b;
            int y = a * b;
            int z = a + b;
            Console.WriteLine("Q6: a={0}, b={1}, w={2}, x={3}, y={4}, z={5}", a, b, w, x, y, z);
        }

        // (question to be answered is inside function)
        private static void Q7()
        {
            string filePath = Path.Combine(Environment.CurrentDirectory, "Program.cs");

            // Below are 3 different methods for determining whether or not the file "Program.cs" 
            // exists. There is one method that is definitely better than the other two. Which 
            // method is the best of the 3 and why? Include details about what's wrong with the  
            // other 2 methods that makes them inferior options.
            bool method1 = File.Exists(filePath);
            bool method2 = (File.Open(filePath, FileMode.Open, FileAccess.Read) != null);
            bool method3 = (new FileStream(filePath, FileMode.Open, FileAccess.Read)) != null;

            Console.WriteLine("Q7: {0}, {1}, {2}", method1, method2, method3);
        }

        // This functions shows how to use a C# feature called "Anonymous Types" to create a 
        // singly-linked-list with 10 items and display the values. There was no need to 
        // declare a Node class to make this work. The nodes are intialized as anonymous 
        // types. 
        // As nice as this feature may seem, it is likely not what you would want to use in 
        // place of declaring a class for a node in a linked-list. There is a good reason for 
        // this, that you may be able to determine from examining the code. But it's more 
        // likely that you'll have to read online a bit about the feature and/or experiment 
        // with some alterations to the code below. 
        // Explain what's true of anonymous types that may make them a poor choice for nodes 
        // in a singly-linked list.
        private static void Q8()
        {
            // Generate a linked-list with 10 nodes
            dynamic node = new { Value = sr_rand.Next(100), Next = (object)null };
            for (int i = 0; i < 9; i++)
            {
                node = new { Value = sr_rand.Next(100), Next = (object)node };
            }

            // Display the values in the linked-list
            Console.Write("Q8: ");
            while (node.Next != null)
            {
                Console.Write(node.Value.ToString() + " -> ");
                node = node.Next;
            }
            Console.WriteLine(node.Value.ToString());
        }

        // This function creates 2 threads that increment a shared integer value 100,000 times each. The 
        // result would be 200,00 every time if the code were thread-safe. Explain how to make the code 
        // thread safe:
        // 1. Without increasing the number of code statments
        // 2. Without using the "lock" keyword (although you may use other threading-oriented 
        //    functionality in the .NET framework)
        private static void Q9()
        {
            s_sharedValue = 0;
            Thread[] threads = new Thread[2];
            for (int threadNum = 0; threadNum < threads.Length; threadNum++)
            {
                threads[threadNum] = new Thread(delegate()
                    {
                        for (int i = 0; i < 100000; i++) { Interlocked.Increment(ref s_sharedValue); }
                    });
                threads[threadNum].Start();
            } 

            // Wait for each thread to complete
            foreach (Thread t in threads) { t.Join(); }

            Console.WriteLine("Q9: {0}", s_sharedValue);
        }

        // Find a way to initialize stringValue to a non-null string that causes the "doc.Root.Add..." 
        // line to throw an exception. You must not alter any code other than the hard-coded value for 
        // stringValue, which again, cannot be null. Explain the string you chose and why it causes the 
        // XDocument code to throw an exception.
        private static void Q10()
        {
            string stringValue = "http://www.fafd.com"; 

            XDocument doc = new XDocument();
            doc.Add(new XElement("simple_root"));
            doc.Root.Add(new XElement("bad_value", stringValue));
            Console.WriteLine("Q10: " + doc.ToString());
        }
    }
}
