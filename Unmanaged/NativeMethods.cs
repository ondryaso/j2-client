using System;
using System.Runtime.InteropServices;
using static SIClient.NativeDefinitions;

namespace SIClient
{
    public class NativeMethods
    {
        [DllImport("gdi32.dll")]
        internal static extern uint DeleteObject(IntPtr hDc);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out ManagedRect lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, out ManagedWindowPlacement lpwndpl);
    }
}