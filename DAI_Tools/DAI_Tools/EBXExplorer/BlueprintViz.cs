using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAI_Tools.Frostbite;

namespace DAI_Tools.EBXExplorer
{
    public partial class BlueprintViz : Form
    {
        private EbxDataContainers ebxContainers;
        private string assetGuid;
        
        public BlueprintViz(EbxDataContainers ebxContainers, string assetGuid)
        {
            this.ebxContainers = ebxContainers;
            this.assetGuid = assetGuid;
            
            InitializeComponent();
        }
    }
}
