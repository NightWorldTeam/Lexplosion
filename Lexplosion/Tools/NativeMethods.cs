using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

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

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left; // X coordinate of topleft point
            public int Top; // Y coordinate of topleft point
            public int Right; // X coordinate of bottomright point
            public int Bottom; // Y coordinate of bottomright point
        }

        /// <summary>
        /// Возвращает координаты точек элемента
        /// </summary>
        /// <param name="uIElement"></param>
        /// <returns>Возвращает массив из 4 элементов [LEFT, TOP, RIGHT, BOTTOM]</returns>
        public static int[] GetControlCoordinate(UIElement uIElement)
        {
            IntPtr handle = (PresentationSource.FromVisual(uIElement) as HwndSource).Handle;
            RECT rect = new RECT();
            GetWindowRect(handle, out rect);
            return new int[] { rect.Left, rect.Top, rect.Right, rect.Bottom };
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        /// <summary>
        /// Возвращает количество ОЗУ на устройстве в мегабайтах.
        /// </summary>
        public static long GetRamCount()
        {
            try
            {
                long memKb;
                GetPhysicallyInstalledSystemMemory(out memKb);

                return memKb / 1024;
            }
            catch
            {
                return 1024;
            }
        }
    }
}
