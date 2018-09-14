using System;
using System.Windows.Input;

namespace Player.Hook
{
	public class RawKeyEventArgs : EventArgs
	{
		public int VKCode;
		public Key Key;
		public bool IsSysKey;

		public RawKeyEventArgs(int vKCode, bool isSysKey)
		{
			VKCode = vKCode;
			IsSysKey = isSysKey;
			Key = KeyInterop.KeyFromVirtualKey(VKCode);
		}
	}
}