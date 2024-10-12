using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Proton;

namespace Sensum.Framework.Growtopia.Entities.Structs;

public readonly unsafe struct NetMessageDelegate(NetMessageType type, ENetPacket* packet)
{
    public readonly NetMessageType Type = type;
    public readonly ENetPacket* Packet = packet;
}

public readonly unsafe struct GameUpdatePacketDelegate(GameUpdatePacket* packet)
{
    public readonly GameUpdatePacket* Packet = packet;
}