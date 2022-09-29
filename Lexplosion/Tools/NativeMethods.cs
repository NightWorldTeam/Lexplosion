using System;
using System.Runtime.InteropServices;

namespace Lexplosion.Tools
{
    public static class NativeMethods
    {
        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
            IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int showWindowCommand);

        public static void ShowProcessWindows(IntPtr hWnd)
        {
            ShowWindow(hWnd, 1);
            SetForegroundWindow(hWnd);
        }
    }
}
