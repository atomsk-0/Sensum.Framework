using System.Drawing;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;

namespace Sensum.Framework.Growtopia.Entities.Structs;

public readonly struct ItemInfo(ushort itemId, ushort rarity, string name, int time, Color color, ItemType type, TileCollisionType collosionType, string texture, int textureHash, Vector2Int texturePos)
{
    public readonly ushort Id = itemId;
    public readonly ushort Rarity = rarity;
    public readonly string Name = name;
    public readonly int Time = time;
    public readonly Color Color = color;
    public readonly ItemType Type = type;
    public readonly TileCollisionType CollisionType = collosionType;
    public readonly string Texutre = texture;
    public readonly int TexutreHash = textureHash;
    public readonly Vector2Int TexturePos = texturePos;
}