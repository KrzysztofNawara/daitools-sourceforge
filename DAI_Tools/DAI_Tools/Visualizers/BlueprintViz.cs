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
using Microsoft.Msagl.GraphViewerGdi;

namespace DAI_Tools.EBXExplorer
{
    public partial class BlueprintViz : Form
    {
        private EbxDataContainers ebxDataContainers;
        private string assetGuid;
        private GViewer viewer;

        public BlueprintViz(EbxDataContainers ebxContainers, string assetGuid)
        {
            this.ebxDataContainers = ebxContainers;
            this.assetGuid = assetGuid;
            
            InitializeComponent();
        }

        private void BlueprintViz_Load(object sender, EventArgs e)
        {
            //create a viewer object 
            viewer = new GViewer();
                
            this.SuspendLayout();
            
            //create the graph content 
            drawGraphSafely();

            //associate the viewer with the form 
            viewer.Dock = DockStyle.Fill;
            this.Controls.Add(viewer);
            this.ResumeLayout();
        }

        private void drawGraphSafely()
        {
            try {
                drawGraph();
            } catch (Exception ex)
            {
                MessageBox.Show("Exception:\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private class NodeDesc
        {
            public string name;
            public string labelName;
        }

        private void drawGraph()
        {
            var uiGraphAsset = ebxDataContainers.instances[assetGuid];
            var assetName = uiGraphAsset.data.get("Name").castTo<ASimpleValue>().Val;
            
            Graph graph = new Graph(assetName);

            /* draw graph */
            var nodes = buildNodeDescritorMap(uiGraphAsset.data);

            foreach (var t in nodes)
            {
                graph.AddNode(t.Value.labelName);
            }

            viewer.Graph = graph;
        }

        private Dictionary<string, NodeDesc> buildNodeDescritorMap(AStruct dataRoot)
        {
            var objectRefsArray = dataRoot.get("Objects").castTo<AArray>();
            var objects = new List<Tuple<string, AStruct>>(objectRefsArray.elements.Count);

            foreach (var possiblyRef in objectRefsArray.elements)
            {
                if (possiblyRef.Type == ValueTypes.IN_REF)
                {
                    var inRef = possiblyRef.castTo<AIntRef>();
                    objects.Add(new Tuple<string, AStruct>(inRef.instanceGuid, inRef.refTarget.castTo<AStruct>()));
                }
                else 
                    throw new Exception("Incorret type found in array: " + possiblyRef.Type);
            }

            var guidToNodeDesc = new Dictionary<string, NodeDesc>();
            var nextNodeId = 0;

            foreach (var t in objects)
            {
                var nodeDesc = new NodeDesc();
                nodeDesc.name = t.Item2.name;
                nodeDesc.labelName = "N" + nextNodeId + ": " + nodeDesc.name;
                nextNodeId += 1;

                guidToNodeDesc.Add(t.Item1, nodeDesc);
            }

            return guidToNodeDesc;
        }
    }
}
