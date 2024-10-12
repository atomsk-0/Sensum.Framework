namespace Sensum.Framework.Growtopia.Entities.Enums;

public enum NetMessageType : uint
{
    Unknown = 0, // Hex[0x0], Decimal[0]
    ServerHello, // Hex[0x1], Decimal[1]
    GenericText, // Hex[0x2], Decimal[2]
    GameMessage, // Hex[0x3], Decimal[3]
    GamePacket, // Hex[0x4], Decimal[4]
    Error, // Hex[0x5], Decimal[5]
    Track, // Hex[0x6], Decimal[6]
    LogRequest, // Hex[0x7], Decimal[7]
    LogResponse // Hex[0x8], Decimal[8]
}