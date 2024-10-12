using System.Numerics;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Proton;
using Sensum.Framework.Utils.Extensions;

namespace Sensum.Framework.Growtopia;

public static class Actions
{
    public static void JoinWorld(this ENetClient client, string worldInput, bool reset = true)
    {
        if (reset)
        {
            client.World.Reset();
            client.NetObjectManager.Reset();
            client.BotDetector.Reset();
        }

        string[] split = worldInput.Split('|');
        if (split[0].Equals("exit", StringComparison.InvariantCultureIgnoreCase))
        {
            client.LeaveWorld();
            return;
        }
        client.SendGameMessage($"action|join_request\nname|{worldInput}\ninvitedWorld|0\n");
    }

    public static bool JoinWorldSync(this ENetClient client, string worldInput)
    {
        string worldName = worldInput.Split('|')[0];
        if (client.World.Name is not null && client.World.Name.Equals(worldName, StringComparison.CurrentCultureIgnoreCase))
        {
            client.JoinWorld(worldInput, false);
            return true;
        }

        client.JoinWorld(worldInput);
        const byte timeout = 10;
        byte time = 0;
        while (client.World.Loaded == false)
        {
            if (client.World.Failed) return false;
            if (time >= timeout) return false;
            time++;
            Thread.Sleep(1000);
        }
        return true;
    }

    public static void LeaveWorld(this ENetClient client)
    {
        client.SendGameMessage("action|quit_to_exit\n");
    }

    public static void SetIcon(this ENetClient client, in IconState state)
    {
        client.NetAvatar.IconState = state;
        var packet = new GameUpdatePacket
        {
            Type = GamePacketType.SetIconState,
            NetId = client.NetAvatar.NetId,
            TilePos = new Vector2Int((int)client.NetAvatar.IconState, 0)
        };
        client.SendPacket(packet);
    }

    public static void Say(this ENetClient client, string input)
    {
        if (client.World.Loaded == false) return;
        client.SendGenericText($"action|input\n|text|{input}\n");
    }

    public static void SendPlayerState(this ENetClient client, in VisualState state, in Vector2 worldPos, in Vector2Int tilePos, ushort itemId = 0)
    {
        if (SafeChecks.PositionCheck(client, new Vector2Int((int)(worldPos.X / 32), (int)(worldPos.Y / 32))) == false) return;
        client.NetAvatar.Pos = worldPos;
        client.NetAvatar.VisualState = state;

        var packet = new GameUpdatePacket
        {
            Type = GamePacketType.State,
            WorldPos = client.NetAvatar.Pos,
            CharacterState = (int)client.NetAvatar.VisualState,
            NetId = client.NetAvatar.NetId,
            Value = itemId,
            TilePos = tilePos
        };

        client.SendPacket(packet);
    }


    public static void MoveUp(this ENetClient client)
    {
        SendPlayerState(client, client.NetAvatar.VisualState, client.NetAvatar.Pos with { Y = client.NetAvatar.Pos.Y - 32 }, Vector2Int.NEGATIVE);
    }

    public static void MoveDown(this ENetClient client)
    {
        SendPlayerState(client, client.NetAvatar.VisualState, client.NetAvatar.Pos with { Y = client.NetAvatar.Pos.Y + 32 }, Vector2Int.NEGATIVE);
    }

    public static void MoveLeft(this ENetClient client)
    {
        SendPlayerState(client, VisualState.StandingLeft, client.NetAvatar.Pos with { X = client.NetAvatar.Pos.X - 32 }, Vector2Int.NEGATIVE);
    }

    public static void MoveRight(this ENetClient client)
    {
        SendPlayerState(client, VisualState.StandingRight, client.NetAvatar.Pos with {X = client.NetAvatar.Pos.X + 32 }, Vector2Int.NEGATIVE);
    }

    public static void ActivateTile(this ENetClient client, int itemId, in Vector2Int tilePos)
    {
        var packet = new GameUpdatePacket
        {
            Type = GamePacketType.TileActivateRequest,
            Value = itemId,
            WorldPos = client.NetAvatar.Pos,
            TilePos = tilePos
        };
        client.SendPacket(packet);
    }

    public static void WrenchPlayer(this ENetClient client, int netId = -1)
    {
        if (netId == -1) netId = client.NetAvatar.NetId;
        client.SendGenericText($"action|wrench\n|netid|{netId}\n");
    }


