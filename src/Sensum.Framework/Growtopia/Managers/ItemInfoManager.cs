using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Cysharp.Text;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Proton;
using Sensum.Framework.Utils;

namespace Sensum.Framework.Growtopia.Managers;

public static class ItemInfoManager
{
    public static ItemInfo[]? Items;
    internal static bool ItemsLoaded;
    internal static bool LoadingItems;

    // local items.dat for s.f should be always in same folder as exe
    private static readonly string cached_items_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "items.dat");
    private const byte format_version = 18;
    private static int offset;

    public static bool HasItem(ushort itemId) => Items?.Any(c => c.Id == itemId) ?? false;
    public static ItemInfo? GetItem(ushort itemId) => HasItem(itemId) ? Items?.First(c => c.Id == itemId) : null;

    /// <summary>
    /// Recommended to load cached one as socks5 proxies are slow/have data usage limit.
    /// </summary>
    public static unsafe bool TryLoadFromLocal()
    {
        if (ItemsLoaded) return true;
        if (File.Exists(cached_items_path) == false) return false;
        byte[] data = File.ReadAllBytes(cached_items_path);
        fixed (byte* ptr = data)
        {
            LoadFromMem(ptr, data.Length, true);
        }
        return Items is { Length: > 10 };
    }

    public static unsafe void LoadFromMem(byte* data, int length, bool dontFree = false)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        ushort formatVersion = Memory.Read<ushort>(data, ref offset, length);
        if (formatVersion > format_version)
        {
            if (File.Exists(cached_items_path))
            {
                byte[] fileData = File.ReadAllBytes(cached_items_path);
                fixed (byte* ptr = fileData)
                {
                    LoadFromMem(ptr, fileData.Length, true);
                }
                return;
            }
            throw new Exception("Unsupported item format version");
        }
        int itemCount = Memory.Read<int>(data, ref offset, length);
        Items = new ItemInfo[itemCount];
        for (int i = 0; i < itemCount; i++)
        {
            int id = Memory.Read<int>(data, ref offset, length);
            Memory.Skip(ref offset, 2);
            ItemType type = (ItemType)Memory.Read<byte>(data, ref offset, length);
            Memory.Skip(ref offset, 1);
            ReadOnlySpan<char> name = readStringEncrypted(data, id, length);
            ushort textureLength = Memory.Read<ushort>(data, ref offset, length);
            var textureBuilder = new StringBuilder(textureLength);
            for (int j = 0; j < textureLength; j++)
            {
                textureBuilder.Append((char)Memory.Read<byte>(data, ref offset, length));
            }
            string texture = textureBuilder.ToString();
            int textureHash = Memory.Read<int>(data, ref offset, length);
            Memory.Skip(ref offset, 5);
            Vector2Int texturePos = new Vector2Int(Memory.Read<byte>(data, ref offset, length), Memory.Read<byte>(data, ref offset, length));
            Memory.Skip(ref offset, 2);
            TileCollisionType collisionType = (TileCollisionType)Memory.Read<byte>(data, ref offset, length);
            Memory.Skip(ref offset, 6);
            ushort rarity = Memory.Read<ushort>(data, ref offset, length);
            Memory.Skip(ref offset, 1);
            skipString(data, length);
            Memory.Skip(ref offset, 8);
            skipString(data, length);
            skipString(data, length);
            skipString(data, length);
            skipString(data, length);
            Memory.Skip(ref offset, 4);

            int seedColor = Memory.Read<int>(data, ref offset, length);
            Memory.Skip(ref offset, 8);
            int growTime = Memory.Read<int>(data, ref offset, length);
            Memory.Skip(ref offset, 4);
            skipString(data, length);
            skipString(data, length);
            skipString(data, length);
            Memory.Skip(ref offset, 80);

            skipString(data, length);
            Memory.Skip(ref offset, 46);
            skipString(data, length);
            skipString(data, length);
            Memory.Skip(ref offset, 8);

            byte[] colorBytes = BitConverter.GetBytes(seedColor);
            Items[i] = new ItemInfo((ushort)id, rarity, name.ToString(), growTime, Color.FromArgb(colorBytes[1], colorBytes[2], colorBytes[3]), type, collisionType, texture, textureHash, texturePos);
        }
        LoadingItems = false;

        if (File.Exists(cached_items_path))
            File.Delete(cached_items_path);

        using UnmanagedMemoryStream stream = new UnmanagedMemoryStream(data, length);
        using FileStream fileStream = new FileStream(cached_items_path, FileMode.Create, FileAccess.Write);

        stream.CopyTo(fileStream);
        stream.Close();
        fileStream.Close();

        if (dontFree == false)
        {
            Marshal.FreeHGlobal((IntPtr)data);
        }

        ItemsLoaded = true;
    }

    private static unsafe void skipString(byte* data, int dataSize)
    {
        ushort length = Memory.Read<ushort>(data, ref offset, dataSize);
        Memory.Skip(ref offset, length);
    }

    private static unsafe ReadOnlySpan<char> readStringEncrypted(byte* data, int itemId, int dataSize)
    {
        const string secret = "PBG892FXX982ABC*";
        ushort length = Memory.Read<ushort>(data, ref offset, dataSize);
        using var builder = ZString.CreateStringBuilder(true);
        for (int i = 0; i < length; i++)
        {
            builder.Append((char)(Memory.Read<byte>(data, ref offset, dataSize) ^ secret[(i + itemId) % secret.Length]));
        }
        return builder.ToString();
    }
}