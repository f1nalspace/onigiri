using Finalspace.Onigiri.ViewModels;
using System.Runtime.InteropServices;
using System;
using System.Windows;
using System.Diagnostics;

namespace Finalspace.Onigiri.Helpers
{
    public static class WindowHelper
    {
        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void SetTheme(Window window, MainTheme theme)
        {
            Debug.Assert(theme != MainTheme.Automatic);
            bool isDarkMode = theme == MainTheme.Dark;
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            if(hwnd == IntPtr.Zero)
                return;
            int attributeValue = isDarkMode ? 1 : 0;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref attributeValue, sizeof(int));
        }
    }
}
