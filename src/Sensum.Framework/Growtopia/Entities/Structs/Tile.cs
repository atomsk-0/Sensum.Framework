using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Proton;
using Sensum.Framework.Utils;

namespace Sensum.Framework.Growtopia.Entities.Structs;

public unsafe class Tile(ENetClient client)
{
    public short Index;
    public ushort Foreground, Background, ParentTileIndex;
    public Vector2Int Pos;
    public TileFlag Flags;
    public TileExtra? Extra;

    public void Serialize(byte* data, ref int offset, int dataSize)
    {
        Foreground = Memory.Read<ushort>(data, ref offset, dataSize);
        Background = Memory.Read<ushort>(data, ref offset, dataSize);
        ParentTileIndex = Memory.Read<ushort>(data, ref offset, dataSize);
        Flags = (TileFlag)Memory.Read<ushort>(data, ref offset, dataSize);

        if (ParentTileIndex > 0)
            ParentTileIndex = Memory.Read<ushort>(data, ref offset, dataSize);

        if (HasFlag(TileFlag.ExtraData))
        {
            var type = (TileExtraType)Memory.Read<byte>(data, ref offset, dataSize);
            Extra = new TileExtra(type);
            Extra.Serialize(data, ref offset, this, dataSize);
        }

        if (Foreground == 14666) // Surgeon Station
        {
            int stringLength = Memory.Read<int>(data, ref offset, dataSize);
            Memory.Skip(ref offset, stringLength);
        }

        if (Foreground == 14662)
        {
            Memory.Skip(ref offset, 32);
        }
    }

    public bool IsWorldLock => Foreground is 242 or 1796 or 2408 or 4428 or 4802 or 5814 or 5980 or 7188 or 8470 or 9640 or 10410 or 11550 or 13200;

    public void PlantTree(in ItemInfo info)
    {
        uint currentTime = MiscUtils.UtcUnixTimeStamp;

        Extra = new TileExtra(TileExtraType.Tree)
        {
            PlantTime = currentTime,
            ReadyTime = (uint)(currentTime + info.Time),
            Fruits = 0
        };
    }

    public bool IsLock()
    {
        if (Extra == null) return false;
        return Extra.Type == TileExtraType.Lock;
    }

    public bool HasAccess()
    {
        if (HasFlag(TileFlag.Public)) return true;
        if (client.World.WorldTileMap.Tiles == null) return false;
        Tile? worldLockTile = client.World.WorldTileMap.Tiles.FirstOrDefault(c => c.IsWorldLock);
        if (ParentTileIndex == 0)
        {
            if (worldLockTile?.Extra == null) return true;
            if (worldLockTile.Extra.HasAccessToLock(client.NetAvatar.UserId)) return true;
        }
        else
        {
            Tile parentTile = client.World.WorldTileMap.Tiles![ParentTileIndex];
            if (parentTile.IsLock() == false) return true;
            if (parentTile.Extra == null) return true;
            if (parentTile.Extra.HasAccessToLock(client.NetAvatar.UserId)) return true;
        }
        return false;
    }

    public bool IsCollideable()
    {
        // TODO Improve position checks
        ItemInfo? itemInfo = ItemInfoManager.GetItem(Foreground);
        if (itemInfo.HasValue == false) return true;
        switch (itemInfo.Value.CollisionType)
        {
            case TileCollisionType.Solid:
            {
                return true;
            }
            case TileCollisionType.Gateway:
            {
                return HasAccess() == false;
            }
            case TileCollisionType.OneWay:
            {
                if (HasFlag(TileFlag.Flipped)) //Facing left
                {
                    return client.NetAvatar.TilePos.X < Pos.X;
                }
                return client.NetAvatar.TilePos.X > Pos.X;
            }
            case TileCollisionType.JumpDown:
            {
                return client.NetAvatar.TilePos.Y > Pos.Y;
            }
            case TileCollisionType.IfOff:
            {
                return HasFlag(TileFlag.Enabled) == false;
            }
            case TileCollisionType.JumpThrough:
            {
                return client.NetAvatar.TilePos.Y < Pos.Y;
            }
            default:
            {
                return false;
            }
        }
    }

    public bool HasFlag(TileFlag flag)
    {
        return (Flags & flag) == flag;
    }

    public void SetFlag(TileFlag flag)
    {
        Flags |= flag;
    }

    public void ToggleFlag(TileFlag flag)
    {
        Flags ^= flag;
    }

    public void RemoveFlag(TileFlag flag)
    {
        Flags &= ~flag;
    }
}