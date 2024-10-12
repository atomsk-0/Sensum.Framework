using System.Numerics;
using Sensum.Framework.Utils;

namespace Sensum.Framework.Growtopia.Entities.Structs;

public unsafe struct WorldObject
{
    public Vector2 Pos; //0x18 for X and 0x1C for Y
    public ushort ItemId; //0x20
    public byte Count; //0x22
    public byte Flags; //0x23 // TODO Create enum for this
    public uint ObjectId; //0x24

    public void Serialize(byte* data, ref int offset, int dataSize)
    {
        ItemId = Memory.Read<ushort>(data, ref offset, dataSize);
        Pos = Memory.Read<Vector2>(data, ref offset, dataSize);
        Count = (byte)Memory.Read<ushort>(data, ref offset, dataSize);
        ObjectId = Memory.Read<uint>(data, ref offset, dataSize);
    }
}