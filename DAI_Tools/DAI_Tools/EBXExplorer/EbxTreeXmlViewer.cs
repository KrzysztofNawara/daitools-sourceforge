using System;
using System.Windows.Forms;
using DAI_Tools.Frostbite;

namespace DAI_Tools.EBXExplorer
{
    public partial class EbxTreeXmlViewer : UserControl
    {
        private EbxDataContainers currentEbx = null;        

        public EbxTreeXmlViewer()
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
            this.Visible = false;
        }

        public void setEbxFile(DAIEbx ebxFile)
        {
            if (ebxFile != null)
            {
                currentEbx = EbxDataContainers.fromDAIEbx(ebxFile);
                redrawTree();
            }
        }

        private void redrawTree()
        {
            treeView1.Nodes.Clear();

            if (!Visible)
                return;

            if (currentEbx != null)
            {
                var root = new TreeNode("EBX: " + currentEbx.fileGuid);
                var tbc = new TreeBuilderContext();
                tbc.containers = currentEbx;
                tbc.intRefMaxDepth = Decimal.ToUInt32(intRefMaxDepth.Value);

                /* traverse tree, cache references */
                foreach (var instance in currentEbx.instances)
                {
                    var tnode = processField(instance.Key, instance.Value.data, 0, tbc);
                    root.Nodes.Add(tnode);
                }

                treeView1.Nodes.Add(root);
            }
        }

        private class TreeBuilderContext
        {
            public EbxDataContainers containers;
            public uint intRefMaxDepth;
        }

        private TreeNode processField(String fieldName, AValue fieldValue, uint intRefDepthCounter, TreeBuilderContext tbc)
        {
            if (intRefDepthCounter >= tbc.intRefMaxDepth)
            {
                return new TreeNode("IntRef limit exceeded");
            }
            
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
                    var aintref = fieldValue.castTo<AIntRef>();

                    switch (aintref.refStatus) {
                        case RefStatus.UNRESOLVED:
                            throw new Exception("At this point intrefs should be resolved!");
                        case RefStatus.RESOLVED_SUCCESS:
                            tnode = simpleFieldTNode(fieldName, "INTREF");
                            var target = tbc.containers.instances[aintref.instanceGuid];
                            var childTNode = processField(target.guid, target.data, intRefDepthCounter+1, tbc);
                            tnode.Nodes.Add(childTNode);
                            break;
                        case RefStatus.RESOLVED_FAILURE:
                            tnode = simpleFieldTNode(fieldName, "Unresolved INTREF: " + aintref.instanceGuid);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

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
                        var childTNode = processField(childField.Key, childField.Value, intRefDepthCounter, tbc);
                        tnode.Nodes.Add(childTNode);
                    }

                    break;
                case ValueTypes.ARRAY:
                    tnode = new TreeNode(fieldName);
                    var childElements = fieldValue.castTo<AArray>().elements;
                    for(int idx = 0; idx < childElements.Count; idx++)
                    {
                        var childTNode = processField(idx.ToString(), childElements[idx], intRefDepthCounter, tbc);
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

        private void intRefMaxDepth_ValueChanged(object sender, EventArgs e)
        {
            redrawTree();
        }

        private void EbxTreeXmlViewer_VisibleChanged(object sender, EventArgs e)
        {
            redrawTree();
        }
    }
}
