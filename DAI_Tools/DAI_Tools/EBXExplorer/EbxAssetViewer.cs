using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
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
        
        public EbxAssetViewer()
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
        }

        public void setEbxFile(DAIEbx ebxFile)
        {
            assetList.Rows.Clear();
            currentContainers = null;
            
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
                }
            }
        }

        private void assetList_RowLeave(object sender, DataGridViewCellEventArgs e)
        {
            partialsLabel.Text = "";
        }
    }
}
