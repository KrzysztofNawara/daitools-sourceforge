using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using DA_Tool.Frostbite;
using Be.Windows.Forms;

namespace DA_Tool.CASExplorer
{
    public partial class CASExplorer : Form
    {
        public CASFile cas;
        public CATFile cat;

        public CASExplorer()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.cas|*.cas";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                cas = new CASFile(d.FileName);
                OpenFileDialog d2 = new OpenFileDialog();
                d2.Filter = "*.cat|*.cat";
                if (d2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    cat = new CATFile(d2.FileName);
                    cas.SetCAT(cat);
                    if (cas.Indexes.Count > 100)
                        hs1.Maximum = cas.Indexes.Count - 100;
                    else
                        hs1.Maximum = 0;
                    RefreshList();
                }
            }
        }

        public void RefreshList()
        {
            if (cas == null || cat == null)
                return;
            listBox1.Items.Clear();
            for (int n = 0; n < 100; n++)
            {
                int idx = hs1.Value + n;
                if (idx >= 0 && idx < cas.Indexes.Count)
                {
                    uint[] line = cat.lines[cas.Indexes[idx]];
                    string s = (idx).ToString("d4") + " : ";
                    foreach (uint u in line)
                        s += u.ToString("X8") + " ";
                    listBox1.Items.Add(s);
                }
            }
        }

        private void hs1_Scroll(object sender, ScrollEventArgs e)
        {
            RefreshList();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int m = hs1.Value;
            int n = listBox1.SelectedIndex;
            if (n == -1 || m == -1 || cas == null || cat == null)
                return;
            int Idx = m + n;
            CASFile.CASEntry en = cas.ReadEntry(Idx);
            hb1.ByteProvider = new DynamicByteProvider(en.data.ToArray());
        }

        private void exportSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int m = hs1.Value;
            int n = listBox1.SelectedIndex;
            if (n == -1 || m == -1 || cas == null || cat == null)
                return;
            int Idx = m + n;
            CASFile.CASEntry en = cas.ReadEntry(Idx);
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            string filename = "CAS" + cas.casnumber.ToString("d2") + "_";
            for (int i = 0; i < 5; i++)
                filename += cat.lines[cas.Indexes[Idx]][i].ToString("x8");
            filename += ".bin";
            d.FileName = filename;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(d.FileName, en.data.ToArray());
                MessageBox.Show("Done.");
            }
        }

        private void sHA1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int m = hs1.Value;
            int n = listBox1.SelectedIndex;
            if (n == -1 || m == -1 || cas == null || cat == null)
                return;
            int Idx = m + n;
            CASFile.CASEntry en = cas.ReadEntry(Idx);
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] result = sha1.ComputeHash(en.compressed);
            string s = "SHA1:\n";
            int count = 1;
            foreach (byte b in result)
                if ((count++) % 4 == 0)
                    s += b.ToString("X2") + "  ";
                else
                    s += b.ToString("X2") + " ";
            MessageBox.Show(s);
        }
    }
}
