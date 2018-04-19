﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace H07_YKYC
{
    public partial class TelecmdProduce : Form
    {
        public H07_YKYC.MainForm mform;
        String ZhenStr;
        public TelecmdProduce(H07_YKYC.MainForm parent)
        {
            InitializeComponent();
            mform = parent;

            dataGridView1.Rows.Add(1);
            dataGridView1.AllowUserToAddRows = false;

            dataGridView2.Rows.Add(1);
            dataGridView2.AllowUserToAddRows = false;

            dataGridView1.Rows[0].Cells[0].Value = "00";
            dataGridView1.Rows[0].Cells[1].Value = "0";
            dataGridView1.Rows[0].Cells[2].Value = "0";
            dataGridView1.Rows[0].Cells[3].Value = "00";
            dataGridView1.Rows[0].Cells[4].Value = "44";
            dataGridView1.Rows[0].Cells[5].Value = "01";
            dataGridView1.Rows[0].Cells[6].Value = "1E";
            dataGridView1.Rows[0].Cells[7].Value = "00";
            dataGridView1.Rows[0].Cells[8].Value = "";

            dataGridView1.Rows[0].Cells[9].Value = "0000";
            this.Column9.ReadOnly = true;

            dataGridView2.Rows[0].Cells[0].Value = "00";
            dataGridView2.Rows[0].Cells[1].Value = "00";
            dataGridView2.Rows[0].Cells[2].Value = "000";



        }

        private void TelecmdProduce_Load(object sender, EventArgs e)
        {

        }

        private void button3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void TelecmdProduce_Paint(object sender, PaintEventArgs e)
        {
            Pen mypen = new Pen(Color.Black);
            mypen.Width = 1;
            mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

            e.Graphics.DrawLine(mypen, label5.Location.X, label5.Location.Y, dataGridView2.Location.X, dataGridView2.Location.Y);
            e.Graphics.DrawLine(mypen, label5.Location.X + label5.Width, label5.Location.Y, dataGridView2.Location.X + dataGridView2.Width, dataGridView2.Location.Y);

            e.Graphics.DrawLine(mypen, label6.Location.X, label6.Location.Y, textBox9.Location.X, textBox9.Location.Y);
            e.Graphics.DrawLine(mypen, label6.Location.X + label6.Width, label6.Location.Y, textBox9.Location.X + textBox9.Width, textBox9.Location.Y);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            String DataStr = this.textBox9.Text.Replace(" ", "").Replace("\r", "").Replace("\n", "");

            int tempLen = DataStr.Length;
            if (tempLen != 44)
            {
                DialogResult dr = MessageBox.Show("请输入22个Byte的有效数据", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.button1.Enabled = false;
            }
            else
            {
                //GuoChengTag = 11bits应用过程标识符
                int temp1 = Convert.ToInt32(dataGridView2.Rows[0].Cells[0].FormattedValue.ToString(), 16);
                int temp1_2 = temp1 & 0x7ff;
                String GuoChengTag = Convert.ToString(temp1_2, 2).PadLeft(11, '0');
                GuoChengTag = GuoChengTag.Substring(GuoChengTag.Length - 11);//防止超出11bit，取最低11bit

                String StrBin = GuoChengTag +                                                     //过程标志符
                    dataGridView2.Rows[0].Cells[1].FormattedValue.ToString().Substring(0, 2) +      //序列标志
                     dataGridView2.Rows[0].Cells[2].FormattedValue.ToString().Substring(0, 3);      //序列计数

                String temp3 = Convert.ToInt32(StrBin, 2).ToString("x4");
                //完整遥控包
                String BagStr = temp3+ DataStr;


                //HTTag = 10bit航天器标志
                int tempTag1 = Convert.ToInt32(dataGridView1.Rows[0].Cells[4].FormattedValue.ToString(), 16);
                int tempTag1_2 = tempTag1 & 0x3ff;
                String HTTag = (Convert.ToString(tempTag1_2, 2).PadLeft(10, '0'));

                //XNTag = 6bit虚拟信道标志
                int tempTag2 = Convert.ToInt32(dataGridView1.Rows[0].Cells[5].FormattedValue.ToString(), 16);
                int tempTag2_2 = tempTag2 & 0x3f;
                String XNTag = Convert.ToString(tempTag2_2, 2).PadLeft(6, '0');

                //ZhenChangTag
                int tempTagZC = Convert.ToInt32(dataGridView1.Rows[0].Cells[6].FormattedValue.ToString(),16);
                String ZhenChangTag = Convert.ToString(tempTagZC, 2).PadLeft(10, '0');

                //XuLieTag = 8bits帧序列序号
                int tempTag3 = int.Parse(dataGridView1.Rows[0].Cells[7].FormattedValue.ToString());
                //String XuLieTag = Convert.ToString(tempTag3, 2).PadLeft(8, '0');
                String XuLieTag = tempTag3.ToString("x2");      //1Byte的序列号

                /*
                int temptemp = 0;
                bool tag = int.TryParse(dataGridView1.Rows[0].Cells[7].FormattedValue.ToString(), out temptemp);
                if (!tag)
                {
                    dataGridView1.Rows[0].Cells[7].Value = "请重新输入";
                }
                String XuLieTag = Convert.ToString(temptemp, 2).PadLeft(8, '0');
                */


                String StrYKBin = dataGridView1.Rows[0].Cells[0].FormattedValue.ToString()          //版本号
                            + dataGridView1.Rows[0].Cells[1].FormattedValue.ToString()              //通过标志
                            + dataGridView1.Rows[0].Cells[2].FormattedValue.ToString()              //控制命令字标志
                             + dataGridView1.Rows[0].Cells[3].FormattedValue.ToString()             //空闲位
                             + HTTag                                                                //航天器标志
                             + XNTag                                                                //虚拟信道标志
                            + ZhenChangTag;                                                   //帧长
                                                                                              //主导头
                String tempDT = string.Format("{0:X}", Convert.ToInt32(StrYKBin, 2)).PadLeft(8, '0') + XuLieTag;

                //帧头+遥控包+差错控制码
                ZhenStr = tempDT + BagStr;// + dataGridView1.Rows[0].Cells[9].FormattedValue.ToString();


                ushort CRC = 0xffff;
                ushort genpoly = 0x1021;


                for (int i = 0; i < ZhenStr.Length / 2; i++)
                {
                    byte temp = Convert.ToByte(ZhenStr.Substring(i * 2, 2), 16);
                    // byte temp = (byte)(int.Parse(ZhenStr.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber));
                    CRC = Function.CRChware(temp, genpoly, CRC);
                }

                this.dataGridView1.Rows[0].Cells[9].Value = CRC.ToString("x4");
                ZhenStr += CRC.ToString("x4");

                button1.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mform.textBox1.Text = "08547C00"+ZhenStr;
            mform.label_ykname.Text = "自定义";
            button1.Enabled = false;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String Path = mform.Path + @"遥控包\";
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            saveFileDialog1.InitialDirectory = Path;

            saveFileDialog1.Filter = "文本文件(*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string localFilePath = saveFileDialog1.FileName.ToString(); //获得文件路径 

                FileStream file0 = new FileStream(localFilePath, FileMode.Create);
                StreamWriter sw = new StreamWriter(file0);
                sw.WriteLine(ZhenStr);
                sw.Flush();
                sw.Close();
                file0.Close();
                MessageBox.Show("存储文件成功！", "保存文件");

            }
        }

        int DataCount;

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            String DataStr = this.textBox9.Text.Replace(" ", "");

            if (DataStr.Length % 2 == 0)
            {
                DataCount = DataStr.Length / 2;
                this.label1.Text = "已输入数据（" + DataCount.ToString().PadLeft(2, '0') + ".0/22.0）";
            }
            else
            {
                DataCount = DataStr.Length / 2;
                this.label1.Text = "已输入数据（" + DataCount.ToString().PadLeft(2, '0') + ".5/22.0）";
            }

        }
    }
}
