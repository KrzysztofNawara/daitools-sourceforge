using System;
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
                var ebx = EbxDataContainers.fromDAIEbx(ebxFile);
                var root = new TreeNode("EBX: " + ebx.fileGuid);
                var tbc = new TreeBuilderContext();
                tbc.containers = ebx;

                /* traverse tree, cache references */
                foreach (var instance in ebx.instances)
                {
                    var tnode = processField(instance.Key, instance.Value.data, tbc);
                    root.Nodes.Add(tnode);
                }

                treeView1.Nodes.Add(root);
            }
        }

        private class TreeBuilderContext
        {
            public EbxDataContainers containers;
        }

        private TreeNode processField(String fieldName, AValue fieldValue, TreeBuilderContext tbc)
        {
            TreeNode tnode = null;

            switch (fieldValue.Type)
            {
                case ValueTypes.SIMPLE:
                    tnode = simpleFieldTNode(fieldName, fieldValue.castTo<ASimpleValue>().Val);
                    break;
                case ValueTypes.NULL_REF:
                    tnode = simpleFieldTNode(fieldName, "[null]");
                    break;
                case ValueTypes.IN_REF:
                    tnode = simpleFieldTNode(fieldName, fieldValue.castTo<AIntRef>().instanceGuid);
                    break;
                case ValueTypes.EX_REF:
                    var aexref = fieldValue.castTo<AExRef>();
                    tnode = simpleFieldTNode(fieldName, aexref.fileGuid + " | " + aexref.instanceGuid);
                    break;
                case ValueTypes.STRUCT:
                    var astruct = fieldValue.castTo<AStruct>();
                    var tnodeText = fieldName + " -> " + astruct.name;
                    tnode = new TreeNode(tnodeText);

                    foreach (var childField in astruct.fields)
                    {
                        var childTNode = processField(childField.Key, childField.Value, tbc);
                        tnode.Nodes.Add(childTNode);
                    }

                    break;
                case ValueTypes.ARRAY:
                    tnode = new TreeNode(fieldName);
                    var childElements = fieldValue.castTo<AArray>().elements;
                    for(int idx = 0; idx < childElements.Count; idx++)
                    {
                        var childTNode = processField(idx.ToString(), childElements[idx], tbc);
                        tnode.Nodes.Add(childTNode);
                    } 
                    break;
            }

            return tnode;
        }

        private TreeNode simpleFieldTNode(String name, String value)
        {
            return new TreeNode(name + ": " + value);
        }
    }
}
