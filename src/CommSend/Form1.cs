using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;

namespace CommSend
{
    public partial class CommTool : Form
    {
        int tick = 0;
        private SerialDataReceivedEventHandler handler;
        public static bool isOpenWave = false;
        private readonly string[] Rate = {"600", "1200", "2400", "4800", "9600", "19200", "38400"};
        private readonly string[] Bits = { "8", "7", "6", "5", "4" };
        private readonly string[] Stop = { "One", "Two", "OnePointFive" };
        private readonly string[] _Parity = {"None", "Odd", "Even", "Mark", "Space" };
        private Form printWave;
        public CommTool()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Update_SerialPort_Name(ComboBox CBox)
        {
            string[] ArrayPort = SerialPort.GetPortNames();
            CBox.Items.Clear();
            for (int i = 0; i< ArrayPort.Length; i++)
            {
                CBox.Items.Add(ArrayPort[i]);
            }
            if (CBox.Items.Count > 0)
            {
                CBox.Text = ArrayPort[0];
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!isOpenWave)
            {
                if (!radioButton3.Checked)
                {
                    string str = serialPort1.ReadExisting();
                    textBox1.AppendText(str);
                
                }
                else
                {
                    byte data;
                    data = (byte)serialPort1.ReadByte();
                    string str = Convert.ToString(data, 16).ToUpper();
                    textBox1.AppendText("0x" + ((str.Length == 1) ? "0" + str : str));
                }
            }

        }

        private void CommTool_Load(object sender, EventArgs e)
        {
            comboBox1.Text = "COM1";
            Update_SerialPort_Name(comboBox1);
            comboBox2.Text = "9600";
            comboBox2.Items.AddRange(Rate);
            comboBox3.Text = "8";
            comboBox3.Items.AddRange(Bits);
            comboBox4.Text = "One";
            comboBox4.Items.AddRange(Stop);
            comboBox5.Text = "None";
            comboBox5.Items.AddRange(_Parity);
            radioButton1.Checked = true;
            radioButton3.Checked = true;
            handler = new SerialDataReceivedEventHandler(this.Port_DataReceived);
            serialPort1.DataReceived += handler;
        }

        private void initPort()
        {
            serialPort1.PortName = comboBox1.Text;
            serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text, 10);
            serialPort1.DataBits = Convert.ToInt32(comboBox3.Text, 10);
            serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), comboBox5.Text);
            serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), comboBox4.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            initPort();
            try
            {
                serialPort1.Open();
                button1.Enabled = false;
                button2.Enabled = true;
            }
            catch
            {
                MessageBox.Show("端口错误，请检查串口", "打开串口错误");
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                MessageBox.Show("串口未打开", "关闭错误");
                button1.Enabled = true;
                button2.Enabled = true;
            }
            else
            {
                try
                {
                    serialPort1.Close();
                    button1.Enabled = true;
                    button2.Enabled = false;
                }
                catch
                {
                    button1.Enabled = true;
                    button2.Enabled = true;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[1];
            if (serialPort1.IsOpen)
            {
                if (textBox2.Text != "")
                {
                    if (!radioButton1.Checked)
                    {
                        try
                        {
                            if (checkBox1.Checked)
                            {
                                serialPort1.WriteLine(textBox2.Text);
                            }
                            else
                            {
                                serialPort1.Write(textBox2.Text);
                            }
                            
                        }
                        catch(Exception err)
                        {
                            MessageBox.Show("串口数据写入错误\n" + err.Message, "发送错误");
                        }
                    }
                    else
                    {
                        try
                        {
                            int dataLength = textBox2.Text.Length;
                            int dataRemain = textBox2.Text.Length % 2;
                            for (int i = 0; i < (dataLength - dataRemain)/2; i++)
                            {
                                data[0] = Convert.ToByte(textBox2.Text.Substring(i * 2, 2), 16);
                                serialPort1.Write(data, 0, 1);
                            }
                            if (dataRemain != 0)
                            {
                                data[0] = Convert.ToByte(textBox2.Text.Substring(dataLength - 1, 1), 16);
                                serialPort1.Write(data, 0, 1);
                            }
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show("串口数据写入错误\n" + err.Message, "发送错误");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("串口未打开", "发送错误");
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.ScrollToCaret();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.ScrollToCaret();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!isOpenWave)
            {
                isOpenWave = true;
                printWave = new PrintWave(serialPort1);
                printWave.Show();
            }
            else
            {
                MessageBox.Show("你已经打开了一个示波界面!", "警告");
            }
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!isOpenWave)
            {
                MessageBox.Show("你并未打开任意示波界面!", "警告");
            }
            else
            {
                printWave.Hide();
                printWave.Close();
                //printWave.Dispose();
                isOpenWave = false;
                printWave = null;
            }
        }

        private void CommTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                DialogResult dr = MessageBox.Show("串口仍然开启中！\n是否继续操作", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    e.Cancel = true;
                }
                else
                {
                    serialPort1.DataReceived -= handler;
                    serialPort1.Close();
                    button1.Enabled = true;
                    button2.Enabled = true;
                    e.Cancel = false;
                }
                
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                int a = (int)Math.Round(50 + (float)50 * Math.Sin(Math.PI * 4 * tick / 50), 6, MidpointRounding.AwayFromZero);
                a += (new Random()).Next(-10, 10);
                serialPort1.Write(a.ToString().PadLeft(4, ';'));
                tick += 1;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
            }
            else
            {
                timer1.Start();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Update_SerialPort_Name(comboBox1);
        }
    }
}
