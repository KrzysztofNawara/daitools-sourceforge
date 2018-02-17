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

                /* traverse tree, cache references */
                foreach (var instance in currentEbx.instances)
                {
                    var tnode = processField(instance.Key, instance.Value.data, currentEbx);
                    root.Nodes.Add(tnode);
                }

                treeView1.Nodes.Add(root);
            }
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            foreach (var childNodeObj in e.Node.Nodes)
            {
                var childNode = (TreeNode) childNodeObj; 
                var childTag = (TreeNodeTag) childNode.Tag;
                childTag.expand(childNode, currentEbx);
            }
        }

        private abstract class TreeNodeTag
        {
            public void expand(TreeNode myNode, EbxDataContainers ebx)
            {
                doExpand(myNode, ebx);
                myNode.Tag = null; // deactivate expansion logic
            }

            internal abstract void doExpand(TreeNode myNode, EbxDataContainers ebx);
        }

        private class TNStructTag : TreeNodeTag
        {
            private AStruct astruct;

            public TNStructTag(AStruct astruct) { this.astruct = astruct; }

            internal override void doExpand(TreeNode myNode, EbxDataContainers ebx)
            {
                foreach (var childField in astruct.fields)
                    myNode.Nodes.Add(processField(childField.Key, childField.Value, ebx));
            }
        }
        private class TNArrayTag : TreeNodeTag
        {
            private AArray aarray;

            public TNArrayTag(AArray aarray) { this.aarray = aarray; }

            internal override void doExpand(TreeNode myNode, EbxDataContainers ebx)
            {
                var elements = aarray.elements;
                for(int idx = 0; idx < elements.Count; idx++)
                    myNode.Nodes.Add(processField(idx.ToString(), elements[idx], ebx));
            }
        }

        private class TNDataRootTag : TreeNodeTag
        {
            private string fieldName;
            private AStruct astruct;

            public TNDataRootTag(string fieldName, AStruct astruct)
            {
                this.fieldName = fieldName;
                this.astruct = astruct;
            }

            internal override void doExpand(TreeNode myNode, EbxDataContainers ebx)
            {
                myNode.Nodes.Add(processField(fieldName, astruct, ebx));
            }
        }

        private static TreeNode processField(String fieldName, AValue fieldValue, EbxDataContainers containers)
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
                    var aintref = fieldValue.castTo<AIntRef>();

                    switch (aintref.refStatus) {
                        case RefStatus.UNRESOLVED:
                            throw new Exception("At this point intrefs should be resolved!");
                        case RefStatus.RESOLVED_SUCCESS:
                            tnode = simpleFieldTNode(fieldName, "INTREF");
                            tnode.Tag = new TNDataRootTag(aintref.instanceGuid, containers.instances[aintref.instanceGuid].data);
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
                    tnode.Tag = new TNStructTag(astruct);
                    break;
                case ValueTypes.ARRAY:
                    tnode = new TreeNode(fieldName);
                    tnode.Tag = new TNArrayTag(fieldValue.castTo<AArray>());
                    break;
            }

            return tnode;
        }

        private static TreeNode simpleFieldTNode(String name, String value)
        {
            return new TreeNode(name + ": " + value);
        }

        private void EbxTreeXmlViewer_VisibleChanged(object sender, EventArgs e)
        {
            redrawTree();
        }
    }
}
