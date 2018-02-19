using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAI_Tools.EBXExplorer;
using DAI_Tools.Frostbite;

namespace DAI_Tools
{
    public partial class Frontend : Form
    {
        public bool init = false;
        public AboutBox box;
        private Action<string> statusConsumer;

        public Frontend()
        {
            InitializeComponent();
            
        }

        private void Frontend_Load(object sender, EventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "DAI Tools by Warranty Voider, Ehamloptiran, Wogoodes and more... Version : " + version;
            
            statusConsumer = newStatus => updateStatus(newStatus);
            updateStatus("Initializing...");
        }

        private void SetStatus(string s)
        {
            status.Text = "Status : " + s;
        }

        private void Init()
        {
            init = true;
            int steps = 0;
            try
            {
                Database.CheckIfDBExists(); steps++;
                Database.LoadSettings(); steps++;
                Database.CheckIfScanIsNeeded(); steps++;
            }
            catch (Exception ex)
            {
                switch(steps)
                { 
                    case 0:
                        SetStatus("Error on finding/creating database");                        
                        break;
                    case 1:
                        SetStatus("Error on loading settings");
                        break;
                    case 2:
                        SetStatus("Error on scanning");
                        break;
                    default:
                        SetStatus("Error on initializing!");
                        break;
                }
                return;
            }
            SetStatus("Ready");
            menuStrip1.Visible = true;
        }



        private void Frontend_Activated(object sender, EventArgs e)
        {
            if (!init)
                Init();
        }

        private void databaseManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new DBManager.DBManager());
        }

        private void OpenMaximized(Form f)
        {
            f.MdiParent = this;
            f.Show();
            f.WindowState = FormWindowState.Maximized;
        }

        private void bundleBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new BundleBrowser.BundleBrowser());
        }

        private void soundExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new SoundExplorer.SoundExplorer());
        }

        private void textureExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new TextureExplorer.TextureExplorer());
        }

        private void eBXExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new EBXExplorer.EBXExplorer(statusConsumer));
        }

        private void scriptExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new ScriptExplorer.ScriptExplorer());
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (box != null)
                box.Close();
            box = new AboutBox();
            box.Show();
        }

        private void modScriptToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new ModScriptTool.ModScriptTool());
        }

        private void talktableExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new TalktableExplorer.TalktableExplorer());
        }

        private void shaderExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new ShaderExplorer.ShaderExplorer());
        }

        private void updateStatus(String newStatus)
        {
            status.Text = newStatus;
            statusStrip.Refresh();
        }

        private static string hudCombinedHUDGuid = "919EA7A5E1FA3911DB5137B4482C0D7BC42851F7";
        private static string popUpEventsPrefabGuid = "6E712022A2DE2A7C71A9EAC55D585547E10BEAF6";
        private static string damageEffectPrefabGuid = "E78139132A608C0339BC7F04F121420E4BF117FA";
        private static string uiGameLogicPrefabGuid = "654D92C5C09870BC591C834DE0EBC0B7AE210CBB";

        private void uiGraphVizButton_Click(object sender, EventArgs e)
        {
            var containers = loadEbx(hudCombinedHUDGuid);
            var assetGuid = findAsset(containers, "UIGraphAsset");
            new UIGraphAssetViz(containers, assetGuid).Show();
        }

        private EbxDataContainers loadEbx(string ebxGuid)
        {
            string path = GlobalStuff.FindSetting("gamepath");
            path += "Data\\cas.cat";
            var cat = new CATFile(path);
            
            byte[] data = Tools.GetDataBySHA1(ebxGuid, cat);

            DAIEbx ebxFile = new DAIEbx();
            ebxFile.Serialize(new MemoryStream(data));
            var containers = EbxDataContainers.fromDAIEbx(ebxFile, statusConsumer);

            return containers;
        }

        private string findAsset(EbxDataContainers containers, string type)
        {
            foreach (var instance in containers.instances)
                if (instance.Value.hasPartial(type))
                    return instance.Key;

            return null;
        }

        private void popUpEventToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var containers = loadEbx(popUpEventsPrefabGuid);
            var assetGuid = findAsset(containers, "LogicPrefabBlueprint");
            new BlueprintViz(containers, assetGuid, statusConsumer).Show();
        }

        private void damageLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var containers = loadEbx(damageEffectPrefabGuid);
            var assetGuid = findAsset(containers, "LogicPrefabBlueprint");
            new BlueprintViz(containers, assetGuid, statusConsumer).Show();
        }

        private void uiGameLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var containers = loadEbx(uiGameLogicPrefabGuid);
            var assetGuid = findAsset(containers, "LogicPrefabBlueprint");
            new BlueprintViz(containers, assetGuid, statusConsumer).Show();
        }
    }
}
