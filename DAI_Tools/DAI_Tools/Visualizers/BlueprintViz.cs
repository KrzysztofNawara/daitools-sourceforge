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
using Color = System.Drawing.Color;

namespace DAI_Tools.EBXExplorer
{
    public partial class BlueprintViz : Form
    {
        private EbxDataContainers ebxDataContainers;
        private string assetGuid;
        private GViewer viewer;
        private EbxTreeXmlViewer ebxTreeViewer;
        private Action<string> statusConsumer;

        public BlueprintViz(EbxDataContainers ebxContainers, string assetGuid, Action<string> statusConsumer)
        {
            this.ebxDataContainers = ebxContainers;
            this.assetGuid = assetGuid;
            this.statusConsumer = statusConsumer;

            InitializeComponent();

            ebxTreeViewer = new EbxTreeXmlViewer(statusConsumer);
            toolsSplitContainer.Panel2.Controls.Add(ebxTreeViewer);
            ebxTreeViewer.Visible = true;
            ebxTreeViewer.setData(ebxContainers);
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

            viewer.Click += nodeSelected;
        }

        private void nodeSelected(object sender, EventArgs e)
        {
            GViewer viewer = sender as GViewer;
            if (viewer.SelectedObject != null && viewer.SelectedObject is Node)
            {
                Node node = viewer.SelectedObject as Node;
                ebxTreeViewer.selectByGuid(node.Id);
            }
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
            public string nodeGuid;
            public bool isInterface = false;
            public Dictionary<string, PortDesc> ownedPortIdToPortDesc = new Dictionary<string, PortDesc>();

            public int getEdgeCount()
            {
                int count = 0;
                foreach (var pdesc in ownedPortIdToPortDesc.Values)
                    count += pdesc.refCount;
                return count;
            }

            public override string ToString()
            {
                return "NODE[" + name + "," + nodeGuid + "]";
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
            public List<string> partials;
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
            metadata.partials = uiGraphAsset.partialsList;
            metadata.dataRoot = uiGraphAsset.data;
            
            processObjects(metadata);
            processInterface(metadata);
            processConnections(metadata, "PropertyConnections", getDirectExtractor("SourceFieldId"), getDirectExtractor("TargetFieldId"), Type.PROPERTY);
            processConnections(metadata, "LinkConnections", getDirectExtractor("SourceFieldId"), getDirectExtractor("TargetFieldId"), Type.LINK);
            processConnections(metadata, "EventConnections", getEventExtractor("SourceEvent"), getEventExtractor("TargetEvent"), Type.EVENT);

            /* graph data processed, start drawing and formatting */
            foreach (var t in metadata.nodeGuidToNodeDesc)
            {
                var refCount = t.Value.getEdgeCount();
                var labelSb = new StringBuilder(refCount + t.Value.name.Length);
                labelSb.Append('\n', refCount/2);
                labelSb.Append(t.Value.name);
                labelSb.Append('\n');
                labelSb.Append('\n', refCount/2);
                
                var node = graph.AddNode(t.Value.nodeGuid);
                node.Label.Text = labelSb.ToString();

                node.Attr.LabelMargin = 30;
                node.Label.FontSize = 16;

                if (t.Value.isInterface)
                {
                    node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.MediumPurple;
                    node.Attr.LabelMargin = 50;
                }
            }

            foreach (var edge in metadata.edges)
            {
                var correspondingCheckBox = getEdgeTypeCheckbox(edge, metadata);
                Color edgeColor = correspondingCheckBox.BackColor;
                bool show = correspondingCheckBox.Checked;

                if (show)
                {
                    var label = getLabel(edge, metadata);
                    var graphEdge = graph.AddEdge(edge.startNodeGuid, label, edge.endNodeGuid);
                    graphEdge.Attr.Color = colorConv(edgeColor);
                    graphEdge.Label.FontColor = graphEdge.Attr.Color;
                }
            }

            if (showUnconnPortsCbkb.Checked)
            {
                var color = colorConv(showUnconnPortsCbkb.BackColor);
                int pidx = 0;
                foreach (var ndesc in metadata.nodeGuidToNodeDesc.Values)
                    foreach (var pdesc in ndesc.ownedPortIdToPortDesc.Values)
                        if (pdesc.refCount == 0)
                        {
                            var pnodeId = pdesc.id + "_" + pidx;
                            var pnode = graph.AddNode(pnodeId);
                            var pedge = graph.AddEdge(ndesc.nodeGuid, "", pnodeId);
                            pidx += 1;

                            pnode.Label.Text = "P" + pidx + "[" + pdesc.type + "," + pdesc.direction + "] " + pdesc.id;
                            pnode.Attr.Color = color;
                            pedge.Attr.Color = color;
                        }
            }

            viewer.Graph = graph;
        }

        private Microsoft.Msagl.Drawing.Color colorConv(Color c)
        {
            return new Microsoft.Msagl.Drawing.Color(c.R, c.G, c.B);
        }

        private string getLabel(Edge e, Metadata mdata)
        {
            var srcPortDesc = getEdgePortDesc(e, true, mdata);
            var tgPortDesc = getEdgePortDesc(e, false, mdata);

            if (srcPortDesc.id.Length == 0 && tgPortDesc.id.Length == 0)
                return "";
            else 
                return srcPortDesc.id + " -> " + tgPortDesc.id;
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
            var objects = new List<Tuple<string, AStruct>>();
            
            if (mdata.partials.Contains("PrefabBlueprint"))
            {
                var objectRefsArray = mdata.dataRoot.get("Objects").castTo<AArray>();

                foreach (var possiblyRef in objectRefsArray.elements)
                {
                    if (possiblyRef.Type == ValueTypes.IN_REF)
                    {
                        var inRef = possiblyRef.castTo<AIntRef>();
                        var referencedData = ebxDataContainers.instances[inRef.instanceGuid].data;
                        objects.Add(new Tuple<string, AStruct>(inRef.instanceGuid, referencedData));
                    }
                    else 
                        throw new Exception("Incorret type found in array: " + possiblyRef.Type);
                }
            }
            else if (mdata.partials.Contains("ObjectBlueprint"))
            {
                foreach (var instance in ebxDataContainers.instances.Values)
                    if (instance.guid != assetGuid)
                        objects.Add(new Tuple<string, AStruct>(instance.guid, instance.data));
            }
            else
                throw new Exception("Unsupported blueprint encountered");
            

            foreach (var t in objects)
            {
                var nodeDesc = new NodeDesc();
                nodeDesc.name = t.Item2.name;
                nodeDesc.nodeGuid = t.Item1;

                mdata.nodeGuidToNodeDesc.Add(t.Item1, nodeDesc);
            }
        }

        private void processInterface(Metadata mdata)
        {
            var iface = mdata.dataRoot.get("Interface");
            if (iface.Type == ValueTypes.IN_REF)
            {
                var inref = iface.castTo<AIntRef>();
                var ifaceAstruct = ebxDataContainers.instances[inref.instanceGuid].data;
            
                var ifaceNodeDesc = new NodeDesc();
                ifaceNodeDesc.name = "Interface";
                ifaceNodeDesc.nodeGuid = inref.instanceGuid;
                ifaceNodeDesc.isInterface = true;

                addAsPorts(ifaceNodeDesc, extractIdsFromArray(ifaceAstruct.get("Fields")), Type.PROPERTY, Dir.UNKNOWN);
                addAsPorts(ifaceNodeDesc, extractIdsFromArray(ifaceAstruct.get("InputEvents")), Type.EVENT, Dir.IN);
                addAsPorts(ifaceNodeDesc, extractIdsFromArray(ifaceAstruct.get("OutputEvents")), Type.EVENT, Dir.OUT);
                addAsPorts(ifaceNodeDesc, extractIdsFromArray(ifaceAstruct.get("InputLinks")), Type.LINK, Dir.IN);
                addAsPorts(ifaceNodeDesc, extractIdsFromArray(ifaceAstruct.get("OutputLinks")), Type.LINK, Dir.OUT);

                mdata.nodeGuidToNodeDesc.Add(inref.instanceGuid, ifaceNodeDesc);
            }
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
            var aval = value.castTo<ASimpleValue>();
            if (aval.unhashed != null)
                return aval.unhashed;
            else 
                return aval.Val;
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

        private void showUnconnPortsCbkb_CheckedChanged(object sender, EventArgs e)
        {
            drawGraphSafely();
        }
    }
}
