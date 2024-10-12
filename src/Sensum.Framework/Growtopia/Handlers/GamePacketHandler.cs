using System.Numerics;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Proton;
using Sensum.Framework.Utils;
using static Sensum.Framework.Proton.ENet;

namespace Sensum.Framework.Growtopia.Handlers;

internal static unsafe class GamePacketHandler
{
    internal static void Handle(ENetClient client, ENetPacket* packet)
    {
        GameUpdatePacket* tankPacket = (GameUpdatePacket*)PacketManager.GetStructPointerFromTankPacket(packet);
        if (tankPacket == (GameUpdatePacket*)0x0) return;
        switch (tankPacket->Type)
        {
            case GamePacketType.State:
                onState(client, tankPacket);
                break;
            case GamePacketType.CallFunction:
                onCallFunction(client, tankPacket);
                break;
            case GamePacketType.TileChangeRequest:
                onTileChangeRequest(client, tankPacket);
                break;
            case GamePacketType.SendMapData:
                onSendMapData(client, tankPacket);
                break;
            case GamePacketType.SendTileUpdateData:
                onSendTileUpdateData(client, tankPacket);
                break;
            case GamePacketType.SendTileUpdateDataMultiple:
                onSendTileUpdateDataMultiple(client, tankPacket);
                break;
            case GamePacketType.TileApplyDamage:
                onTileApplyDamage(client, tankPacket);
                break;
            case GamePacketType.SendInventoryState:
                onSendInventoryState(client, tankPacket);
                break;
            case GamePacketType.SendTileTreeState:
                onSendTileTreeState(client, tankPacket);
                break;
            case GamePacketType.ModifyItemInventory:
                onModifyItemInventory(client, tankPacket);
                break;
            case GamePacketType.ItemChangeObject:
                onitemChangeObject(client, tankPacket);
                break;
            case GamePacketType.SendLock:
                onSendLock(client, tankPacket);
                break;
            case GamePacketType.SendItemDatabaseData:
                onSendItemDatabaseData(client, tankPacket);
                break;
            case GamePacketType.SendParticleEffect:
                onSendParticleEffect(client, tankPacket);
                break;
            case GamePacketType.SetIconState:
                onSetIconState(client, tankPacket);
                break;
            case GamePacketType.PingRequest:
                onPingRequest(client, tankPacket);
                break;
        }
        client.GameUpdatePacketReceivedCallback?.Invoke(new GameUpdatePacketDelegate(tankPacket));
    }

    private static void onState(ENetClient client, GameUpdatePacket* packet)
    {
        var player = client.NetObjectManager.GetPlayerByNetId(packet->NetId);
        player?.UpdateState(packet);
        if (player == null) return;
       // BotDetector.MovementCheck(player);
    }

    private static void onCallFunction(ENetClient client, GameUpdatePacket* packet)
    {
        VariantFunctionHandler.Handle(client, packet);
    }

    private static void onTileChangeRequest(ENetClient client, GameUpdatePacket* packet)
    {
        if (client.World.Loaded == false) return;
        client.World.WorldTileMap.SetTile(packet->TilePos, (ushort)packet->Value);

        if (packet->NetId == client.NetAvatar.NetId && packet->Value != 18)
        {
            client.PlayerItems.Remove((ushort)packet->Value, 1);
        }
    }

    private static void onSendMapData(ENetClient client, GameUpdatePacket* packet)
    {
        client.NetObjectManager.Reset(); // May cause issues calling here as if onSpawn functions arrive before this packet gets sent to client
        client.BotDetector.Reset();
        byte* extended = (byte*)PacketManager.GetExtendedDataPointerFromTankPacket(packet);
        client.World.LoadFromMem(client, extended, packet->ExtraDataSize);
    }


    private static void onSendTileUpdateData(ENetClient client, GameUpdatePacket* packet)
    {
        if (client.World.Loaded == false) return;
        byte* extended = (byte*)PacketManager.GetExtendedDataPointerFromTankPacket(packet);
        int offset = 0;
        client.World.WorldTileMap.ModifyTileByPos(packet->TilePos, tile => tile.Serialize(extended, ref offset, packet->ExtraDataSize));
    }

    private static void onSendTileUpdateDataMultiple(ENetClient client, GameUpdatePacket* packet)
    {
        if (client.World.Loaded == false) return;
        // Implement this
    }

    private static void onTileApplyDamage(ENetClient client, GameUpdatePacket* packet)
    {
        if (client.World.Loaded == false) return;
        // Implement this
    }

    private static void onSendInventoryState(ENetClient client, GameUpdatePacket* packet)
    {
        byte* extended = (byte*)PacketManager.GetExtendedDataPointerFromTankPacket(packet);
        client.PlayerItems.Serialize(extended, packet->ExtraDataSize);
    }


    private static void onSendTileTreeState(ENetClient client, GameUpdatePacket* packet)
    {
        if (client.World.Loaded == false) return;
        client.World.WorldTileMap.SetTile(packet->TilePos, 18);
    }

