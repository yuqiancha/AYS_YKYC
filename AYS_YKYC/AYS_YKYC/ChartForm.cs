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


            this.z1.GraphPane.Title = "图表";
            double[] x = new double[100];
            double[] y = new double[100];
            for (int i = 0; i < 100; i++)
            {
                x[i] = (double)i / 100.0 * Math.PI * 2.0;
                y[i] = Math.Sin(x[i]);
            }
            z1.GraphPane.AddCurve("haha", x, y, Color.Black, ZedGraph.SymbolType.Default);
            z1.AxisChange();
            z1.Invalidate();

            z1.GraphPane.XAxis.Title = "时间";
            z1.GraphPane.YAxis.Title = "值";

        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            for (int i = 0; i < NodeList.Count(); i++)
            {
                if (treeView1.SelectedNode.Parent == NodeList[i])
                {
                    Trace.WriteLine(treeView1.SelectedNode.Text); 
                    //根据此处的APID-内容，进行下一步解析和处理

                }             
            }
        }
    }
}
