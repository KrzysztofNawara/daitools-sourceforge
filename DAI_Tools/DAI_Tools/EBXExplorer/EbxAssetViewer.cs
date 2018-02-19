using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAI_Tools.Frostbite;

namespace DAI_Tools.EBXExplorer
{
    public partial class EbxAssetViewer : UserControl
    {
        private EbxDataContainers currentContainers = null;
        private DataContainer currentlySelectedAsset = null;
        
        public EbxAssetViewer()
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
            assetList.AllowUserToAddRows = false;
        }

        public void setEbxFile(DAIEbx ebxFile)
        {
            assetList.Rows.Clear();
            currentContainers = null;
            currentlySelectedAsset = null;
            graphVizButton.Enabled = false;
            
            if (ebxFile != null)
            {
                currentContainers = EbxDataContainers.fromDAIEbx(ebxFile);
                var assets = currentContainers.getAllWithPartial("Asset");

                foreach (var asset in assets)
                {
                    var assetType = asset.data.name;
                    var assetName = asset.getPartial("Asset").fields["Name"].castTo<ASimpleValue>().Val;

                    assetList.Rows.Add(new string[]{assetType, assetName, asset.guid});
                }
            }
        }

        private void assetList_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (currentContainers != null && assetList.SelectedRows.Count > 0)
            {
                String selectedAssetGuid = (String) assetList.SelectedRows[0].Cells["assetGuid"].Value;
                
                if (selectedAssetGuid != null && selectedAssetGuid.Length > 0)
                {
                    var selectedAsset = currentContainers.instances[selectedAssetGuid];
                    partialsLabel.Text = String.Join(" -> ", selectedAsset.getAllPartials());
                    currentlySelectedAsset = selectedAsset;

                    if (currentlySelectedAsset.hasPartial("UIGraphAsset"))
                        graphVizButton.Enabled = true;
                    else if (currentlySelectedAsset.hasPartial("PrefabBlueprint"))
                        blueprintVizButton.Enabled = true;
                }
            }
        }

        private void assetList_RowLeave(object sender, DataGridViewCellEventArgs e)
        {
            partialsLabel.Text = "";
            currentlySelectedAsset = null;
            graphVizButton.Enabled = false;
            blueprintVizButton.Enabled = false;
        }

        private void graphVizButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(currentContainers != null);
            Debug.Assert(currentlySelectedAsset != null);
            Debug.Assert(currentlySelectedAsset.hasPartial("UIGraphAsset"));

            new UIGraphAssetViz(currentContainers, currentlySelectedAsset.guid).Show();
        }

        private void blueprintVizButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(currentContainers != null);
            Debug.Assert(currentlySelectedAsset != null);
            Debug.Assert(currentlySelectedAsset.hasPartial("PrefabBlueprint"));

            new BlueprintViz(currentContainers, currentlySelectedAsset.guid).Show();
        }
    }
}
