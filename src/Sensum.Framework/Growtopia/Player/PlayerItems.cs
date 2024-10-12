using System.Buffers;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Utils;
using Sensum.Framework.Utils.Extensions;

namespace Sensum.Framework.Growtopia.Player;

public unsafe class PlayerItems : IResourceLifecycle
{
    private static readonly ArrayPool<InventoryItem> inventory_item_pool = ArrayPool<InventoryItem>.Shared;
    public InventoryItem[]? Items;
    public uint BackpackSize;
    public int Gems;

    private int offset;

    public void Serialize(byte* data, int dataSize)
    {
        Reset();

        Memory.Skip(ref offset, 1);
        BackpackSize = Memory.Read<uint>(data, ref offset, dataSize);
        ushort count = Memory.Read<ushort>(data, ref offset, dataSize);
        Items = inventory_item_pool.Rent((int)BackpackSize);
        for (ushort i = 0; i < count; i++)
        {
            Add(Memory.Read<ushort>(data, ref offset, dataSize), Memory.Read<byte>(data, ref offset, dataSize), (InventoryItemFlags)Memory.Read<byte>(data, ref offset, dataSize));
        }
        InventoryLoadedCallback?.Invoke();
    }

    public void RebuildInventory(uint newBackpackSize)
    {
        if (Items == null) return;
        InventoryItem[] newItems = inventory_item_pool.Rent((int)newBackpackSize);
        for (int i = 0; i < newBackpackSize; i++)
        {
            if (i < Items.Length)
            {
                newItems[i] = Items[i];
            }
            else
            {
                newItems[i] = new InventoryItem();
            }
        }
        inventory_item_pool.Return(Items);
        Items = newItems;
        BackpackSize = newBackpackSize;
    }


    public void Add(ushort itemId, byte amount, InventoryItemFlags flag = InventoryItemFlags.None)
    {
        if (Items == null) return;
        if (itemId == 112)
        {
            Gems += amount;
            return;
        }

        if (HasItem(itemId))
        {
            ModifyItemById(itemId, (ref InventoryItem item) =>
            {
                int total = item.Count + amount;
                if (total > 200) total = 200;
                item.Count = (byte)total;
            });
            ItemAddedCallback?.Invoke(itemId, amount);
            return;
        }

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].Id == 0)
            {
                Items[i] = new InventoryItem
                {
                    Id = itemId,
                    Count = amount,
                    Flags = flag
                };
                ItemAddedCallback?.Invoke(itemId, amount);

                if (Items.Length == BackpackSize)
                {
                    InventoryFullCallback?.Invoke();
                }
                return;
            }
        }
    }


    public void Remove(ushort itemId, byte amount)
    {
        if (Items == null) return;
        if (itemId == 112)
        {
            Gems -= amount;
            return;
        }

        byte count = GetItemCount(itemId);
        count -= amount;
        if (count <= 0)
        {
            Delete(itemId);
            ItemRemovedCallback?.Invoke(itemId, amount);
            return;
        }

        ModifyItemById(itemId, (ref InventoryItem item) =>
        {
            item.Count = count;
        });
        ItemRemovedCallback?.Invoke(itemId, amount);
    }


    public void Delete(ushort itemId)
    {
        if (HasItem(itemId) == false) return;
        ModifyItemById(itemId, (ref InventoryItem item) =>
        {
            item.Id = 0;
            item.Count = 0;
            item.Flags = InventoryItemFlags.None;
        });
    }

    public bool HasItem(ushort itemId)
    {
        return Items != null && Items.Any(c => c.Id == itemId);
    }

    public byte GetItemCount(ushort itemId) => Items?.FirstOrDefault(c => c.Id == itemId).Count ?? 0;

    public void ModifyItemById(ushort itemId, RefAction<InventoryItem> modifyAction)
    {
        if (Items == null) return;
        int index = Items.FindIndex(c => c.Id == itemId);
        if (index == -1) return;
        var item = Items[index];
        modifyAction(ref item);
        Items[index] = item;
    }

    public void Reset()
    {
        offset = 0;

        if (Items != null)
        {
            ItemsClearedCallback?.Invoke();
            Array.Clear(Items, 0, Items.Length); // Clear the array
            inventory_item_pool.Return(Items); // Return the array to the pool
            Items = null;
        }
    }

    public void Destroy()
    {
        Reset();
        InventoryLoadedCallback = null;
        ItemAddedCallback = null;
        ItemRemovedCallback = null;
        ItemsClearedCallback = null;
        InventoryFullCallback = null;
    }

    public Action? InventoryLoadedCallback;
    public Action? InventoryFullCallback;
    public Action<ushort, byte>? ItemAddedCallback;
    public Action<ushort, byte>? ItemRemovedCallback;
    public Action? ItemsClearedCallback;
}