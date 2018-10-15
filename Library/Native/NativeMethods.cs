using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Player.Native;

namespace Player
{
	internal static class NativeMethods
	{
		#region Keyboard Hook

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(int idHook, Hook.InterceptKeys.LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		#endregion

		public delegate IntPtr MessageHandler(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled);
		[DllImport("shell32.dll", EntryPoint = "CommandLineToArgvW", CharSet = CharSet.Unicode)]
		private static extern IntPtr _CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string cmdLine, out int numArgs);
		[DllImport("kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
		private static extern IntPtr _LocalFree(IntPtr hMem);
		internal enum ShellAddToRecentDocsFlags { Pidl = 0x001, Path = 0x002, }
		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeleteObject([In] IntPtr hObject);
		internal static string[] CommandLineToArgvW(string cmdLine)
		{
			IntPtr argv = IntPtr.Zero;
			try
			{
				argv = _CommandLineToArgvW(cmdLine, out int numArgs);
				if (argv == IntPtr.Zero)
					throw new Win32Exception();
				string[] result = new string[numArgs];
				for (int i = 0; i < numArgs; i++)
				{
					IntPtr currArg = Marshal.ReadIntPtr(argv, i * Marshal.SizeOf(typeof(IntPtr)));
					result[i] = Marshal.PtrToStringUni(currArg);
				}
				return result;
			}
			finally
			{
				IntPtr p = _LocalFree(argv);
			}
		}
	}
}
