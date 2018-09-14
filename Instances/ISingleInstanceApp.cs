using System.Collections.Generic;

namespace Player.Instances
{
	public interface ISingleInstanceApp
	{
		bool SignalExternalCommandLineArgs(IList<string> args);
	}
}