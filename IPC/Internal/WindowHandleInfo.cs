using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace IPC.Internal
{
    internal class WindowHandleInfo
    {
        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        private readonly IntPtr _mainHandle;

        public WindowHandleInfo(IntPtr handle)
        {
            this._mainHandle = handle;
        }

        public List<IntPtr> GetAllChildHandles()
        {
            var childHandles = new List<IntPtr>();

            var gcChildhandlesList = GCHandle.Alloc(childHandles);
            var pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                var childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(this._mainHandle, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            var gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList.Target == null)
            {
                return false;
            }

            if (gcChildhandlesList.Target is List<IntPtr> childHandles)
            {
                childHandles.Add(hWnd);
            }

            return true;
        }
    }
}
