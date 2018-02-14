using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAI_Tools.Frostbite;

namespace DAI_Tools.EBXExplorer
{
    public partial class EbxTreeXmlViewer : UserControl
    {
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
                var fileGuid = DAIEbx.GuidToString(ebxFile.FileGuid);
                var root = new TreeNode("EBX: " + fileGuid);

                foreach (var instance in ebxFile.Instances)
                {
                    var instanceGuid = DAIEbx.GuidToString(instance.Key);
                    processEbxTree(wrapWithFakeField(instanceGuid, instance.Value), root, ebxFile);
                }

                treeView1.Nodes.Add(root);
            }
        }

        private DAIField wrapWithFakeField(String fieldName, DAIComplex value)
        {
            var fakeField = new DAIField();
            fakeField.ValueType = DAIFieldType.DAI_Complex;
            fakeField.Descriptor = new DAIFieldDescriptor();
            fakeField.Descriptor.FieldName = fieldName;
            fakeField.ComplexValue = value;
            return fakeField;
        }

        private void processEbxTree(DAIField field, TreeNode parentNode, DAIEbx file)
        {
            var fieldName = field.Descriptor.FieldName;
            

            /* for complex fields spawn recursive actions, for simple attach leaf nodes */
            if (field.ValueType == DAIFieldType.DAI_Complex)
            {
                var tnode = attachComplexField(field, parentNode);
                spawnRecursiveAction(field.GetComplexValue().Fields, tnode, file);
            }
            else if(field.ValueType == DAIFieldType.DAI_Array)
            {
                var tnode = attachComplexField(field, parentNode);
                spawnRecursiveAction(field.GetArrayValue().Fields, tnode, file);
            }
            else
            {
                String strValue = null;

                switch (field.ValueType)
                {
                    case DAIFieldType.DAI_String:
                        strValue = field.GetStringValue();
                        break;
                    case DAIFieldType.DAI_Enum:
                        strValue = field.GetEnumValue();
                        break;
                    case DAIFieldType.DAI_Int:
                        strValue = field.GetIntValue().ToString();
                        break;
                    case DAIFieldType.DAI_UInt:
                        strValue = field.GetUIntValue().ToString();
                        break;
                    case DAIFieldType.DAI_Double:
                    case DAIFieldType.DAI_Float:
                        strValue = field.GetFloatValue().ToString();
                        break;
                    case DAIFieldType.DAI_Short:
                        strValue = field.GetShortValue().ToString();
                        break;
                    case DAIFieldType.DAI_UShort:
                        strValue = field.GetUShortValue().ToString();
                        break;
                    case DAIFieldType.DAI_Byte:
                    case DAIFieldType.DAI_UByte:
                        strValue = field.GetByteValue().ToString();
                        break;
                    case DAIFieldType.DAI_Long:
                        strValue = field.GetLongValue().ToString();
                        break;
                    case DAIFieldType.DAI_LongLong:
                        strValue = "LL " + DAIEbx.GuidToString(field.GetLongLongValue());
                        break;
                    case DAIFieldType.DAI_Guid:
                        var guid = file.GetDaiGuidFieldValue(field);
                        var fileGuidPrefix = (guid.external) ? (guid.fileGuid + " ") : "";
                        strValue = "[" + fileGuidPrefix + guid.instanceGuid + "]";
                        break;
                    case DAIFieldType.DAI_Bool:
                        strValue = field.GetBoolValue().ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                attachSimpleField(fieldName, strValue, parentNode);
            }
        }

        private void spawnRecursiveAction(List<DAIField> childFields, TreeNode parentNode, DAIEbx ebxFile)
        {
            foreach (var childField in childFields)
                processEbxTree(childField, parentNode, ebxFile);
        }

        private TreeNode attachComplexField(DAIField field, TreeNode parent)
        {
            var str = field.Descriptor.FieldName + " -> " + field.GetComplexValue().Descriptor.FieldName;
            var tnode = new TreeNode(str);
            parent.Nodes.Add(tnode);
            return tnode;
        }

        private void attachSimpleField(String name, String value, TreeNode parent)
        {
            var tnode = new TreeNode(name + ": " + value);
            parent.Nodes.Add(tnode);
        }
    }
}