    private static void onModifyItemInventory(ENetClient client, GameUpdatePacket* packet)
    {
        if (*(byte*)&packet->Padding2 == 0)
        {
            client.PlayerItems.Add((ushort)packet->Value, *(byte *)((long)&packet->Padding2 + 1));
            return;
        }
        client.PlayerItems.Remove((ushort)packet->Value, *(byte*)&packet->Padding2);
    }

    private static void onitemChangeObject(ENetClient client, GameUpdatePacket* packet)
    {
        if (client.World.Loaded == false) return;

        if (packet->WorldPos == Vector2.Zero && packet->NetId == client.NetAvatar.NetId)
        {
            WorldObject? worldObject = client.World.WorldObjectMap.GetWorldObjectByObjectId((uint)packet->Value);
            if (worldObject.HasValue)
            {
                client.PlayerItems.Add(worldObject.Value.ItemId, worldObject.Value.Count);
            }
        }

        switch (packet->NetId)
        {
            case -3: // Modifies existing worldObject
            {
                client.World.WorldObjectMap.ModifyWorldObjectByObjectId((uint)packet->SecondaryNetId, (ref WorldObject worldObject) =>
                {
                    worldObject.ItemId = (ushort)packet->Value;
                    worldObject.Count = (byte)packet->Flags;
                    worldObject.Pos = packet->WorldPos;
                    worldObject.Flags = packet->Padding1;
                });
                break;
            }
            case -1: // Insert new worldObject
            {
                client.World.WorldObjectMap.Add(new WorldObject
                {
                    ItemId = (ushort)packet->Value,
                    Pos = packet->WorldPos,
                    Count = (byte)packet->Flags,
                    Flags = packet->Padding1,
                    ObjectId = ++client.World.WorldObjectMap.IdTracker,
                });
                break;
            }
            default: // Remove worldObject
            {
                client.World.WorldObjectMap.RemoveByObjectId((uint)packet->Value);
                break;
            }
        }
    }

    private static void onSendLock(ENetClient client, GameUpdatePacket* packet)
    {
        if (client.World.Loaded == false) return;
        var tile = client.World.WorldTileMap.GetTileByPos(packet->TilePos);
        if (tile == null) return;
        if (tile.IsLock() == false || tile.Extra == null || tile.Extra.Owner != packet->NetId)
        {
            client.World.WorldTileMap.ModifyTileByPos(packet->TilePos, tileRef =>
            {
                tileRef.Extra = new TileExtra(TileExtraType.Lock) { Owner = (uint)packet->NetId };
            });
        }
        //TODO Refresh/Set tileparents /* WorldTileMap::ApplyLockFromGamePacket(GameUpdatePacket*) */
    }

    private static void onSendItemDatabaseData(ENetClient client, GameUpdatePacket* packet)
    {
        byte* extended = (byte*)PacketManager.GetExtendedDataPointerFromTankPacket(packet);
        if (packet->Value < 1)
        {
            return;
        }
        byte* decompressedData = ResourceUtils.ZLibInflateToMemory(extended, packet->ExtraDataSize, packet->Value);
        ItemInfoManager.LoadFromMem(decompressedData, packet->Value);
    }

    private static void onSendParticleEffect(ENetClient client, GameUpdatePacket* packet)
    {
        if (packet->Velocity.Y == 1122238464)
        {
            // Geiger stuff
            int signalColor = packet->Velocity.X;
            switch (signalColor)
            {
                case GameConstants.RED_GEIGER_SIGNAL:
                    client.GeigerSignalChangedCallback?.Invoke(GeigerSignal.Red);
                    break;
                case GameConstants.YELLOW_GEIGER_SIGNAL:
                    client.GeigerSignalChangedCallback?.Invoke(GeigerSignal.Yellow);
                    break;
                case GameConstants.GREEN_GEIGER_SIGNAL:
                    var signal = client.TimeSinceLastGeigerSignal is > 0 and < 1400 ? GeigerSignal.RapidGreen : GeigerSignal.Green;
                    client.GeigerSignalChangedCallback?.Invoke(signal);
                    break;
            }

            client.TimeSinceLastGeigerSignal = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    private static void onSetIconState(ENetClient client, GameUpdatePacket* packet)
    {
        var player = client.NetObjectManager.GetPlayerByNetId(packet->NetId);
        player?.OnSetIconState((IconState)packet->TilePos.X, client);
    }

    private static void onPingRequest(ENetClient client, GameUpdatePacket* packet)
    {
        var pingPacket = new GameUpdatePacket
        {
            Type = GamePacketType.PingReply,
            SecondaryNetId = (int)MiscUtils.HashBytes(BitConverter.GetBytes(packet->Value)),
            Value = packet->Value,
            WorldPos = new Vector2(64f, 64f), // Build range
            TilePos = new Vector2Int(1000, 250) // Velocity/Speed, Gravity
        };
        client.SendPacket(pingPacket);
    }
}