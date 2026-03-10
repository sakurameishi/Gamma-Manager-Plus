using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Gamma_Manager.Services
{
    public class HotkeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr _hWnd;
        private int _currentId;
        private Dictionary<int, Action> _callbacks;

        public HotkeyManager(IntPtr hWnd)
        {
            _hWnd = hWnd;
            _currentId = 0;
            _callbacks = new Dictionary<int, Action>();
        }

        public int Register(Keys key, Action callback)
        {
            _currentId++;
            uint modifiers = 0;
            if ((key & Keys.Alt) == Keys.Alt) modifiers |= 0x0001;
            if ((key & Keys.Control) == Keys.Control) modifiers |= 0x0002;
            if ((key & Keys.Shift) == Keys.Shift) modifiers |= 0x0004;
            if ((key & Keys.LWin) == Keys.LWin || (key & Keys.RWin) == Keys.RWin) modifiers |= 0x0008;

            uint vk = (uint)(key & Keys.KeyCode);

            if (RegisterHotKey(_hWnd, _currentId, modifiers, vk))
            {
                _callbacks.Add(_currentId, callback);
                return _currentId;
            }
            return -1;
        }

        public void Unregister(int id)
        {
            if (_callbacks.ContainsKey(id))
            {
                UnregisterHotKey(_hWnd, id);
                _callbacks.Remove(id);
            }
        }

        public void UnregisterAll()
        {
            foreach (var id in _callbacks.Keys)
            {
                UnregisterHotKey(_hWnd, id);
            }
            _callbacks.Clear();
        }

        public void ProcessMessage(Message m)
        {
            if (m.Msg == 0x0312) // WM_HOTKEY
            {
                int id = m.WParam.ToInt32();
                if (_callbacks.ContainsKey(id))
                {
                    _callbacks[id]?.Invoke();
                }
            }
        }
    }
}
