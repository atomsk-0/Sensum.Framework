namespace Sensum.Framework.Growtopia.Entities.Structs;

public readonly struct SafeVaultItem(string buttonId, ushort itemId, byte amount)
{
    public readonly string ButtonId = buttonId;
    public readonly ushort ItemId = itemId;
    public readonly byte Amount = amount;
}