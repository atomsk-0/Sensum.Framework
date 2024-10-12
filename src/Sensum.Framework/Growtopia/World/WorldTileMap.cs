using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Utils.Extensions;

namespace Sensum.Framework.Growtopia.World;

public unsafe class WorldTileMap : IResourceLifecycle
{
    public Tile[]? Tiles;

    private ENetClient? client;

    public bool Serialize(ENetClient clientP, byte* data, ref int offset, int dataSize, WorldMap worldMap)
    {
        client ??= clientP;
        int length = worldMap.Width * worldMap.Height;
        Tiles = new Tile[length];
        for (int i = 0; i < length; i++)
        {
            var tile = new Tile(client) {Index = (short)i , Pos = new Vector2Int(i % worldMap.Width, i / worldMap.Width)};
            tile.Serialize(data, ref offset, dataSize);
            if (ItemInfoManager.HasItem(tile.Foreground) == false)
            {
                Console.WriteLine($"Invalid item {tile.Foreground} index: {i}, pos: {tile.Pos}");
                return false;
            }
            Tiles[i] = tile;
        }
        return true;
    }


    public Tile? GetTileByPos(Vector2Int pos)
    {
        return Tiles?.FirstOrDefault(c => c.Pos == pos) ?? null;
    }

    public void SetTile(Vector2Int pos, ushort value)
    {
        var itemInfo = ItemInfoManager.GetItem(value);
        if (itemInfo.HasValue == false) return;
        ModifyTileByPos(pos, tile =>
        {
            switch (itemInfo.Value.Type)
            {
                case ItemType.Fist:
                {
                    if (tile.Foreground != 0)
                    {
                        tile.Foreground = 0;
                        tile.Extra = null;
                        break;
                    }
                    tile.Background = 0;
                    break;
                }
                case ItemType.Background:
                {
                    tile.Background = value;
                    break;
                }
                case ItemType.Seed:
                {
                    tile.Foreground = value;
                    tile.PlantTree(itemInfo.Value);
                    break;
                }
                default:
                {
                    tile.Foreground = value;
                    if (value == 0x1268)
                    {
                        tile.SetFlag(TileFlag.Enabled);
                    }
                    break;
                }
            }
        });
    }

    public void ModifyTileByPos(Vector2Int pos, Action<Tile> modifyAction)
    {
        if (Tiles == null) return;
        int index = Tiles.FindIndex(c => c.Pos == pos);
        if (index < 0) return;
        var tile = Tiles[index];
        var copy = new Tile(client!)
        {
            Foreground = tile.Foreground,
            Background = tile.Background,
            Extra = tile.Extra,
            Flags = tile.Flags,
            Index = tile.Index,
            ParentTileIndex = tile.ParentTileIndex,
            Pos = tile.Pos
        };
        modifyAction(tile);
        TileModifiedCallback?.Invoke(copy, tile);
    }

    public void Reset()
    {
        if (Tiles != null)
        {
            Array.Clear(Tiles, 0, Tiles.Length);
            Tiles = null;
        }
    }

    public void Destroy()
    {
        Reset();
        TileModifiedCallback = null;
    }

    public Action<Tile, Tile>? TileModifiedCallback;
}