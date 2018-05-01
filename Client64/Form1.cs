using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IPC;

namespace Client64
{
    public partial class Form1 : Form
    {
        private IPCChannel _ipc;

        public Form1()
        {
            InitializeComponent();
            _ipc = new IPCChannel(this, "Client64");
            _ipc.OnMessage += OnMessage;
        }

        private int OnMessage(IWin32Window receiver, string message)
        {
            MessageBox.Show(message, "Client64");
            return 1;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await IPCChannel.SendMessage("Client32", "from 64");
        }
    }
}
