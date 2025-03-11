using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ClusterOS.InteropPro
{
    public static class HwndSubclass
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLongW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "CallWindowProc")]
        private static extern IntPtr CallWindowProcW(IntPtr lpPrevWndFunc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private static readonly int GWLP_WNDPROC = -4;
        private static readonly Dictionary<IntPtr, List<HwndSourceHook>> _hooks = new Dictionary<IntPtr, List<HwndSourceHook>>();
        private static readonly Dictionary<IntPtr, IntPtr> _origWndProcs = new Dictionary<IntPtr, IntPtr>();

        public delegate IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);

        public static HwndSource FromHwnd(IntPtr hwnd)
        {
            return new HwndSource(hwnd);
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (_hooks.TryGetValue(hwnd, out var hooks))
            {
                bool handled = false;
                foreach (var hook in hooks)
                {
                    hook(hwnd, msg, wParam, lParam, ref handled);
                    if (handled)
                        return IntPtr.Zero;
                }
            }

            return _origWndProcs.TryGetValue(hwnd, out var origWndProc)
                ? CallWindowProcW(origWndProc, hwnd, msg, wParam, lParam)
                : IntPtr.Zero;
        }

        private static readonly WndProcDelegate s_wndProcDelegate = WndProc;
        private static readonly IntPtr s_wndProcFunctionPtr = Marshal.GetFunctionPointerForDelegate(s_wndProcDelegate);

        private delegate IntPtr WndProcDelegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        public class HwndSource
        {
            private readonly IntPtr _hwnd;

            public HwndSource(IntPtr hwnd)
            {
                _hwnd = hwnd;
            }

            public void AddHook(HwndSourceHook hook)
            {
                if (!_hooks.TryGetValue(_hwnd, out var hooks))
                {
                    hooks = new List<HwndSourceHook>();
                    _hooks[_hwnd] = hooks;

                    IntPtr origWndProc;
                    if (IntPtr.Size == 8)
                        origWndProc = SetWindowLongPtrW(_hwnd, GWLP_WNDPROC, s_wndProcFunctionPtr);
                    else
                        origWndProc = SetWindowLongW(_hwnd, GWLP_WNDPROC, s_wndProcFunctionPtr);

                    _origWndProcs[_hwnd] = origWndProc;
                }

                if (!_hooks[_hwnd].Contains(hook))
                    _hooks[_hwnd].Add(hook);
            }
        }
    }
}
