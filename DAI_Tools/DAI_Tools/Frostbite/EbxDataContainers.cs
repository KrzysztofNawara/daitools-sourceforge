using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
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
        public ASimpleValue(String value, String unhashed = null) : base(ValueTypes.SIMPLE)
        {
            this.Val = value;
            this.unhashed = unhashed;
        }
        public String Val { get; }
        public String unhashed { get; }
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
            this.refStatus = RefStatus.UNRESOLVED;
        }

        public String instanceGuid { get; set; }
        public RefStatus refStatus { get; set; }
    }

    public class AExRef : AValue
    {
        public AExRef(String fileGuid, String instanceGuid) : base(ValueTypes.EX_REF)
        {
            this.fileGuid = fileGuid;
            this.instanceGuid = instanceGuid;
            this.refStatus = RefStatus.UNRESOLVED;
        }

        public String fileGuid { get; set; }
        public String instanceGuid { get; set; }
        public String refName { get; set; }
        public String refType { get; set; }
        public RefStatus refStatus { get; set; }
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
            this.partialsList = new List<string>();
            this.partialsMap = new Dictionary<string, AStruct>();
        }

        public AStruct flattenedData { get; set; }
        public List<String> partialsList { get; }
        
        public AStruct data { get; set; }
        public String guid;
        public uint internalRefCount = 0;
        public List<string> intRefs { get; }
        /* order: most specific to most generic */
        private Dictionary<String, AStruct> partialsMap;
       
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
    }
    
    /**
     * Offers higher-level view on EBX files - as an asset container. 
     */
    public class EbxDataContainers
    {
        public static EbxDataContainers fromDAIEbx(DAIEbx file, Action<string> statusConsumer)
        {
            Dictionary<String, DataContainer> instances = new Dictionary<string, DataContainer>();

            var ctx = new ConverterContext();
            ctx.file = file;

            statusConsumer("Converting instances...");
            foreach (var instance in file.Instances)
            {
                var instanceGuid = DAIEbx.GuidToString(instance.Key);
                statusConsumer($"Converting {instanceGuid}...");
                ctx.instanceGuid = instanceGuid;
                var rootFakeField = wrapWithFakeField(instance.Value);
                AValue convertedTreeRoot = convert(rootFakeField, ctx);

                Debug.Assert(convertedTreeRoot.Type == ValueTypes.STRUCT);
                AStruct treeRoot = (AStruct) convertedTreeRoot;
                instances.Add(instanceGuid, new DataContainer(instanceGuid, treeRoot));
            }

            statusConsumer("Processing IntRefs...");
            foreach (var refEntry in ctx.intReferences)
            {
                var refObj = refEntry.Item1;
                var targetGuid = refObj.instanceGuid;

                if (instances.ContainsKey(targetGuid))
                {
                    var target = instances[targetGuid];
                    target.internalRefCount += 1;
                    refObj.refStatus = RefStatus.RESOLVED_SUCCESS;
                } else 
                {
                    refObj.refStatus = RefStatus.RESOLVED_FAILURE;
                }

                var refObjTreeRootGuid = refEntry.Item2; 
                instances[refObjTreeRootGuid].addIntRef(targetGuid);
            }

            statusConsumer("Processing ExRefs...");
            using(var dbconn = Database.GetConnection())
            {
                dbconn.Open();
                using (var dbtrans = dbconn.BeginTransaction())
                {
                    int processedCount = 0;
                    foreach (var exRefEntry in ctx.extRefs)
                    {
                        var exref = exRefEntry.Item1;
                        var sqlCmdText = $"select name, type from ebx where guid = \"{exref.fileGuid}\"";
                        using (var reader = new SQLiteCommand(sqlCmdText, dbconn).ExecuteReader())
                        {
                            if (!reader.HasRows)
                                exref.refStatus = RefStatus.RESOLVED_FAILURE;
                            else
                            {
                                reader.Read();
                                var values = new object[2];
                                reader.GetValues(values);

                                exref.refName = (string) values[0];
                                exref.refType = (string) values[1];
                                exref.refStatus = RefStatus.RESOLVED_SUCCESS;
                            }
                        }
                        processedCount += 1;
                        statusConsumer($"Processed ExtRefs: {processedCount}/{ctx.extRefs.Count}");
                    }
                    dbtrans.Commit();
                }
            }

            statusConsumer("Populating partials...");
            var fileGuid = DAIEbx.GuidToString(file.FileGuid);
            var edc = new EbxDataContainers(fileGuid, instances, file);
            edc.populatePartials();

            statusConsumer("DAIEbx -> EbxDataContainers done.");
            return edc;
        }

        private class ConverterContext
        {
            public DAIEbx file;
            public string instanceGuid;
            /* inref to resolve, whom it belongs to */
            public List<Tuple<AIntRef, string>> intReferences = new List<Tuple<AIntRef, string>>();
            public List<Tuple<AExRef, string>> extRefs = new List<Tuple<AExRef, string>>();
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
                    {
                        var aexref = new AExRef(guid.fileGuid, guid.instanceGuid); 
                        ctx.extRefs.Add(new Tuple<AExRef, string>(aexref, ctx.instanceGuid));
                        result = aexref;
                    }
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

                result = new ASimpleValue(strValue, tryUnhash(strValue));
            }

            return result;
        }

        private static string tryUnhash(string value)
        {
            int hash = -1;
            if (int.TryParse(value, out hash))
                if (Frontend.hashToString.ContainsKey(hash))
                    return Frontend.hashToString[hash];

            return null;
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

        public AStruct getFlattenedDataFor(string containerGuid)
        {
            var container = instances[containerGuid];
            if (container.flattenedData == null)
                container.flattenedData = flatten(container.data);
            return container.flattenedData;
        }

        private static AStruct flatten(AStruct what)
        {
            if (what.fields.ContainsKey("$"))
            {
                var flattened = new AStruct();
                flattened.name = what.name;
                flattened.correspondingDaiFields = what.correspondingDaiFields;
                doFlatten(what, flattened);
                return flattened;
            }
            else
                return what;
            
        }

        private static AArray flatten(AArray what)
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

        private static void doFlatten(AStruct toProcess, AStruct toAdd)
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
