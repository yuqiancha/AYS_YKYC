using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CalcuError
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Byte[] ReadBuf;
        long ReadNums=0;
        int FrameNums=0;

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                openFileDialog1.InitialDirectory = Program.GetStartupPath();
                try
                {
                    textBox_Value.Text = openFileDialog1.FileName;   
                    ReadBuf = System.IO.File.ReadAllBytes(openFileDialog1.FileName);
                    ReadNums = ReadBuf.Length;
                    textBox1.Text = ReadNums.ToString();
                    FrameNums = (int)(ReadNums / 220);
                    textBox2.Text = FrameNums.ToString();
                }
                catch
                {
                    // MyLog.Error("加载发送码本失败！");             
                    //MessageBox.Show("运行日志打开失败！");
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string temp = textBox_Key.Text.Replace(" ", "");

            if(ReadNums<220)
            {
                MessageBox.Show("选择文件内容长度小于1帧，请重新选择！");
                return;
            }
            else if(temp.Length != 440)
            {
                MessageBox.Show("输入比对内容长度小于220Byte，请重新选择！");
                return;
            }
            else
            {
                byte[] KeyBuf = StrToHexByte(temp);

                int ErrorBytes = 0;
                int ErrorFrames = 0;
                int pos = 0;

                for(int i=0;i<FrameNums;i++)
                {
                    byte[] tempBuf = new byte[220];
                    Array.Copy(ReadBuf, pos, tempBuf, 0, 220);
                    pos = pos + 220;
                    bool FrameRight = true;
                    for(int j=0;j<220;j++)
                    {
                        if (tempBuf[j] != KeyBuf[j])
                        {
                            ErrorBytes += 1;
                            FrameRight = false;
                        }
                    }
                    if (!FrameRight) ErrorFrames += 1;
                    
                }

                textBox3.Text = ErrorBytes.ToString();
                textBox4.Text = ErrorFrames.ToString();

                textBox_Error.Text = (((double)ErrorBytes / (double)ReadNums)*100).ToString("#0.0000")+"%";
            }
        }

        private static byte[] StrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "").Replace("\r", "").Replace("\n", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";

            byte[] returnBytes = new byte[hexString.Length / 2];

            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;

        }

    }
}
