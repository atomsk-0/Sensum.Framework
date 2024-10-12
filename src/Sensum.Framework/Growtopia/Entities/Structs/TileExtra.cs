using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Proton;
using Sensum.Framework.Utils;

namespace Sensum.Framework.Growtopia.Entities.Structs;

public unsafe class TileExtra(TileExtraType type)
{
    public readonly TileExtraType Type = type;

    public string? Text;

    /* Lock */
    public uint Owner;
    public uint[]? Admins;

    /* Tree */
    public uint PlantTime;
    public uint ReadyTime;
    public byte Fruits;

    /* Vend */
    public int ItemId;
    public int Price;

    /* Provider */
    public int TimeLeft;

    public List<StorageBoxItem>? StorageBoxItems;

    public bool IsReady => ReadyTime <= MiscUtils.UtcUnixTimeStamp;
    public bool HasAccessToLock(uint userId) => Owner == userId || Admins?.Contains(userId) == true;

    public void Serialize(byte* data, ref int offset, Tile tile, int dataSize)
    {
        // Un comment to use when debugging tile extras
        /*var item = ItemInfoManager.GetItem(tile.Foreground);
        string itemName = "";
        if (item.HasValue)
        {
            itemName = item.Value.Name;
            if (item.Value.Type is not (ItemType.Mannequin or ItemType.DisplayBlock))
                Console.WriteLine($"Type: {Type}, pos: {tile.Pos}, fg: {tile.Foreground}, name: {itemName}");
        }*/

        int tempInt;
        switch (Type)
        {
            case TileExtraType.Door:
                Text = Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, 1);
                break;
            case TileExtraType.Sign:
                Text = Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, 4);
                break;
            case TileExtraType.Lock:
                Memory.Skip(ref offset, 1); // Flag -> Ignore empty air
                Owner = Memory.Read<uint>(data, ref offset, dataSize);
                Admins = new uint[Memory.Read<int>(data, ref offset, dataSize)];
                for (int i = 0; i < Admins.Length; i++)
                {
                    Admins[i] = Memory.Read<uint>(data, ref offset, dataSize);
                }
                Memory.Skip(ref offset, 8); // 16??
                break;
            case TileExtraType.Tree:
                PlantTime = (uint)(MiscUtils.UtcUnixTimeStamp - Memory.Read<int>(data, ref offset, dataSize));
                var itm = ItemInfoManager.GetItem(tile.Foreground);
                if (itm.HasValue)
                {
                    ReadyTime = (uint)(PlantTime + ItemInfoManager.GetItem(tile.Foreground)!.Value.Time);
                }
                Fruits = Memory.Read<byte>(data, ref offset, dataSize);
                break;
            case TileExtraType.Mailbox:
                goto case TileExtraType.ToyBox;
            case TileExtraType.Bulletin:
                goto case TileExtraType.ToyBox;
            case TileExtraType.Dice:
                goto case TileExtraType.GameBlock;
            case TileExtraType.Provider:
                TimeLeft = Memory.Read<int>(data, ref offset, dataSize);
                if (tile.Foreground is 0x14c6 or 0x29A0)
                {
                    Memory.Skip(ref offset, 4);
                }
                break;
            case TileExtraType.AchievementBlock:
                Memory.Skip(ref offset, 5);
                break;
            case TileExtraType.HeartMonitor:
                Memory.Skip(ref offset, 4);
                Text = Memory.ReadString(data, ref offset, dataSize);
                break;
            case TileExtraType.DonationBox:
                goto case TileExtraType.ToyBox;
            case TileExtraType.ToyBox: // ..
                break;
            case TileExtraType.Mannequin:
                Text = Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, 23);
                break;
            case TileExtraType.MagicEgg:
                Memory.Skip(ref offset, 4);
                break;
            case TileExtraType.GameBlock:
                Memory.Skip(ref offset, 1);
                break;
            case TileExtraType.GameGenerator: // ..
                break;
            case TileExtraType.Xenonite:
                goto case TileExtraType.Solar;
            case TileExtraType.Dressup:
                Memory.Skip(ref offset, 18);
                break;
            case TileExtraType.Crystal:
                tempInt = Memory.Read<ushort>(data, ref offset, dataSize);
                Memory.Skip(ref offset, tempInt);
                break;
            case TileExtraType.Burglar:
                Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, 5);
                break;
            case TileExtraType.Spotlight: // ..
                break;
            case TileExtraType.DisplayBlock:
                Memory.Skip(ref offset, 4);
                break;
            case TileExtraType.VendingMachine:
                ItemId = Memory.Read<int>(data, ref offset, dataSize);
                Price = Memory.Read<int>(data, ref offset, dataSize);

                break;
            case TileExtraType.Fishtank:
                Memory.Skip(ref offset, 1);
                tempInt = Memory.Read<int>(data, ref offset, dataSize);
                for (int i = 0; i < tempInt / 2; i++)
                {
                    Memory.Skip(ref offset, 8);
                }
                break;
            case TileExtraType.Solar:
                Memory.Skip(ref offset, 5);
                break;
            case TileExtraType.Forge:
                goto case TileExtraType.GeigerCharger;
            case TileExtraType.GivingTree:
                Memory.Skip(ref offset, 6);
                break;
            case TileExtraType.GivingTreeStump: // ..
                break;
            case TileExtraType.SteamOrgan:
                Memory.Skip(ref offset, 5);
                break;
            case TileExtraType.Silkworm:
                Memory.Skip(ref offset, 1);
                Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, 29); // 25
                break;
            case TileExtraType.SewingMachine:
                Memory.Skip(ref offset, Memory.Read<int>(data, ref offset, dataSize) * 4);
                break;
            case TileExtraType.Flag:
                if (tile.Foreground == 0xD42)
                {
                    Text = Memory.ReadString(data, ref offset, dataSize);
                }

                break;
            case TileExtraType.LobsterTrap: // ..
                //Memory.Skip(ref offset, 8);
                break;
            case TileExtraType.ArtCanvas:
                Memory.Skip(ref offset, 4);
                Text = Memory.ReadString(data, ref offset, dataSize);
                break;
            case TileExtraType.BattleCage:
                Text = Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, 12);
                break;
            case TileExtraType.PetTrainer:
                Text = Memory.ReadString(data, ref offset, dataSize);
                tempInt = Memory.Read<int>(data, ref offset, dataSize);
                Memory.Skip(ref offset, 4 + (tempInt * 4));
                break;
            case TileExtraType.SteamEngine:
                goto case TileExtraType.GeigerCharger;
            case TileExtraType.LockBot:
                goto case TileExtraType.GeigerCharger;
            case TileExtraType.BackgroundWeather:
                goto case TileExtraType.GeigerCharger;
            case TileExtraType.SpiritStorage:
                goto case TileExtraType.GeigerCharger;
            case TileExtraType.DataBedrock:
                Memory.Skip(ref offset, 21);
                break;
            case TileExtraType.DisplayShelf:
                Memory.Skip(ref offset, 16);
                break;
            case TileExtraType.VipTimer:
                Memory.Skip(ref offset, 5);
                Memory.Skip(ref offset, Memory.Read<int>(data, ref offset, dataSize) * 4);
                break;
            case TileExtraType.ChallengeTimer: // ..
                /*Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, 17);*/
                break;
            case TileExtraType.FishMount:
                Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, 5);
                break;
            case TileExtraType.Portrait:
                Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, 26);
                break;
            case TileExtraType.StuffWeather:
                Memory.Skip(ref offset, 9);
                break;
            case TileExtraType.FossilPrep:
                goto case TileExtraType.GeigerCharger;
            case TileExtraType.DnaMachine: // ..
                /* Memory.Skip(ref offset, 4);
                 Memory.ReadStringSafe(data, ref offset, dataSize); */ // Maybe
                break;
            case TileExtraType.Trickster: // ..
                //Memory.Skip(ref offset, 6);
                break;
            case TileExtraType.Chemtank:
                Memory.Skip(ref offset, 8);
                break;
            case TileExtraType.Storage:
                tempInt = Memory.Read<ushort>(data, ref offset, dataSize);
                StorageBoxItems = [];
                for (int i = 0; i < tempInt / 13; i++)
                {
                    Memory.Skip(ref offset, 3);
                    uint itemId = Memory.Read<uint>(data, ref offset, dataSize);
                    Memory.Skip(ref offset, 2);
                    uint itemAmount = Memory.Read<uint>(data, ref offset, dataSize);
                    StorageBoxItems.Add(new StorageBoxItem { Itemid = (ushort)itemId, Amount = itemAmount });
                }
                break;
            case TileExtraType.Oven:
                Memory.Skip(ref offset, 4);
                tempInt = Memory.Read<int>(data, ref offset, dataSize) / 2;
                for (int i = 0; i < tempInt; i++)
                {
                    Memory.Skip(ref offset, 8);
                }

                Memory.Skip(ref offset, 12);
                break;
            case TileExtraType.SuperMusic:
                Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, 4);
                break;
            case TileExtraType.GeigerCharger:
                Memory.Skip(ref offset, 4);
                break;
            case TileExtraType.AdventureReset: // ..
                break;
            case TileExtraType.TombRobber: // ..
                /*Memory.ReadString(data, ref offset, dataSize);
                Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, 1);*/
                break;
            case TileExtraType.Faction:
                Memory.Skip(ref offset, 5);
                break;
            case TileExtraType.TrainingFishPort:
                Memory.Skip(ref offset, 35);
                break;
            case TileExtraType.ItemSucker:
                Memory.Skip(ref offset, 14);
                break;
            case TileExtraType.Robot:
                tempInt = Memory.Read<int>(data, ref offset, dataSize);
                for (int i = 0; i < tempInt; i++)
                {
                    Memory.Skip(ref offset, 15);
                }

                Memory.Skip(ref offset, 8);
                break;
            case TileExtraType.Ticket: // ..
                // Memory.ReadString(data, ref offset, dataSize);
                break;
            case TileExtraType.GuildItem:
                Memory.Skip(ref offset, 17);
                break;
            case TileExtraType.StatsBlock:
                Memory.Skip(ref offset, 1);
                break;
            case TileExtraType.FieldNode:
                Memory.Skip(ref offset, 4);
                Memory.Skip(ref offset, Memory.Read<int>(data, ref offset, dataSize) * 4);
                break;
            case TileExtraType.OuijaBoard:
                Memory.Skip(ref offset, 4);
                Memory.ReadString(data, ref offset, dataSize);
                Memory.ReadString(data, ref offset, dataSize);
                Memory.Skip(ref offset, Memory.Read<int>(data, ref offset, dataSize) * 4);
                break;
            case TileExtraType.AutoBreak:
                goto case TileExtraType.AutoHarvest;
            case TileExtraType.AutoHarvest:
                Memory.Skip(ref offset, 16);
                break;
            case TileExtraType.AutoHarvestSucker:
                Memory.Skip(ref offset, 30);
                break;
            case TileExtraType.LightningIfOn:
                Memory.Skip(ref offset, 12);
                break;
            case TileExtraType.PhasedBlock:
                goto case TileExtraType.FeedingBlock;
            case TileExtraType.SafeVault: // ..
                break;
            case TileExtraType.PhasedBlock2:
                Memory.Skip(ref offset, 7);
                break;
            case TileExtraType.PveNpc: // ..
                //Memory.Skip(ref offset, 0x138);
                break;
            case TileExtraType.InfinityWeather:
                Memory.Skip(ref offset, 4);
                Memory.Skip(ref offset, Memory.Read<int>(data, ref offset, dataSize) * 4);
                break;
            case TileExtraType.Completionist: // ..
                goto case TileExtraType.FeedingBlock;
            case TileExtraType.FeedingBlock:
                Memory.Skip(ref offset, 4);
                break;
            case TileExtraType.KrankensBlock:
                Memory.Skip(ref offset, 8);
                break;
            case TileExtraType.FriendsEntrance:
                Memory.Skip(ref offset, 6);
                tempInt = Memory.Read<ushort>(data, ref offset, dataSize); //maybe int instead ushort and skip 2
                Memory.Skip(ref offset, tempInt * 4);
                break;
        }
    }
}