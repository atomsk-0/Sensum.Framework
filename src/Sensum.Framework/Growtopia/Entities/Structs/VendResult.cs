namespace Sensum.Framework.Growtopia.Entities.Structs;

public readonly struct VendResult(VendResultType type, string message, int quanity)
{
    public readonly VendResultType Type = type;
    public readonly string Message = message;
    public readonly int Quanity = quanity;
}

public enum VendResultType
{
    None,
    Success,
    OutOfStock,
    Other,
}