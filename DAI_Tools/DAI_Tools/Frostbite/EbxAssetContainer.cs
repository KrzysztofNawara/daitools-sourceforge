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
            BYTE,
            BOOL,
            SHORT,
            U_SHORT,
            INT,
            U_INT,
            LONG,
            LONGLONG,
            FLOAT,
            ENUM,
            STRING,
            GUID,
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

        abstract class ASimpleValue<T> : AValue
        {
            public ASimpleValue(ValueTypes type, T value) : base(type) { this.Val = value; }
            public T Val { get; }
        }

        class AByte : ASimpleValue<byte> { public AByte(byte v) : base(ValueTypes.BYTE, v) { } }
        class ABool : ASimpleValue<bool> { public ABool(bool v) : base(ValueTypes.BOOL, v) { } }
        class AShort : ASimpleValue<short> { public AShort(short v) : base(ValueTypes.SHORT, v) { } }
        class AUShort : ASimpleValue<ushort> { public AUShort(ushort v) : base(ValueTypes.U_SHORT, v) { } }
        class AInt : ASimpleValue<int> { public AInt(int v) : base(ValueTypes.INT, v) { } }
        class AUInt : ASimpleValue<uint> { public AUInt(uint v) : base(ValueTypes.U_INT, v) { } }
        class ALong : ASimpleValue<long> { public ALong(uint v) : base(ValueTypes.LONG, v) { } }
        class ALongLong : ASimpleValue<byte[]> { public ALongLong(byte[] v) : base(ValueTypes.LONGLONG, v) { } } // @todo add toString!
        class AFloat : ASimpleValue<float> { public AFloat(float v) : base(ValueTypes.FLOAT, v) { } }
        class AEnum : ASimpleValue<String> { public AEnum(String v) : base(ValueTypes.ENUM, v) { } }
        class AString : ASimpleValue<String> { public AString(String v) : base(ValueTypes.STRING, v) { } }

        class AGuid : ASimpleValue<String> { public AGuid(String v) : base(ValueTypes.GUID, v) { } }

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
