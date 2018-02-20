using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using DAI_Tools.Frostbite;
using Be.Windows.Forms;

namespace DAI_Tools.EBXExplorer
{
    public partial class EBXExplorer : Form
    {
        public bool init = false;
        public bool stop = false;
        public bool ignoreonce = false;

        private String rawXmlViewerStr = "RawXML";
        private String treeXmlViewerStr = "TreeXML";
        private String assetViewerStr = "Assets";
        private String textViewerStr = "Text";
        private EbxRawXmlViewer rawXmlViewer;
        private EbxTreeXmlViewer treeXmlViewer;
        private EbxAssetViewer assetViewer;
        private EbxTextViewer textViewer;
        private Control currentViewer = null;
        private Action<string> statusConsumer;

        public List<Database.EBXEntry> EBXList;
        
        public EBXExplorer(Action<string> statusConsumer)
        {
            this.statusConsumer = statusConsumer;
            
            InitializeComponent();

            rawXmlViewer = new EbxRawXmlViewer();
            treeXmlViewer = new EbxTreeXmlViewer(statusConsumer);
            assetViewer = new EbxAssetViewer(statusConsumer);
            textViewer = new EbxTextViewer();
            viewerSelector.Items.Add(assetViewerStr);
            viewerSelector.Items.Add(rawXmlViewerStr);
            viewerSelector.Items.Add(treeXmlViewerStr);
            viewerSelector.Items.Add(textViewerStr);

            currentViewer = assetViewer;
            splitContainer1.Panel2.Controls.Add(currentViewer);
            treeXmlViewer.Visible = false;
            rawXmlViewer.Visible = false;
            
            viewerSelector.SelectedIndex = 0;

            hideViewer();
        }

        private void EBXExplorer_Activated(object sender, EventArgs e)
        {
            if (!init)
                Init();
        }

        public void Init()
        {
            if (GlobalStuff.FindSetting("isNew") == "1")
            {
                MessageBox.Show("Please initialize the database in Misc > Database with Scan");
                this.BeginInvoke(new MethodInvoker(Close));
                return;
            }
            this.WindowState = FormWindowState.Maximized;
            Application.DoEvents();
            statusConsumer("Querying...");
            EBXList = Database.LoadAllEbxEntries();
            statusConsumer("Making Tree...");
            MakeTree();
            statusConsumer("Done.");
            init = true;
        }

        public void MakeTree()
        {
            treeView1.Nodes.Clear();
            TreeNode t = new TreeNode("EBX");
            foreach (Database.EBXEntry e in EBXList)
                t = AddPath(t, e.path, e.sha1);
            t.Expand();
            treeView1.Nodes.Add(t);
            treeView1.Sort();
        }

