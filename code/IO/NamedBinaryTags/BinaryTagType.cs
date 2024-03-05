

namespace Sandcube.IO.NamedBinaryTags;

public enum BinaryTagType : byte
{
    //Unmanaged
    Byte = 0,
    SByte = 1,
    Short = 2,
    UShort = 3,
    Int = 4,
    UInt = 5,
    Long = 6,
    ULong = 7,
    Float = 8,
    Double = 9,
    Decimal = 10,
    Bool = 11,
    Char = 12,

    //String
    String = 13,

    //Complex
    Compound = 14,
    List = 15,
}
