using System.Numerics;
using System.Text.RegularExpressions;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Growtopia.Player;
using Sensum.Framework.Proton;
using Sensum.Framework.Utils;

namespace Sensum.Framework.Growtopia.Handlers;

internal static unsafe class VariantFunctionHandler
{
    internal static void Handle(ENetClient client, GameUpdatePacket* packet)
    {
        byte* extended = (byte*)PacketManager.GetExtendedDataPointerFromTankPacket(packet);
        Variant variant = Variant.Serialize(extended, packet->ExtraDataSize);
        switch (variant.Function)
        {
            case VariantFunction.OnReconnect:
                break;
            case VariantFunction.OnSpawn:
                onSpawn(client, variant);
                break;
            case VariantFunction.OnSetPos:
                onSetPos(client, variant);
                break;
            case VariantFunction.OnLogout:
                break;
            case VariantFunction.OnRemove:
                onRemove(client, variant);
                break;
            case VariantFunction.OnRequestWorldSelectMenu:
                onRequestWorldSelectMenu(client);
                break;
            case VariantFunction.OnConsoleMessage:
                onConsoleMessage(client, variant);
                break;
            case VariantFunction.OnSDBroadcast:
                break;
            case VariantFunction.OnTalkBubble:
                onTalkBubble(client, variant);
                break;
            case VariantFunction.OnTextOverlay:
                break;
            case VariantFunction.OnPersistentNPCMessage:
                break;
            case VariantFunction.OnPersistentNPCMessageInputDependent:
                break;
            case VariantFunction.OnClearNPCMessage:
                break;
            case VariantFunction.OnFTUECameraLock:
                break;
            case VariantFunction.OnTutorialArrow:
                break;
            case VariantFunction.OnClearTutorialArrow:
                break;
            case VariantFunction.OnClearAllTutorialArrows:
                break;
            case VariantFunction.OnDialogRequest:
                onDialogRequest(client, variant);
                break;
            case VariantFunction.OnDailyRewardRequest:
                break;
            case VariantFunction.OnWinterRallyRequest:
                break;
            case VariantFunction.OnCardBattleDialogRequest:
                break;
            case VariantFunction.OnHideMenusRequest:
                break;
            case VariantFunction.OnHideInventoryRequest:
                break;
            case VariantFunction.OnResetInventoryPosRequest:
                break;
            case VariantFunction.OnHideChatRequest:
                break;
            case VariantFunction.OnActivateMenusRequest:
                break;
            case VariantFunction.OnShowInventoryRequest:
                break;
            case VariantFunction.OnShowChatRequest:
                break;
            case VariantFunction.OnAutoSetChatToWorldRequest:
                break;
            case VariantFunction.OnFTUEWorldSwitchSetActive:
                break;
            case VariantFunction.OnWorldMenuFTUEMessageRequest:
                break;
            case VariantFunction.FTUESetBeginingState:
                break;
            case VariantFunction.FTUESetLastState:
                break;
            case VariantFunction.FTUESetInventoryUIHeightPercent:
                break;
            case VariantFunction.CloseWorldMenuFTUEMessage:
                break;
            case VariantFunction.ShowStartFTUEPopup:
                break;
            case VariantFunction.OnStoreRequest:
                break;
            case VariantFunction.OnSurveyRequest:
                break;
            case VariantFunction.OnStorePurchaseResult:
                onStorePurchaseResult(client, variant);
                break;
            case VariantFunction.OnZoomCamera:
                break;
            case VariantFunction.OnHelpRequest:
                break;
            case VariantFunction.OnCommunityHubRequest:
                break;
            case VariantFunction.OnUbiclubRequest:
                break;
            case VariantFunction.OnFailedToEnterWorld:
                break;
            case VariantFunction.SetHasGrowID:
                break;
            case VariantFunction.SetHasAccountSecured:
                break;
            case VariantFunction.GrowIDChanged:
                break;
            case VariantFunction.SetShowChatOnlyFromFriends:
                break;
            case VariantFunction.OnSuperMainStartAcceptLogonHrdxs47254722215a:
                onSuperMainStartAcceptLogonHrdxs47254722215A(client);
                break;
            case VariantFunction.OnSetBux:
                onSetBux(client, variant);
                break;
            case VariantFunction.OnSetPearl:
                break;
            case VariantFunction.OnSetDungeonToken:
                break;
            case VariantFunction.OnParticleEffect:
                break;
            case VariantFunction.OnParticleEffectV2:
                break;
            case VariantFunction.OnItemEffect:
                break;
            case VariantFunction.OnAddNotification:
                break;
            case VariantFunction.OnStartTrade:
                break;
            case VariantFunction.OnForceTradeEnd:
                break;
            case VariantFunction.OnTradeStatus:
                break;
            case VariantFunction.OnPartyUIUpdate:
                break;
            case VariantFunction.OnDisableRemoveIAPScreen:
                break;
            case VariantFunction.OnSendToServer:
                onSendToServer(client, variant);
                break;
            case VariantFunction.OnSendToCaptcha:
                break;
            case VariantFunction.OnToggleBetaMode:
                break;
            case VariantFunction.OnSetBetaMode:
                break;
            case VariantFunction.OnSetBetaSilent:
                break;
            case VariantFunction.OnSendIAPConfirmation:
                break;
            case VariantFunction.OnSetBaseWeather:
                break;
            case VariantFunction.OnSetCurrentWeather:
                break;
            case VariantFunction.OnRestorePurchases:
                break;
            case VariantFunction.OnRedirectServer:
                break;
            case VariantFunction.OnPlanterActivated:
                break;
            case VariantFunction.OnMagicCompassTrackingItemIDChanged:
                break;
            case VariantFunction.OnEmoticonDataChanged:
                break;
            case VariantFunction.OnPaw2018SkinColor1Changed:
                break;
            case VariantFunction.OnPaw2018SkinColor2Changed:
                break;
            case VariantFunction.OnSetRoleSkinsAndTitles:
                break;
            case VariantFunction.OnAchievementCompleted:
                break;
            case VariantFunction.OnAchievementProgress:
                break;
            case VariantFunction.OnOverrideGDPRFromServer:
                break;
            case VariantFunction.OnShowCaptcha:
                break;
            case VariantFunction.OnCaptchaFailed:
                break;
            case VariantFunction.OnClashEventIsActiveChanged:
                break;
            case VariantFunction.OnUbiClubRewardGiven:
                break;
            case VariantFunction.InviteEnterFailed:
                break;
            case VariantFunction.OnWrongDetails:
                break;
            case VariantFunction.OnFriendOnlineID:
                break;
            case VariantFunction.OnThrowToStartMenu:
                break;
            case VariantFunction.OnAchievementListLogon:
                break;
            case VariantFunction.OnProgressUISet:
                break;
            case VariantFunction.OnProgressUIUpdateValue:
                break;
            case VariantFunction.OnProgressUIPlayAnim:
                break;
            case VariantFunction.OnOpenTapjoyTV:
                break;
            case VariantFunction.OnUpdatePVENPC:
                break;
            case VariantFunction.OnCorrectNPCPos:
                break;
            case VariantFunction.OnActivatePVENPCManager:
                break;
            case VariantFunction.NPC_target_update:
                break;
            case VariantFunction.OnDeactivatePVENPCManager:
                break;
            case VariantFunction.OnTodaysDate:
                break;
            case VariantFunction.OnReceivedZodiacMode:
                break;
            case VariantFunction.OnSetMissionTimer:
                break;
            case VariantFunction.OnShowDialogMessage:
                break;
            case VariantFunction.OnHideDialogMessage:
                break;
            case VariantFunction.OnStorylineStatsUpdate:
                break;
            case VariantFunction.OnEndMission:
                break;
            case VariantFunction.HideTimer:
                break;
            case VariantFunction.HidePearlCurrency:
                break;
            case VariantFunction.ShowPearlCurrency:
                break;
            case VariantFunction.HideWOTDCoin:
                break;
            case VariantFunction.ShowWOTDCoin:
                break;
            case VariantFunction.OnDisabledParticleList:
                break;
            case VariantFunction.OnUpdateRespawnTimeout:
                break;
            case VariantFunction.OnUpdateThanksGivingBossUI:
                break;
            case VariantFunction.Client:
                break;
            case VariantFunction.CrashTheGameClient:
                break;
            case VariantFunction.Up:
                break;
            case VariantFunction.OnPlayerLeveledUp:
                break;
        }
        client.VariantReceivedCallback?.Invoke(variant);
    }

