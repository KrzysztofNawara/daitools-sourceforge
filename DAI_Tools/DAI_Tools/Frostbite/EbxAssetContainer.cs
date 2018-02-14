using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAI_Tools.Frostbite
{
    /**
     * Offers higher-level view on EBX files - as an asset container. 
     */
    class EbxAssetContainer
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

            public SortedDictionary<String, AValue> fields { get; }
            public Dictionary<String, DAIField> correspondingDaiFields { get; }
        }

        class AArray : AValue
        {
            public AArray() : base(ValueTypes.ARRAY) { elements = new List<AValue>(); }

            public List<AValue> elements { get; }
            public List<DAIField> correspondingDaiFields { get; }
        }

        public static EbxAssetContainer fromDAIEbx(DAIEbx file)
        {


            return null;
        }
    }
}
