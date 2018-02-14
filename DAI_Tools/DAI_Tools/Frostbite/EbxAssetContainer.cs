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

    abstract class ASimpleValue : AValue
    {
        public ASimpleValue(String value) : base(ValueTypes.SIMPLE) { this.Val = value; }
        public String Val { get; }
    }

    class ANullRef : AValue { public ANullRef(String v) : base(ValueTypes.NULL_REF) { } }

    class AIntRef : AValue
    {
        public AIntRef(String instanceGuid) : base(ValueTypes.IN_REF)
        {
            this.instanceGuid = instanceGuid;
        }

        public String instanceGuid { get; set; }
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

    
    /**
     * Offers higher-level view on EBX files - as an asset container. 
     */
    class EbxAssetContainer
    {
        public static EbxAssetContainer fromDAIEbx(DAIEbx file)
        {
            Dictionary<String, AStruct> instances = new Dictionary<string, AStruct>();

            var ctx = new ConverterContext();
            ctx.file = file;

            foreach (var instance in file.Instances)
            {
                var instanceGuid = DAIEbx.GuidToString(instance.Key);
                var rootFakeField = wrapWithFakeField(instance.Value);
                AValue convertedTreeRoot = convert(rootFakeField, ctx);

                Debug.Assert(convertedTreeRoot.Type == ValueTypes.STRUCT);
                AStruct treeRoot = (AStruct) convertedTreeRoot;
                instances.Add(instanceGuid, treeRoot);
            }

            var fileGuid = DAIEbx.GuidToString(file.FileGuid);
            return new EbxAssetContainer(fileGuid, instances, file);
        }

        private class ConverterContext
        {
            public DAIEbx file;
        }

        private static DAIField wrapWithFakeField(DAIComplex value)
        {
            var fakeField = new DAIField();
            fakeField.ValueType = DAIFieldType.DAI_Complex;
            fakeField.ComplexValue = value;
            return fakeField;
        }

        private static AValue convert(DAIField root, ConverterContext ctx)
        {
            
            return null;
        }

        EbxAssetContainer(String fileGuid, Dictionary<String, AStruct> instances, DAIEbx correspondingEbx)
        {
            this.fileGuid = fileGuid;
            this.instances = instances;
            this.correspondingEbx = correspondingEbx;
        }

        private String fileGuid;
        private Dictionary<String, AStruct> instances;
        private DAIEbx correspondingEbx;
        
        private bool intRefResolved = false;
    }
}
