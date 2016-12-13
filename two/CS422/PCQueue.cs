using System;

namespace CS422
{
	public class PCQueue
	{
		private Node front; 
		private Node back; 

		public PCQueue ()
		{
			front = new Node (0); //"null" node
			back = front;
		}

		//consumer (only ever able to consume), non-blocking method.
		public bool Dequeue(ref int out_value){
			bool success = false;
			
			//case if not empty
			if (!Object.ReferenceEquals(back, front)) {
				Node tempHead = front.Next;

				//NOTE: front.Next is equivalent to the actual front of your node.
				//one node case
				//this works for both one node case and > 1 node case.
				front = front.Next; //this makes us "empty" if 1 node case

				//after dequeuing, grab out_value
				out_value = tempHead.DataValue;

				success = true;
			}

			return success;
		}

		//producer (only ever able to produce)
		public void Enqueue(int dataValue){

			/* case if queue is empty
			 * can only go in here
			 */
			if (Object.ReferenceEquals(front, back)) {
				front.Next = new Node (dataValue);
				back = front.Next;
				//only at this point will Dequeue be able to go
				//into it's non-empty queue if statement
			} else {

				//back is only ever doing the producer work
				back.Next = new Node (dataValue);
				back = back.Next; 
			}
		}

		public void printQueue(){
			Node cur = front.Next;

			while (cur != null) {
				Console.WriteLine (cur.DataValue);
				cur = cur.Next;
			}
		}

		private class Node{
			public int DataValue{get; set;}
			public Node Next{ get; set;}

			public Node(int dataValue){
				DataValue = dataValue;
			}
		}
	}
}

