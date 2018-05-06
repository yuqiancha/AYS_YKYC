using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace H07_YKYC
{
    public partial class QueryMyDB : Form
    {
        public QueryMyDB()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string TableName = "table_Telemetry";
            switch (comboBox1.Text)
            {
                case "接收遥测VCDU":
                    TableName = "table_Telemetry";
                    break;
                case "解析EPDU":
                    TableName = "table_Epdu";
                    break;
                case "发送遥控":
                    TableName = "table_Telecmd";
                    break;
                default:
                    TableName = "table_Telecmd";
                    break;
            }

            //select * from table_All where CreateTime >= '2018-03-23 17:07:19' and CreateTime < '2019-08-02 00:00:00'

            SQLiteConnection dbConnection = new SQLiteConnection("data source=mydb.db");
            string cmd = "Select * From " + TableName + " where " + "CreateTime >= '" + dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' and CreateTime <= '" + dateTimePicker2.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            Trace.WriteLine(cmd);
            SQLiteDataAdapter mAdapter = new SQLiteDataAdapter(cmd, dbConnection);
            DataTable mTable = new DataTable(); // Don't forget initialize!
            mAdapter.Fill(mTable);

            // 绑定数据到DataGridView
            dataGridView1.DataSource = mTable;

            dataGridView1.Columns[0].Width = 50;
            dataGridView1.Columns[dataGridView1.ColumnCount - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[1].Width = 150;
          
        }

        private void QueryMyDB_Load(object sender, EventArgs e)
        {
            dateTimePicker2.Value = DateTime.Now;
            dateTimePicker1.Value = DateTime.Now.AddDays(-1);

            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("解析EPDU");
        }
    }
}
