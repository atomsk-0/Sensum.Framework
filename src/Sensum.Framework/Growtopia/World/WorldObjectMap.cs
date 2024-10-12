using System.Numerics;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Utils;
namespace Sensum.Framework.Growtopia.World;

public unsafe class WorldObjectMap: IResourceLifecycle
{
    public readonly LinkedList<WorldObject> WorldObjects = [];
    public uint IdTracker;

    public bool Serialize(byte* data, ref int offset, int dataSize)
    {
        int worldObjectCount = Memory.Read<int>(data, ref offset, dataSize);
        IdTracker = Memory.Read<uint>(data, ref offset, dataSize);
        for (int i = 0; i < worldObjectCount; i++)
        {
            var worldObject = new WorldObject();
            worldObject.Serialize(data, ref offset, dataSize);
            if (ItemInfoManager.HasItem(worldObject.ItemId) == false) return false;
            Add(worldObject);
        }
        return true;
    }


    public WorldObject? GetWorldObjectByObjectId(uint objectId)
    {
        return WorldObjects.FirstOrDefault(c => c.ObjectId == objectId);
    }

    public void Add(Vector2 pos, ushort itemId, byte count, byte flags, uint objectId)
    {
        WorldObjects.AddLast(new WorldObject
        {
            Pos = pos,
            ItemId = itemId,
            Count = count,
            Flags = flags,
            ObjectId = objectId
        });
        WorldObjectAddedCallback?.Invoke(itemId, count);
    }

    public void Add(Vector2 pos, ushort itemId, byte count, byte flags)
    {
        WorldObjects.AddLast(new WorldObject
        {
            Pos = pos,
            ItemId = itemId,
            Count = count,
            Flags = flags,
            ObjectId = ++IdTracker
        });
        WorldObjectAddedCallback?.Invoke(itemId, count);
    }

    public void Remove(WorldObject worldObject)
    {
        ushort itemId = worldObject.ItemId;
        byte amount = worldObject.Count;
        WorldObjects.Remove(worldObject);
        WorldObjectRemovedCallback?.Invoke(itemId, amount);
    }

    public void RemoveByObjectId(uint objectId)
    {
        var worldObject = WorldObjects.FirstOrDefault(c => c.ObjectId == objectId);
        Remove(worldObject);
    }

    public void ModifyWorldObjectByObjectId(uint objectId, RefAction<WorldObject> modifyAction)
    {
        var worldObject = WorldObjects.First(c => c.ObjectId == objectId);
        byte ogCount = worldObject.Count;
        modifyAction(ref worldObject);
        byte newCount = worldObject.Count;
        int difference = newCount - ogCount;
        switch (difference)
        {
            case > 0:
                WorldObjectAddedCallback?.Invoke(worldObject.ItemId, (byte)difference);
                break;
            case < 0:
                WorldObjectRemovedCallback?.Invoke(worldObject.ItemId, (byte)-difference);
                break;
        }
    }

    public void Add(WorldObject worldObject)
    {
        WorldObjects.AddLast(worldObject);
    }

    public void Reset()
    {
        IdTracker = 0;
        WorldObjects.Clear();
    }

    public void Destroy()
    {
        Reset();
        WorldObjectAddedCallback = null;
        WorldObjectRemovedCallback = null;
    }

    public Action<ushort, byte>? WorldObjectAddedCallback;
    public Action<ushort, byte>? WorldObjectRemovedCallback;
}