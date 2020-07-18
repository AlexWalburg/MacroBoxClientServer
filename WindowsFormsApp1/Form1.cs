using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form 
    {
        NotifyIcon notifyIcon;
        String[][] keys = new String[5][];
        bool[] doNext = new bool[5];
        int i; int j;
        public String port;
        public static String path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MacroBox\\MacroBoxConfig.txt";
        public Form1()
        {
            for(int i = 0; i < keys.Length; i++)
            {
                keys[i] = new String[2];
                doNext[i] = false;
            }
            InitializeComponent();
            Resize += Form1_Resize;
            notifyIcon = new NotifyIcon();
            notifyIcon.BalloonTipText = "Smug";
            notifyIcon.BalloonTipTitle = "Smug";
            notifyIcon.Text = "Smug";
            notifyIcon.Icon = Properties.Resources.Untitled;
            notifyIcon.MouseClick+= notifyIcon_MouseDoubleClick;
            backgroundWorker1.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;
            this.Icon = Properties.Resources.Untitled;

            if (System.IO.File.Exists(path))
            {
                LoadFromFile();
            } else
            {
                if (SerialPort.GetPortNames().Length > 0)
                {
                    port = SerialPort.GetPortNames()[0];
                }
                else
                {
                    port = "No ports found"; 
                }
            }
            RefreshPorts_Click(null, null);
            PortList.SelectedItem = port;

            backgroundWorker1.RunWorkerAsync();
        }

        private void LoadFromFile()
        {
            try
            {
                String[] lines = System.IO.File.ReadAllLines(path);
                port = lines[0];
                for (int i = 1; i < lines.Length; i++)
                {
                    keys[(i - 1) / keys[2].Length][(i - 1) % keys[0].Length] = lines[i];
                }
                textBox1.Text = lines[1];
                textBox2.Text = lines[2];
                textBox4.Text = lines[3];
                textBox3.Text = lines[4];
                textBox6.Text = lines[5];
                textBox5.Text = lines[6];
                textBox8.Text = lines[7];
                textBox7.Text = lines[8];
                textBox12.Text = lines[9];
                textBox11.Text = lines[10];
            } catch(Exception e)
            {
                //just pass here, we set  as many as we could
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            keys[0][0] = textBox1.Text;
            keys[0][1] = textBox2.Text;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            keys[1][0] = textBox4.Text;
            keys[1][1] = textBox3.Text;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            keys[2][0] = textBox6.Text;
            keys[2][1] = textBox5.Text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            keys[3][0] = textBox8.Text;
            keys[3][1] = textBox7.Text;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            keys[4][0] = textBox12.Text;
            keys[4][1] = textBox11.Text;
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon.Visible = true;
                this.ShowInTaskbar = false;
                notifyIcon.ShowBalloonTip(3000);
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon.Visible = false;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            SerialPort serialPort = new SerialPort(port, 9600);
            serialPort.RtsEnable = false;
            serialPort.Open();
            while (true)
            {
                if (worker.CancellationPending ==true)
                {
                    e.Cancel = true;
                    serialPort.Close();
                    e.Result = "Restart";
                    return;
                }
                else if (serialPort.BytesToRead > 0)
                {
                    int i = serialPort.ReadByte();
                    if (i > 4)
                    {
                        throw new InvalidEnumArgumentException("This is not the right port!");
                    }
                    else
                    {
                        SendKeys.SendWait(keys[i][doNext[i] ? 1 : 0]);
                        doNext[i] = !doNext[i];
                    }
                }
                System.Threading.Thread.Sleep(100);
            }
            
        }
        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error!=null)
            {
                MessageBox.Show(e.Error.Message + "\n\n" + "I reccomend finding the correct com port and then pressing the \"Reconnect\" button", "An error has occured with reading the Macro Box");
            }
            if (e.Cancelled)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void TitleStuff_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.TitleStuff.LinkVisited = true;
            System.Diagnostics.Process.Start("https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys.send?view=netcore-3.1");
        }

        private void RefreshPorts_Click(object sender, EventArgs e)
        {
            PortList.Items.Clear();
            foreach(String s in SerialPort.GetPortNames())
            {
                PortList.Items.Add(s);
            }
        }

        private void Reconnect_Click(object sender, EventArgs e)
        {
            port = PortList.SelectedItem.ToString();
            if (backgroundWorker1.IsBusy) backgroundWorker1.CancelAsync();
            else backgroundWorker1.RunWorkerAsync();
        }

        private void SaveConfig_Click(object sender, EventArgs e)
        {
            button1_Click(null, null);
            button2_Click_1(null, null);
            button3_Click(null, null);
            button4_Click(null, null);
            button6_Click(null, null);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
            {
                file.WriteLineAsync(port);
                foreach (String[] ss in keys)
                {
                    foreach (String s in ss)
                    {
                        file.WriteLineAsync(s);
                    }
                }
            }
        }
    }
}
