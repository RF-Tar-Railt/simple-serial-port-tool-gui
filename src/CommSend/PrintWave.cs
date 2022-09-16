using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommSend
{
    public partial class PrintWave : Form
    {
        private SerialPort serialPort;
        private SerialDataReceivedEventHandler handler;

        int offsetV;
        int offsetH;
        float multipleX;
        float multipleY;
        
        int tick2;
        double thero;
        static int scopeDataCount = 512;
        private float drawStepX = 8f;
        private float drawStepY = 1f;

        private const int maxLengthX = 600;
        private const int maxLengthY = 360;

        Point p1StartFrame = new Point(180, 40 + maxLengthY);

        private float valueMaxY = 100;
        private float valueMinY = -100;
        private float positionORI = 0;
        private Brush background = Brushes.White;
        private readonly Pen tablePen = new Pen(Color.FromArgb(0x3c, 0x3c, 0x3c));
        readonly Pen dotPen = new Pen(Color.DarkSlateGray, 1);

        private readonly Pen linesPen = new Pen(Color.DarkCyan, 2f);
        public static volatile List<float> origin = new List<float>(scopeDataCount);
        public static volatile List<float> cache = new List<float>(scopeDataCount);


        bool enableCH1;
        bool update;

        private void Paint_MouseWheel(object sender, MouseEventArgs e)
        {
            if(e.Delta > 0)
            {
                //float index_old = ((e.X - p1StartFrame.X) / drawStepX);
                //(i * multipleX * drawStepX)
                if (multipleY >= 1f)
                {
                    multipleX += 0.1f;
                    //float delta = index_old * (multipleX - 1);
                    //System.Diagnostics.Debug.WriteLine(index_old.ToString() + ", " + (delta / multipleX).ToString());
                    //offsetH += (int)(delta/multipleX);
                }
                multipleY += 0.1f;
 
            }
            else
            {
                if (multipleX >= 1.1f)
                {
                    multipleX -= 0.1f;
                }
                if (multipleY >= 0.1f)
                {
                    multipleY -= 0.1f;
                }
                //System.Diagnostics.Debug.WriteLine(e.Location);
                //System.Diagnostics.Debug.WriteLine(e.X.ToString() + ", " + e.Y.ToString());
            }
        }

        private void ScopeInit()
        {
            //int[] arrayLine1 = new int[scopeDataCount];
            dotPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            dotPen.DashPattern = new float[] { 1, 3 };

            valueMaxY = 100;
            valueMinY = -100;
            positionORI = 0;

            drawStepY = 1f * (maxLengthY) / (valueMaxY - valueMinY);
            drawStepX = 1f * maxLengthX / (scopeDataCount - 1);

            positionORI = -valueMinY * drawStepY;
            //for (int i = 0; i < scopeDataCount; i++)
            //{
            //    cache.Add(0);
            //}
            timer1.Interval = 10; // 100
            timer1.Start();
            
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        public PrintWave(SerialPort port)
        {
            InitializeComponent();
            serialPort = port;
            offsetV = 0;
            offsetH = 0;
            tick2 = 0;
            thero = 0.1;
            multipleX = 1f;
            multipleY = 1f;
            enableCH1 = false;
            update = true;
            saveFileDialog1.Filter = @"jpeg|*.jpg|bmp|*.bmp|png|*.png";
            panel1.BackColor = ((SolidBrush)background).Color;
            panel2.BackColor = linesPen.Color;
            textBox1.Text = thero.ToString();
            radioButton1.Checked = true;
            radioButton2.Checked = false;
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (CommTool.isOpenWave)
                {
                    List<string> temp = new List<string>();

                    //string str = serialPort.ReadByte().Replace("\n", "").Replace("\r", "").Trim();

                    //string str = serialPort.ReadExisting();
                    //foreach (char ch in str)
                    //{
                    //    temp.Add(Convert.ToString((int)ch, 10));
                    //}
                    //System.Diagnostics.Debug.WriteLine(string.Join("|", temp1) + ";");


                    //List<string> data = new List<string>();
                    //System.Diagnostics.Debug.WriteLine(serialPort.BytesToRead);
                    //for(int i = 0; i < 60; i++)
                    //{
                    //    data.Add(Convert.ToString(serialPort.ReadChar(), 16).ToUpper());
                    //}
                    //System.Diagnostics.Debug.WriteLine(string.Join("|", data));
                    //System.Diagnostics.Debug.WriteLine(serialPort.BytesToRead);
                    //string str = "";
                    for (int i = 0; i < serialPort.BytesToRead; i++)
                    {
                        int data = serialPort.ReadByte();
                        System.Diagnostics.Debug.WriteLine(data);
                        //str = Convert.ToString(data >> 1, 16).ToUpper() + str;
                        string str = Convert.ToString(data, 16).ToUpper();
                        str = "0x" + ((str.Length == 1) ? "0" + str : str);
                        temp.Add(str);
                    }
                    //if (str != "")
                    //{
                    //    str = "0x" + ((str.Length == 1) ? "0" + str : str);
                    //    temp.Add(str);
                    //    System.Diagnostics.Debug.WriteLine(string.Join("/", temp));
                    //}
                    
                    
                    //System.Diagnostics.Debug.WriteLine(string.Join("|", temp));
                    //List<string> temp = new List<string>();
                    //for(int i = 0; i< str.Length - (str.Length % 4);i+=4)
                    //{
                    //    temp.Add(str.Substring(i, 4));
                    //}
                    //if (str.Length % 4 != 0)
                    //{
                    //    temp.Add(str.Substring(str.Length - -str.Length % 4, str.Length % 4).ToString().PadLeft(4, ';'));
                    //temp.Add(str[str.Length - str.Length % 4].ToString().PadLeft(4, '0'));
                    //}
                    int length = temp.Count;
                    if (update)
                    {
                        //if (cache.Count >= scopeDataCount)
                        //{
                        //    cache.RemoveRange(0, length);
                        //}
                        for (int i = 0; i < temp.Count; i++)
                        {
                            cache.Add(Convert.ToInt32(temp[i].Replace(";", ""), 16));
                        }
                        while (cache.Count > scopeDataCount)
                        {
                            cache.RemoveAt(0);
                        }
                        if (checkBox1.Checked && origin.Count >= scopeDataCount)
                        {
                            origin = radioButton1.Checked ? FFTrans.LowPassTrans(cache, thero) : FFTrans.HighPassTrans(cache, thero);
                        }
                        else
                        {
                            origin = cache;
                        }
                    }

                    //}
                    //));
                }
            }
            catch
            {
                if (serialPort.IsOpen)
                {
                    MessageBox.Show("接收串口数据发生意外", "警告");
                }
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                thero = Convert.ToDouble(textBox1.Text.Replace("%", ""));
                if (thero > 1 && textBox1.Text.Contains("%"))
                {
                    thero /= 100;
                }
            }
            catch
            { }
            Invalidate();
        }


        private void DrawLine(Point p, PaintEventArgs e, Pen pen)
        {
            if (origin.Count < 2)
            {
                return;
            }
            //(int)(origin.Count / multipleX) + offsetH <= origin.Count
            // 7 + 4 > 10 -> 6, 4
            // 4 -> 3
            List<float> show = new List<float>();
            if (offsetH > 0 && (int)(origin.Count / multipleX) + offsetH > origin.Count)
            {
                offsetH = origin.Count - (int)(origin.Count / multipleX);
            }
            show.AddRange(origin.GetRange(offsetH, (int)(origin.Count / multipleX)));
            
            for (int i = 0; i < show.Count - 1; i++)
            {
                float valY = (p.Y - (show[i] * drawStepY) * multipleY - positionORI)  + offsetV;
                float valY1 = (p.Y - (show[i + 1] * drawStepY)* multipleY - positionORI)  + offsetV;
                if (p.Y < valY)
                {
                    valY = (float)p.Y;
                }
                if (p.Y < valY1)
                {
                    valY1 = (float)p.Y;
                }
                if (valY < p.Y - maxLengthY)
                {
                    valY = (float)(p.Y - maxLengthY);
                }
                if (valY1 < p.Y - maxLengthY)
                {
                    valY1 = (float)(p.Y - maxLengthY);
                }
                e.Graphics.DrawLine(
                    pen,
                    p.X + (i * multipleX * drawStepX),
                    valY,
                    p.X + (i + 1) * multipleX * drawStepX,
                    valY1
                );
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            e.Graphics.FillRectangle(background, new Rectangle(p1StartFrame.X, p1StartFrame.Y - maxLengthY, maxLengthX, maxLengthY));

            float scaleLine1 = (float)maxLengthY / 40;
            int scaleLine2 = 5;
            float scaleLine3 = (float)maxLengthX / 100;
            float dotPenLineY = (float)maxLengthY / 8;
            float dotPenLineX = (float)maxLengthX / 10;

            e.Graphics.DrawLine(
                tablePen,
                (float)p1StartFrame.X,
                (float)p1StartFrame.Y,
                (float)(p1StartFrame.X + maxLengthX),
                (float)(p1StartFrame.Y)
            );
            e.Graphics.DrawLine(
                tablePen,
                (float)p1StartFrame.X,
                (float)(p1StartFrame.Y - maxLengthY),
                (float)(p1StartFrame.X + maxLengthX),
                (float)(p1StartFrame.Y - maxLengthY)
            );

            for (int i = 1; i < 40; i++)
            {
                e.Graphics.DrawLine(
                    tablePen,
                    (float)p1StartFrame.X,
                    (float)(p1StartFrame.Y - i * scaleLine1),
                    (float)(p1StartFrame.X + scaleLine2),
                    (float)(p1StartFrame.Y - i * scaleLine1)
                );
                e.Graphics.DrawLine(
                    tablePen,
                    (float)(p1StartFrame.X + maxLengthX),
                    (float)(p1StartFrame.Y - i * scaleLine1),
                    (float)(p1StartFrame.X - scaleLine2 + maxLengthX),
                    (float)(p1StartFrame.Y - i * scaleLine1)
                );
                e.Graphics.DrawLine(
                    tablePen,
                    (float)(p1StartFrame.X + maxLengthX / 2) - (float)(scaleLine2 / 2),
                    (float)(p1StartFrame.Y - i * scaleLine1),
                    (float)(p1StartFrame.X + maxLengthX / 2) + (float)(scaleLine2 / 2),
                    (float)(p1StartFrame.Y - i * scaleLine1)
                );
            }
            for (int i = 1; i <= 7; i++)
            {
                e.Graphics.DrawLine(
                    dotPen,
                    (float)p1StartFrame.X,
                    (float)(p1StartFrame.Y - dotPenLineY * i),
                    (float)(p1StartFrame.X + maxLengthX),
                    (float)(p1StartFrame.Y - dotPenLineY * i)
                );
            }

            e.Graphics.DrawLine(
                tablePen,
                (float)p1StartFrame.X,
                (float)p1StartFrame.Y,
                (float)p1StartFrame.X,
                (float)(p1StartFrame.Y - maxLengthY)
            );
            e.Graphics.DrawLine(
                tablePen,
                (float)(p1StartFrame.X + maxLengthX),
                (float)(p1StartFrame.Y - maxLengthY),
                (float)(p1StartFrame.X + maxLengthX),
                (float)(p1StartFrame.Y - maxLengthY)
            );

            for (int i = 1; i < 100; i++)
            {
                e.Graphics.DrawLine(
                    tablePen,
                    (float)(p1StartFrame.X + i * scaleLine3),
                    (float)(p1StartFrame.Y),
                    (float)(p1StartFrame.X + i * scaleLine3),
                    (float)(p1StartFrame.Y - scaleLine2)
                );
                e.Graphics.DrawLine(
                    tablePen,
                    (float)(p1StartFrame.X + i * scaleLine3),
                    (float)(p1StartFrame.Y - maxLengthY / 2) - (float)(scaleLine2 / 2),
                    (float)(p1StartFrame.X + i * scaleLine3),
                    (float)(p1StartFrame.Y - maxLengthY / 2) + (float)(scaleLine2 / 2)
                );
                e.Graphics.DrawLine(
                    tablePen,
                    (float)(p1StartFrame.X + i * scaleLine3),
                    (float)(p1StartFrame.Y - maxLengthY),
                    (float)(p1StartFrame.X + i * scaleLine3),
                    (float)(p1StartFrame.Y - maxLengthY + scaleLine2)
                );
            }
            for (int i = 1; i <= 10; i++)
            {
                e.Graphics.DrawLine(
                    dotPen,
                    (float)(p1StartFrame.X + i * dotPenLineX),
                    (float)(p1StartFrame.Y),
                    (float)(p1StartFrame.X + i * dotPenLineX),
                    (float)(p1StartFrame.Y - maxLengthY)
                );
            }

            gp.AddString(
                valueMinY.ToString(),
                this.Font.FontFamily,
                (int)FontStyle.Regular, 10,
                new RectangleF(p1StartFrame.X - 5, p1StartFrame.Y, 400, 50),
                null
            );
            
            e.Graphics.DrawPath(Pens.Blue, gp);

            if (enableCH1)
            {
                DrawLine(p1StartFrame, e, linesPen);
            }
        }

        private void PrintWave_Load(object sender, EventArgs e)
        {
            handler = new SerialDataReceivedEventHandler(this.DataReceived);
            serialPort.DataReceived += handler;
            if (SystemInformation.MouseWheelPresent && SystemInformation.NativeMouseWheelSupport)
            {
                this.MouseWheel += new MouseEventHandler(this.Paint_MouseWheel);
            }
            ScopeInit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (enableCH1 == false)
            {
                enableCH1 = true;
                button1.BackColor = SystemColors.ActiveCaption;
            }
            else
            {
                enableCH1 = false;
                button1.BackColor = SystemColors.Control;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (update)
            {
                //timer1.Stop();
                update = false;
                button2.Text = "继续";
            }
            else
            {
                //timer1.Start();
                update = true;
                button2.Text = "暂停";
            }
        }

        private void PrintWave_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            CommTool.isOpenWave = false;
            serialPort.DataReceived -= handler;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            cache.Clear();
            origin.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            offsetV = 0;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (tick2 > 1)
            {
                offsetV -= 5;
            }
            tick2 += 1;
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (tick2 > 1)
            {
                offsetV += 5;
            }
            tick2 += 1;
        }

        private void button4_MouseDown(object sender, MouseEventArgs e)
        {
            offsetV -= 5;
            tick2 = 0;
            timer2.Tick += this.timer2_Tick;
            timer2.Start();
        }

        private void button5_MouseDown(object sender, MouseEventArgs e)
        {
            offsetV += 5;
            tick2 = 0;
            timer2.Tick += timer3_Tick;
            timer2.Start();
        }

        private void button4_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Tick -= timer2_Tick;
            timer2.Stop();
        }

        private void button5_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Tick -= timer3_Tick;
            timer2.Stop();
        }

        private void button7_MouseDown(object sender, MouseEventArgs e)
        {
            multipleX += 0.1f;
            tick2 = 0;
            timer2.Tick += this.timer4_Tick;
            timer2.Start();
        }

        private void button8_MouseDown(object sender, MouseEventArgs e)
        {
            if (multipleX >= 1.1f)
            {
                multipleX -= 0.1f;
                tick2 = 0;
                timer2.Tick += this.timer5_Tick;
                timer2.Start();
            }
        }

        private void button9_MouseDown(object sender, MouseEventArgs e)
        {
            multipleY += 0.1f;
            tick2 = 0;
            timer2.Tick += this.timer6_Tick;
            timer2.Start();
        }

        private void button10_MouseDown(object sender, MouseEventArgs e)
        {
            if (multipleY >= 0.1f)
            {
                multipleY -= 0.1f;
                tick2 = 0;
                timer2.Tick += this.timer7_Tick;
                timer2.Start();
            } 
        }

        private void button12_Click(object sender, EventArgs e)
        {
            multipleX = 1f;
            offsetH = 0;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            multipleY = 1f;
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            if (tick2 > 1)
            {
                multipleX += 0.1f;
            }
            tick2 += 1;
            
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            if (multipleX >= 1.1f)
            {
                if (tick2 > 1)
                {
                    multipleX -= 0.1f;
                }
                tick2 += 1;
            }
        }

        private void timer6_Tick(object sender, EventArgs e)
        {
            if (tick2 > 1)
            {
                multipleY += 0.1f;
            }
            tick2 += 1;
        }

        private void timer7_Tick(object sender, EventArgs e)
        {
            if (multipleY >= 0.1f)
            {
                if (tick2 > 1)
                {
                    multipleY -= 0.1f;
                }
                tick2 += 1;
            }
            
        }

        private void button7_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Tick -= timer4_Tick;
            timer2.Stop();
        }

        private void button8_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Tick -= timer5_Tick;
            timer2.Stop();
        }

        private void button9_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Tick -= timer6_Tick;
            timer2.Stop();
        }

        private void button10_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Tick -= timer7_Tick;
            timer2.Stop();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Bitmap client = new Bitmap(Width, Height);
            this.DrawToBitmap(client, RestoreBounds);
            float mx = (float)(RestoreBounds.Width * 1.0 / ClientRectangle.Width);
            float my = (float)(RestoreBounds.Height * 1.0 / ClientRectangle.Height);
            Bitmap bitmap = new Bitmap((int)(maxLengthX * mx) + 20, (int)(maxLengthY * my) + p1StartFrame.Y - maxLengthY);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.DrawImage(
                client, 
                0, 0, 
                new Rectangle(
                    (int)(p1StartFrame.X * mx) - 10, 
                    (int)(40 * my), 
                    (int)((p1StartFrame.X + maxLengthX) *mx) + 10, 
                    (int)((40 + maxLengthY) * my) + p1StartFrame.Y - maxLengthY
                ), 
                GraphicsUnit.Pixel
            );
            bool isSave = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog1.FileName;
                if (fileName != null && fileName != "")
                {
                    string fileExt = fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower();
                    System.Drawing.Imaging.ImageFormat imageFormat = null;
                    if (fileExt != "")
                    {
                        switch(fileExt)
                        {
                            case "jpg":
                                imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                                break;
                            case "bmp":
                                imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                                break;
                            case "png":
                                imageFormat = System.Drawing.Imaging.ImageFormat.Png;
                                break;
                            default:
                                MessageBox.Show("只能存取为 jpg, bmp, png 格式", "格式错误");
                                isSave = false;
                                break;
                        }
                    }
                    if (imageFormat == null)
                    {
                        imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                    }
                    if (isSave)
                    {
                        try
                        {
                            bitmap.Save(fileName, imageFormat);
                        }
                        catch
                        {
                            MessageBox.Show("保存失败, 你还没有截取过图片或已经清空图片", "警告");
                        }
                    }
                }
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            DialogResult dr = colorDialog1.ShowDialog();
            if(dr == DialogResult.OK)
            {
                background = new SolidBrush(colorDialog1.Color);
                panel1.BackColor = colorDialog1.Color;
            }
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            DialogResult dr = colorDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                linesPen.Color = colorDialog1.Color;
                panel2.BackColor = colorDialog1.Color;
            }
        }

        private void button14_MouseDown(object sender, MouseEventArgs e)
        {
            if (offsetH >= 1)
            {
                offsetH -= 1;
                tick2 = 0;
                timer2.Tick += this.timer8_Tick;
                timer2.Start();
            }
            
        }

        private void button15_MouseDown(object sender, MouseEventArgs e)
        {
            if (offsetH < origin.Count * (1 -  1 / multipleX))
            {
                offsetH += 1;
                tick2 = 0;
                timer2.Tick += timer9_Tick;
                timer2.Start();
            }
            
        }

        private void button14_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Tick -= timer8_Tick;
            timer2.Stop();
        }

        private void button15_MouseUp(object sender, MouseEventArgs e)
        {
            timer2.Tick -= timer9_Tick;
            timer2.Stop();
        }

        private void timer8_Tick(object sender, EventArgs e)
        {
            if (offsetH >= 1)
            {
                if (tick2 > 1)
                {
                    offsetH -= 1;
                }
                tick2 += 1;
            }
                
        }

        private void timer9_Tick(object sender, EventArgs e)
        {
            if (offsetH < origin.Count * (1 - 1 / multipleX))
            {
                if (tick2 > 1)
                {
                    offsetH += 1;
                }
                tick2 += 1;
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            offsetH = 0;
        }
    }
}
