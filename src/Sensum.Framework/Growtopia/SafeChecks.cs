using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Proton;
using Sensum.Framework.Utils.Extensions;

namespace Sensum.Framework.Growtopia;

public static class SafeChecks
{
    public static bool PositionCheck(ENetClient client, Vector2Int targetPos)
    {
        if (client.World.Loaded == false) return false;
        if (targetPos.X < 0 || targetPos.Y < 0) return false;
        if (targetPos.X >= client.World.Width * 32 || targetPos.Y >= client.World.Height * 32) return false;
        if (MiscUtils.IsInside(client.NetAvatar.TilePos, 8, targetPos) == false) return false;
        if (client.World.WorldTileMap.Tiles == null) return false;
        var tile = client.World.WorldTileMap.Tiles.FirstOrDefault(c => c.Pos == targetPos);
        if (tile == null) return false;
        return tile.IsCollideable() == false;
    }

    public static bool DropCheck(ENetClient client, ushort itemId, byte amount)
    {
        if (client.PlayerItems.HasItem(itemId) == false) return false;
        if (client.PlayerItems.GetItemCount(itemId) < amount) return false;
        if (client.World.Loaded == false) return false;
        var pos = client.NetAvatar.TilePos;
        var tilePos = client.NetAvatar.IsFacingLeft ? pos + new Vector2Int(-1, 0) : pos + new Vector2Int(1, 0);
        if (tilePos.X >= client.World.Width || tilePos.X < 0) return false;
        if (tilePos.Y >= client.World.Height || tilePos.Y < 0) return false;
        var tile = client.World.WorldTileMap.GetTileByPos(tilePos);
        if (tile == null) return false;
        return tile.IsCollideable() == false;
    }

    public static bool CollectCheck(ENetClient client, in WorldObject worldObject)
    {
        if (client.World.Loaded == false) return false;
        Tile? tile = client.World.WorldTileMap.GetTileByPos(worldObject.Pos.ToTilePosition());
        if (tile == null) return false;
        if (tile.IsCollideable()) return false;
        if (client.PlayerItems.GetItemCount(worldObject.ItemId) == 200) return false;
        if (client.PlayerItems.Items == null) return false;
        if (MiscUtils.IsInside(worldObject.Pos, 256, client.NetAvatar.Pos) == false) return false;
        return true;
    }

    public static bool PunchCheck(ENetClient client, Vector2Int pos)
    {
        return MiscUtils.IsInside(client.NetAvatar.TilePos, 4, pos);
    }

    public static bool PlaceCheck(ENetClient client, Vector2Int pos, ushort itemId)
    {
        return client.PlayerItems.HasItem(itemId) && MiscUtils.IsInside(client.NetAvatar.TilePos, 4, pos);
    }
}