﻿using System;
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

                var tbc = new TreeBuilderContext();
                tbc.file = ebxFile;

                foreach (var instance in ebxFile.Instances)
                {
                    var instanceGuid = DAIEbx.GuidToString(instance.Key);
                    var tnode = processEbxTree(wrapWithFakeField(instanceGuid, instance.Value), tbc);
                    root.Nodes.Add(tnode);
                }

                treeView1.Nodes.Add(root);
            }
        }

        private class TreeBuilderContext
        {
            public DAIEbx file;
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

        private TreeNode processEbxTree(DAIField field, TreeBuilderContext tbc)
        {
            var fieldName = field.Descriptor.FieldName;
            TreeNode tnode;

            /* for complex fields spawn recursive actions, for simple attach leaf nodes */
            if (field.ValueType == DAIFieldType.DAI_Complex)
            {
                tnode = complexFieldTNode(field);
                spawnRecursiveActionAndAttach(field.GetComplexValue().Fields, tnode, tbc);
            }
            else if(field.ValueType == DAIFieldType.DAI_Array)
            {
                tnode = complexFieldTNode(field);
                spawnRecursiveActionAndAttach(field.GetArrayValue().Fields, tnode, tbc);
            }
            else
            {
                String strValue;

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
                        var guid = tbc.file.GetDaiGuidFieldValue(field);
                        var fileGuidPrefix = (guid.external) ? (guid.fileGuid + " ") : "";
                        strValue = "[" + fileGuidPrefix + guid.instanceGuid + "]";
                        break;
                    case DAIFieldType.DAI_Bool:
                        strValue = field.GetBoolValue().ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                tnode = simpleFieldTNode(fieldName, strValue);
            }

            return tnode;
        }

        private void spawnRecursiveActionAndAttach(List<DAIField> childFields, TreeNode parentNode, TreeBuilderContext tbc)
        {
            foreach (var childField in childFields)
            {
                var tnode = processEbxTree(childField, tbc);
                parentNode.Nodes.Add(tnode);
            }
        }

        private TreeNode complexFieldTNode(DAIField field)
        {
            var str = field.Descriptor.FieldName + " -> " + field.GetComplexValue().Descriptor.FieldName;
            var tnode = new TreeNode(str);
            return tnode;
        }

        private TreeNode simpleFieldTNode(String name, String value)
        {
            return new TreeNode(name + ": " + value);
        }
    }
}
