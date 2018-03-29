using Player.Hook.WinAPI;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Player.Hook.Implementations
{
    internal delegate HookResult Subscribe(Callback callbck);
    internal delegate bool Callback(CallbackData data);

    internal class GlobalKeyListener : KeyListener
    {
        public GlobalKeyListener()
            : base(HookHelper.HookGlobalKeyboard) { }
        protected override IEnumerable<KeyPressEventArgsExt> GetPressEventArgs(CallbackData data) => KeyPressEventArgsExt.FromRawDataGlobal(data);
        protected override KeyEventArgsExt GetDownUpEventArgs(CallbackData data) => KeyEventArgsExt.FromRawDataGlobal(data);
    }
    internal class GlobalEventFacade : EventFacade
    {
        protected override KeyListener CreateKeyListener() => new GlobalKeyListener();
    }
    internal abstract class KeyListener : BaseListener, IKeyboardEvents
    {
        protected KeyListener(Subscribe subscribe)
            : base(subscribe) { }

        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyPress;
        public event KeyEventHandler KeyUp;

        public void InvokeKeyDown(KeyEventArgsExt e)
        {
            var handler = KeyDown;
            if (handler == null || e.Handled || !e.IsKeyDown)
                return;
            handler(this, e);
        }
        public void InvokeKeyPress(KeyPressEventArgsExt e)
        {
            var handler = KeyPress;
            if (handler == null || e.Handled || e.IsNonChar)
                return;
            handler(this, e);
        }
        public void InvokeKeyUp(KeyEventArgsExt e)
        {
            var handler = KeyUp;
            if (handler == null || e.Handled || !e.IsKeyUp)
                return;
            handler(this, e);
        }

        protected override bool Callback(CallbackData data)
        {
            var eDownUp = GetDownUpEventArgs(data);
            var pressEventArgs = GetPressEventArgs(data);

            InvokeKeyDown(eDownUp);
            foreach (var pressEventArg in pressEventArgs)
                InvokeKeyPress(pressEventArg);
            InvokeKeyUp(eDownUp);
            return !eDownUp.Handled;
        }

        protected abstract IEnumerable<KeyPressEventArgsExt> GetPressEventArgs(CallbackData data);
        protected abstract KeyEventArgsExt GetDownUpEventArgs(CallbackData data);
    }
    internal class KeyboardState
    {
        private readonly byte[] m_KeyboardStateNative;
        private KeyboardState(byte[] keyboardStateNative) => m_KeyboardStateNative = keyboardStateNative;
        public static KeyboardState GetCurrent()
        {
            byte[] keyboardStateNative = new byte[256];
            KeyboardNativeMethods.GetKeyboardState(keyboardStateNative);
            return new KeyboardState(keyboardStateNative);
        }

        internal byte[] GetNativeState() => m_KeyboardStateNative;

        public bool IsDown(Key key) => GetHighBit(GetKeyState(key));
        public bool IsToggled(Key key) => GetLowBit(GetKeyState(key));

        public bool AreAllDown(IEnumerable<Key> keys)
        {
            foreach (Key key in keys)
                if (!IsDown(key))
                    return true;
            return false;
        }

        private byte GetKeyState(Key key)
        {
            int virtualKeyCode = (int)key;
            if (virtualKeyCode < 0 || virtualKeyCode > 255)
                throw new ArgumentOutOfRangeException("key", key, "The value must be between 0 and 255.");
            return m_KeyboardStateNative[virtualKeyCode];
        }

        private static bool GetHighBit(byte value) => (value >> 7) != 0;
        private static bool GetLowBit(byte value) => (value & 1) != 0;
    }
    internal abstract class EventFacade : IKeyboardMouseEvents
    {
        private KeyListener m_KeyListenerCache;

        public event KeyEventHandler KeyDown
        {
            add { GetKeyListener().KeyDown += value; }
            remove { GetKeyListener().KeyDown -= value; }
        }

        public event KeyEventHandler KeyPress
        {
            add { GetKeyListener().KeyPress += value; }
            remove { GetKeyListener().KeyPress -= value; }
        }

        public event KeyEventHandler KeyUp
        {
            add { GetKeyListener().KeyUp += value; }
            remove { GetKeyListener().KeyUp -= value; }
        }

        public void Dispose()
        {
            if (m_KeyListenerCache != null)
                m_KeyListenerCache.Dispose();
        }

        private KeyListener GetKeyListener()
        {
            var target = m_KeyListenerCache;
            if (target != null) return target;
            target = CreateKeyListener();
            m_KeyListenerCache = target;
            return target;
        }

        protected abstract KeyListener CreateKeyListener();
    }
    internal abstract class BaseListener : IDisposable
    {
        protected BaseListener(Subscribe subscribe) => Handle = subscribe(Callback);
        protected HookResult Handle { get; set; }
        public void Dispose() => Handle.Dispose();
        protected abstract bool Callback(CallbackData data);
    }
    internal class AppEventFacade : EventFacade
    {
        protected override KeyListener CreateKeyListener() => new AppKeyListener();
    }
    internal class AppKeyListener : KeyListener
    {
        public AppKeyListener()
            : base(HookHelper.HookAppKeyboard) { }
        protected override IEnumerable<KeyPressEventArgsExt> GetPressEventArgs(CallbackData data) => KeyPressEventArgsExt.FromRawDataApp(data);
        protected override KeyEventArgsExt GetDownUpEventArgs(CallbackData data) => KeyEventArgsExt.FromRawDataApp(data);
    }
}