

namespace VoxelWorld.IO.NamedBinaryTags;

public enum BinaryTagType : byte
{
    Empty = 0,
    //Unmanaged
    Byte = 1,
    SByte = 2,
    Short = 3,
    UShort = 4,
    Int = 5,
    UInt = 6,
    Long = 7,
    ULong = 8,
    Float = 9,
    Double = 10,
    Decimal = 11,
    Bool = 12,
    Char = 13,

    //String
    String = 14,

    //Collections
    Compound = 15,
    List = 16,
}
