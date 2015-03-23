using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO.Ports;

namespace IDE4Arduino
{
    public partial class TerminalWindow : DockContent
    {
        SerialPort com;

        public string port 
        {
            get { return com.PortName; }
        }

        public bool isConnected
        {
            get { return com.IsOpen; }
        }

        public bool autoReconnect
        {
            get { return openPort.Checked; }
        }

        public TerminalWindow()
        {
            InitializeComponent();
            com = new SerialPort();
        }

        private void TerminalWindow_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            cbPorts.Items.AddRange(ports);

            cbBaud.SelectedItem = "9600";
            cbAddNewline.SelectedIndex = 0;
            cbPorts.SelectedIndex = 0;


        }

        private void TerminalWindow_Activated(object sender, EventArgs e)
        {
            //databox.Text += "Terminal activated"+Environment.NewLine;
            if (openPort.Checked)
            {
                if (!com.IsOpen)
                    connect();
            }
        }

        private void TerminalWindow_Enter(object sender, EventArgs e)
        {
            if (openPort.Checked)
            {
                if (!com.IsOpen)
                    connect();
            }
        }

        public bool connect()
        {
            try
            {
                com.PortName = cbPorts.SelectedItem.ToString();
                com.BaudRate = int.Parse(cbBaud.SelectedItem.ToString());
                com.WriteTimeout = 500;
                com.ReceivedBytesThreshold = 1;
                com.Open();

                com.DiscardInBuffer();
                com.DiscardOutBuffer();

                com.DataReceived += com_DataReceived;

                databox.Text += Environment.NewLine + "========== " + com.PortName + " connected" + " ==========" + Environment.NewLine;
                return true;

            }
            catch (Exception ex) {
                System.Console.WriteLine(ex.ToString());
            }
            return false;
        }


        void addDataToBox(string data)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (hEXToolStripMenuItem.Checked)
                {
                    data = "0x" + BitConverter.ToString(ASCIIEncoding.ASCII.GetBytes(data)).Replace("-", " 0x") + " ";

                }
                else
                {
                    data = data.Replace("\n", Environment.NewLine);
                    data = data.Replace("\r", Environment.NewLine);
                }

                databox.Text += data;
                databox.SelectionStart = databox.Text.Length;
                databox.ScrollToCaret();                

            });
        }

        public void disconnect()
        {
            try
            {
                com.Close();
                databox.Text += Environment.NewLine + "========== " + com.PortName + " disconnected" + " ==========" + Environment.NewLine;
            }
            catch { }
        }

        void com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string read = com.ReadExisting();
            addDataToBox(read);
            System.Console.WriteLine("rx: <" + read+">");
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            if (com.IsOpen)
            {
                disconnect();
            }
            else
            {
                connect();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string add = "";

            if (cbAddNewline.SelectedIndex == 1)
                add = "\r";
            else if (cbAddNewline.SelectedIndex == 2)
                add = "\n\r";

            
            if (com.IsOpen)
            {
                com.Write(sendData.Text+add);
            }
        }

        private void aSCIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aSCIIToolStripMenuItem.Checked = true;
            hEXToolStripMenuItem.Checked = false;           
        }

        private void hEXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aSCIIToolStripMenuItem.Checked = false;
            hEXToolStripMenuItem.Checked = true;            
        }

        private void TerminalWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (com.IsOpen)
                com.Close();
        }

    }
}
