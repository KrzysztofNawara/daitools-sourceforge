using System;
using System.Windows.Forms;
using DAI_Tools.Frostbite;

namespace DAI_Tools.EBXExplorer
{
    public partial class EbxTreeXmlViewer : UserControl
    {
        public static uint MAX_INT_REF_DEPTH = 3; 
        
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
                    var tnode = processField(instance.Key, instance.Value.data, 0, tbc);
                    root.Nodes.Add(tnode);
                }

                treeView1.Nodes.Add(root);
            }
        }

        private class TreeBuilderContext
        {
            public EbxDataContainers containers;
        }

        private TreeNode processField(String fieldName, AValue fieldValue, uint intRefDepthCounter, TreeBuilderContext tbc)
        {
            if (intRefDepthCounter >= MAX_INT_REF_DEPTH)
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
    }
}
