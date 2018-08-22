using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Player.LibraryHook
{
	public class KeyboardListener : IDisposable
	{
		public static KeyboardListener Create() => new KeyboardListener();

		private static IntPtr hookId = IntPtr.Zero;
		
		public event EventHandler<RawKeyEventArgs> KeyDown;
		public event EventHandler<RawKeyEventArgs> KeyUp;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			try { return HookCallbackInner(nCode, wParam, lParam); }
			catch { return InterceptKeys.CallNextHookEx(hookId, nCode, wParam, lParam); }
		}

		private IntPtr HookCallbackInner(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				int vkCode = Marshal.ReadInt32(lParam);
				if (wParam == (IntPtr)InterceptKeys.WM_KEYDOWN) KeyDown?.Invoke(this, new RawKeyEventArgs(vkCode, false));
				else if (wParam == (IntPtr)InterceptKeys.WM_KEYUP) KeyUp?.Invoke(this, new RawKeyEventArgs(vkCode, false));
			}
			return InterceptKeys.CallNextHookEx(hookId, nCode, wParam, lParam);
		}

		KeyboardListener() => hookId = InterceptKeys.SetHook(HookCallback);
		~KeyboardListener() => Dispose();

		public void Dispose() => InterceptKeys.UnhookWindowsHookEx(hookId);
	}
}