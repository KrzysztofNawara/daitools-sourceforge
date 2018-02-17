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

        private enum Type
        {
            PROPERTY,
            LINK,
            EVENT,
        }

        private enum Dir
        {
            UNKNOWN,
            IN,
            OUT,
        }

        private class PortDesc
        {
            public PortDesc(string id, Type type, Dir direction)
            {
                this.id = id;
                this.type = type;
                this.direction = direction;
            }
            
            public string id;
            public Type type;
            public Dir direction;
            public int refCount;

            public override string ToString()
            {
                return "PORT[" + id + "," + type + "," + direction + "]";
            }
        }

        private class NodeDesc
        {
            public string name;
            public string labelName;
            public Dictionary<string, PortDesc> ownedPortIdToPortDesc = new Dictionary<string, PortDesc>();

            public override string ToString()
            {
                return "NODE[" + name + "," + labelName + "]";
            }
        }

        private class Edge
        {
            public Edge(string startId, string endId)
            {
                this.startId = startId;
                this.endId = endId;
            }
            
            public string startId;
            public string endId;
        }

        private class Metadata
        {
            public AStruct dataRoot;
            public Dictionary<string, NodeDesc> nodeGuidToNodeDesc = new Dictionary<string, NodeDesc>();
            public List<Edge> edges = new List<Edge>();
            public Dictionary<string, NodeDesc> portIdToOwningNode = new Dictionary<string, NodeDesc>();
        }

        private void drawGraph()
        {
            var uiGraphAsset = ebxDataContainers.instances[assetGuid];
            var assetName = uiGraphAsset.data.get("Name").castTo<ASimpleValue>().Val;
            
            Graph graph = new Graph(assetName);

            /* draw graph */
            var metadata = new Metadata();
            metadata.dataRoot = uiGraphAsset.data;
            
            processObjects(metadata);
            processPropertyConnections(metadata);

            foreach (var t in metadata.nodeGuidToNodeDesc)
            {
                graph.AddNode(t.Value.labelName);
            }

            viewer.Graph = graph;
        }

        private void processObjects(Metadata mdata)
        {
            var objectRefsArray = mdata.dataRoot.get("Objects").castTo<AArray>();
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

            var nextNodeId = 0;

            foreach (var t in objects)
            {
                var nodeDesc = new NodeDesc();
                nodeDesc.name = t.Item2.name;
                nodeDesc.labelName = "N" + nextNodeId + ": " + nodeDesc.name;
                nextNodeId += 1;

                mdata.nodeGuidToNodeDesc.Add(t.Item1, nodeDesc);
            }
        }

        private void processPropertyConnections(Metadata mdata)
        {
            var propertyArray = mdata.dataRoot.get("PropertyConnections").castTo<AArray>();

            foreach (var propConnection in propertyArray.elements)
            {
                var astruct = propConnection.castTo<AStruct>();
                var srcNodeGuid = extractInRef(astruct.get("Source"));
                var targetNodeGuid = extractInRef(astruct.get("Target"));
                var srcPort = extractId(astruct.get("SourceFieldId"));
                var targetPort = extractId(astruct.get("TargetFieldId"));

                /* add ports to nodes */
                var srcNodeDesc = mdata.nodeGuidToNodeDesc[srcNodeGuid];
                var targetNodeDesc = mdata.nodeGuidToNodeDesc[targetNodeGuid];
                var srcPortDesc = ensurePortAdded(srcNodeDesc, new PortDesc(srcPort, Type.PROPERTY, Dir.OUT), mdata);
                var tgPortDesc = ensurePortAdded(targetNodeDesc, new PortDesc(targetPort, Type.PROPERTY, Dir.IN), mdata);

                srcPortDesc.refCount += 1;
                tgPortDesc.refCount += 1;

                /* add edge */
                mdata.edges.Add(new Edge(srcPort, targetPort));
            }
        }

        private PortDesc ensurePortAdded(NodeDesc ndesc, PortDesc pdesc, Metadata mdata)
        {
            if (ndesc.ownedPortIdToPortDesc.ContainsKey(pdesc.id))
            {
                var epd = ndesc.ownedPortIdToPortDesc[pdesc.id];
                if (epd.direction == Dir.UNKNOWN)
                    epd.direction = pdesc.direction;

                if (epd.direction != pdesc.direction || epd.type != pdesc.type)
                    throw new Exception("Port conflict. Tried to merge: " + pdesc.ToString() + " with existing " + epd.ToString());

                return epd;
            } 
            else
            {
                ndesc.ownedPortIdToPortDesc.Add(pdesc.id, pdesc);

                if (mdata.portIdToOwningNode.ContainsKey(pdesc.id))
                {
                    var endesc = mdata.portIdToOwningNode[pdesc.id];
                    if (endesc != ndesc)
                        throw new Exception("PortID  " + pdesc.id + "conflict. Originally mapped to " + endesc + ", trying to map to " + ndesc);
                } 
                else
                    mdata.portIdToOwningNode.Add(pdesc.id, ndesc);

                return pdesc;
            }
        }

        private string extractId(AValue value)
        {
            return value.castTo<ASimpleValue>().Val;
        }

        private string extractInRef(AValue value)
        {
            return value.castTo<AIntRef>().instanceGuid;
        }
    }
}
