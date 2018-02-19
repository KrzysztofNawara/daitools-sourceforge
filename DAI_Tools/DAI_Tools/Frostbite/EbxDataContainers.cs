using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAI_Tools.Frostbite
{
    public enum ValueTypes
    {
        SIMPLE,
        NULL_REF,
        IN_REF,
        EX_REF,
        STRUCT,
        ARRAY,
    }

    public abstract class AValue
    {
        public AValue(ValueTypes type) { this.Type = type; }
        public ValueTypes Type { get; }
        public T castTo<T>() { return (T) Convert.ChangeType(this, typeof(T)); }
    }

    public class ASimpleValue : AValue
    {
        public ASimpleValue(String value) : base(ValueTypes.SIMPLE) { this.Val = value; }
        public String Val { get; }
    }

    public class ANullRef : AValue { public ANullRef() : base(ValueTypes.NULL_REF) { } }

    public enum RefStatus
    {
        UNRESOLVED,
        RESOLVED_SUCCESS,
        RESOLVED_FAILURE,
    }

    public class AIntRef : AValue
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

    public class AExRef : AValue
    {
        public AExRef(String fileGuid, String instanceGuid) : base(ValueTypes.EX_REF)
        {
            this.fileGuid = fileGuid;
            this.instanceGuid = instanceGuid;
        }

        public String fileGuid { get; set; }
        public String instanceGuid { get; set; }
    }

    public class AStruct : AValue
    {
        public AStruct() : base(ValueTypes.STRUCT)
        {
            fields = new SortedDictionary<String, AValue>();
            correspondingDaiFields = new Dictionary<string, DAIField>();
        }

        public String name { get; set; }
        public SortedDictionary<String, AValue> fields { get; }
        public Dictionary<String, DAIField> correspondingDaiFields { get; set; }

        public AValue get(string fieldName, bool searchAncestors = true)
        {
            bool shouldStop = false;
            AStruct toSearch = this;
            while(!shouldStop)
            {
                if (toSearch.fields.ContainsKey(fieldName))
                    return toSearch.fields[fieldName];
                else if (toSearch.fields.ContainsKey("$"))
                    toSearch = toSearch.fields["$"].castTo<AStruct>();
                else
                    shouldStop = true;
            }

            return null;
        }
    }

    class AArray : AValue
    {
        public AArray() : base(ValueTypes.ARRAY)
        {
            elements = new List<AValue>();
            correspondingDaiFields = new List<DAIField>();
        }

        public AArray(List<AValue> elements, List<DAIField> correspondinDaiFields) : base(ValueTypes.ARRAY)
        {
            this.elements = elements;
            this.correspondingDaiFields = correspondinDaiFields;
        }

        public List<AValue> elements { get; }
        public List<DAIField> correspondingDaiFields { get; }
    }

    /**
     * Partials are returned with original casing, but can be searched by any - they are case-insenitive
     */
    public class DataContainer
    {
        public DataContainer(String guid, AStruct data)
        {
            this.guid = guid;
            this.data = data;
            this.flattenedData = null;
            this.intRefs = new List<string>();
        }
        
        public String guid;
        public AStruct data;
        public uint internalRefCount = 0;
        public List<string> intRefs { get; }

        private AStruct flattenedData;
       
        public AStruct getFlattenedData()
        {
            if (flattenedData == null)
                this.flattenedData = flatten(data);
            return flattenedData;
        }

        public List<String> getAllPartials() { return partialsList; }

        public AStruct getPartial(String typeName)
        {
            return partialsMap[typeName.ToLower()];
        }

        public void addPartial(String typeName, AStruct partialData)
        {
            if (hasPartial(typeName))
                throw new Exception("Already have partial: " + typeName);
            
            partialsList.Add(typeName);
            partialsMap.Add(typeName.ToLower(), partialData);
        }

        public bool hasPartial(String typeName)
        {
            return partialsMap.ContainsKey(typeName.ToLower());
        }

        public void addIntRef(String guid)
        {
            intRefs.Add(guid);
        }

        private AStruct flatten(AStruct what)
        {
            Debug.Assert(data != null);
            
            if (what.fields.ContainsKey("$"))
            {
                var flattened = new AStruct();
                flattened.name = data.name;
                flattened.correspondingDaiFields = data.correspondingDaiFields;
                doFlatten(data, flattened);
                return flattened;
            }
            else
                return what;
            
        }

        private AArray flatten(AArray what)
        {
            var processedFields = new List<AValue>();
            var atLeastOneChanged = false;
            foreach (var origElement in what.elements)
            {
                if (origElement.Type == ValueTypes.STRUCT)
                {
                    var flattened = flatten(origElement.castTo<AStruct>()); 
                    processedFields.Add(flattened);
                    if (!object.ReferenceEquals(flattened, origElement))
                        atLeastOneChanged = true;
                }
            }

            if (atLeastOneChanged)
                return new AArray(processedFields, what.correspondingDaiFields);
            else 
                return what;
        }

        private void doFlatten(AStruct toProcess, AStruct toAdd)
        {
            foreach (var field in toProcess.fields)
            {
                if (field.Key.Equals("$"))
                    doFlatten(field.Value.castTo<AStruct>(), toAdd);
                else
                {
                    AValue val;
                    var ftype = field.Value.Type;
                    if (ftype == ValueTypes.STRUCT)
                        val = flatten(field.Value.castTo<AStruct>());
                    else if (ftype == ValueTypes.ARRAY)
                        val = flatten(field.Value.castTo<AArray>());
                    else
                        val = field.Value;

                    toAdd.fields.Add(field.Key, val);
                } 
            }
        }

        /* order: most specific to most generic */
        private List<String> partialsList = new List<string>();
        private Dictionary<String, AStruct> partialsMap = new Dictionary<string, AStruct>();
    }
    
    /**
     * Offers higher-level view on EBX files - as an asset container. 
     */
    public class EbxDataContainers
    {
        public static EbxDataContainers fromDAIEbx(DAIEbx file)
        {
            Dictionary<String, DataContainer> instances = new Dictionary<string, DataContainer>();

            var ctx = new ConverterContext();
            ctx.file = file;

            foreach (var instance in file.Instances)
            {
                var instanceGuid = DAIEbx.GuidToString(instance.Key);
                ctx.instanceGuid = instanceGuid;
                var rootFakeField = wrapWithFakeField(instance.Value);
                AValue convertedTreeRoot = convert(rootFakeField, ctx);

                Debug.Assert(convertedTreeRoot.Type == ValueTypes.STRUCT);
                AStruct treeRoot = (AStruct) convertedTreeRoot;
                instances.Add(instanceGuid, new DataContainer(instanceGuid, treeRoot));
            }

            foreach (var refEntry in ctx.intReferences)
            {
                var refObj = refEntry.Item1;
                var targetGuid = refObj.instanceGuid;

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

                var refObjTreeRootGuid = refEntry.Item2; 
                instances[refObjTreeRootGuid].addIntRef(targetGuid);
            }

            var fileGuid = DAIEbx.GuidToString(file.FileGuid);
            var edc = new EbxDataContainers(fileGuid, instances, file);
            edc.populatePartials();

            return edc;
        }

        private class ConverterContext
        {
            public DAIEbx file;
            public string instanceGuid;
            /* inref to resolve, whom it belongs to */
            public List<Tuple<AIntRef, string>> intReferences = new List<Tuple<AIntRef, string>>();
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
                
                if (value == null) 
                    result = new ASimpleValue("{null}");
                else
                {
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
                        ctx.intReferences.Add(new Tuple<AIntRef, string>(ainref, ctx.instanceGuid));
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

        private EbxDataContainers(String fileGuid, Dictionary<String, DataContainer> instances, DAIEbx correspondingEbx)
        {
            this.fileGuid = fileGuid;
            this.instances = instances;
            this.correspondingEbx = correspondingEbx;
        }

        public String fileGuid { get; }
        public Dictionary<String, DataContainer> instances { get; }
        private DAIEbx correspondingEbx;

        public List<DataContainer> getAllWithPartial(String typeName)
        {
            var result = new List<DataContainer>();

            foreach (var instance in instances.Values)
            {
                if (instance.hasPartial(typeName))
                    result.Add(instance);
            }

            return result;
        }

        public List<DataContainer> getIntRefedObjsByTypeFor(String containerGuid, string type)
        {
            var result = new List<DataContainer>();
            foreach(var intGuid in instances[containerGuid].intRefs)
            {
                var refedContainer = instances[intGuid];
                if (refedContainer.hasPartial(type))
                    result.Add(refedContainer);
            }
            return result;
        }

        private void populatePartials()
        {
            foreach(var instance in instances)
            {
                var dataRoot = instance.Value.data;

                AStruct partialToProcess =  dataRoot;
                while (partialToProcess != null)
                {
                    instance.Value.addPartial(partialToProcess.name, partialToProcess);
                    partialToProcess = partialToProcess.fields.ContainsKey("$") ? partialToProcess.fields["$"].castTo<AStruct>() : null;
                }
            }
        }
    }
}
