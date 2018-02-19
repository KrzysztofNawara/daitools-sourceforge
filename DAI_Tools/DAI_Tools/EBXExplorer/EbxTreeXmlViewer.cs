using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DAI_Tools.Frostbite;

namespace DAI_Tools.EBXExplorer
{
    public partial class EbxTreeXmlViewer : UserControl
    {
        private EbxDataContainers currentEbx = null;
        private Dictionary<string, TreeNode> guidToTreeNodes = new Dictionary<string, TreeNode>();

        public EbxTreeXmlViewer()
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
            this.Visible = false;
        }

        public void setEbxFile(DAIEbx ebxFile)
        {
            if (ebxFile != null)
                setData(EbxDataContainers.fromDAIEbx(ebxFile));
        }

        public void setData(EbxDataContainers ebxData)
        {
            currentEbx = ebxData;
            redrawTree();
        }

        public void selectByGuid(string guid)
        {
            if (guidToTreeNodes.ContainsKey(guid))
            {
                var tnode = guidToTreeNodes[guid];
                
                if (!tnode.IsExpanded)
                    tnode.Expand();
                
                treeView1.SelectedNode = tnode;
                tnode.BackColor = Color.Yellow;
            }
        }

        private void redrawTree()
        {
            guidToTreeNodes.Clear();
            treeView1.Nodes.Clear();

            if (!Visible)
                return;

            if (currentEbx != null)
            {
                var root = new TreeNode("EBX: " + currentEbx.fileGuid);
                var rootTag = new TNDataRootTag(currentEbx.instances.Values.ToList());
                root.Tag = rootTag;
                rootTag.expand(root, currentEbx, new TreeSettings(flattendChbx.Checked));

                guidToTreeNodes = rootTag.guidToTreeNode;

                treeView1.Nodes.Add(root);
            }
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            foreach (var childNodeObj in e.Node.Nodes)
            {
                var childNode = (TreeNode) childNodeObj;
                var childTag = (TreeNodeTag) childNode.Tag;

                if (childTag != null)
                    childTag.expand(childNode, currentEbx, new TreeSettings(flattendChbx.Checked));
            }
        }

        private class TreeSettings
        {
            public bool flattened = false;

            public TreeSettings(bool flattened)
            {
                this.flattened = flattened;
            }
        }

        private abstract class TreeNodeTag
        {
            public void expand(TreeNode myNode, EbxDataContainers ebx, TreeSettings settings)
            {
                doExpand(myNode, ebx, settings);
                myNode.Tag = null; // deactivate expansion logic
            }

            internal abstract void doExpand(TreeNode myNode, EbxDataContainers ebx, TreeSettings settings);
        }

        private class TNStructTag : TreeNodeTag
        {
            private AStruct astruct;

            public TNStructTag(AStruct astruct) { this.astruct = astruct; }

            internal override void doExpand(TreeNode myNode, EbxDataContainers ebx, TreeSettings settings)
            {
                foreach (var childField in astruct.fields)
                    myNode.Nodes.Add(processField(childField.Key, childField.Value, ebx, settings));
            }
        }
        private class TNArrayTag : TreeNodeTag
        {
            private AArray aarray;

            public TNArrayTag(AArray aarray) { this.aarray = aarray; }

            internal override void doExpand(TreeNode myNode, EbxDataContainers ebx, TreeSettings settings)
            {
                var elements = aarray.elements;
                for(int idx = 0; idx < elements.Count; idx++)
                    myNode.Nodes.Add(processField(idx.ToString(), elements[idx], ebx, settings));
            }
        }

        private class TNDataRootTag : TreeNodeTag
        {
            public Dictionary<string, TreeNode> guidToTreeNode { get; }
            private List<DataContainer> containers;

            public TNDataRootTag(List<DataContainer> containers)
            {
                this.containers = containers;
                this.guidToTreeNode = new Dictionary<string, TreeNode>();
            }

            internal override void doExpand(TreeNode myNode, EbxDataContainers ebx, TreeSettings settings)
            {
                foreach (var container in containers)
                {
                    AStruct dataRoot = settings.flattened ? container.getFlattenedData() : container.data;
                    
                    var tnode = processField(container.guid, dataRoot, ebx, settings);
                    myNode.Nodes.Add(tnode);
                    guidToTreeNode.Add(container.guid, tnode);
                }
            }
        }

        private static TreeNode processField(String fieldName, AValue fieldValue, EbxDataContainers containers, TreeSettings settings)
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
                            var singletonContainerList = new List<DataContainer>();
                            singletonContainerList.Add(containers.instances[aintref.instanceGuid]);
                            tnode.Tag = new TNDataRootTag(singletonContainerList);
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

        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (treeView1.SelectedNode != null)
                treeView1.SelectedNode.BackColor = Color.White;
        }

        private void flattendChbx_CheckedChanged(object sender, EventArgs e)
        {
            redrawTree();
        }
    }
}
