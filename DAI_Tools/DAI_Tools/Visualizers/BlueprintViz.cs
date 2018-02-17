﻿using System;
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
using Color = System.Drawing.Color;

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
            contentContainer.Panel1.Controls.Add(viewer);
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
            BIDIR,
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
            public bool isInterface = false;
            public Dictionary<string, PortDesc> ownedPortIdToPortDesc = new Dictionary<string, PortDesc>();

            public override string ToString()
            {
                return "NODE[" + name + "," + labelName + "]";
            }
        }

        private class Edge
        {
            public Edge(string startNodeGuid, string startId, string endNodeGuid, string endId)
            {
                this.startId = startId;
                this.endId = endId;
                this.startNodeGuid = startNodeGuid;
                this.endNodeGuid = endNodeGuid;
            }
             
            public string startNodeGuid;
            public string startId;
            public string endNodeGuid;
            public string endId;
        }

        private class Metadata
        {
            public AStruct dataRoot;
            public Dictionary<string, NodeDesc> nodeGuidToNodeDesc = new Dictionary<string, NodeDesc>();
            public List<Edge> edges = new List<Edge>();
        }

        private Func<AStruct, string> getDirectExtractor(string fieldname)
        {
            return astruct => extractId(astruct.get(fieldname));
        }

        private Func<AStruct, string> getEventExtractor(string fieldname)
        {
            return astruct => extractId(astruct.get(fieldname).castTo<AStruct>().get("Id"));
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
            processInterface(metadata);
            processConnections(metadata, "PropertyConnections", getDirectExtractor("SourceFieldId"), getDirectExtractor("TargetFieldId"), Type.PROPERTY);
            processConnections(metadata, "LinkConnections", getDirectExtractor("SourceFieldId"), getDirectExtractor("TargetFieldId"), Type.LINK);
            processConnections(metadata, "EventConnections", getEventExtractor("SourceEvent"), getEventExtractor("TargetEvent"), Type.EVENT);

            /* graph data processed, start drawing and formatting */
            foreach (var t in metadata.nodeGuidToNodeDesc)
            {
                graph.AddNode(t.Value.labelName);
            }

            foreach (var edge in metadata.edges)
            {
                var srcNodeLabel = metadata.nodeGuidToNodeDesc[edge.startNodeGuid].labelName;
                var tgNodeLabel = metadata.nodeGuidToNodeDesc[edge.endNodeGuid].labelName;

                var correspondingCheckBox = getEdgeTypeCheckbox(edge, metadata);
                Color edgeColor = correspondingCheckBox.BackColor;
                bool show = correspondingCheckBox.Checked;

                if (show)
                {
                    var label = getLabel(edge, metadata);
                    var graphEdge = graph.AddEdge(srcNodeLabel, label, tgNodeLabel);
                    graphEdge.Attr.Color = new Microsoft.Msagl.Drawing.Color(edgeColor.R, edgeColor.G, edgeColor.B);
                    graphEdge.Label.FontColor = graphEdge.Attr.Color;
                }
            }

            viewer.Graph = graph;
        }

        private string getLabel(Edge e, Metadata mdata)
        {
            var srcPortDesc = getEdgePortDesc(e, true, mdata);
            var tgPortDesc = getEdgePortDesc(e, false, mdata);

            if (srcPortDesc.id.Length == 0 && tgPortDesc.id.Length == 0)
                return "";
            else 
                return srcPortDesc.id + " > " + tgPortDesc.id;
        }

        private CheckBox getEdgeTypeCheckbox(Edge e, Metadata mdata)
        {
            CheckBox correspondingCheckBox;
            switch (determineEdgeType(e, mdata))
            {
                case Type.PROPERTY:
                    correspondingCheckBox = showPropertyConnsCheckbox;
                    break;
                case Type.LINK:
                    correspondingCheckBox = showLinkConnsCheckbox;
                    break;
                case Type.EVENT:
                    correspondingCheckBox = showEventConnsCheckbox;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return correspondingCheckBox;
        }

        private Type determineEdgeType(Edge e, Metadata mdata)
        {
            var srcPortDesc = getEdgePortDesc(e, true, mdata);
            var tgPortDesc = getEdgePortDesc(e, false, mdata);

            if (srcPortDesc.type == tgPortDesc.type)
                return srcPortDesc.type;
            else
            {
                MessageBox.Show("Edges of mixed type. Src: " + srcPortDesc + ", Tg: " + tgPortDesc);
                return srcPortDesc.type;
            } 
        }

        private PortDesc getEdgePortDesc(Edge e, bool start, Metadata mdata)
        {
            var nguid = start ? e.startNodeGuid : e.endNodeGuid;
            var pid = start ? e.startId : e.endId;
            return mdata.nodeGuidToNodeDesc[nguid].ownedPortIdToPortDesc[pid];
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

        private void processInterface(Metadata mdata)
        {
            var inref = mdata.dataRoot.get("Interface").castTo<AIntRef>();
            var ifaceAstruct = inref.refTarget.castTo<AStruct>();
            
            var ifaceNodeDesc = new NodeDesc();
            ifaceNodeDesc.name = "Interface";
            ifaceNodeDesc.labelName = "Interface";
            ifaceNodeDesc.isInterface = true;

            addAsPorts(ifaceNodeDesc, extractIdsFromArray(ifaceAstruct.get("Fields")), Type.PROPERTY, Dir.UNKNOWN);
            addAsPorts(ifaceNodeDesc, extractIdsFromArray(ifaceAstruct.get("InputEvents")), Type.EVENT, Dir.IN);
            addAsPorts(ifaceNodeDesc, extractIdsFromArray(ifaceAstruct.get("OutputEvents")), Type.EVENT, Dir.OUT);
            addAsPorts(ifaceNodeDesc, extractIdsFromArray(ifaceAstruct.get("InputLinks")), Type.LINK, Dir.IN);
            addAsPorts(ifaceNodeDesc, extractIdsFromArray(ifaceAstruct.get("OutputLinks")), Type.LINK, Dir.OUT);

            mdata.nodeGuidToNodeDesc.Add(inref.instanceGuid, ifaceNodeDesc);
        }

        private void processConnections(Metadata mdata, string holdingFieldName, Func<AStruct, string> srcPortIdExtractor, Func<AStruct, string> tgPortIdExtractor, Type type)
        {
            var propertyArray = mdata.dataRoot.get(holdingFieldName).castTo<AArray>();

            foreach (var propConnection in propertyArray.elements)
            {
                var astruct = propConnection.castTo<AStruct>();
                var srcNodeGuid = extractInRef(astruct.get("Source"));
                var targetNodeGuid = extractInRef(astruct.get("Target"));
                var srcPort = srcPortIdExtractor(astruct);
                var targetPort = tgPortIdExtractor(astruct);

                /* add ports to nodes */
                var srcNodeDesc = mdata.nodeGuidToNodeDesc[srcNodeGuid];
                var targetNodeDesc = mdata.nodeGuidToNodeDesc[targetNodeGuid];
                var srcPortDesc = ensurePortAdded(srcNodeDesc, new PortDesc(srcPort, type, Dir.OUT));
                var tgPortDesc = ensurePortAdded(targetNodeDesc, new PortDesc(targetPort, type, Dir.IN));

                srcPortDesc.refCount += 1;
                tgPortDesc.refCount += 1;

                /* add edge */
                mdata.edges.Add(new Edge(srcNodeGuid, srcPort, targetNodeGuid, targetPort));
            }
        }

        private PortDesc ensurePortAdded(NodeDesc ndesc, PortDesc pdesc)
        {
            if (ndesc.ownedPortIdToPortDesc.ContainsKey(pdesc.id))
            {
                var epd = ndesc.ownedPortIdToPortDesc[pdesc.id];
                if (epd.direction == Dir.UNKNOWN)
                    epd.direction = pdesc.direction;

                if (epd.direction != pdesc.direction)
                    epd.direction = Dir.BIDIR;

                if (epd.type != pdesc.type)
                    throw new Exception("Port conflict. Tried to merge: " + pdesc.ToString() + " with existing " + epd.ToString());

                return epd;
            } 
            else
            {
                ndesc.ownedPortIdToPortDesc.Add(pdesc.id, pdesc);
                return pdesc;
            }
        }

        private void addAsPorts(NodeDesc node, List<string> ids, Type type, Dir direction)
        {
            foreach (var id in ids)
                ensurePortAdded(node, new PortDesc(id, type, direction));
        }

        private List<string> extractIdsFromArray(AValue array)
        {
            var ids = new List<string>();
            foreach (var el in array.castTo<AArray>().elements)
            {
                var value = el.castTo<AStruct>().get("Id").castTo<ASimpleValue>().Val;
                ids.Add(value);
            }
            return ids;
        }

        private string extractId(AValue value)
        {
            return value.castTo<ASimpleValue>().Val;
        }

        private string extractInRef(AValue value)
        {
            return value.castTo<AIntRef>().instanceGuid;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            drawGraphSafely();
        }

        private void showPropertyConnsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            drawGraphSafely();
        }

        private void showLinkConnsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            drawGraphSafely();
        }
    }
}
