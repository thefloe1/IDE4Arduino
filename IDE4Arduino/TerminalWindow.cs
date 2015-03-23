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
            get { return checkBox1.Checked; }
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
            comboBox1.Items.AddRange(ports);

            comboBox2.SelectedItem = "9600";
            comboBox3.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;


        }

        private void TerminalWindow_Activated(object sender, EventArgs e)
        {
            //databox.Text += "Terminal activated"+Environment.NewLine;
            if (checkBox1.Checked)
            {
                if (!com.IsOpen)
                    connect();
            }
        }

        private void TerminalWindow_Enter(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (!com.IsOpen)
                    connect();
            }
        }

        public bool connect()
        {
            try
            {
                com.PortName = comboBox1.SelectedItem.ToString();
                com.BaudRate = int.Parse(comboBox2.SelectedItem.ToString());
                com.WriteTimeout = 500;
                com.ReceivedBytesThreshold = 1;
                com.Open();

                com.DiscardInBuffer();
                com.DiscardOutBuffer();

                com.DataReceived += com_DataReceived;

                databox.Text += "=========="+Environment.NewLine+com.PortName + " connected" + "==========" + Environment.NewLine;
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
                    data = "0x"+BitConverter.ToString(ASCIIEncoding.ASCII.GetBytes(data)).Replace("-"," 0x")+" ";

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
                databox.Text += "=========="+Environment.NewLine + com.PortName + " disconnected" + "==========" +Environment.NewLine;
            }
            catch { }
        }

        void com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
             addDataToBox(com.ReadExisting());            
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

            if (comboBox3.SelectedIndex == 1)
                add = "\r";
            else if (comboBox3.SelectedIndex == 2)
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

    }
}
