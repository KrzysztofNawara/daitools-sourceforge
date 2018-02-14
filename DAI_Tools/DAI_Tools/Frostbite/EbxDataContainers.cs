using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAI_Tools.Frostbite
{
    enum ValueTypes
    {
        SIMPLE,
        NULL_REF,
        IN_REF,
        EX_REF,
        STRUCT,
        ARRAY,
    }

    abstract class AValue
    {
        public AValue(ValueTypes type) { this.Type = type; }
        public ValueTypes Type { get; }
    }

    class ASimpleValue : AValue
    {
        public ASimpleValue(String value) : base(ValueTypes.SIMPLE) { this.Val = value; }
        public String Val { get; }
    }

    class ANullRef : AValue { public ANullRef() : base(ValueTypes.NULL_REF) { } }

    enum RefStatus
    {
        UNRESOLVED,
        RESOLVED_SUCCESS,
        RESOLVED_FAILURE,
    }

    class AIntRef : AValue
    {
        public AIntRef(String instanceGuid) : base(ValueTypes.IN_REF)
        {
            this.instanceGuid = instanceGuid;
            this.refTarget = null;
            this.refStatus = RefStatus.UNRESOLVED;
        }

        public String instanceGuid { get; set; }
        public AValue refTarget { get; set; }
        public RefStatus refStatus { get; set; }
    }

    class AExRef : AValue
    {
        public AExRef(String fileGuid, String instanceGuid) : base(ValueTypes.EX_REF)
        {
            this.fileGuid = fileGuid;
            this.instanceGuid = instanceGuid;
        }

        public String fileGuid { get; set; }
        public String instanceGuid { get; set; }
    }

    class AStruct : AValue
    {
        public AStruct() : base(ValueTypes.STRUCT)
        {
            fields = new SortedDictionary<String, AValue>();
            correspondingDaiFields = new Dictionary<string, DAIField>();
        }

        public String name { get; set; }
        public SortedDictionary<String, AValue> fields { get; }
        public Dictionary<String, DAIField> correspondingDaiFields { get; }
    }

    class AArray : AValue
    {
        public AArray() : base(ValueTypes.ARRAY) { elements = new List<AValue>(); }

        public List<AValue> elements { get; }
        public List<DAIField> correspondingDaiFields { get; }
    }

    class DataContainer
    {
        public DataContainer(String guid, AStruct data)
        {
            this.guid = guid;
            this.data = data;
        }
        
        public String guid;
        public AStruct data;
        public uint internalRefCount = 0;
    }
    
    /**
     * Offers higher-level view on EBX files - as an asset container. 
     */
    class EbxDataContainers
    {
        public static EbxDataContainers fromDAIEbx(DAIEbx file)
        {
            Dictionary<String, DataContainer> instances = new Dictionary<string, DataContainer>();

            var ctx = new ConverterContext();
            ctx.file = file;

            foreach (var instance in file.Instances)
            {
                var instanceGuid = DAIEbx.GuidToString(instance.Key);
                var rootFakeField = wrapWithFakeField(instance.Value);
                AValue convertedTreeRoot = convert(rootFakeField, ctx);

                Debug.Assert(convertedTreeRoot.Type == ValueTypes.STRUCT);
                AStruct treeRoot = (AStruct) convertedTreeRoot;
                instances.Add(instanceGuid, new DataContainer(instanceGuid, treeRoot));
            }

            foreach (var refEntry in ctx.intReferences)
            {
                var targetGuid = refEntry.Item1;
                var refObj = refEntry.Item2;

                if (instances.ContainsKey(targetGuid))
                {
                    var target = instances[targetGuid];
                    refObj.refTarget = target.data;
                    target.internalRefCount += 1;
                    refObj.refStatus = RefStatus.RESOLVED_SUCCESS;
                } else 
                {
                    refObj.refStatus = RefStatus.RESOLVED_FAILURE;
                }
            }

            var fileGuid = DAIEbx.GuidToString(file.FileGuid);
            return new EbxDataContainers(fileGuid, instances, file);
        }

        private class ConverterContext
        {
            public DAIEbx file;
            public List<Tuple<String, AIntRef>> intReferences = new List<Tuple<string, AIntRef>>();
        }

        private static DAIField wrapWithFakeField(DAIComplex value)
        {
            var fakeField = new DAIField();
            fakeField.ValueType = DAIFieldType.DAI_Complex;
            fakeField.ComplexValue = value;
            return fakeField;
        }

        private static AValue convert(DAIField field, ConverterContext ctx)
        {
            AValue result;
            
            if (field.ValueType == DAIFieldType.DAI_Complex)
            {
                var value = field.GetComplexValue();
                var astruct = new AStruct();
                astruct.name = value.GetName();

                foreach (var childField in value.Fields)
                {
                    AValue convertedChild = convert(childField, ctx);
                    var childFieldName = childField.Descriptor.FieldName;
                    astruct.fields.Add(childFieldName, convertedChild);
                    astruct.correspondingDaiFields.Add(childFieldName, childField);
                }

                result = astruct;
            }
            else if(field.ValueType == DAIFieldType.DAI_Array)
            {
                var value = field.GetArrayValue();
                var aarray = new AArray();

                foreach (var memberField in value.Fields)
                {
                    AValue convertedMember = convert(memberField, ctx);
                    aarray.elements.Add(convertedMember);
                    aarray.correspondingDaiFields.Add(memberField);
                }

                result = aarray;
            }
            else if (field.ValueType == DAIFieldType.DAI_Guid)
            {
                var guid = ctx.file.GetDaiGuidFieldValue(field);

                if (guid.instanceGuid.Equals("null"))
                    result = new ANullRef();
                else
                {
                    if (guid.external)
                        result = new AExRef(guid.fileGuid, guid.instanceGuid);
                    else
                    {
                        var ainref = new AIntRef(guid.instanceGuid);
                        ctx.intReferences.Add(new Tuple<string, AIntRef>(guid.instanceGuid, ainref));
                        result = ainref;
                    }
                }
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
                    case DAIFieldType.DAI_Bool:
                        strValue = field.GetBoolValue().ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                result = new ASimpleValue(strValue);
            }

            return result;
        }

        EbxDataContainers(String fileGuid, Dictionary<String, DataContainer> instances, DAIEbx correspondingEbx)
        {
            this.fileGuid = fileGuid;
            this.instances = instances;
            this.correspondingEbx = correspondingEbx;
        }

        private String fileGuid;
        private Dictionary<String, DataContainer> instances;
        private DAIEbx correspondingEbx;
        
        private bool intRefResolved = false;
    }
}
