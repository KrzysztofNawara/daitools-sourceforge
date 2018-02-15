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
        public EbxAssetViewer()
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
        }

        public void setEbxFile(DAIEbx ebxFile)
        {
            assetList.Rows.Clear();
            
            if (ebxFile != null)
            {
                var ebxDataContainers = EbxDataContainers.fromDAIEbx(ebxFile);
                var assets = ebxDataContainers.getAllWithPartial("Asset");

                foreach (var asset in assets)
                {
                    var assetType = asset.data.name;
                    var assetName = asset.getPartial("Asset").fields["Name"].castTo<ASimpleValue>().Val;

                    assetList.Rows.Add(new string[]{assetType, assetName});
                }
            }
        }
    }
}