    public static void AcceptAccess(this ENetClient client)
    {
        client.DialogRequestCallback = _ =>
        {
            client.SendGenericText($"action|dialog_return\ndialog_name|popup\nnetID|{client.NetAvatar.NetId}|\nbuttonClicked|acceptlock\n");
            client.DialogRequestCallback = _ =>
            {
                client.SendGenericText("action|dialog_return\ndialog_name|acceptaccess\n");
                client.DialogRequestCallback = null;
            };
        };
        WrenchPlayer(client);
    }

    public static void Drop(this ENetClient client, ushort itemId, byte count = 200)
    {
        if (count == 200) count = client.PlayerItems.GetItemCount(itemId);
        if (SafeChecks.DropCheck(client, itemId, count) == false) return;
        client.DialogRequestCallback = _ =>
        {
            client.SendGenericText($"action|dialog_return\ndialog_name|drop_item\nitemID|{itemId}|\ncount|{count}\n");
            client.DialogRequestCallback = null;
        };
        client.SendGenericText($"action|drop\n|itemID|{itemId}\n");
    }

    public static void Trash(this ENetClient client, ushort itemId, byte count = 200)
    {
        if (client.PlayerItems.HasItem(itemId) == false) return;
        if (count == 200) count = client.PlayerItems.GetItemCount(itemId);
        if (client.PlayerItems.GetItemCount(itemId) < count) return;
        client.DialogRequestCallback = _ =>
        {
            client.SendGenericText($"action|dialog_return\ndialog_name|trash_item\nitemID|{itemId}|\ncount|{count}\n");
            client.DialogRequestCallback = null;
        };
        client.SendGenericText($"action|trash\n|itemID|{itemId}\n");
    }


    public static void ActivateItem(this ENetClient client, ushort itemId)
    {
        if (client.PlayerItems.HasItem(itemId) == false) return;
        client.PlayerItems.ModifyItemById(itemId, (ref InventoryItem item) => item.Flags = (InventoryItemFlags)(item.Flags == 0 ? 1 : 0));
        var packet = new GameUpdatePacket
        {
            Type = GamePacketType.ItemActivateRequest,
            Value = itemId,
        };
        client.SendPacket(packet);
    }


    public static void Collect(this ENetClient client, in WorldObject worldObject)
    {
        if (SafeChecks.CollectCheck(client, worldObject) == false) return;
        var packet = new GameUpdatePacket
        {
            Type = GamePacketType.ItemActivateObjectRequest,
            WorldPos = worldObject.Pos,
            NetId = -1,
            Value = (int)worldObject.ObjectId
        };
        client.SendPacket(packet);
    }


    public static void CollectFromRange(this ENetClient client, int range, params ushort[] ignoreItems)
    {
        if (client.World.Loaded == false) return;
        foreach (var worldObject in client.World.WorldObjectMap.WorldObjects.ToArray())
        {
            if (ignoreItems.Contains(worldObject.ItemId)) continue;
            if (MiscUtils.IsInside(client.NetAvatar.Pos, range, worldObject.Pos) == false) continue;
            Collect(client, worldObject);
        }
    }


    public static void CollectFromTile(this ENetClient client, in Vector2Int pos, params ushort[] ignoreItems)
    {
        if (client.World.Loaded == false) return;
        foreach (var worldObject in client.World.WorldObjectMap.WorldObjects.ToArray())
        {
            if (ignoreItems.Contains(worldObject.ItemId)) continue;
            if (MiscUtils.IsInside(pos.ToWorldPosition(), 28, worldObject.Pos) == false) continue;
            Collect(client, worldObject);
        }
    }

    public static void Punch(this ENetClient client, in Vector2Int pos, bool showVisual = true)
    {
        if (client.World.Loaded == false) return;
        if (SafeChecks.PunchCheck(client, pos) == false) return;
        var tile = client.World.WorldTileMap.GetTileByPos(pos)!;

        var playerPos = client.NetAvatar.TilePos;
        var punchVisualState = playerPos.X < tile.Pos.X ? VisualState.PunchRight : VisualState.PunchLeft;
        if (playerPos.X == tile.Pos.X)
            punchVisualState = VisualState.Punch;

        var punchPacket = new GameUpdatePacket
        {
            Type = GamePacketType.TileChangeRequest,
            Value = 18,
            WorldPos = client.NetAvatar.Pos,
            TilePos = pos
        };

        client.SendPacket(punchPacket);
        if (showVisual) client.SendPlayerState(punchVisualState, client.NetAvatar.Pos, pos, 18);
    }

