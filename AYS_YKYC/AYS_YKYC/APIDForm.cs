using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace H07_YKYC
{
    public partial class APIDForm : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public APIDForm(string apidstr)
        {
            InitializeComponent();
            this.Text = apidstr;
        }

        private void APIDForm_Load(object sender, EventArgs e)
        {

        }
    }
}
