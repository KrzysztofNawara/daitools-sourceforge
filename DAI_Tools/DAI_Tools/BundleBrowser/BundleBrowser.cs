using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;
using DAI_Tools.Frostbite;

namespace DAI_Tools.BundleBrowser
{
    public partial class BundleBrowser : Form
    {
        public bool init = false;
        public List<int> ids;

        public BundleBrowser()
        {
            InitializeComponent();
        }

        public void Init()
        {
            if (GlobalStuff.FindSetting("isNew") == "1")
            {
                MessageBox.Show("Please initialize the database in Misc > Database with Scan");
                this.BeginInvoke(new MethodInvoker(Close));
            }
            ids = new List<int>();
            listBox1.Items.Clear();
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            SQLiteDataReader reader = Database.getAllSorted("bundles", "frostpath", con);
            while (reader.Read())
            {
                listBox1.Items.Add(reader.GetString(2) + " (" + reader.GetInt32(3) + "/" + reader.GetInt32(4) + "/" + reader.GetInt32(5) + ")");
                ids.Add(reader.GetInt32(0));
            }
            con.Close();
            init = true;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            int id = ids[n];
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            SQLiteDataReader reader = Database.getAllWhere("ebx", "bundle = " + id + " ORDER BY name", con);
            while (reader.Read())
                listBox2.Items.Add(reader.GetString(0) + " (" + reader.GetString(3) + ")");
            reader = Database.getAllWhere("res", "bundle = " + id + " ORDER BY name", con);
            while (reader.Read())
                listBox3.Items.Add(reader.GetString(0));
            reader = Database.getAllWhere("chunk", "bundle = " + id + " ORDER BY id", con);
            while (reader.Read())
                listBox4.Items.Add(reader.GetString(0) + " : " + reader.GetString(1));
            con.Close();
        }

        private void BundleBrowser_Load(object sender, EventArgs e)
        {
            if (!init)
                Init();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || listBox2.SelectedIndex == -1)
                return;
            int id = ids[n];
            string ebxname = listBox2.SelectedItem.ToString();
            int t = ebxname.IndexOf(':');
            ebxname = ebxname.Substring(t + 1, ebxname.Length - t - 1);
            t = ebxname.IndexOf(" (");
            ebxname = ebxname.Substring(0, t);
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            SQLiteDataReader reader = Database.getAllWhere("ebx", "bundle = " + id + " AND name ='" + ebxname + "'", con);
            if (!reader.Read())
                return;
            string sha1 = reader.GetString(1);
            byte[] buff = Database.getDataBySHA1(sha1, con);
            try
            {
                rtb1.Text = Encoding.UTF8.GetString(Tools.ExtractEbx(new MemoryStream(buff)));
                rtb1.BringToFront();
            }
            catch (Exception)
            {
            }
            con.Close();
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || listBox3.SelectedIndex == -1)
                return;
            int id = ids[n];
            string resname = listBox3.SelectedItem.ToString();
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            SQLiteDataReader reader = Database.getAllWhere("res", "bundle = " + id + " AND name ='" + resname + "'", con);
            if (!reader.Read())
                return;
            string sha1 = reader.GetString(1);
            hb1.BringToFront();
            hb1.ByteProvider = new DynamicByteProvider(Database.getDataBySHA1(sha1, con));
            con.Close();
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || listBox4.SelectedIndex == -1)
                return;
            int id = ids[n];
            string chunkname = listBox4.SelectedItem.ToString();
            string sha1 = chunkname.Split(':')[1].Trim();
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            hb1.BringToFront();
            hb1.ByteProvider = new DynamicByteProvider(Database.getDataBySHA1(sha1, con));
            con.Close();
        }

        
    }
}
