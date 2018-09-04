using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Player.Native
{
	internal static class Methods
	{
		public delegate IntPtr MessageHandler(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled);
		[DllImport("shell32.dll", EntryPoint = "CommandLineToArgvW", CharSet = CharSet.Unicode)]
		private static extern IntPtr _CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string cmdLine, out int numArgs);
		[DllImport("kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
		private static extern IntPtr _LocalFree(IntPtr hMem);
		internal enum ShellAddToRecentDocsFlags { Pidl = 0x001, Path = 0x002, }
		public static string[] CommandLineToArgvW(string cmdLine)
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
