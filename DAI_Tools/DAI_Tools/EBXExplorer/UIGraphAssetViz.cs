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
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.MDS;
using Color = Microsoft.Msagl.Drawing.Color;

namespace DAI_Tools.EBXExplorer
{
    public partial class UIGraphAssetViz : Form
    {
        public UIGraphAssetViz(EbxDataContainers ebxDataContainers, string assetGuid)
        {
            this.ebxDataContainers = ebxDataContainers;
            this.assetGuid = assetGuid;
            InitializeComponent();
        }

        private EbxDataContainers ebxDataContainers;
        private string assetGuid;

        private void UIGraphAssetViz_Load(object sender, EventArgs e)
        {
            try {
                //create a viewer object 
                Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();

                //create a graph object 
                Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");

                //create the graph content 
                configureGraph(graph);

                //bind the graph to the viewer 
                viewer.Graph = graph;

                //associate the viewer with the form 
                this.SuspendLayout();
                viewer.Dock = System.Windows.Forms.DockStyle.Fill;
                this.Controls.Add(viewer);
                this.ResumeLayout();
            } catch (Exception ex)
            {
                MessageBox.Show("Exception:\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private class PortDesc
        {
            public PortDesc(int portIdx, string portName, string nodeLabel)
            {
                this.portIdx = portIdx;
                this.portName = portName;
                this.nodeLabel = nodeLabel;
            }

            public int refCount = 0;
            public int portIdx;
            public string portName;
            public string nodeLabel;
        }

        private void configureGraph(Graph graph)
        {
            var uiGraphAsset = ebxDataContainers.instances[assetGuid];
            AArray nodes = uiGraphAsset.data.get("Nodes").castTo<AArray>();
            var portsGuidToPortDesc = new Dictionary<string, PortDesc>();

            int nodeNextIdx = 0;
            int portNextIdx = 0;

            foreach (var nodeRef in nodes.elements)
            {
                var nodeInRef = nodeRef.castTo<AIntRef>();
                var nodeName = nodeInRef.refTarget.castTo<AStruct>().get("Name").castTo<ASimpleValue>().Val;
                var nodeType = ebxDataContainers.instances[nodeInRef.instanceGuid].data.name;
                var nodeLabel = "N" + nodeNextIdx.ToString() + ": " + nodeName + "\n[" + nodeType + "]";
                nodeNextIdx += 1;
                var nodeNode = graph.AddNode(nodeLabel);
                
                var ports = ebxDataContainers.getIntRefedObjsByTypeFor(nodeInRef.instanceGuid, "UINodePort");

                foreach (var dataContainer in ports)
                {
                    var portName = dataContainer.data.get("Name").castTo<ASimpleValue>().Val;
                    var portDesc = new PortDesc(portNextIdx, portName, nodeLabel);
                    portNextIdx += 1;

                    if (!portsGuidToPortDesc.ContainsKey(dataContainer.guid))
                        portsGuidToPortDesc.Add(dataContainer.guid, portDesc);
                }

                /* some visual formatting */
                nodeNode.Attr.LabelMargin = 3;
                nodeNode.Attr.Padding = 2;
                nodeNode.Attr.FillColor = Color.LightGreen;
                nodeNode.Attr.Shape = Shape.Box;
            }

            var connections = uiGraphAsset.data.get("Connections").castTo<AArray>();

            foreach (var connRef in connections.elements)
            {
                var conn = connRef.castTo<AIntRef>().refTarget.castTo<AStruct>();
                var srcGuid = conn.get("SourcePort").castTo<AIntRef>().instanceGuid;
                var targetGuid = conn.get("TargetPort").castTo<AIntRef>().instanceGuid;
                var srcPortDesc = portsGuidToPortDesc[srcGuid];
                var targetPortDesc = portsGuidToPortDesc[targetGuid];

                var connLabel = "";
                if (srcPortDesc.portName.Length > 0 || targetPortDesc.portName.Length > 0)
                    connLabel = srcPortDesc.portName + " -> " + targetPortDesc.portName;

                graph.AddEdge(srcPortDesc.nodeLabel, connLabel, targetPortDesc.nodeLabel);

                srcPortDesc.refCount += 1;
                targetPortDesc.refCount += 1;
            }

            foreach (var portDesc in portsGuidToPortDesc.Values)
            {
                if (portDesc.refCount < 1)
                {
                    var portLabel = "P" + portDesc.portIdx + ": " + portDesc.portName;
                    var portNode = graph.AddNode(portLabel);

                    var portEdge = graph.AddEdge(portDesc.nodeLabel, "", portLabel);

                    /* visual formatting */
                    portNode.Attr.FillColor = Color.Orange;
                    portNode.Attr.Shape = Shape.Box;
                    portEdge.Attr.ArrowheadAtSource = ArrowStyle.None;
                    portEdge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                }
            }
            
            /* some visual formatting */
            var layoutSettings = new MdsLayoutSettings();
            graph.LayoutAlgorithmSettings = layoutSettings;
        }
    }
}
