using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Proton;
using static Sensum.Framework.Proton.ENet;

namespace Sensum.Framework.Growtopia.Handlers;

internal static unsafe class NetMessageHandler
{
    internal static void Handle(ENetClient client, ENetPacket* packet)
    {
        NetMessageType type = PacketManager.GetMessageTypeFromPacket(packet);
        switch (type)
        {
            case NetMessageType.ServerHello:
                onServerHello(client);
                break;
            case NetMessageType.GameMessage:
                onGameMessage(client, packet);
                break;
            case NetMessageType.GamePacket:
                onGamePacket(client, packet);
                break;
            case NetMessageType.Track:
                onTrack(client, packet);
                break;
        }
        client.NetMessageReceivedCallback?.Invoke(new NetMessageDelegate(type, packet));
    }

    private static void onServerHello(ENetClient client)
    {
        client.SendGenericText(client.LoginBuilder.Build(client));
    }

    private static void onGameMessage(ENetClient client, ENetPacket* packet)
    {
        string text = PacketManager.GetTextPointerFromPacket(packet);
        if (text.Contains("action|log"))
        {
            using var textScanner = new TextScanner(text);
            if (textScanner.Contains("msg"))
            {
                client.ConsoleManager.Append(textScanner.Get<string>("msg"), client.FeatureFlags);
            }
        }
    }

    private static void onGamePacket(ENetClient client, ENetPacket* packet)
    {
        GamePacketHandler.Handle(client, packet);
    }

    private static void onTrack(ENetClient client, ENetPacket* packet)
    {
        SessionHandler.Handle(client, packet);
    }
}