        public TreeNode AddPath(TreeNode t, string path, string sha1)
        {
            string[] parts = path.Split('/');
            TreeNode f = null;
            foreach (TreeNode c in t.Nodes)
                if (c.Text == parts[0])
                {
                    f = c;
                    break;
                }
            if (f == null)
            {
                f = new TreeNode(parts[0]);
                if (parts.Length == 1)
                    f.Name = sha1;
                else
                    f.Name = "";
                t.Nodes.Add(f);
            }
            if (parts.Length > 1)
            {
                string subpath = path.Substring(parts[0].Length + 1, path.Length - 1 - parts[0].Length);
                f = AddPath(f, subpath, sha1);
            }
            return t;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try {
                if (ignoreonce)
                {
                    ignoreonce = false;
                    hideViewer();
                }
                else
                {
                    statusConsumer("Loading requested EBX...");

                    TreeNode t = treeView1.SelectedNode;
                    if (t == null || t.Name == "")
                        return;

                    string sha1 = t.Name;
                    byte[] data = Tools.GetDataBySHA1(sha1, GlobalStuff.getCatFile());

                    DAIEbx ebxFile = deserializeEbx(data);
                    setEbxFile(ebxFile);

                    statusConsumer("Done.");
                    showViewer();
            }
            } catch (Exception ex)
            {
                messageBoxOnException(ex);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null)
                t = treeView1.Nodes[0];
            string search = toolStripTextBox1.Text;
            toolStripButton2.Visible = true;
            stop = false;
            while ((t = FindNext(t)) != null)
            {
                Application.DoEvents();
                statusConsumer("Searching : " + GetPath(t) + "...");
                if (stop)
                {
                    statusConsumer("Search stopped.");
                    toolStripButton2.Visible = false;
                    return;
                }
                try
                {
                    byte[] data = Tools.GetDataBySHA1(t.Name, GlobalStuff.getCatFile());
                    if (data.Length != 0)
                    {
                        DAIEbx ebxFile = deserializeEbx(data);
                        string xml = ebxFile.ToXml();

                        if (xml.Contains(search))
                        {
                            ignoreonce = true;
                            treeView1.SelectedNode = t;

                            showViewer();
                            setEbxFile(ebxFile);
                            rawXmlViewer.search(search);

                            statusConsumer("Match!");
                            toolStripButton2.Visible = false;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    messageBoxOnException(ex);
                }
            }
            toolStripButton2.Visible = false;
            statusConsumer("Not found.");
        }

        private string GetPath(TreeNode t)
        {
            if (t.Parent != null && t.Parent.Text != "EBX")
                return GetPath(t.Parent) + "/" + t.Text;
            else
                return t.Text;
        }

        private TreeNode FindNext(TreeNode start)
        {
            TreeNode t = FindNextSub(start);
            if (t != null && t != start)
                return t;
            TreeNode next = start.NextNode;
            if (next != null)
            {
                if (next.Name != "")
                    return next;
                t = FindNext(next);
                if (t != null)
                    return t;
            }
            TreeNode p = start.Parent;
            while (p != null)
            {
                if (p.Parent != null && p.NextNode != null)
                {
                    t = FindNext(p.NextNode);
                    if (t != null)
                        return t;
                }
                p = p.Parent;
            }
            return null;
        }

        private TreeNode FindNextSub(TreeNode start)
        {
            foreach (TreeNode n in start.Nodes)
            {
                TreeNode t = FindNextSub(n);
                if (t != null)
                    return t;
            }
            if (start.Name != "")
                return start;
            else
                return null;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            stop = true;
        }

        private void EBXExplorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            stop = true;
        }

        private void setEbxFile(DAIEbx ebxFile)
        {
            rawXmlViewer.setEbxFile(ebxFile);
            treeXmlViewer.setEbxFile(ebxFile);
            assetViewer.setEbxFile(ebxFile);
            textViewer.setEbxFile(ebxFile);
        }

        private void hideViewer()
        {
            currentViewer.Visible = false;
        }

        private void showViewer()
        {
            currentViewer.Visible = true;
        }

        private void viewerSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selection = (String) viewerSelector.SelectedItem;
            Control newlySelectedViewer;

            if (selection.Equals(treeXmlViewerStr))
                newlySelectedViewer = treeXmlViewer;
            else if (selection.Equals(assetViewerStr))
                newlySelectedViewer = assetViewer;
            else if (selection.Equals(textViewerStr))
                newlySelectedViewer = textViewer;
            else
                newlySelectedViewer = rawXmlViewer;

            if (newlySelectedViewer != this.currentViewer)
            {
                newlySelectedViewer.Visible = currentViewer.Visible;
                currentViewer.Visible = false;

                splitContainer1.Panel2.Controls.Clear();
                splitContainer1.Panel2.Controls.Add(newlySelectedViewer);
                this.currentViewer = newlySelectedViewer;
            }
        }

        private DAIEbx deserializeEbx(byte[] bytes)
        {
            DAIEbx ebxFile = new DAIEbx();
            ebxFile.Serialize(new MemoryStream(bytes));
            return ebxFile;
        }

        private void messageBoxOnException(Exception ex)
        {
            MessageBox.Show("ERROR!\n" + ex.Message + "\n" + ex.StackTrace);
        }
    }
}
