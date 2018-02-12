using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DA_Tool.Frostbite;

namespace DA_Tool.CATReader
{
    public partial class CATReader : Form
    {
        public CATFile cat;


        public CATReader()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.cat|*.cat";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                cat = new CATFile(d.FileName);
                if (cat.lines.Count > 100)
                    hScrollBar1.Maximum = cat.lines.Count - 100;
                else
                    hScrollBar1.Maximum = 0;
                RefreshList();
            }
        }

        public void RefreshList()
        {
            if (cat == null)
                return;
            listBox1.Items.Clear();
            for (int n = hScrollBar1.Value; n < cat.lines.Count && n < hScrollBar1.Value + 100; n++) 
            {
                uint[] line = cat.lines[n];
                string s = n.ToString("d4") + " : ";
                foreach (uint u in line)
                    s += u.ToString("X8") + " ";
                listBox1.Items.Add(s);
            }
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            RefreshList();
        }
    }
}