    public static void Place(this ENetClient client, in Vector2Int pos, ushort itemId, bool showVisual = true)
    {
        if (client.World.Loaded == false) return;
        if (SafeChecks.PlaceCheck(client, pos, itemId) == false) return;
        var tile = client.World.WorldTileMap.GetTileByPos(pos)!;

        var playerPos = client.NetAvatar.TilePos;
        var punchVisualState = playerPos.X < tile.Pos.X ? VisualState.PlaceRight : VisualState.PlaceLeft;
        if (playerPos.X == tile.Pos.X)
            punchVisualState = VisualState.Place;

        var punchPacket = new GameUpdatePacket
        {
            Type = GamePacketType.TileChangeRequest,
            Value = itemId,
            WorldPos = client.NetAvatar.Pos,
            TilePos = pos
        };

        client.SendPacket(punchPacket);
        if (showVisual) client.SendPlayerState(punchVisualState, client.NetAvatar.Pos, pos, itemId);
    }

    public static void BuyPack(this ENetClient client, string packId)
    {
        client.SendGenericText($"action|buy\nitem|{packId}\n");
    }

    public static bool BuyPack(this ENetClient client, string packId, out StoreResult result)
    {
        client.LastStoreResult = StoreResult.None;
        client.BuyPack(packId);

        int timeout = 0;
        while (client.LastStoreResult == StoreResult.None)
        {
            if (timeout >= 10)
            {
                result = StoreResult.None;
                return false;
            }
            Thread.Sleep(1000);
            timeout++;
        }
        result = client.LastStoreResult;
        return true;
    }

    public static void PlaceToStorage(this ENetClient client, Vector2Int pos, ushort itemId, byte count)
    {
        client.DialogRequestCallback = _ =>
        {
            client.DialogRequestCallback = _ =>
            {
                client.SendGenericText($"action|dialog_return\ndialog_name|storageboxxtreme\ntilex|{pos.X}|\ntiley|{pos.Y}|\nitemid|{itemId}\nbuttonClicked|do_add\nitemcount|{count}\n");
                client.DialogRequestCallback = null;
            };
            client.SendGenericText($"action|dialog_return\ndialog_name|storageboxxtreme\ntilex|{pos.X}|\ntiley|{pos.Y}|\nitemid|{itemId}\n");
        };
        client.Place(pos, 32, false); // Sends wrench action to storage box
    }

    public static void CollectFromStorage(this ENetClient client, Vector2Int pos, ushort itemId, byte count)
    {
        client.DialogRequestCallback = _ =>
        {
            if (client.Dialog.Raw?.Contains("storageboxxtreme") == false)
            {
                client.DialogRequestCallback = null;
                return;
            }
            List<SearchableItem> safeVaultItems = [];
            safeVaultItems.AddRange(client.Dialog.Entities.Where(entity => entity.EntityType == EntityType.SearchableItem).Cast<SearchableItem>());
            if (safeVaultItems.All(c => c.ItemId != itemId))
            {
                client.DialogRequestCallback = null;
                return;
            }
            var vaultItem = safeVaultItems.First(c => c.ItemId == itemId);
            client.DialogRequestCallback = _ =>
            {
                client.SendGenericText($"action|dialog_return\ndialog_name|storageboxxtreme\ntilex|{pos.X}|\ntiley|{pos.Y}|\nitemid|{itemId}|\nbuttonClicked|do_take\nitemcount|{count}\n");
                client.DialogRequestCallback = null;
            };
            client.SendGenericText($"action|dialog_return\ndialog_name|storageboxxtreme\ntilex|{pos.X}|\ntiley|{pos.Y}|\nbuttonClicked|{vaultItem.ButtonId}\n");
        };
        client.Place(pos, 32, false); // Sends wrench action to storage box
    }

