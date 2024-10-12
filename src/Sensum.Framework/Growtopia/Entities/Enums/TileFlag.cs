namespace Sensum.Framework.Growtopia.Entities.Enums;

[Flags]
public enum TileFlag : ushort
{
    None = 0x0,
    ExtraData = 0x1,
    Locked = 0x2,
    SplicedTree = 0x4,
    TreeWillDropSeed = 0x8,
    Tree = 0x10,
    Flipped = 0x20,
    Enabled = 0x40,
    Public = 0x80,
    ExtraFrame = 0x100,
    Silenced = 0x200,
    Water = 0x400,
    Glue = 0x800,
    Fire = 0x1000,
    Red = 0x2000,
    Green = 0x4000,
    Blue = 0x8000
}