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
            GUID_REF,
            STRUCT,
            ARRAY,
        }

        abstract class AValue<T>
        {
            public AValue(ValueTypes type) { this.Type = type; }
            public ValueTypes Type { get; }
        }

        abstract class ASimpleValue<T> : AValue<T>
        {
            public ASimpleValue(ValueTypes type, T value) : base(type) { this.Val = value; }
            public T Val { get; }
        }

        abstract class AComplexValue<T> : AValue<T>
        {
            public AComplexValue(ValueTypes type, T value, DAIComplex ebxRef) : base(type) { this.ebxRef = ebxRef; }
            private DAIComplex ebxRef;
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



        public static EbxAssetContainer fromDAIEbx(DAIEbx file)
        {


            return null;
        }
    }
}
