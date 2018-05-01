using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IPC.Internal
{
    internal static class User32
    {
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;
        public const int WM_COPYDATA = 0x004a;

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.DLL")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetProp(IntPtr hWnd, string lpString, IntPtr hData);

        [DllImport("user32.dll")]
        public static extern IntPtr GetProp(IntPtr hWnd, string lpString);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint msg, ChangeWindowMessageFilterExAction action, ref CHANGEFILTERSTRUCT changeInfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;

            [MarshalAs(UnmanagedType.LPStr, SizeParamIndex = 1)]
            public string lpData;
        }

        public enum MessageFilterInfo : uint
        {
            None = 0, AlreadyAllowed = 1, AlreadyDisAllowed = 2, AllowedHigher = 3
        };

        public enum ChangeWindowMessageFilterExAction : uint
        {
            Reset = 0, Allow = 1, DisAllow = 2
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct CHANGEFILTERSTRUCT
        {
            public uint size;
            public MessageFilterInfo info;
        }

        public static void StealFocus(Form form)
        {
            if (form.WindowState == FormWindowState.Minimized)
            {
                form.WindowState = FormWindowState.Normal;
            }

            form.TopMost = true;
            form.TopMost = false;
            SetForegroundWindow(form.Handle);
            form.Focus();
        }
    }
}
