using Be.Windows.Forms;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using DAI_Tools.Frostbite;

namespace DAI_Tools.ModScriptTool
{
    public partial class ModScriptTool : Form
    {
        private bool init = false;
        public string basepath = Application.StartupPath + "\\";
        SQLiteConnection con = Database.GetConnection();
        public Mod MODs = new Mod();

        public ModScriptTool()
        {
            InitializeComponent();
        }

        private void ModScriptTool_Activated(object sender, EventArgs e)
        {
            if (!init)
                Init();
        }

        public void Init()
        {
            if (GlobalStuff.FindSetting("isNew") == "1")
            {
                MessageBox.Show("Please initialize the database in Database Manager with Scan");
                this.BeginInvoke(new MethodInvoker(Close));
                return;
            }
            Scripting.SetScriptOutput(rtb2);
            init = true;
        }

        private void saveMODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.daimod|*.daimod";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MODs.Save(d.FileName);
                RefreshMe();
                MessageBox.Show("Done.");
            }
        }

        private void loadMODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.daimod|*.daimod";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MODs.Load(d.FileName);
                RefreshMe();
                MessageBox.Show("Done.");
            }            
        }

        private void RefreshMe()
        {
            listBox1.Items.Clear();
            foreach (Mod.Modjob mod in MODs.jobs)
                listBox1.Items.Add(mod.name);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Mod.Modjob mod = MODs.jobs[n];
            string input = Microsoft.VisualBasic.Interaction.InputBox("Please enter new name", "Rename", mod.name);
            if (input == "")
                return;
            mod.name = input;
            MODs.jobs[n] = mod;
            RefreshMe();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            DialogResult res = MessageBox.Show("Sure?", "Delete", MessageBoxButtons.YesNo);
            if (res == System.Windows.Forms.DialogResult.Yes)
            {
                MODs.jobs.RemoveAt(n);
                RefreshMe();
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mod.Modjob mod = new Mod.Modjob();
            mod.script = File.ReadAllText(basepath + "templates\\empty_script.cs");
            string input = Microsoft.VisualBasic.Interaction.InputBox("Please enter name for mod", "Add", mod.name);
            if (input == "")
                return;
            mod.name = input;
            mod.data = new List<byte[]>();
            MODs.jobs.Add(mod);
            RefreshMe();
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Mod.Modjob mod = MODs.jobs[n];
            rtb1.Text = mod.script;
            rtb3.Text = mod.xml;
            mod.meta = Mod.MakeMetafromJobXML(mod.xml);
            comboBox1.Items.Clear();
            int counter = 0;
            foreach (byte[] data in mod.data)
                comboBox1.Items.Add("data_" + (counter++).ToString());
            if (counter != 0)
                comboBox1.SelectedIndex = 0;
            rtb4.Text = "";
            rtb4.AppendText("Mod ID: " + mod.meta.id);
            rtb4.AppendText("\nName : " + mod.meta.details.name);            
            rtb4.AppendText("\nVersion : " + mod.meta.details.version);
            rtb4.AppendText("\nAuthor: " + mod.meta.details.author);
            rtb4.AppendText("\nDescription: " + mod.meta.details.description);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Mod.Modjob mod = MODs.jobs[n];
            mod.script = rtb1.Text;
            MODs.jobs[n] = mod;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Mod.Modjob mod = MODs.jobs[n];
            try
            {
                Scripting.CompileCode(mod.script);
            }
            catch (Exception ex)
            {
                rtb2.Text = ex.Message;
            }
            rtb2.Text = "Compiled Successfully";
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Mod.Modjob mod = MODs.jobs[n];
            try
            {
                Scripting.SetData(mod.data);
                Scripting.Clear(true);
                Thread t = Scripting.RunScriptThreaded(mod.script);
                while (t.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                rtb2.Text = ex.Message;
            }
        }

        private void runSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Mod.Modjob mod = MODs.jobs[n];
            try
            {
                Scripting.SetData(mod.data);
                mod.meta = Mod.MakeMetafromJobXML(mod.xml);
                Scripting.SetMeta(mod.meta);
                Scripting.Clear(true);
                Thread t = Scripting.RunScriptThreaded(mod.script);
                while (t.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                rtb2.Text = ex.Message;
            }
        }

        private void runAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pb1.Maximum = MODs.jobs.Count;
            pb1.Value = 0;
            for (int n = 0; n < MODs.jobs.Count; n++)
            {
                pb1.Value = n;
                Application.DoEvents();
                Mod.Modjob mod = MODs.jobs[n];
                try
                {
                    Scripting.SetData(mod.data);
                    Scripting.Clear(true);
                    Thread t = Scripting.RunScriptThreaded(mod.script);
                    while (t.IsAlive)
                    {
                        Application.DoEvents();
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    rtb2.Text = ex.Message;
                }
            }
            pb1.Value = 0;
        }

        private void appendMODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.daimod|*.daimod";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Mod mod = new Mod();
                mod.Load(d.FileName);
                MODs.jobs.AddRange(mod.jobs);
                RefreshMe();
                MessageBox.Show("Done.");
            }   
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = comboBox1.SelectedIndex;
            if (n == -1 || m == -1)
                return;
            Mod.Modjob mod = MODs.jobs[n];
            hb1.ByteProvider = new DynamicByteProvider(mod.data[m]);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Mod.Modjob mod = MODs.jobs[n];
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.*|*.*";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                byte[] data = File.ReadAllBytes(d.FileName);
                mod.data.Add(data);
                MODs.jobs[n] = mod;
                RefreshMe();
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                MessageBox.Show("Done.");
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = comboBox1.SelectedIndex;
            if (n == -1 || m == -1)
                return;
            Mod.Modjob mod = MODs.jobs[n];
            DialogResult res = MessageBox.Show("Sure?", "Delete", MessageBoxButtons.YesNo);
            if (res == System.Windows.Forms.DialogResult.Yes)
            {
                mod.data.RemoveAt(m);
                MODs.jobs[n] = mod;
                RefreshMe();
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                MessageBox.Show("Done.");
            }
        }

    }
}
