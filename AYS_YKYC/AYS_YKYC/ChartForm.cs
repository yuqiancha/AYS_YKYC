using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace H07_YKYC
{
    public partial class ChartForm : Form
    {

        TreeNode node1 = new TreeNode("重要遥测");
        TreeNode node2 = new TreeNode("计算机");
        TreeNode node3 = new TreeNode("软件");
        TreeNode node4 = new TreeNode("GNC");
        TreeNode node5 = new TreeNode("热控");
        TreeNode node6 = new TreeNode("IMU_A");
        TreeNode node7 = new TreeNode("IMU_B");
        TreeNode node8 = new TreeNode("测控");
        TreeNode node9 = new TreeNode("数传");
        TreeNode node10 = new TreeNode("载荷3S");
        TreeNode node11 = new TreeNode("GNSS");
        TreeNode node12 = new TreeNode("OBC_B");
        TreeNode node13 = new TreeNode("系统延时遥测");
        TreeNode node14 = new TreeNode("GNC延时遥测");
        TreeNode[] NodeList;

        SQLiteConnection dbConnection = new SQLiteConnection("data source=mydb.db");

        public ChartForm()
        {
            InitializeComponent();
        }

        private void ChartForm_Load(object sender, EventArgs e)
        {
            NodeList = new TreeNode[] { node1, node2, node3, node4, node5, node6, node7, node8, node9, node10, node11, node12, node13, node14 };
            try
            {
                for (int i = 0; i < NodeList.Count(); i++)
                {
                    TreeNode nodet = NodeList[i];
                    treeView1.Nodes.Add(NodeList[i]);

                    List<string> TempList = Data.GetConfigNormal(Data.APIDconfigPath, NodeList[i].Text);
                    for (int j = 0; j < TempList.Count(); j++)
                    {
                        NodeList[i].Nodes.Add(new TreeNode(TempList[j]));
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            Trace.WriteLine("TreeView完成初始化！");

            DateTime timenow = System.DateTime.Now;
            String temp = timenow.ToString("yyyy-MM-dd HH:mm:ss");
            dateTimePicker2.Text = temp;
            DateTime timebefore = timenow.AddDays(-1);
            String tempbefore = timebefore.ToString("yyyy-MM-dd HH:mm:ss");
            dateTimePicker1.Text = tempbefore;


            z1.GraphPane.Title = "图表";
            z1.GraphPane.XAxis.Title = "时间";
            z1.GraphPane.YAxis.Title = "值";
            z1.GraphPane.XAxis.MinAuto = true;
            z1.GraphPane.XAxis.MaxAuto = true;
            z1.GraphPane.XAxis.Type = ZedGraph.AxisType.Date;
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            for (int i = 0; i < NodeList.Count(); i++)
            {
                if (treeView1.SelectedNode.Parent == NodeList[i])
                {
                    Trace.WriteLine(treeView1.SelectedNode.Parent.Text+":"+treeView1.SelectedNode.Text);
                    String TableName = "table_" + treeView1.SelectedNode.Parent.Text;
                    String SelectColum = treeView1.SelectedNode.Text;
                    //根据此处的APID-内容，进行下一步解析和处理


                    string Str_Condition_time = "CreateTime >= '" + dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'"
                                     + "and CreateTime <= '" + dateTimePicker2.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    
                    string cmd = "Select CreateTime,["+SelectColum+"] From " + TableName +" where "+ Str_Condition_time;

                    SQLiteDataAdapter mAdapter = new SQLiteDataAdapter(cmd, dbConnection);
                    DataTable mTable = new DataTable(); // Don't forget initialize!
                    mAdapter.Fill(mTable);
                    dataGridView1.DataSource = mTable;
                    
                    double[] x = new double[mTable.Rows.Count];
                    double[] y = new double[mTable.Rows.Count];

                    for(int j=0;j< mTable.Rows.Count;j++)
                    {
                       // Trace.WriteLine(mTable.Rows[j]["CreateTime"] + ":" + mTable.Rows[j][SelectColum]);

                        DateTime time = Convert.ToDateTime(mTable.Rows[j]["CreateTime"]);
                        x[j] = (double)new XDate(time);

                        string value = (string)mTable.Rows[j][SelectColum];                
                        y[j] = Convert.ToInt64(value.Substring(2,value.Length-2),16);
                    }



                    z1.GraphPane.AddCurve(SelectColum, x, y, Color.Black, ZedGraph.SymbolType.Circle);

                    int t = z1.GraphPane.CurveList.Count;
                    for(int m=0;m<t;m++)
                    {
                        CurveItem mycurve = z1.GraphPane.CurveList[m];
                        Trace.WriteLine(mycurve.Label);                       
                    }


                    z1.AxisChange();
                    z1.Invalidate();

                    comboBox1.Items.Add(SelectColum);
                }             
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void ShowXAxis()
        {
            while(true)
            {



                Thread.Sleep(1000);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            z1.GraphPane.CurveList.Clear();
        }
    }
}
