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
                    processEbxTree(wrapWithFakeField(instanceGuid, instance.Value), root);
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

        /**
         * Processes passed field, attaches new node for that field (fieldname + DAIComplex name)
         * For each simple value attaches node for it
         * For each complex value simply calls recursively processEbxTree - it'll handle adding node for DAIComplex
         */
        private void processEbxTree(DAIField field, TreeNode parentNode)
        {
            Debug.Assert(field.ValueType == DAIFieldType.DAI_Complex || field.ValueType == DAIFieldType.DAI_Array, 
                "this method should be invoked only for complex/array fields (invoked for: " + field.ValueType.ToString() + ")");

            var treeNode = new TreeNode(formatComplexField(field));
            parentNode.Nodes.Add(treeNode);

            var value = field.GetComplexValue();

            foreach (var childField in value.Fields)
            {
                var fieldName = childField.Descriptor.FieldName;
                String strValue = null;
                
                switch (childField.ValueType)
                {
                    case DAIFieldType.DAI_Complex:
                    case DAIFieldType.DAI_Array:
                        processEbxTree(childField, treeNode);
                        break;
                    case DAIFieldType.DAI_String:
                        strValue = childField.GetStringValue();
                        break;
                    case DAIFieldType.DAI_Enum:
                        strValue = childField.GetEnumValue();
                        break;
                    case DAIFieldType.DAI_Int:
                        strValue = childField.GetIntValue().ToString();
                        break;
                    case DAIFieldType.DAI_UInt:
                        strValue = childField.GetUIntValue().ToString();
                        break;
                    case DAIFieldType.DAI_Double:
                    case DAIFieldType.DAI_Float:
                        strValue = childField.GetFloatValue().ToString();
                        break;
                    case DAIFieldType.DAI_Short:
                        strValue = childField.GetShortValue().ToString();
                        break;
                    case DAIFieldType.DAI_UShort:
                        strValue = childField.GetUShortValue().ToString();
                        break;
                    case DAIFieldType.DAI_Byte:
                    case DAIFieldType.DAI_UByte:
                        strValue = childField.GetByteValue().ToString();
                        break;
                    case DAIFieldType.DAI_Long:
                        strValue = childField.GetLongValue().ToString();
                        break;
                    case DAIFieldType.DAI_LongLong:
                        // @ToDo!
                        strValue = "some long long byte[]...";
                        break;
                    case DAIFieldType.DAI_Guid:
                        // @ToDo!
                        strValue = "GUID";
                        break;
                    case DAIFieldType.DAI_Bool:
                        strValue = childField.GetBoolValue().ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                attachSimpleField(fieldName, strValue, treeNode);
            }
        }

        private String formatComplexField(DAIField field)
        {
            return field.Descriptor.FieldName + " -> " + field.GetComplexValue().Descriptor.FieldName;
        }

        private void attachSimpleField(String name, String value, TreeNode parent)
        {
            var tnode = new TreeNode(name + ": " + value);
            parent.Nodes.Add(tnode);
        }
    }
}
