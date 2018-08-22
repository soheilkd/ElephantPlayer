using System;

namespace Player.LibraryHook
{
	public static class Events
	{
		private static KeyboardListener Listener = KeyboardListener.Create();

		public static event EventHandler<RawKeyEventArgs> KeyDown
		{
			add => Listener.KeyDown += value;
			remove => Listener.KeyDown -= value;
		}
		public static event EventHandler<RawKeyEventArgs> KeyUp
		{
			add => Listener.KeyUp += value;
			remove => Listener.KeyUp -= value;
		}

		public static void Dispose() => Listener.Dispose();
	}
}
