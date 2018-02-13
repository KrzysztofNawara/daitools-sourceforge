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
    public partial class EbxTreeXmlViewer : UserControl
    {
        public EbxTreeXmlViewer()
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
        }

        public void setEbxFile(DAIEbx ebxFile)
        {
            treeView1.Nodes.Clear();

            if (ebxFile != null)
            {
                var fileGuid = DAIEbx.GuidToString(ebxFile.FileGuid);
                var root = new TreeNode("EBX: " + fileGuid);

                foreach (var instance in ebxFile.Instances)
                {
                    var instanceGuid = DAIEbx.GuidToString(instance.Key);

                    var instanceParentNode = new TreeNode(instanceGuid);
                    root.Nodes.Add(instanceParentNode);
                    
                    processEbxTree(instance.Value, instanceParentNode);
                }

                treeView1.Nodes.Add(root);
            }
        }

        /**
         * Processes passed ebxRoot and schedules processing for all children nodes of complex type
         */
        private void processEbxTree(DAIComplex ebxRoot, TreeNode parentNode)
        {
            
        }
    }
}
