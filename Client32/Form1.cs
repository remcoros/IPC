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

namespace Client32
{
    public partial class Form1 : Form
    {
        private IPCChannel _ipc;

        public Form1()
        {
            InitializeComponent();
            _ipc = new IPCChannel(this, "Client32");
            _ipc.OnMessage += OnMessage;
        }

        private int OnMessage(IWin32Window receiver, string message)
        {
            MessageBox.Show(message, "Client32");
            return 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPCChannel.SendMessage("Client64", "from 32");
        }
    }
}
