using System;
using System.Collections.Generic;
using System.Linq;

namespace Player.Instances
{
	public class InstanceEventArgs : EventArgs
	{
		private InstanceEventArgs() { }
		public InstanceEventArgs(IList<string> args) { _Args = args; }
		private IList<string> _Args { get; set; }
		public string this[int index] => Args[index];
		public int ArgsCount => _Args.Count;
		public string[] Args => _Args.ToArray();
	}
}