    // Really messy and shitly implemented, maybe refactor later
    public static bool BuyFromVend(this ENetClient client, Vector2Int pos, int itemId, int price, int quanity, out VendResult outResult)
    {
        int stock = 0;
        string resultMessage = "";
        VendResultType vendResultType = VendResultType.None;
        client.DialogRequestCallback = raw =>
        {
            string[] lines = raw.Split('\n');
            foreach (string line in lines)
            {
                if (line.StartsWith("add_label_with_icon|sml|The machine contains a total of "))
                {
                    stock = int.Parse(line.Split(' ')[6]);
                    break;
                }
            }

            int perValue = 0;
            if (price < 0)
            {
                perValue = -price;
            }

            if (stock == 0 || stock < perValue)
            {
                vendResultType = VendResultType.OutOfStock;
                resultMessage = "Vend is out of stock";
                return;
            }

            // Fixes the quanity
            if (stock < quanity)
            {
                quanity = stock;
            }
            if (perValue > 1)
            {
                if (quanity % perValue != 0)
                {
                    quanity -= quanity % perValue;
                }
            }

            if (perValue == 1)
            {
                price *= quanity;
            }

            client.DialogRequestCallback = rawInside =>
            {
                if (rawInside.Contains("Purchase Confirmation") == false)
                {
                    vendResultType = VendResultType.Other;
                    resultMessage = "Unknown error 0x1";
                    return;
                }
                client.PlayerItems.ItemAddedCallback = (addedItemId, _) =>
                {
                    if (addedItemId == itemId)
                    {
                        vendResultType = VendResultType.Success;
                        resultMessage = "Successfully bought item";
                        client.PlayerItems.ItemAddedCallback = null;
                    }
                };
                client.SendGenericText($"expectitem|{itemId}\naction|dialog_return\ndialog_name|vending\ntilex|{pos.X}\ntiley|{pos.Y}\nverify|1\nbuycount|{quanity}expectprice|{price}\n");
            };
            client.SendGenericText($"expectitem|{itemId}\naction|dialog_return\ndialog_name|vending\ntilex|{pos.X}\ntiley|{pos.Y}\nexpectprice|{price}\nbuycount|{quanity}\n");
        };
        client.Place(pos, 32, false);
        const byte timeout = 30;
        byte time = 0;
        while (vendResultType == VendResultType.None)
        {
            if (time >= timeout)
            {
                client.DialogRequestCallback = null;
                outResult = new VendResult(vendResultType, resultMessage, quanity);
                return false;
            }
            time++;
            Thread.Sleep(1000);
        }
        client.DialogRequestCallback = null;
        outResult = new VendResult(vendResultType, resultMessage, quanity);
        return true;
    }

    public static bool PlaceToDonationBox(this ENetClient client, Vector2Int pos, ushort itemId, byte amount, string text, out DonationResultType resultType)
    {
        var result = DonationResultType.None;
        client.DialogRequestCallback = _ =>
        {
            client.PlayerItems.ItemRemovedCallback += (removedItemId, removedAmount) =>
            {
                if (removedItemId == itemId && removedAmount == amount)
                {
                    result = DonationResultType.Success;
                    client.PlayerItems.ItemRemovedCallback = null;
                }
            };
            client.SendGenericText($"action|dialog_return\ndialog_name|give_item\ntilex|{pos.X}\nitemID|{itemId}\ntiley|{pos.Y}\nsign_text|{text}\nbuttonClicked|give\ncount|{amount}\n");
        };
        client.Place(pos, itemId, false);

        const byte timeout = 7;
        byte time = 0;
        while (result == DonationResultType.None)
        {
            if (time >= timeout)
            {
                client.DialogRequestCallback = null;
                resultType = result;
                return false;
            }
            Thread.Sleep(1000);
            time++;
        }
        resultType = result;
        return true;
    }

    // Really messy and shitly implemented, maybe refactor later (untested too)
    public static bool EmptyVend(this ENetClient client, Vector2Int pos, out int emptyResult)
    {
        int result = 0;
        client.DialogRequestCallback = raw =>
        {
            if (raw.Contains("add_button|withdraw") == false)
            {
                result = 1; // Vend is empty
                return;
            }
            ushort price = 0;
            foreach (string line in raw.Split('\n'))
            {
                if (line.StartsWith("add_text_input|setprice|"))
                {
                    price = ushort.Parse(line.Split('|')[3]);
                    break;
                }
            }
            if (price == 0)
            {
                result = -1; // Smth went wrong
            }

            client.PlayerItems.ItemAddedCallback = (itemId, _) =>
            {
                if (itemId is 242 or 1796)
                {
                    result = 2; // Successfully emptied vend
                    client.PlayerItems.ItemAddedCallback = null;
                }
            };
            client.SendGenericText($"action|dialog_return\ndialog_name|vending\ntilex|{pos.X}\nchk_perlock|0\nchk_peritem|1\ntiley|{pos.Y}\nbuttonClicked|withdraw\nsetprice|{price}\n");
        };
        client.Place(pos, 32, false);
        const byte timeout = 10;
        byte time = 0;
        while (result == 0)
        {
            if (time >= timeout)
            {
                client.DialogRequestCallback = null;
                emptyResult = 0;
                return false;
            }
            Thread.Sleep(1000);
            time++;
        }
        emptyResult = result;
        return true;
    }
}