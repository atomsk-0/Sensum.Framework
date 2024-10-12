using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Utils;

namespace Sensum.Framework.Growtopia.World;

public unsafe class WorldMap : IResourceLifecycle
{
    public readonly WorldTileMap WorldTileMap = new();
    public readonly WorldObjectMap WorldObjectMap = new();

    public string? Name;
    public string? DoorId;
    public ushort Version;
    public byte Width, Height;
    public bool Loaded;
    public bool Failed;

    private int offset;

    #if DEBUG
    private byte* cachedData;
    private int cachedDataLength;

    public void LoadFromCache(ENetClient client)
    {
        LoadFromMem(client, cachedData, cachedDataLength);
    }
    #endif




    public void LoadFromMem(ENetClient client, byte* data, int dataSize)
    {
        try
        {
            #if DEBUG
            if (cachedDataLength != dataSize)
            {
                if (cachedData != null)
                {
                    NativeMemory.Free(cachedData);
                }

                cachedData = (byte*)NativeMemory.Alloc((UIntPtr)dataSize);
                Unsafe.CopyBlock(cachedData, data, (uint)dataSize);
                cachedDataLength = dataSize;
            }
            #endif

            Reset();

            Version = Memory.Read<ushort>(data, ref offset, dataSize);
            Memory.Skip(ref offset, 4);
            Name = Memory.ReadString(data, ref offset, dataSize);
            Width = (byte)Memory.Read<int>(data, ref offset, dataSize);
            Height = (byte)Memory.Read<int>(data, ref offset, dataSize);

            Memory.Skip(ref offset, 9);

            if (WorldTileMap.Serialize(client, data, ref offset, dataSize, this) == false)
            {
                LoadFailedCallback?.Invoke("Failed to load world map (TileMap)");
                Reset();
                Failed = true;
                return;
            }

            Memory.Skip(ref offset, 12);

            if (WorldObjectMap.Serialize(data, ref offset, dataSize) == false)
            {
                LoadFailedCallback?.Invoke("Failed to load world map (ObjectMap)");
                Reset();
                Failed = true;
                return;
            }

            // TODO: Weather, currently unneeded
            // default weather = 2, skip 2, activeWeather = 2

            JoinedWorldCallback?.Invoke();
            Loaded = true;
        }
        catch
        {
            LoadFailedCallback?.Invoke("Failed to load world map (Catched exception)");
            Reset();
            Failed = true;
        }
    }

    public void Reset()
    {
        WorldTileMap.Reset();
        WorldObjectMap.Reset();
        Name = null;
        DoorId = null;
        Width = 0;
        Height = 0;
        Loaded = false;
        offset = 0;
        Failed = false;
        LeftWorldCallback?.Invoke();
    }

    public void Destroy()
    {
        Reset();
        LoadFailedCallback = null;
        JoinedWorldCallback = null;
        LeftWorldCallback = null;
    }

    public Action<string>? LoadFailedCallback;
    public Action? JoinedWorldCallback;
    public Action? LeftWorldCallback;
}