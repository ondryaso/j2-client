using System.Runtime.InteropServices;

namespace SIClient
{
    public class NativeDefinitions
    {
        #region Windows structure definitions

        [StructLayout(LayoutKind.Sequential)]
        public struct ManagedRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ManagedPoint
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ManagedWindowPlacement
        {
            public int length;
            public int flags;
            public int showCmd;
            public ManagedPoint ptMinPosition;
            public ManagedPoint ptMaxPosition;
            public ManagedRect rcNormalPosition;
        }

        #endregion Windows structure definitions
    }
}