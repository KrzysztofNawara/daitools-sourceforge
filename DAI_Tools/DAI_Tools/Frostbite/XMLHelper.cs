using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;

namespace DAI_Tools.Frostbite
{
    public static class XMLHelper
    {
        public class Node
        {
            public string name;
            public List<NodeProp> properties;
            public List<Node> childs;
            public string content;
            public Node()
            {
            }
            public Node(string _name, string _content)
            {
                name = _name;
                content = _content;
            }
        }

        public class NodeProp
        {
            public string name;
            public string value;
            public NodeProp(string _name, string _value)
            {
                name = _name;
                value = _value;
            }
        }

        public static void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i;
            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                for (i = 0; i <= nodeList.Count - 1; i++)
                {
                    xNode = inXmlNode.ChildNodes[i];
                    inTreeNode.Nodes.Add(new TreeNode(xNode.Name));
                    tNode = inTreeNode.Nodes[i];
                    AddNode(xNode, tNode);
                }
            }
            else
                inTreeNode.Text = (inXmlNode.OuterXml).Trim();
        }

        public static bool Validate(string xmlcontent, string xsdcontent)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                XmlSchema schema = XmlSchema.Read(new StringReader(xsdcontent), (sender, args) => { throw new Exception("HANDLE VALIDATION FAILED"); });
                settings.Schemas.Add(schema);
                settings.ValidationType = ValidationType.Schema;
                var reader = XmlReader.Create(new StringReader(xmlcontent), settings);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string MakeXML(Node node, int depth)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder t = new StringBuilder();
            for (int i = 0; i < depth; i++)
                t.Append(" ");
            sb.Append(t + "<" + node.name);
            if (node.properties != null)
                foreach (NodeProp prop in node.properties)
                    sb.Append(" " + prop.name + "=\"" + prop.value + "\"");
            sb.Append(">");
            if (node.childs != null)
            {
                sb.Append("\n");
                foreach (Node child in node.childs)
                    sb.Append(MakeXML(child, depth + 1));
                sb.Append(t + "</" + node.name + ">\n");
            }
            else
            {
                sb.Append(node.content);                
                sb.Append("</" + node.name + ">\n");
            }
            return sb.ToString();
        }

        public static string[] repList = { "<", "&lt;", ">", "&gt;", "\n", "[/n]", "\r", "[/r]" };

        public static string toXML(string input)
        {
            string s = input;
            for (int i = 0; i < repList.Length / 2; i++)
                s = s.Replace(repList[i * 2], repList[i * 2 + 1]);
            return s;
        }

        public static string fromXML(string input)
        {
            string s = input;
            for (int i = 0; i < repList.Length / 2; i++)
                s = s.Replace(repList[i * 2 + 1], repList[i * 2]);
            return s;
        }
    }
}
