using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace H07_YKYC
{
    public partial class APIDForm : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        MainForm myform;
        public string CurrentApidName;
        public int TotalLen = 0;//数据域总长度，多少个byte

        DataTable dt = new DataTable();
        DataTable dtAPid = new DataTable();


        public Queue<byte[]> DataQueue = new Queue<byte[]>();   //处理APID对应的Queue
        public APIDForm(string apidstr, H07_YKYC.MainForm parent)
        {
            InitializeComponent();
            this.Text = apidstr;
            CurrentApidName = apidstr;
            myform = parent;
        }

        private void APIDForm_Load(object sender, EventArgs e)
        {
            dt.Columns.Add("接收时间", typeof(string));
            dt.Columns.Add("版本号", typeof(string));
            dt.Columns.Add("类型", typeof(string));
            dt.Columns.Add("副导头标识", typeof(string));
            dt.Columns.Add("应用过程标识符", typeof(string));
            dt.Columns.Add("分组标识", typeof(string));
            dt.Columns.Add("包序列计数", typeof(string));
            dt.Columns.Add("包长", typeof(string));

            dt.Columns.Add("EPDU数据域", typeof(string));
            dataGridView_EPDU.DataSource = dt;

            dtAPid.Columns.Add("名称", typeof(string));
            dtAPid.Columns.Add("占位", typeof(string));
            dtAPid.Columns.Add("值", typeof(string));


            List<string> List = Data.GetConfigNormal(Data.APIDconfigPath, CurrentApidName);

  
            for (int i =0;i<List.Count;i++)
            {
                DataRow dr = dtAPid.NewRow();
                dr["名称"] = List[i];
                dr["占位"] = Data.GetConfigStr(Data.APIDconfigPath, CurrentApidName, List[i], "len");
                dtAPid.Rows.Add(dr);

                TotalLen += int.Parse((string)dr["占位"]);//这里算出来总共多少位
            }

            TotalLen = TotalLen / 8;//算出来多少Byte

            dataGridView1.DataSource = dtAPid;


            new Thread(() => { DealWithEPDU(); }).Start();
        }

        private void APIDForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            for(int i=0;i<myform.dataGridView3.Rows.Count;i++)
            {
                if(CurrentApidName==(string)Data.dtAPID.Rows[i]["名称"])
                {
                    DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)myform.dataGridView3.Rows[i].Cells[0];
                    checkCell.Value = false;

                    for (int j = 0; j < Data.ApidList.Count; j++)
                    {
                        if (CurrentApidName == Data.ApidList[j].apidName)
                        {
                            Data.ApidList.Remove(Data.ApidList[j]);
                            break;
                        }
                    }
                    break;
                }
            }
        }


        private void DealWithEPDU()
        {
            while(Data.AllThreadTag)
            {
                if(DataQueue.Count>0)
                {
                    byte[] Epdu = DataQueue.Dequeue();

                    string timestr = string.Format("{0}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    string epdustr = null;
                    for(int i=0;i<Epdu.Length;i++)
                    {
                        epdustr += Epdu[i].ToString("x2");
                    }
                    DataRow dr = dt.NewRow();
                    dr["接收时间"] = timestr;

                    dr["版本号"] = Convert.ToString(Epdu[0], 2).PadLeft(8, '0').Substring(0, 3);
                    dr["类型"] = Convert.ToString(Epdu[0], 2).PadLeft(8, '0').Substring(3, 1);
                    dr["副导头标识"] = Convert.ToString(Epdu[0], 2).PadLeft(8, '0').Substring(4, 1);

                    int ap = ((byte)(Epdu[0] & 0x07))*256 + Epdu[1];
                    dr["应用过程标识符"] = "0x" + ap.ToString("x3");

                    dr["分组标识"] = Convert.ToString(Epdu[2], 2).PadLeft(8, '0').Substring(0, 2) ;

                    int ct = ((byte)(Epdu[2] & 0x3f)) * 256 + Epdu[3];
                    dr["包序列计数"] = "0x" + ct.ToString("x3");

                    dr["包长"] = "0x" + Epdu[4].ToString("x2") + Epdu[5].ToString("x2"); 

                    dr["EPDU数据域"] = epdustr;
                    dt.Rows.Add(dr);

                    if(Epdu.Length < TotalLen)
                    {
                        MessageBox.Show(CurrentApidName + "收到EPDU长度错误，无法解析!!");
                    }
                    else
                    {
                        string tempstr = "";//将EPDU转化为二进制string
                        for (int i = 0; i < Epdu.Length; i++) tempstr += Convert.ToString(Epdu[i], 2).PadLeft(8,'0');
                        for(int j=0;j<dtAPid.Rows.Count;j++)
                        {
                            int len = int.Parse((string)dtAPid.Rows[j]["占位"]);
                            dtAPid.Rows[j]["值"] = tempstr.Substring(0, len);
                            tempstr = tempstr.Substring(len, tempstr.Length - len);
                        }


                    }

                    //int index = this.dataGridView_EPDU.Rows.Add();
                    //this.dataGridView_EPDU.Rows[index].Cells[0].Value = timestr;
                    //this.dataGridView_EPDU.Rows[index].Cells[1].Value = epdustr;
                    this.dataGridView_EPDU.BeginInvoke(new Action(() => { this.dataGridView_EPDU.Refresh(); }));
                }
                else
                {
                    Thread.Sleep(100);                    
                }
            }
        }

    }
}
