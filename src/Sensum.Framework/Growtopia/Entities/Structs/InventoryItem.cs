using Sensum.Framework.Growtopia.Entities.Enums;

namespace Sensum.Framework.Growtopia.Entities.Structs;

public struct InventoryItem
{
    public ushort Id;
    public byte Count;
    public InventoryItemFlags Flags;
}