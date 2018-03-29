using Player.Hook.Implementations;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Player.Hook.HotKeys
{
    public class HotKeySet
    {
        public delegate void HotKeyHandler(object sender, HotKeyArgs e);

        private readonly Dictionary<Key, bool> m_hotkeystate; 
        private readonly Dictionary<Key, Key> m_remapping; 
        private bool m_enabled = true; 
        private int m_hotkeydowncount; 
        private int m_remappingCount;
        public HotKeySet(IEnumerable<Key> hotkeys)
        {
            m_hotkeystate = new Dictionary<Key, bool>();
            m_remapping = new Dictionary<Key, Key>();
            HotKeys = hotkeys;
            InitializeKeys();
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<Key> HotKeys { get; }
        public bool HotKeysActivated => m_hotkeydowncount == (m_hotkeystate.Count - m_remappingCount); 
        public bool Enabled
        {
            get  => m_enabled; 
            set
            {
                if (value)
                    InitializeKeys(); 
                m_enabled = value;
            }
        }

        public event HotKeyHandler OnHotKeysDownHold;
        public event HotKeyHandler OnHotKeysUp;
        public event HotKeyHandler OnHotKeysDownOnce;

        private void InvokeHotKeyHandler(HotKeyHandler hotKeyDelegate) => hotKeyDelegate?.Invoke(this, new HotKeyArgs(DateTime.Now));

        private void InitializeKeys()
        {
            foreach (Key k in HotKeys)
            {
                if (m_hotkeystate.ContainsKey(k))
                    m_hotkeystate.Add(k, false);
                m_hotkeystate[k] = KeyboardState.GetCurrent().IsDown(k);
            }
        }

        public bool UnregisterExclusiveOrKey(Key anyKeyInTheExclusiveOrSet)
        {
            Key primaryKey = GetExclusiveOrPrimaryKey(anyKeyInTheExclusiveOrSet);

            if (primaryKey == Key.None || !m_remapping.ContainsValue(primaryKey))
                return false;

            List<Key> keystoremove = new List<Key>();

            foreach (KeyValuePair<Key, Key> pair in m_remapping)
                if (pair.Value == primaryKey)
                    keystoremove.Add(pair.Key);
            foreach (Key k in keystoremove)
                m_remapping.Remove(k);

            --m_remappingCount;
        
            return true;
        }
        
        public Key RegisterExclusiveOrKey(IEnumerable<Key> orKeySet)
        {
            foreach (Key k in orKeySet)
                if (!m_hotkeystate.ContainsKey(k))
                    return Key.None;
            int i = 0;
            Key primaryKey = Key.None;
            foreach (Key k in orKeySet)
            {
                if (i++ == 0)
                    primaryKey = k;
                m_remapping[k] = primaryKey;
            }
            
            ++m_remappingCount;
            return primaryKey;
        }
        
        private Key GetExclusiveOrPrimaryKey(Key k) => (m_remapping.ContainsKey(k) ? m_remapping[k] : Key.None);
        private Key GetPrimaryKey(Key k) => (m_remapping.ContainsKey(k) ? m_remapping[k] : k);

        internal void OnKey(KeyEventArgsExt kex)
        {
            if (!Enabled)
                return;
            Key primaryKey = GetPrimaryKey(kex.Key);

            if (kex.IsKeyDown)
                OnKeyDown(primaryKey);
            else //reset
                OnKeyUp(primaryKey);
        }

        private void OnKeyDown(Key k)
        {
            if (HotKeysActivated)
                InvokeHotKeyHandler(OnHotKeysDownHold); 
            else if (m_hotkeystate.ContainsKey(k) && !m_hotkeystate[k])
            {
                m_hotkeystate[k] = true; 
                ++m_hotkeydowncount; 
                if (HotKeysActivated) 
                    InvokeHotKeyHandler(OnHotKeysDownOnce); 
            }
        }

        private void OnKeyUp(Key k)
        {
            if (m_hotkeystate.ContainsKey(k) && m_hotkeystate[k]) //indicates the key's state was down but now it's up
            {
                bool wasActive = HotKeysActivated;
                m_hotkeystate[k] = false; 
                --m_hotkeydowncount; 
                if (wasActive)
                    InvokeHotKeyHandler(OnHotKeysUp); //call the KeyUp event because the set is no longer active
            }
        }
    }
    public sealed class HotKeySetCollection : List<HotKeySet>
    {
        private KeyChainHandler m_keyChain;

        public new void Add(HotKeySet hks)
        {
            m_keyChain += hks.OnKey;
            base.Add(hks);
        }
        public new void Remove(HotKeySet hks)
        {
            m_keyChain -= hks.OnKey;
            base.Remove(hks);
        }
        internal void OnKey(KeyEventArgsExt e)
        {
            if (m_keyChain != null)
                m_keyChain(e);
        }

        private delegate void KeyChainHandler(KeyEventArgsExt kex);
    }
    public sealed class HotKeyArgs : EventArgs
    {
        public DateTime Time { get; }
        public HotKeyArgs(DateTime triggeredAt) => Time = triggeredAt;
    }
}
