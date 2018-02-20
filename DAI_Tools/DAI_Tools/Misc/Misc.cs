using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using DAI_Tools.Frostbite;
using DAI_Tools.Search;

namespace DAI_Tools.DBManager
{
    public partial class DBManager : Form
    {
        public DBManager()
        {
            InitializeComponent();
        }

        private void Log(string s)
        {
            rtb1.AppendText(s + "\n");
            rtb1.SelectionStart = rtb1.Text.Length;
            rtb1.ScrollToCaret();
            Application.DoEvents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to reset database?", "Clear", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                Database.CreateDataBase();
                Database.LoadSettings();
                MessageBox.Show("Done.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            switch (combo1.SelectedIndex)
            {
                case 0:
                    Database.LoadSettings();
                    StringBuilder sb = new StringBuilder();
                    foreach (KeyValuePair<string, string> setting in GlobalStuff.settings)
                        sb.AppendFormat("{0} = {1}\n", setting.Key, setting.Value);
                    rtb1.Text = sb.ToString();
                    break;
                case 1:
                    ListPaths("sbfiles", con);
                    break;
                case 2:
                    ListPaths("langsbfiles", con);
                    break;
                case 3:
                    ListPaths("tocfiles", con);
                    break;
                case 4:
                    ListPaths("langtocfiles", con);
                    break;
                case 5:
                    ListPaths("casfiles", con);
                    break;
                case 6:
                    ListBundles( con);
                    break;
                case 7:
                    ListEbxTypes(con);
                    break;
                case 8:
                    SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type = 'table'", con);
                    SQLiteDataReader reader = command.ExecuteReader();
                    sb = new StringBuilder();
                    List<string> tablenames = new List<string>();
                    while (reader.Read())
                        tablenames.Add(reader.GetString(0));
                    foreach (string table in tablenames)
                    {
                        sb.Append("Definition for table : " + table + "\n");
                        sb.Append(CustomQuery("PRAGMA table_info(" + table + ")") + "\n");
                    }
                    rtb1.Text = sb.ToString();
                    break;
            }
            con.Close();
        }

        private void ListEbxTypes(SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT DISTINCT type FROM ebx ORDER BY type", con);
            SQLiteDataReader reader = command.ExecuteReader();
            StringBuilder sb = new StringBuilder();
            int counter = 0;
            while (reader.Read())
                if (reader.FieldCount > 0)
                    sb.AppendFormat("{0} = {1}\n", counter++, reader.GetString(0));
            rtb1.Text = sb.ToString();
        }

        private void ListBundles(SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM bundles ORDER BY frostpath", con);
            SQLiteDataReader reader = command.ExecuteReader();
            StringBuilder sb = new StringBuilder();
            int counter = 0;
            while (reader.Read())
                sb.AppendFormat("{0} = {1}\n", counter++, reader.GetString(2));
            rtb1.Text = sb.ToString();
        }

        private void ListPaths(string tablename, SQLiteConnection con)
        {
            SQLiteDataReader reader = Database.getAll(tablename, con);
            StringBuilder sb = new StringBuilder();
            int counter = 0;
            while (reader.Read())
                sb.AppendFormat("{0} = {1}\n", counter++, reader.GetString(0));
            rtb1.Text = sb.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (GlobalStuff.FindSetting("isNew") != "1")
            {
                MessageBox.Show("Database was already filled with data, please clear it before scan.");
                return;
            }
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "DragonAgeInquisition.exe|DragonAgeInquisition.exe";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                StartScan(Path.GetDirectoryName(d.FileName) + "\\");
        }

        private void StartScan(string basepath)
        {
            rtb1.Text = "";
            Log("Starting Scan...");
            Mod.GetOrSetAuthor();
            Stopwatch sp = new Stopwatch();
            sp.Start();
            GlobalStuff.AssignSetting("isNew", "0");
            GlobalStuff.settings.Add("gamepath", basepath);
            Database.SaveSettings();
            CATFile cat = ScanCAT();
            ScanFiles();
            ScanBundles(cat);
            sp.Stop();
            Log("\n\n===============\nTime : " + sp.Elapsed.ToString() +"\nDone.");
            MessageBox.Show("Done.");
        }

        private CATFile ScanCAT()
        {
            Log("Loading CAT...");            
            string path = GlobalStuff.FindSetting("gamepath") + "Data\\cas.cat";
            CATFile cat = new CATFile(path);
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            var transaction = con.BeginTransaction();
            Database.ClearSHA1db(con);
            int counter = 0;
            Log("Saving sha1s into db...");
            foreach (uint[] line in cat.lines)
            {
                Database.AddSHA1(line, con);
                if ((counter % 100000) == 0)
                {
                    rtb1.AppendText(counter + "/" + cat.lines.Count + "\n");
                    rtb1.SelectionStart = rtb1.Text.Length;
                    rtb1.ScrollToCaret();
                    
                    transaction.Commit();
                    transaction = con.BeginTransaction();
                }
                counter++;
            }
            Log("Saving chunk ids into db...");
            foreach (CATFile.ChunkType c in cat.chunks)
                Database.AddChunk(c.id, c.sha1, con);
            transaction.Commit();
            con.Close();
            return cat;
        }

        private void ScanFiles()
        {
            Log("Saving file paths into db...");
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            var transaction = con.BeginTransaction();
            string[] files = Directory.GetFiles(GlobalStuff.FindSetting("gamepath"), "*.sb", SearchOption.AllDirectories);
            Log("SB files...");
            foreach (string file in files)
                if (!file.ToLower().Contains("\\update\\"))//exlude patches
                {
                    if (!file.Contains("\\loc\\") &&
                        !file.Contains("\\locfacefx\\") &&
                        !file.Contains("\\loctext\\"))
                        Database.AddSBFile(file, con);
                    else
                        Database.AddLanguageSBFile(file, con);
                }
            transaction.Commit();
            transaction = con.BeginTransaction();
            Log("TOC files...");
            files = Directory.GetFiles(GlobalStuff.FindSetting("gamepath"), "*.toc", SearchOption.AllDirectories);
            foreach (string file in files)
                if (!file.ToLower().Contains("\\update\\"))//exlude patches
                {
                    if (!file.Contains("\\loc\\") &&
                        !file.Contains("\\locfacefx\\") &&
                        !file.Contains("\\loctext\\"))
                        Database.AddTOCFile(file, con);
                    else
                        Database.AddLanguageTOCFile(file, con);
                }
            transaction.Commit();
            transaction = con.BeginTransaction();
            Log("CAS files...");
            files = Directory.GetFiles(GlobalStuff.FindSetting("gamepath"), "*.cas", SearchOption.AllDirectories);
            foreach (string file in files)
                Database.AddCASFile(file, con);
            transaction.Commit();
            con.Close();
        }

        private void ScanBundles(CATFile cat)
        {
            Log("Saving bundles into db...");
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            SQLiteDataReader reader = Database.getAll("sbfiles", con);
            StringBuilder sb = new StringBuilder();
            List<string> files = new List<string>();
            while (reader.Read())
                files.Add(reader.GetString(0));
            int counter = 1;
            Stopwatch sp = new Stopwatch();
            sp.Start();
            foreach (string file in files)
            {
                Log("Opening " + file + " ...");
                SBFile sbfile = new SBFile(file);
                Log(" found " + sbfile.bundles.Count + " bundles, saving to db...");
                var transaction = con.BeginTransaction();
                int counter2 = 1;
                foreach (Bundle b in sbfile.bundles)
                {
                    if (b.ebx == null)
                        b.ebx = new List<Bundle.ebxtype>();
                    Log("  processing bundle: " + (counter2++) + "/" + sbfile.bundles.Count + " \"" + b.path + "\" (ebxcount = " + b.ebx.Count + ") ...");
                    Database.AddBundle(file, b, con, cat);                    
                }
                transaction.Commit();
                long elapsed = sp.ElapsedMilliseconds;
                long ETA = ((elapsed / counter) * files.Count);
                TimeSpan ETAt = TimeSpan.FromMilliseconds(ETA);
                Log((counter++) + "/" + files.Count + " files done." + " - Elapsed: " + sp.Elapsed.ToString() + " ETA: " + ETAt.ToString());
            }
            con.Clone();
        }

        private void DBManager_Activated(object sender, EventArgs e)
        {
            combo1.Items.Clear();
            combo1.Items.Add("settings");
            combo1.Items.Add("sb files");
            combo1.Items.Add("language sb files");
            combo1.Items.Add("toc files");
            combo1.Items.Add("language toc files");
            combo1.Items.Add("cas files");
            combo1.Items.Add("bundles");
            combo1.Items.Add("ebx types");
            combo1.Items.Add("tables");
            combo1.SelectedIndex = 0;
        }

        private string CustomQuery(string query)
        {
            StringBuilder sb = new StringBuilder();
            Application.DoEvents();
            try
            {
                SQLiteConnection con = Database.GetConnection();
                con.Open();
                SQLiteCommand command = new SQLiteCommand(query, con);
                SQLiteDataReader reader = command.ExecuteReader();
                int counter = 0;
                while (reader.Read())
                {
                    sb.Append(counter++ + " : ");
                    for (int i = 0; i < reader.FieldCount; i++)
                        sb.Append(reader.GetValue(i).ToString() + ", ");
                    sb.Append("\n");
                }
                con.Clone();
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                rtb1.Text = "Querying...";
                rtb1.Text = CustomQuery(textBox1.Text);
            }
        }

        private void exportAllButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            if (d.ShowDialog() == DialogResult.OK)
            {
                foreach (var ebxEntry in Database.LoadAllEbxEntries())
                {
                    var bytes = Tools.GetDataBySHA1(ebxEntry.sha1, GlobalStuff.getCatFile());
                    var daiEbx = new DAIEbx();
                    daiEbx.Serialize(new MemoryStream(bytes));
                    var ebxContainers = EbxDataContainers.fromDAIEbx(daiEbx, str => {});
                    var txt = ebxContainers.toText();

                    var outPath = Path.Combine(d.SelectedPath, ebxEntry.path);
                    var dir = Path.GetDirectoryName(outPath);
                    Directory.CreateDirectory(dir);
                    File.WriteAllText(outPath, txt, Encoding.UTF8);
                }
            }
        }
    }
}
