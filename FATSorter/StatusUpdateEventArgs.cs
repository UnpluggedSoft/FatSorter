using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnpluggedSoft.FatSorter
{
	public class StatusUpdateEventArgs : EventArgs
	{
		private readonly String statusText = String.Empty;
		private readonly bool doReset;

		public StatusUpdateEventArgs()
		{
			this.doReset = true;
		}

		public StatusUpdateEventArgs(String newText)
		{
			this.statusText = newText;
		}

		public String StatusText
		{
			get
			{
				return this.statusText;
			}
		}

		public bool DoReset
		{
			get
			{
				return this.doReset;
			}
		}
	}
}