    private static void onSpawn(ENetClient client, in Variant variant)
    {
        string raw = variant.Get<string>(0);

        //fix to work with textscanner
        foreach (string line in raw.Split('\n'))
        {
            if (line.StartsWith("colrect"))
            {
                raw = raw.Replace(line, "");
            }
            else if (line.StartsWith("onlineID"))
            {
                raw = raw.Replace(line, "");
            }
            else if (line.StartsWith("titleIcon"))
            {
                raw = raw.Replace(line, "");
            }
            else if (line.StartsWith("posXY"))
            {
                raw = raw.Replace(line, "");
                int posX = (int)float.Parse(line.Split('|')[1]);
                int posY = (int)float.Parse(line.Split('|')[2]);
                raw += $"posX|{posX}\nposY|{posY}";
            }
        }

        using TextScanner scanner = new TextScanner(raw);
        if (scanner.Contains("type"))
        {
            client.NetAvatar.Pos = new Vector2(scanner.Get<int>("posX"), scanner.Get<int>("posY"));
            client.NetAvatar.NetId = scanner.Get<int>("netID");
            client.NetAvatar.UserId = scanner.Get<uint>("userID");
            client.NetAvatar.Name = scanner.Get<string>("name");
            client.LoginBuilder.Name = client.NetAvatar.FixedName;
        }
        else
        {
            NetAvatar netAvatar = new NetAvatar
            {
                Pos = new Vector2(scanner.Get<int>("posX"), scanner.Get<int>("posY")),
                NetId = scanner.Get<int>("netID"),
                UserId = scanner.Get<uint>("userID"),
                Name = scanner.Get<string>("name"),
                IsInvisible = scanner.Get<byte>("invis") == 1,
                IsMod = scanner.Get<byte>("mstate") == 1
            };
            client.NetObjectManager.Add(netAvatar);
        }
    }

