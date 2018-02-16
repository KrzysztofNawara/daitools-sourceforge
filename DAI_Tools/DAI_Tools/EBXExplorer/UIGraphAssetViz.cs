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

        private void configureGraph(Graph graph)
        {
            var uiGraphAsset = ebxDataContainers.instances[assetGuid];
            AArray nodes = uiGraphAsset.data.get("Nodes").castTo<AArray>();
            var portsGuidToPortsNode = new Dictionary<string, Node>();

            int nodeNextIdx = 0;
            int portNextIdx = 0;

            foreach (var nodeRef in nodes.elements)
            {
                var nodeInRef = nodeRef.castTo<AIntRef>();
                var nodeName = nodeInRef.refTarget.castTo<AStruct>().get("Name").castTo<ASimpleValue>().Val;
                var nodeLabel = "N" + nodeNextIdx.ToString() + ": " + nodeName;
                nodeNextIdx += 1;
                graph.AddNode(nodeLabel);

                var ports = ebxDataContainers.getIntRefedObjsByTypeFor(nodeInRef.instanceGuid, "UINodePort");

                foreach (var dataContainer in ports)
                {
                    var portName = dataContainer.data.get("Name").castTo<ASimpleValue>().Val;
                    var portLabel = "P" + portNextIdx + ": " + portName;
                    portNextIdx += 1;
                    var portNode = graph.AddNode(portLabel);
                    graph.AddEdge(nodeLabel, portLabel);

                    portsGuidToPortsNode.Add(dataContainer.guid, portNode);
                }
            }

            var connections = uiGraphAsset.data.get("Connections").castTo<AArray>();

            foreach (var connRef in connections.elements)
            {
                var conn = connRef.castTo<AIntRef>().refTarget.castTo<AStruct>();
                var srcGuid = conn.get("SourcePort").castTo<AIntRef>().instanceGuid;
                var targetGuid = conn.get("TargetPort").castTo<AIntRef>().instanceGuid;
                var srcNode = portsGuidToPortsNode[srcGuid];
                var targetNode = portsGuidToPortsNode[targetGuid];

                srcNode.AddOutEdge(new Edge(srcNode, targetNode, ConnectionToGraph.Connected));
            }
            
            /*
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("A", "C").Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
            graph.FindNode("A").Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
            graph.FindNode("B").Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
            Microsoft.Msagl.Drawing.Node c = graph.FindNode("C");
            c.Attr.FillColor = Microsoft.Msagl.Drawing.Color.PaleGreen;
            c.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Diamond;
            */
        }
    }
}
