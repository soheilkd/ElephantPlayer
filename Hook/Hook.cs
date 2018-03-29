// This code is distributed under MIT license. 
// Copyright (c) 2015 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using Player.Hook.Implementations;
using Player.Hook.WinAPI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Player.Hook
{
    public interface IKeyboardMouseEvents : IKeyboardEvents, IDisposable { }
    public class KeyEventArgsExt : KeyEventArgs
    {
        public KeyEventArgsExt(Key keyData)
             : base(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 1, keyData) { }

        internal KeyEventArgsExt(Key keyData, int timestamp, bool isKeyDown, bool isKeyUp)
            : this(keyData)
        {
            Timestamp = timestamp;
            IsKeyDown = isKeyDown;
            IsKeyUp = isKeyUp;
        }

        public int Timestamp { get; private set; }
        public bool IsKeyDown { get; private set; }
        public bool IsKeyUp { get; private set; }

        internal static KeyEventArgsExt FromRawDataApp(CallbackData data)
        {
            var wParam = data.WParam;
            var lParam = data.LParam;
            const uint maskKeydown = 0x40000000; 
            const uint maskKeyup = 0x80000000; 

            int timestamp = Environment.TickCount;

            var flags = (uint)lParam.ToInt64();
            bool wasKeyDown = (flags & maskKeydown) > 0;
            bool isKeyReleased = (flags & maskKeyup) > 0;

            Key keyData = AppendModifierStates((Key)wParam);

            bool isKeyDown = !isKeyReleased;
            bool isKeyUp = wasKeyDown && isKeyReleased;

            return new KeyEventArgsExt(keyData, timestamp, isKeyDown, isKeyUp);
        }

        internal static KeyEventArgsExt FromRawDataGlobal(CallbackData data)
        {
            var wParam = data.WParam;
            var lParam = data.LParam;
            var keyboardHookStruct =
                (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
            var keyData = AppendModifierStates((Key)keyboardHookStruct.VirtualKeyCode);

            var keyCode = (int)wParam;
            bool isKeyDown = (keyCode == Messages.WM_KEYDOWN || keyCode == Messages.WM_SYSKEYDOWN);
            bool isKeyUp = (keyCode == Messages.WM_KEYUP || keyCode == Messages.WM_SYSKEYUP);

            return new KeyEventArgsExt(keyData, keyboardHookStruct.Time, isKeyDown, isKeyUp);
        }

        private static bool CheckModifier(int vKey) => (KeyboardNativeMethods.GetKeyState(vKey) & 0x8000) > 0;
        private static Key AppendModifierStates(Key keyData)
            => keyData |
                   (CheckModifier(KeyboardNativeMethods.VK_CONTROL) ? Key.LeftCtrl : Key.None) |
                   (CheckModifier(KeyboardNativeMethods.VK_SHIFT) ? Key.LeftShift : Key.None) |
                   (CheckModifier(KeyboardNativeMethods.VK_MENU) ? Key.LeftAlt : Key.None);
    }
    public static class Hook
    {
        public static IKeyboardMouseEvents AppEvents() => new AppEventFacade();
        public static IKeyboardMouseEvents GlobalEvents() => new GlobalEventFacade();
    }
    public class KeyPressEventArgsExt : KeyEventArgs
    {
        internal KeyPressEventArgsExt(char keyChar, int timestamp)
            : base(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, timestamp, (Key)keyChar)
        {
            IsNonChar = keyChar == (char)0x0;
            Timestamp = timestamp;
        }

        public KeyPressEventArgsExt(char keyChar)
            : this(keyChar, Environment.TickCount) { }

        public bool IsNonChar { get; private set; }
        public new int Timestamp { get; private set; }

        internal static IEnumerable<KeyPressEventArgsExt> FromRawDataApp(CallbackData data)
        {
            var wParam = data.WParam;
            var lParam = data.LParam;
            const uint maskKeydown = 0x40000000; 
            const uint maskKeyup = 0x80000000; 
            const uint maskScanCode = 0xff0000; 

            var flags = (uint)lParam.ToInt64();
            var wasKeyDown = (flags & maskKeydown) > 0;
            var isKeyReleased = (flags & maskKeyup) > 0;

            if (!wasKeyDown && !isKeyReleased)
                yield break;

            var virtualKeyCode = (int)wParam;
            var scanCode = checked((int)(flags & maskScanCode));
            const int fuState = 0;

            char[] chars;

            KeyboardNativeMethods.TryGetCharFromKeyboardState(virtualKeyCode, scanCode, fuState, out chars);
            if (chars == null) yield break;
            foreach (var ch in chars)
                yield return new KeyPressEventArgsExt(ch);
        }

        internal static IEnumerable<KeyPressEventArgsExt> FromRawDataGlobal(CallbackData data)
        {
            var wParam = data.WParam;
            var lParam = data.LParam;

            if ((int)wParam != Messages.WM_KEYDOWN && (int)wParam != Messages.WM_SYSKEYDOWN)
                yield break;

            KeyboardHookStruct keyboardHookStruct =
                (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

            var virtualKeyCode = keyboardHookStruct.VirtualKeyCode;
            var scanCode = keyboardHookStruct.ScanCode;
            var fuState = keyboardHookStruct.Flags;

            if (virtualKeyCode == KeyboardNativeMethods.VK_PACKET)
            {
                var ch = (char)scanCode;
                yield return new KeyPressEventArgsExt(ch, keyboardHookStruct.Time);
            }
            else
            {
                char[] chars;
                KeyboardNativeMethods.TryGetCharFromKeyboardState(virtualKeyCode, scanCode, fuState, out chars);
                if (chars == null) yield break;
                foreach (var current in chars)
                    yield return new KeyPressEventArgsExt(current, keyboardHookStruct.Time);
            }
        }
    }
    public interface IKeyboardEvents
    {
        event KeyEventHandler KeyDown;
        event KeyEventHandler KeyPress;
        event KeyEventHandler KeyUp;
    }
}