    private static void onSetPos(ENetClient client, in Variant variant)
    {
        client.NetAvatar.Pos = variant.Get<Vector2>(0);
    }

    private static void onRemove(ENetClient client, in Variant variant)
    {
        string raw = variant.Get<string>(0);
        using var scanner = new TextScanner(raw);
        int netId = scanner.Get<int>("netID");
        client.NetObjectManager.Remove(netId);
    }

    private static void onRequestWorldSelectMenu(ENetClient client)
    {
        client.NetObjectManager.Reset();
    }

    private static void onConsoleMessage(ENetClient client, in Variant variant)
    {
        string text = variant.Get<string>(0);

        if (text.Contains("Where would you like to go?"))
        {
            client.NetObjectManager.Reset();
            client.World.Reset();
            client.World.Name = "EXIT";
        }

        if (text.Contains("_CT:"))
        {
            string ok = MiscUtils.Between(text, "CP:", "_CT:");
            text = text.Replace("CP:", "").Replace("_CT:", "").Replace(ok, "").Replace("]_", "]");
        }
        client.ConsoleManager.Append(text, client.FeatureFlags);

        if (text.Contains("You've been") && text.Contains("BANNED"))
        {
            string banDays = MiscUtils.Between(text, "for", "days");
            List<ushort> numbers = [];
            MatchCollection matches = Regex.Matches(banDays, @"\d+");
            foreach (Match match in matches)
            {
                if (ushort.TryParse(match.Value, out ushort number))
                {
                    numbers.Add(number);
                }
            }
            ushort banDaysUshort = 0;
            foreach (ushort number in numbers)
            {
                banDaysUshort += number;
            }
            client.BannedCallback?.Invoke(banDaysUshort);
        }
    }


    private static void onTalkBubble(ENetClient client, in Variant variant)
    {
        if (client.FeatureFlags.HasFlag(ClientFeatureFlags.BotDetection))
        {
            int netId = variant.Get<int>(0);
            string text = variant.Get<string>(1);
            string txt = "";
            if (text.Contains("ID:_"))
            {
                string[] split = text.Split("ID:_");
                if (split.Length < 2) return;
                txt = split.Last();
            }
            string chat = txt.Replace("player_chat=", "").TrimStart();
            NetAvatar? player = client.NetObjectManager.GetPlayerByNetId(netId);
            if (player == null) return;
            client.BotDetector.AppendChatMessage(player, chat);
        }
    }

    private static void onDialogRequest(ENetClient client, in Variant variant)
    {
        string raw = variant.Get<string>(0);
        client.Dialog.Parse(raw);
        client.DialogRequestCallback?.Invoke(variant.Get<string>(0));
    }

    private static void onStorePurchaseResult(ENetClient client, in Variant variant)
    {
        string text = variant.Get<string>(0);
        if (text.Contains("You've purchased "))
        {
            client.LastStoreResult = StoreResult.Success;
        }
        else if (text.Contains("You can't afford "))
        {
            client.LastStoreResult = StoreResult.NoEnoughGems;
        }
        else if (text.Contains("space", StringComparison.CurrentCultureIgnoreCase))
        {
            client.LastStoreResult = StoreResult.NoEnoughSpace;
        }
    }

    private static void onSuperMainStartAcceptLogonHrdxs47254722215A(ENetClient client)
    {
        if (ItemInfoManager.Items == null)
        {
            client.State = ClientState.UpdatingItems;
            if (ItemInfoManager.LoadingItems == false)
            {
                ItemInfoManager.LoadingItems = true;
                client.SendGenericText("action|refresh_item_data\n");
            }
            new Thread(() =>
            {
                while (ItemInfoManager.ItemsLoaded == false)
                {
                    Thread.Sleep(100);
                }
                client.State = ClientState.Connected;
                client.SendGenericText("action|enter_game\n");
            }).Start();
        }
        else
        {
            client.State = ClientState.Connected;
            client.SendGenericText("action|enter_game\n");
        }
    }

    private static void onSetBux(ENetClient client, in Variant variant)
    {
        client.PlayerItems.Gems = variant.Get<int>(0);
    }

    private static void onSendToServer(ENetClient client, in Variant variant)
    {
        ushort port = variant.Get<ushort>(0);
        int token = variant.Get<int>(1);
        int userId = variant.Get<int>(2);

        string[] swap = variant.Get<string>(3).Split('|');
        string ip = swap[0];
        string doorId = swap[1];
        string uuid = swap[2];

        byte mode = variant.Get<byte>(4);

        client.State = ClientState.SwitchingServer;
        client.SendPacket(new GameUpdatePacket { Type = GamePacketType.Disconnect });
        client.SetConnectionData(userId, token, mode, doorId, uuid);
        client.Connect(ip, port);
    }
}