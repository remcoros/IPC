using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using IPC.Internal;

namespace IPC
{
    public delegate int IpcMessage(IWin32Window receiver, string message);

    public class IPCChannel : NativeWindow, IDisposable
    {
        private const string DEFAULTIPCNAME = "IPC";
        private const int DEFAULTIPCID = 483972;
        private readonly IWin32Window _form;
        private bool _disposed;
        private readonly int _ipcId;

        public IPCChannel(IWin32Window host, string ipcName = DEFAULTIPCNAME, int ipcId = DEFAULTIPCID)
        {
            _form = host;
            _ipcId = ipcId;

            AssignHandle(host.Handle);

            var filterStatus = new User32.CHANGEFILTERSTRUCT();
            filterStatus.size = (uint)Marshal.SizeOf(filterStatus);
            filterStatus.info = 0;

            User32.ChangeWindowMessageFilterEx(host.Handle, User32.WM_COPYDATA, User32.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
            User32.SetProp(host.Handle, ipcName, new IntPtr(_ipcId));
        }

        public static readonly string DefaultIpcName = DEFAULTIPCNAME;
        public static readonly int DefaultIpcId = DEFAULTIPCID;

        public IpcMessage OnMessage { get; set; }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == User32.WM_COPYDATA)
            {
                var cds = (User32.COPYDATASTRUCT)m.GetLParam(typeof(User32.COPYDATASTRUCT));

                if (cds.dwData.ToInt32() != _ipcId)
                {
                    return;
                }

                var message = cds.lpData;

                var handler = OnMessage;
                if (handler != null)
                {
                    var result = handler.Invoke(_form, message);
                    m.Result = new IntPtr(result);
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                ReleaseHandle();
            }
        }

        public static Task SendMessage(string ipcName, string command)
        {
            return SendMessage(ipcName, DEFAULTIPCID, command);
        }

        public static async Task SendMessage(string ipcName, int ipcId, string command)
        {
            var wh = new WindowHandleInfo(IntPtr.Zero);
            var targetWindowHandles = wh.GetAllChildHandles()
                .Select(
                    childWindow =>
                    {
                        try
                        {
                            var prop = User32.GetProp(childWindow, ipcName).ToInt32();
                            if (prop == ipcId)
                                return childWindow;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        return IntPtr.Zero;
                    })
                .Where(x => x != IntPtr.Zero)
                .Distinct()
                .ToList();

            var cds = new User32.COPYDATASTRUCT
            {
                dwData = new IntPtr(ipcId),
                cbData = command.Length,
                lpData = command
            };

            if (targetWindowHandles.Count > 0)
            {
                await Task.WhenAll(
                    targetWindowHandles.Select(
                        target => Task.Run(
                            () =>
                            {
                                User32.SendMessage(target, User32.WM_COPYDATA, IntPtr.Zero, ref cds);

                                var lastError = Marshal.GetLastWin32Error();
                                if (lastError != 0)
                                {
                                    using (var eventLog = new EventLog("Application"))
                                    {
                                        // TODO: "Application" / "Application Error" can be used without admin privileges, if we want our own source, it needs to be registered under Admin rights.
                                        eventLog.Source = "Application Error";
                                        eventLog.WriteEntry("WIN32 error while sending message from Ridder.PhoneHelper.App.exe: " + lastError, EventLogEntryType.Error, lastError, 0);
                                    }
                                }
                            })));
            }
        }
    }
}
