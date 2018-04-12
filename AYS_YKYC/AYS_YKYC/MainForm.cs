using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Configuration;
using System.Net.Sockets;

namespace H07_YKYC
{
    public partial class MainForm : Form
    {
        public string path = Program.GetStartupPath() + @"SaveData\";

        public string Path = null;          //程序运行的目录
        public DateTime startDT;
        public SettingForm mySettingForm;
        public QueryForm myQueryForm;
        public SaveFile mySaveFileThread;
        public QueryMyDB mySqlForm;

        ServerAPP myServer = new ServerAPP();
        public int TagLock;

        public Queue<string[]> YKQueue = new Queue<string[]>();  //用于转存遥控日志
        public Queue<string[]> YKLogQueue = new Queue<string[]>();  //用于遥控日志显示存储


        public bool ServerLedThreadTag = false;
        public bool ServerLedThreadTag2 = false;

        //创建K令码表源文件数组
        //public byte[] KcmdText;

        /// <summary>
        /// 修改AppSettings中配置
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">相应值</param>
        public bool SetConfigValue(string key, string value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] != null)
                    config.AppSettings.Settings[key].Value = value;
                else
                    config.AppSettings.Settings.Add(key, value);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch
            {
                return false;
            }
        }
        public MainForm()
        {
            InitializeComponent();
            mySettingForm = new SettingForm(this);
            myQueryForm = new QueryForm(this);

            Path = Program.GetStartupPath();

            //启动日志
            MyLog.richTextBox1 = richTextBox1;
            MyLog.path = Program.GetStartupPath() + @"LogData\";
            MyLog.lines = 50;
            MyLog.start();
            startDT = System.DateTime.Now;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                Data.DealCRTa.CRTName = "USB应答机A";
                Data.DealCRTa.XHLEnable = true;
                Data.DealCRTa.Led = pictureBox_CRTa;
                Data.DealCRTa.init();

                toolStripStatusLabel2.Text = "存储路径" + Path;
                mySaveFileThread = new SaveFile();
                mySaveFileThread.FileInit();
                mySaveFileThread.FileSaveStart();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            initTable();//初始化各类DataTable和datagridview

            Data.TelemetryRealShowBox = this.textBox2;

            Data.sql = new SqLiteHelper("data source=mydb.db");
            //创建名为table数据表
            Data.sql.CreateTable("table_Telemetry", new string[] { "InfoType", "CreateTime", "IP", "DetailInfo" }, new string[] { "TEXT", "TEXT", "TEXT", "TEXT" });
            Data.sql.CreateTable("table_Telecmd", new string[] { "InfoTye", "CreateTime", "IP", "DetailInfo" }, new string[] { "TEXT", "TEXT", "TEXT", "TEXT" });


            List<string> APIDList = Data.GetConfigNormal(Data.YCconfigPath, "add");
            for(int i=0;i<APIDList.Count();i++)
            {
                DataRow dr = Data.dtAPID.NewRow();
                dr["APID"] = APIDList[i];
                dr["名称"] = Data.GetConfig(Data.YCconfigPath, APIDList[i]);
                dr["数量"] = 0;
                Data.dtAPID.Rows.Add(dr);
            }

            TreeNode node1 = new TreeNode("常用指令");
            treeView1.Nodes.Add(node1);
            TreeNode node2 = new TreeNode("重要指令");
            treeView1.Nodes.Add(node2);
            TreeNode node3 = new TreeNode("其它指令");
            treeView1.Nodes.Add(node3);
            try
            {
                List<string> NormalList = Data.GetConfigNormal(Data.YKconfigPath, "Normal");
                for (int i = 0; i < NormalList.Count(); i++)
                {
                    node1.Nodes.Add(new TreeNode(NormalList[i]));
                }
                List<string> ImportantList = Data.GetConfigNormal(Data.YKconfigPath, "Important");
                for (int i = 0; i < ImportantList.Count(); i++)
                {
                    node2.Nodes.Add(new TreeNode(ImportantList[i]));
                }
                List<string> OtherList = Data.GetConfigNormal(Data.YKconfigPath, "Other");
                for (int i = 0; i < OtherList.Count(); i++)
                {
                    node3.Nodes.Add(new TreeNode(OtherList[i]));
                }
                treeView1.ExpandAll();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            Trace.WriteLine("完成初始化！");
        }

        private void initTable()
        {
            #region 初始化DataTable
            try
            {
                #region dtVCDU----dataGridView_VCDU 
                Data.dtVCDU.Columns.Add("版本号", typeof(string));
                Data.dtVCDU.Columns.Add("SCID", typeof(string));
                Data.dtVCDU.Columns.Add("VCID", typeof(string));
                Data.dtVCDU.Columns.Add("虚拟信道帧计数", typeof(string));
                Data.dtVCDU.Columns.Add("回放", typeof(string));
                Data.dtVCDU.Columns.Add("保留", typeof(string));
                Data.dtVCDU.Columns.Add("插入域", typeof(string));
                Data.dtVCDU.Columns.Add("备用", typeof(string));
                Data.dtVCDU.Columns.Add("首导头指针", typeof(string));

                DataRow dr = Data.dtVCDU.NewRow();
                dr["版本号"] = "01";
                dr["SCID"] = "07";
                dr["VCID"] = "06";
                dr["虚拟信道帧计数"] = "000000";
                dr["回放"] = "00";
                dr["保留"] = "00";
                dr["插入域"] = "00000000";
                dr["备用"] = "00000";
                dr["首导头指针"] = "00000000000";
                Data.dtVCDU.Rows.Add(dr);

                dataGridView_VCDU.DataSource = Data.dtVCDU;
                dataGridView_VCDU.AllowUserToAddRows = false;
                #endregion

                #region dtUSRP----dataGridView1
                Data.dtUSRP.Columns.Add("名称", typeof(string));
                Data.dtUSRP.Columns.Add("数量", typeof(Int32));

                DataRow dr1 = Data.dtUSRP.NewRow();
                dr1["名称"] = "遥测帧总帧数";
                dr1["数量"] = 0;
                Data.dtUSRP.Rows.Add(dr1);

                dr1 = Data.dtUSRP.NewRow();
                dr1["名称"] = "遥测帧信息格式正确数";
                dr1["数量"] = 0;
                Data.dtUSRP.Rows.Add(dr1);

                dr1 = Data.dtUSRP.NewRow();
                dr1["名称"] = "遥测帧信息格式错误数";
                dr1["数量"] = 0;
                Data.dtUSRP.Rows.Add(dr1);


                dr1 = Data.dtUSRP.NewRow();
                dr1["名称"] = "遥控帧发送总帧数";
                dr1["数量"] = 0;
                Data.dtUSRP.Rows.Add(dr1);

                dataGridView1.DataSource = Data.dtUSRP;
                dataGridView1.AllowUserToAddRows = false;

                #endregion

                #region dtYC----dataGridView2
                Data.dtYC.Columns.Add("名称", typeof(string));
                Data.dtYC.Columns.Add("数量", typeof(Int32));

                DataRow dr2 = Data.dtYC.NewRow();
                dr2["名称"] = "遥测数据帧接收总数量";
                dr2["数量"] = 0;
                Data.dtYC.Rows.Add(dr2);

                dr2 = Data.dtYC.NewRow();
                dr2["名称"] = "校验正确帧数量";
                dr2["数量"] = 0;
                Data.dtYC.Rows.Add(dr2);

                dr2 = Data.dtYC.NewRow();
                dr2["名称"] = "校验错误帧数量";
                dr2["数量"] = 0;
                Data.dtYC.Rows.Add(dr2);

                dr2 = Data.dtYC.NewRow();
                dr2["名称"] = "成功转发帧数量";
                dr2["数量"] = 0;
                Data.dtYC.Rows.Add(dr2);

                dr2 = Data.dtYC.NewRow();
                dr2["名称"] = "失败转发帧数量";
                dr2["数量"] = 0;
                Data.dtYC.Rows.Add(dr2);

                dataGridView2.DataSource = Data.dtYC;
                dataGridView2.AllowUserToAddRows = false;
                #endregion

                #region dtYKLog----dataGridView_yklog

                Data.dtYKLog.Columns.Add("发送时间", typeof(string));
                Data.dtYKLog.Columns.Add("遥控名称", typeof(string));
                Data.dtYKLog.Columns.Add("遥控源码", typeof(string));
                Data.dtYKLog.Columns.Add("发送结果", typeof(string));

                dataGridView_yklog.DataSource = Data.dtYKLog;
                dataGridView_yklog.AllowUserToAddRows = false;
                #endregion


                #region dtAPID----dataGridView3

                Data.dtAPID.Columns.Add("APID", typeof(string));
                Data.dtAPID.Columns.Add("名称", typeof(string));
                Data.dtAPID.Columns.Add("数量", typeof(int));

                dataGridView3.DataSource = Data.dtAPID;
                dataGridView3.AllowUserToAddRows = false;
                #endregion

            }
            catch (Exception ex)
            {
                Trace.WriteLine("初始化DataTable Failed：" + ex.Message);
            }
            #endregion

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (btn_ZK1_Close.Enabled) btn_ZK1_Close_Click(sender, e);
                if (btn_ZK1_YC_Close.Enabled) btn_ZK1_YC_Close_Click(sender, e);

                if (btn_CRTa_Open.Enabled == false)
                {
                    ClientAPP.Disconnect(ref ClientAPP.Server_CRTa);
                    ClientAPP.Disconnect(ref ClientAPP.Server_CRTa_Return);
                    btn_CRTa_Open.Enabled = true;
                    btn_CRTa_Close.Enabled = false;
                    Data.DealCRTa.LedOff();
                    MyLog.Info("关闭连接--USB应答机A");
                }

                Thread.Sleep(100);
                mySaveFileThread.FileClose();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }


        #region MainForm_Paint
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Trace.WriteLine("MainForm_Paint");

        }
        #endregion

        public bool Logform_state = true;
        public int LogWaitTime = 600;
        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel3.Text = "剩余空间" + DiskInfo.GetFreeSpace(Path[0].ToString()) + "MB";
            toolStripStatusLabel5.Text = "当前时间：" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ";

            TimeSpan ts = DateTime.Now.Subtract(startDT);
            toolStripStatusLabel6.Text = "已运行：" + ts.Days.ToString() + "天" +
                ts.Hours.ToString() + "时" +
                ts.Minutes.ToString() + "分" +
                ts.Seconds.ToString() + "秒";

        }

        private void 系统设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyLog.Info("进行系统设置");
            if (mySettingForm != null)
            {
                mySettingForm.Activate();
            }
            else
            {
                mySettingForm = new SettingForm(this);
            }
            mySettingForm.ShowDialog();
        }



        private void 运行日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Program.GetStartupPath() + @"LogData\";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //   openFileDialog1.InitialDirectory = Program.GetStartupPath() + @"LogData\";
                Process Pnoted = new Process();
                try
                {
                    Pnoted.StartInfo.FileName = openFileDialog1.FileName;
                    Pnoted.Start();
                }
                catch
                {
                    //MessageBox.Show("运行日志打开失败！");
                }
            }
        }


        private void 遥控日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Program.GetStartupPath() + @"LogData\";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //   openFileDialog1.InitialDirectory = Program.GetStartupPath() + @"LogData\";
                Process Pnoted = new Process();
                try
                {
                    Pnoted.StartInfo.FileName = openFileDialog1.FileName;
                    Pnoted.Start();
                }
                catch
                {
                    //MessageBox.Show("运行日志打开失败！");
                }
            }
        }

        private void 数据查询和回放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (myQueryForm != null)
            {
                myQueryForm.Activate();
            }
            else
            {
                myQueryForm = new QueryForm(this);
            }
            myQueryForm.ShowDialog();
        }


        void DealCRT_On(ref Data.CRT_STRUCT myCRT)
        {
            myCRT.LedOn();
            MyLog.Info("连接成功--" + myCRT.CRTName);

        }
        void DealCRT_Off(ref Data.CRT_STRUCT myCRT)
        {
            myCRT.LedOff();
            MyLog.Info("无法连接--" + myCRT.CRTName);
        }
        private void buttonCRT_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "btn_CRTa_Open":
                    MyLog.Info("尝试连接--USB应答机A...");
                    ClientAPP.Server_CRTa.ServerIP = ConfigurationManager.AppSettings["Server_CRTa_Ip"];
                    ClientAPP.Server_CRTa.ServerPORT = ConfigurationManager.AppSettings["Server_CRTa_Port"];
                    ClientAPP.Connect(ref ClientAPP.Server_CRTa);
                    if (ClientAPP.Server_CRTa.IsConnected)
                    {
                        DealCRT_On(ref Data.DealCRTa);
                        MyLog.Info("连接成功--" + Data.DealCRTa.CRTName + "--遥控端口");
                        new Thread(() => { Fun_Transfer2CRT(ref Data.DealCRTa, ref ClientAPP.Server_CRTa, ref SaveFile.DataQueue_out2); }).Start();
                        new Thread(() => { Fun_RecvFromCRT(ref Data.DealCRTa, ref ClientAPP.Server_CRTa); }).Start();
                    }
                    else
                    {
                        DealCRT_Off(ref Data.DealCRTa);
                        return;
                    }
                    btn_CRTa_Open.Enabled = false;
                    btn_CRTa_Close.Enabled = true;

                    ClientAPP.Server_CRTa_Return.ServerIP = ConfigurationManager.AppSettings["Server_CRTa_Ip"];
                    ClientAPP.Server_CRTa_Return.ServerPORT = "3070";
                    ClientAPP.Connect(ref ClientAPP.Server_CRTa_Return);
                    if (ClientAPP.Server_CRTa_Return.IsConnected)
                    {
                        MyLog.Info("连接成功--" + Data.DealCRTa.CRTName + "--遥测端口");
                        new Thread(() => { Fun_RecvFromCRT_Return(ref Data.DealCRTa, ref ClientAPP.Server_CRTa_Return); }).Start();
                    }

                    break;
                case "btn_CRTa_Close":
                    ClientAPP.Disconnect(ref ClientAPP.Server_CRTa);
                    ClientAPP.Disconnect(ref ClientAPP.Server_CRTa_Return);
                    btn_CRTa_Open.Enabled = true;
                    btn_CRTa_Close.Enabled = false;
                    Data.DealCRTa.LedOff();
                    MyLog.Info("关闭连接--USB应答机A");
                    break;

                default:
                    break;
            }
        }

        public delegate void UpdateText(string str, TextBox mytextbox);
        public void UpdateTextMethod(string str, TextBox mytextbox)
        {
            mytextbox.Text = str;
        }


        private void Fun_RecvFromCRT(ref Data.CRT_STRUCT myCRT, ref ClientAPP.TCP_STRUCT Server_CRT)
        {
            Trace.WriteLine("Entering" + myCRT.CRTName + "Fun_RecvFromCRT!!");

            Delegate la = new UpdateText(UpdateTextMethod);

            while (Server_CRT.IsConnected)
            {
                try
                {
                    byte[] RecvBufCRTa = new byte[1024];
                    int RecvNum = Server_CRT.sck.Receive(RecvBufCRTa);
                    if (RecvNum > 0)
                    {
                        int[] RecvBufInt = Program.BytesToInt(RecvBufCRTa);

                        myCRT.TCMsgStatus = RecvBufInt[7];

                        this.Invoke(la, RecvBufInt[7].ToString(), myCRT.mytextbox);

                    }
                    else
                    {
                        Trace.WriteLine("收到数据少于等于0！");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Trace.WriteLine("Exception leave!!");
                    break;
                }
            }
        }


        private void Fun_RecvFromCRT_Return(ref Data.CRT_STRUCT myCRT, ref ClientAPP.TCP_STRUCT Server_CRT)
        {
            Trace.WriteLine("++++++++++Entering" + myCRT.CRTName + "Fun_RecvFromCRT_Return!!");

            //int[] SendReq;
            //byte[] SendReqBytes;
            ////send request
            //SendReq = new int[16];
            //SendReq[0] = 1234567890;        //Start of msg
            //SendReq[1] = 64;                //Size of msg in bytes
            //SendReq[2] = 0;                 //Size of msg
            //SendReq[3] = 0;                 //0:channelA 1:channelB
            //SendReq[4] = 0;                 //Real time telemetry
            //SendReq[5] = 0;                 //Permanent flow 一次请求
            //SendReq[15] = -1234567890;
            //SendReqBytes = Program.IntToBytes(SendReq);

            //int len = Server_CRT.sck.Send(SendReqBytes);

            while (Server_CRT.IsConnected)
            {
                try
                {
                    byte[] RecvBufCRTa = new byte[200];
                    int RecvNum = Server_CRT.sck.Receive(RecvBufCRTa);

                    if (RecvNum > 0)
                    {
                        //TempRecvBuf 本次收到的数据
                        byte[] TempRecvBuf = new byte[RecvNum];
                        Array.Copy(RecvBufCRTa, TempRecvBuf, RecvNum);

                        SaveFile.Lock_Dat3.EnterWriteLock();
                        SaveFile.DataQueue_out3.Enqueue(TempRecvBuf);
                        SaveFile.Lock_Dat3.ExitWriteLock();

                        String tempstr = "";
                        for (int i = 0; i < TempRecvBuf.Length; i++)
                        {
                            tempstr += TempRecvBuf[i].ToString("x2");
                        }
                        Trace.WriteLine(tempstr);

                        Data.dtYC.Rows[0]["数量"] = (int)Data.dtYC.Rows[0]["数量"] + 1; //收到总数

                        //YCBuf 本次收到的实际遥测数据
                        byte[] YCBuf = new byte[RecvNum - 68];
                        Array.Copy(RecvBufCRTa, 64, YCBuf, 0, RecvNum - 68);

                        SaveFile.Lock_Dat4.EnterWriteLock();
                        SaveFile.DataQueue_out4.Enqueue(YCBuf);
                        SaveFile.Lock_Dat4.ExitWriteLock();

                        String tempstr2 = "";
                        for (int i = 0; i < YCBuf.Length; i++)
                        {
                            tempstr2 += YCBuf[i].ToString("x2");
                        }
                        Trace.WriteLine(tempstr2);

                        //ushort CRC = 0xffff;
                        //ushort genpoly = 0x1021;
                        //for (int i = 0; i < YCBuf.Length-2; i = i + 1)
                        //{
                        //    CRC = Function.CRChware(YCBuf[i], genpoly, CRC);
                        //}
                        ////      MyLog.Info("Calc 通道1 CRC = " + CRC.ToString("x4"));
                        //Trace.WriteLine("Calc 通道1 CRC = " + CRC.ToString("x4"));
                    }
                    else
                    {
                        Trace.WriteLine("收到数据少于等于0！");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                    break;
                }
            }
            Trace.WriteLine("----------Leaving" + myCRT.CRTName + "Fun_RecvFromCRT_Return!!");
        }

        private void Fun_Transfer2CRT(ref Data.CRT_STRUCT myCRT, ref ClientAPP.TCP_STRUCT Server_CRT, ref Queue<byte[]> DataQueue_save)
        {
            Delegate la = new UpdateText(UpdateTextMethod);

            while (Server_CRT.IsConnected)
            {
                if (myCRT.DataQueue_CRT.Count() > 0)
                {
                    byte[] SendByte = myCRT.DataQueue_CRT.Dequeue();
                    Server_CRT.sck.Send(SendByte);

                    //Data.dtUSRP.Rows[3]["数量"] = (int)Data.dtUSRP.Rows[3]["数量"] + 1;


                    //增加存储，DataQueue_save为引用的对应SaveFile里面的Queue
                    DataQueue_save.Enqueue(SendByte);

                    myCRT.Transfer2CRTa_TempStr = "";
                    for (int m = 24; m < SendByte.Length - 8; m++)
                    {
                        myCRT.Transfer2CRTa_TempStr += SendByte[m].ToString("x2");
                    }
                    Trace.WriteLine("Fun_Transfer2CRT:" + myCRT.Transfer2CRTa_TempStr);

                }

            }
        }

        /// <summary>
        /// 十六进制String转化为BYTE数组
        /// </summary>
        /// <param name="hexString">参数：输入的十六进制String</param>
        /// <returns>BYTE数组</returns>
        private static byte[] StrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "").Replace("\r", "").Replace("\n", "");
            //if ((hexString.Length % 2) != 0)
            //    hexString += " ";

            byte[] returnBytes = new byte[hexString.Length / 2];

            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;

        }

        /// <summary>
        /// 启动服务器Socket监听USRP设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ZK1_Open_Click(object sender, EventArgs e)
        {
            ServerLedThreadTag = true;
            new Thread(() => { ServerConnect(); }).Start();
            myServer.ServerStart();

            if (myServer.ServerOn)
            {
                btn_ZK1_Open.Enabled = false;
                btn_ZK1_Close.Enabled = true;
            }
            else
            {
                btn_ZK1_Close.Enabled = false;
                btn_ZK1_Open.Enabled = true;
            }
        }

        /// <summary>
        /// 关闭服务器Socket，断开与总控设备连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ZK1_Close_Click(object sender, EventArgs e)
        {
            ServerLedThreadTag = false;
            myServer.ServerStop();

            btn_ZK1_Close.Enabled = false;
            btn_ZK1_Open.Enabled = true;
        }




        private void 启动toolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (启动toolStripMenuItem.Text == "启动")
            {
                btn_ZK1_Open_Click(sender, e);
                btn_ZK1_YC_Open_Click(sender, e);
                //     buttonCRT_Click(btn_CRTa_Open, e);
                //     buttonCRT_Click(btn_CRTb_Open, e);

                启动toolStripMenuItem.Text = "停止";
            }
            else
            {
                btn_ZK1_Close_Click(sender, e);
                btn_ZK1_YC_Close_Click(sender, e);
                //   buttonCRT_Click(btn_CRTa_Close, e);
                //   buttonCRT_Click(btn_CRTb_Close, e);


                启动toolStripMenuItem.Text = "启动";
            }
        }

        private void btn_ZK1_YC_Open_Click(object sender, EventArgs e)
        {
            ServerLedThreadTag2 = true;
            new Thread(() => { ServerConnect2(); }).Start();
            myServer.ServerStart2();

            if (myServer.ServerOn_YC)
            {
                btn_ZK1_YC_Open.Enabled = false;
                btn_ZK1_YC_Close.Enabled = true;
            }
            else
            {
                btn_ZK1_YC_Close.Enabled = false;
                btn_ZK1_YC_Open.Enabled = true;
            }
        }

        private void btn_ZK1_YC_Close_Click(object sender, EventArgs e)
        {
            ServerLedThreadTag2 = false;
            myServer.ServerStop2();

            btn_ZK1_YC_Close.Enabled = false;
            btn_ZK1_YC_Open.Enabled = true;
        }

        private void ServerConnect()
        {
            Trace.WriteLine("Enter-------ServerConnect1");
            while (ServerLedThreadTag)
            {
                Data.ServerConnectEvent.WaitOne();
                Data.ServerConnectEvent.Reset();

                if (ClientAPP.ClientUSRP_Telecmd.IsConnected)
                    pictureBox_ZK1.Image = Properties.Resources.green2;
                else
                    pictureBox_ZK1.Image = Properties.Resources.red2;

            }
            Trace.WriteLine("Leave------ServerConnect1");
        }

        private void ServerConnect2()
        {
            Trace.WriteLine("Enter-------ServerConnect2");
            while (ServerLedThreadTag2)
            {
                Data.ServerConnectEvent2.WaitOne();
                Data.ServerConnectEvent2.Reset();

                if (ClientAPP.ClientUSRP_Telemetry.IsConnected)
                    pictureBox_ZK1_YC.Image = Properties.Resources.green2;
                else
                    pictureBox_ZK1_YC.Image = Properties.Resources.red2;

            }
            Trace.WriteLine("Leave------ServerConnect2");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (DataRow dr in Data.dtUSRP.Rows)
            {
                dr["数量"] = 0;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (DataRow dr in Data.dtYC.Rows)
            {
                dr["数量"] = 0;
            }
        }

        private void btn_SendYC_Click(object sender, EventArgs e)
        {
            string Str_Content = this.textBox1.Text.Replace(" ", "");
            int AddAlen = 16 - (Str_Content.Length % 16);
            Str_Content = Str_Content.PadRight(AddAlen, 'A');

            Trace.WriteLine("遥控发送数据：" + Str_Content);

            byte[] temp = StrToHexByte(Str_Content);

            if(temp.Length==35)
            {
                DataRow dr = Data.dtYKLog.NewRow();
                dr["发送时间"] = string.Format("{0}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                dr["遥控名称"] = label_ykname.Text;
                dr["遥控源码"] = Str_Content;
                if (Data.USRP_telecmd_IsConnected)
                {
                    dr["发送结果"] = "发送成功";
                    Data.DataQueue_USRP_telecmd.Enqueue(temp);
                }
                else
                {
                    dr["发送结果"] = "发送失败，网络未连接";
                }
                Data.dtYKLog.Rows.Add(dr);
            }
            else
            {
                MessageBox.Show("遥控指令格式错误，无法发送！！");
            }       
        }

        TelecmdProduce myFrameProdeceForm;
        private void textBox1_Click(object sender, EventArgs e)
        {
            if (myFrameProdeceForm != null)
            {
                myFrameProdeceForm.Activate();
            }
            else
            {
                myFrameProdeceForm = new TelecmdProduce(this);
            }
            myFrameProdeceForm.ShowDialog();
        }

        private void 数据库查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mySqlForm != null)
            {
                mySqlForm.Activate();
            }
            else
            {
                mySqlForm = new QueryMyDB();
            }
            mySqlForm.ShowDialog();
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            if ((treeView1.SelectedNode != treeView1.Nodes[0])
                && (treeView1.SelectedNode != treeView1.Nodes[1])
                && (treeView1.SelectedNode != treeView1.Nodes[2]))
            {
                if (treeView1.SelectedNode.Parent == treeView1.Nodes[0])
                {
                    String YKCmdStr = Data.GetConfigStr(Data.YKconfigPath, "Normal", treeView1.SelectedNode.Text, "value");
                    textBox1.Text = YKCmdStr;
                }
                else if (treeView1.SelectedNode.Parent == treeView1.Nodes[1])
                {
                    String YKCmdStr = Data.GetConfigStr(Data.YKconfigPath, "Important", treeView1.SelectedNode.Text, "value");
                    textBox1.Text = YKCmdStr;
                }
                else
                {
                    String YKCmdStr = Data.GetConfigStr(Data.YKconfigPath, "Other", treeView1.SelectedNode.Text, "value");
                    textBox1.Text = YKCmdStr;
                }
                label_ykname.Text = treeView1.SelectedNode.Text;
            }
        }

        private void dataGridView_yklog_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            //显示在HeaderCell上
            for (int i = 0; i < this.dataGridView_yklog.Rows.Count; i++)
            {
                DataGridViewRow r = this.dataGridView_yklog.Rows[i];
                r.HeaderCell.Value = string.Format("{0}", i + 1);
            }
            this.dataGridView_yklog.Refresh();
        }

        private void dataGridView_yklog_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 2)                        //输出指令
                {
                    Trace.WriteLine("333");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show(this.dockPanel1);
            form1.DockTo(this.dockPanel1, DockStyle.Fill);
        }
    }
}
