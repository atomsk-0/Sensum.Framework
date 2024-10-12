using System.Numerics;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;

namespace Sensum.Framework.Growtopia.Entities.Structs;

public struct GameUpdatePacket // Size: 0x38 (56 in decimal) bytes
{
    public GamePacketType Type; // Offset: 0x0 (0 in decimal)
    public byte Padding1; // Offset: 0x1 (1 in decimal)
    public ushort Padding2; // Offset: 0x2 (2 in decimal)
    public int NetId; // Offset: 0x4 (4 in decimal)
    public int SecondaryNetId; // Offset: 0x8 (8 in decimal)
    public int CharacterState; // Offset: 0xC (12 in decimal)
    public float Flags; // Offset: 0x10 (16 in decimal)
    public int Value; // Offset: 0x14 (20 in decimal)
    public Vector2 WorldPos; // Offset: 0x18 (24 in decimal) for X and Offset: 0x1C (28 in decimal) for Y
    public Vector2Int Velocity; // Offset: 0x20 (32 in decimal) for X and Offset: 0x24 (36 in decimal) for Y
    public int Padding3; // Offset: 0x28 (40 in decimal)
    public Vector2Int TilePos; // Offset: 0x2C (44 in decimal) for X and Offset: 0x30 (48 in decimal) for Y
    public int ExtraDataSize; // Offset: 0x34 (52 in decimal)
}