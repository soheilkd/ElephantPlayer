using System;
using System.Diagnostics;
using static Player.NativeMethods;

namespace Player.Hook
{
	internal static class InterceptKeys
	{
		public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		private static LowLevelKeyboardProc lowLevelDelegate; //This will prevent proc in SetHook getting collected by GC

		public const int WH_KEYBOARD_LL = 13;
		public const int WM_KEYDOWN = 0x0100;
		public const int WM_KEYUP = 0x0101;

		public static IntPtr SetHook(LowLevelKeyboardProc proc)
		{
			lowLevelDelegate = proc;
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule)
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
		}

	}
}