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
                    processEbxTree(wrapWithFakeField(instanceGuid, instance.Value), root);
                }

                treeView1.Nodes.Add(root);
            }
        }

        private DAIField wrapWithFakeField(String fieldName, DAIComplex value)
        {
            var fakeField = new DAIField();
            fakeField.ValueType = DAIFieldType.DAI_Complex;
            fakeField.Descriptor = new DAIFieldDescriptor();
            fakeField.Descriptor.FieldName = fieldName;
            fakeField.ComplexValue = value;
            return fakeField;
        }

        /**
         * Processes passed field, attaches new node for that field
         * For each simple value attaches node for it
         * For each complex value simply calls recursively processEbxTree - it'll handle adding node for DAIComplex
         */
        private void processEbxTree(DAIField field, TreeNode parentNode)
        {

        }
    }
}
