using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Proton;

namespace Sensum.Framework.Growtopia.Managers;

public static unsafe class PacketManager
{
    private const ushort stackalloc_limit = 1048; //1KB

    private const int max_packet_size = 0xF4240;
    private const byte message_type_size = sizeof(NetMessageType);

    private static readonly Random rand = new();

    /* GetMessageTypeFromPacket(_ENetPacket*) */
    internal static NetMessageType GetMessageTypeFromPacket(ENetPacket* packet)
    {
        return packet->GetDataLength < 4 ? NetMessageType.Unknown : (NetMessageType)(*packet->Data);
    }

    /* GetTextPointerFromPacket(_ENetPacket*) */
    public static string GetTextPointerFromPacket(ENetPacket* packet)
    {
        if (packet->GetDataLength <= 5) return "";

        int length = packet->GetDataLength - 5;
        byte* strBuffer = packet->Data + 4;

        ReadOnlySpan<byte> span = new(strBuffer, length);

        return Encoding.UTF8.GetString(span);
    }

    /* GetExtendedDataPointerFromTankPacket(GameUpdatePacket*) */
    internal static GameUpdatePacket* GetExtendedDataPointerFromTankPacket(GameUpdatePacket* gameUpdatePacket)
    {
        var extendedDataPointer = (GameUpdatePacket*)0x0;
        if ((*(byte*)&gameUpdatePacket->CharacterState & 8) != 0)
        {
            extendedDataPointer = gameUpdatePacket + 1;
        }
        return extendedDataPointer;
    }

    /* GetStructPointerFromTankPacket(_ENetPacket*) */
    internal static byte* GetStructPointerFromTankPacket(ENetPacket* packet)
    {
        const int minimum_data_length = 0x3b;
        const int struct_data_offset = 4;
        const int extended_packet_flag_offset = 0x10;
        const int extended_packet_size_offset = 0x38;
        const int extended_packet_size_addition = 0x3c;

        if (packet->dataLength < minimum_data_length)
        {
            return null;
        }

        byte* tankPacket = (byte*)packet->data;

        if ((*(tankPacket + extended_packet_flag_offset) & 0x08) == 0)
        {
            *(uint*)(tankPacket + extended_packet_size_offset) = 0;
        }
        else if (*(uint*)(tankPacket + extended_packet_size_offset) + extended_packet_size_addition > packet->dataLength)
        {
            return null;
        }

        return tankPacket + struct_data_offset;
    }


    public static void SendGenericText(this ENetClient client, string text)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        client.SendPacket(NetMessageType.GenericText, text);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public static void SendGameMessage(this ENetClient client, string text)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        client.SendPacket(NetMessageType.GameMessage, text);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Obsolete("Use scoped type functions instead: SendGenericText, SendGameMessage for string data packets.")]
    public static void SendPacket(this ENetClient client, NetMessageType type, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        fixed (byte* pAsciiBuffer = Encoding.ASCII.GetBytes(text))
        {
            sendPacketRaw(client, type, pAsciiBuffer, Encoding.ASCII.GetByteCount(text));
        }
        client.NetMessageSentCallback?.Invoke(type, text);
    }

    public static void SendPacket(this ENetClient client, GameUpdatePacket packet)
    {
        // This doesn't require check as it's always 56 bytes
        byte* buffer = stackalloc byte[sizeof(GameUpdatePacket)];
        *(GameUpdatePacket*)buffer = packet;
        sendPacketRaw(client, NetMessageType.GamePacket, buffer, sizeof(GameUpdatePacket));
        client.GamePacketSentCallback?.Invoke(packet);
    }

    private static void sendPacketRaw(this ENetClient client, NetMessageType type, void* raw, int length, ENetPacketFlag flag = ENetPacketFlag.Reliable)
    {
        if (client.Peer == 0 || length > max_packet_size) return;

        int totalPacketSize = length + message_type_size;

        if (totalPacketSize <= stackalloc_limit)
        {
            byte* packetData = (byte*)NativeMemory.Alloc((nuint)totalPacketSize);
            *(int*)packetData = (int)type;
            Unsafe.CopyBlock(packetData + message_type_size, (byte*)raw, (uint)length);

            var packet = ENet.CreatePacket(new nint(packetData), unchecked((nuint)totalPacketSize), flag);
            ENet.Send(client.Peer, (byte)rand.Next(0, 1), packet);
            NativeMemory.Free(packetData);
        }
        else
        {
            byte* packetData = stackalloc byte[totalPacketSize];
            *(int*)packetData = (int)type;
            Unsafe.CopyBlock(packetData + message_type_size, (byte*)raw, (uint)length);

            var packet = ENet.CreatePacket(new nint(packetData), unchecked((nuint)totalPacketSize), flag);
            ENet.Send(client.Peer, (byte)rand.Next(0, 1), packet);
        }

        /*const int max_packet_size = 0xF4240;
        const byte message_type_size = sizeof(NetMessageType);

        if (client.Peer == 0 || length > max_packet_size) return;

        int totalPacketSize = length + message_type_size;

        byte* packetData;
        if (totalPacketSize <= stackalloc_limit)
        {
            packetData = (byte*)NativeMemory.Alloc((UIntPtr)totalPacketSize);
        }
        else
        {
            byte* packetDataStack = stackalloc byte[totalPacketSize];
            packetData = packetDataStack;
        }

        *(int*)packetData = (int)type;
        Unsafe.CopyBlock(packetData + message_type_size, (byte*)raw, (uint)length);

        var packet = enet_packet_create(new nint(packetData), unchecked((nuint)totalPacketSize), flag);
        enet_peer_send(client.Peer, (byte)rand.Next(0, 1), packet);

        if (totalPacketSize <= stackalloc_limit)
        {
            NativeMemory.Free(packetData);
        }*/
    }
}