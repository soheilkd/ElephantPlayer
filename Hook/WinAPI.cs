using Player.Hook.Implementations;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
namespace Player.Hook.WinAPI
{
    internal static class ThreadNativeMethods
    {
        [DllImport("kernel32")]
        internal static extern int GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y) => (X, Y) = (x, y);

        public static bool operator ==(Point a, Point b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Point a, Point b) => !(a == b);
        public bool Equals(Point other) => this == other;
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != typeof(Point)) return false;
            return Equals((Point)obj);
        }

        public override int GetHashCode() => unchecked(X * 397) ^ Y;
    }
    internal static class Messages
    {
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_SYSKEYDOWN = 0x104;
        public const int WM_SYSKEYUP = 0x105;
    }
    public delegate IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam);
    internal class HookProcedureHandle : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
    {
        private static bool _closing;

        static HookProcedureHandle() { }
        public HookProcedureHandle()
            : base(true) { }

        protected override bool ReleaseHandle()
        {
            //NOTE Calling Unhook during processexit causes deley
            if (_closing) return true;
            return HookNativeMethods.UnhookWindowsHookEx(handle) != 0;
        }
    }
    internal class HookResult : IDisposable
    {
        private readonly HookProcedureHandle m_Handle;
        private readonly HookProcedure m_Procedure;

        public HookResult(HookProcedureHandle handle, HookProcedure procedure)
        {
            m_Handle = handle;
            m_Procedure = procedure;
        }

        public HookProcedureHandle Handle => m_Handle;
        public HookProcedure Procedure => m_Procedure;
        public void Dispose() => m_Handle.Dispose();
    }
    internal static class HotkeysNativeMethods
    {
        [DllImport("user32.dll")]
        public static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hwnd, int id);
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct KeyboardHookStruct
    {
        public int VirtualKeyCode;
        public int ScanCode;
        public int Flags;
        public int Time;
        public int ExtraInfo;
    }
    internal static class KeyboardNativeMethods
    {
        //values from Winuser.h in Microsoft SDK.
        public const byte VK_SHIFT = 0x10;
        public const byte VK_CAPITAL = 0x14;
        public const byte VK_NUMLOCK = 0x90;
        public const byte VK_LSHIFT = 0xA0;
        public const byte VK_RSHIFT = 0xA1;
        public const byte VK_LCONTROL = 0xA2;
        public const byte VK_RCONTROL = 0xA3;
        public const byte VK_LMENU = 0xA4;
        public const byte VK_RMENU = 0xA5;
        public const byte VK_LWIN = 0x5B;
        public const byte VK_RWIN = 0x5C;
        public const byte VK_SCROLL = 0x91;
        public const byte VK_INSERT = 0x2D;
        //may be possible to use these aggregates instead of L and R separately (untested)
        public const byte VK_CONTROL = 0x11;
        public const byte VK_MENU = 0x12;
        public const byte VK_PACKET = 0xE7;
        //Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods
        private static int lastVirtualKeyCode = 0;
        private static int lastScanCode = 0;
        private static byte[] lastKeyState = new byte[255];
        private static bool lastIsDead = false;

        internal static void TryGetCharFromKeyboardState(int virtualKeyCode, int fuState, out char[] chars)
        {
            var dwhkl = GetActiveKeyboard();
            int scanCode = MapVirtualKeyEx(virtualKeyCode, (int)MapType.MAPVK_VK_TO_VSC, dwhkl);
            TryGetCharFromKeyboardState(virtualKeyCode, scanCode, fuState, dwhkl, out chars);
        }

        internal static void TryGetCharFromKeyboardState(int virtualKeyCode, int scanCode, int fuState, out char[] chars)
        {
            TryGetCharFromKeyboardState(virtualKeyCode, scanCode, fuState, GetActiveKeyboard(), out chars);
        }
        
        internal static void TryGetCharFromKeyboardState(int virtualKeyCode, int scanCode, int fuState, IntPtr dwhkl, out char[] chars)
        {
            StringBuilder pwszBuff = new StringBuilder(64);
            KeyboardState keyboardState = KeyboardState.GetCurrent();
            byte[] currentKeyboardState = keyboardState.GetNativeState();
            bool isDead = false;

            if (keyboardState.IsDown(Key.LeftShift))
                currentKeyboardState[(byte)Key.LeftShift] = 0x80;
            if (keyboardState.IsToggled(Key.CapsLock))
                currentKeyboardState[(byte)Key.CapsLock] = 0x01;

            var relevantChars = ToUnicodeEx(virtualKeyCode, scanCode, currentKeyboardState, pwszBuff, pwszBuff.Capacity, fuState, dwhkl);

            switch (relevantChars)
            {
                case -1:
                    isDead = true;
                    ClearKeyboardBuffer(virtualKeyCode, scanCode, dwhkl);
                    chars = null;
                    break;
                case 0:
                    chars = null;
                    break;
                case 1:
                    if (pwszBuff.Length > 0) chars = new[] { pwszBuff[0] };
                    else chars = null;
                    break;
                // Two or more (only two of them is relevant)
                default:
                    if (pwszBuff.Length > 1) chars = new[] { pwszBuff[0], pwszBuff[1] };
                    else chars = new[] { pwszBuff[0] };
                    break;
            }

            if (lastVirtualKeyCode != 0 && lastIsDead)
            {
                if (chars != null)
                {
                    StringBuilder sbTemp = new StringBuilder(5);
                    ToUnicodeEx(lastVirtualKeyCode, lastScanCode, lastKeyState, sbTemp, sbTemp.Capacity, 0, dwhkl);
                    lastIsDead = false;
                    lastVirtualKeyCode = 0;
                }
                return;
            }

            lastScanCode = scanCode;
            lastVirtualKeyCode = virtualKeyCode;
            lastIsDead = isDead;
            lastKeyState = (byte[])currentKeyboardState.Clone();
        }


        private static void ClearKeyboardBuffer(int vk, int sc, IntPtr hkl)
        {
            var sb = new StringBuilder(10);
            int rc;
            do
            {
                byte[] lpKeyStateNull = new Byte[255];
                rc = ToUnicodeEx(vk, sc, lpKeyStateNull, sb, sb.Capacity, 0, hkl);
            } while (rc < 0);
        }

        private static IntPtr GetActiveKeyboard()
        {
            IntPtr hActiveWnd = ThreadNativeMethods.GetForegroundWindow();
            int dwProcessId;
            int hCurrentWnd = ThreadNativeMethods.GetWindowThreadProcessId(hActiveWnd, out dwProcessId);
            //thread of focused window
            return GetKeyboardLayout(hCurrentWnd); 
        }

        [Obsolete("Use ToUnicodeEx instead")]
        [DllImport("user32.dll")]
        public static extern int ToAscii(
            int uVirtKey,
            int uScanCode,
            byte[] lpbKeyState,
            byte[] lpwTransKey,
            int fuState);

        [DllImport("user32.dll")]
        public static extern int ToUnicodeEx(int wVirtKey,
            int wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder pwszBuff,
            int cchBuff,
            int wFlags,
            IntPtr dwhkl);

        [DllImport("user32.dll")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetKeyState(int vKey);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int MapVirtualKeyEx(int uCode, int uMapType, IntPtr dwhkl);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetKeyboardLayout(int dwLayout);
        
        internal enum MapType
        {
            MAPVK_VK_TO_VSC,
            MAPVK_VSC_TO_VK,
            MAPVK_VK_TO_CHAR,
            MAPVK_VSC_TO_VK_EX
        }
    }
    internal static class HookNativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr CallNextHookEx(
            IntPtr idHook,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern HookProcedureHandle SetWindowsHookEx(
            int idHook,
            HookProcedure lpfn,
            IntPtr hMod,
            int dwThreadId);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern int UnhookWindowsHookEx(IntPtr idHook);
    }
    internal static class HookIds
    {
        internal const int WH_MOUSE = 7;
        internal const int WH_KEYBOARD = 2;
        internal const int WH_MOUSE_LL = 14;
        internal const int WH_KEYBOARD_LL = 13;
    }
    internal static class HookHelper
    {
        static HookProcedure _appHookProc;
        static HookProcedure _globalHookProc;

        public static HookResult HookAppMouse(Callback callback) => HookApp(HookIds.WH_MOUSE, callback);
        public static HookResult HookAppKeyboard(Callback callback) => HookApp(HookIds.WH_KEYBOARD, callback);
        public static HookResult HookGlobalMouse(Callback callback) => HookGlobal(HookIds.WH_MOUSE_LL, callback);
        public static HookResult HookGlobalKeyboard(Callback callback) => HookGlobal(HookIds.WH_KEYBOARD_LL, callback);
        private static HookResult HookApp(int hookId, Callback callback)
        {
            _appHookProc = (code, param, lParam) => HookProcedure(code, param, lParam, callback);

            var hookHandle = HookNativeMethods.SetWindowsHookEx(
                hookId,
                _appHookProc,
                IntPtr.Zero,
                ThreadNativeMethods.GetCurrentThreadId());

            if (hookHandle.IsInvalid)
                ThrowLastUnmanagedErrorAsException();

            return new HookResult(hookHandle, _appHookProc);
        }
        private static HookResult HookGlobal(int hookId, Callback callback)
        {
            _globalHookProc = (code, param, lParam) => HookProcedure(code, param, lParam, callback);

            var hookHandle = HookNativeMethods.SetWindowsHookEx(
                hookId,
                _globalHookProc,
                System.Diagnostics.Process.GetCurrentProcess().MainModule.BaseAddress,
                0);

            if (hookHandle.IsInvalid)
                ThrowLastUnmanagedErrorAsException();
            return new HookResult(hookHandle, _globalHookProc);
        }

        private static IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam, Callback callback)
        {
            var passThrough = nCode != 0;
            if (passThrough)
            {
                return CallNextHookEx(nCode, wParam, lParam);
            }

            var callbackData = new CallbackData(wParam, lParam);
            var continueProcessing = callback(callbackData);

            if (!continueProcessing)
                return new IntPtr(-1);
            return CallNextHookEx(nCode, wParam, lParam);
        }

        private static IntPtr CallNextHookEx(int nCode, IntPtr wParam, IntPtr lParam)
        {
            return HookNativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static void ThrowLastUnmanagedErrorAsException()
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(errorCode);
        }
    }
    internal struct CallbackData
    {
        private readonly IntPtr m_LParam;
        private readonly IntPtr m_WParam;

        public CallbackData(IntPtr wParam, IntPtr lParam)
        {
            m_WParam = wParam;
            m_LParam = lParam;
        }

        public IntPtr WParam
        {
            get { return m_WParam; }
        }

        public IntPtr LParam
        {
            get { return m_LParam; }
        }
    }
}
