using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FatSorter
{
	public class ProgressUpdateEventArgs : EventArgs
	{
		private readonly int addTotal;
		private readonly bool isFinished;
		private readonly bool incrementCounter;

		public ProgressUpdateEventArgs()
		{
			this.incrementCounter = true;
		}

		public ProgressUpdateEventArgs(bool finished)
		{
			if (finished)
			{
				this.isFinished = true;
			}
			else
			{
				this.incrementCounter = true;
			}
		}

		public ProgressUpdateEventArgs(int newItems)
		{
			this.addTotal = newItems;
		}

		public int AddTotal
		{
			get
			{
				return this.addTotal;
			}
		}

		public bool IncrementCounter
		{
			get
			{
				return this.incrementCounter;
			}
		}

		public bool IsFinished
		{
			get
			{
				return this.isFinished;
			}
		}
	}
}
