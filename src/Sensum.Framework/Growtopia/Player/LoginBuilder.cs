using System.Diagnostics;
using Cysharp.Text;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Proton;

namespace Sensum.Framework.Growtopia.Player;

public class LoginBuilder
{
    public AccountType Type;
    public Device Device;
    public string TankIdName = "", TankIdPass = "", Name = "";
    public string UuidToken = "", DoorId = "", Meta = "";
    public int Token = 0, User = 0;
    public byte LMode = 0;
    public string LoginToken = "";

    private static readonly Random random = new();

    public string BuildNew()
    {
        using var scanner = new TextScanner();
        scanner.Set("tankIDName", "");
        scanner.Set("tankIDPass", "");
        scanner.Set("requestedName", Device.GuestName);
        scanner.Set("f", "0");
        scanner.Set("protocol", App.Protocol);
        scanner.Set("game_version", App.Version);
        scanner.Set("fz", "40875032");
        scanner.Set("lmode", "0");
        scanner.Set("cbits", "1152");
        scanner.Set("player_age", "19");
        scanner.Set("GDPR", "1");
        scanner.Set("category", "_16");
        scanner.Set("totalPlaytime", "0");
        scanner.Set("klv", klv());
        scanner.Set("hash2", Device.Hash2);
        scanner.Set("meta", Meta);
        scanner.Set("fhash", "-716928004");
        scanner.Set("rid", Device.Rid);
        scanner.Set("platformID", "0,1,1");
        scanner.Set("deviceVersion", "0");
        scanner.Set("country", Device.Country.ToLower());
        scanner.Set("hash", Device.Hash);
        scanner.Set("mac", Device.Mac);
        scanner.Set("wk", Device.Wk);
        scanner.Set("zf", "283949556");
        return scanner.ToString();
    }

    public string Build(ENetClient client)
    {
        using var scanner = new TextScanner();

        if (LMode == 0)
        {
            scanner.Set("protocol", App.Protocol);
            scanner.Set("ltoken", LoginToken);
            scanner.Set("platformID", "0,1,1");
            client.ConsoleManager.Append($"Logging on {Name}...", client.FeatureFlags);
        }
        else
        {
            scanner.Set("requestedName", Device.GuestName);
            scanner.Set("f", "0");
            scanner.Set("protocol", App.Protocol);
            scanner.Set("game_version", App.Version);
            scanner.Set("fz", 40687656);
            scanner.Set("lmode", LMode);
            scanner.Set("cbits", 1024);
            scanner.Set("player_age", 28);
            scanner.Set("GDPR", 1);
            scanner.Set("category", "wotd_world");
            scanner.Set("totalPlaytime", 0);
            scanner.Set("klv", klv());
            scanner.Set("hash2", Device.Hash2);
            scanner.Set("meta", Meta);
            scanner.Set("fhash", -716928004);
            scanner.Set("rid", Device.Rid);
            scanner.Set("platformID", "0,1,1");
            scanner.Set("deviceVersion", 0);
            scanner.Set("country", Device.Country);
            scanner.Set("hash", Device.Hash);
            scanner.Set("mac", Device.Mac);

            if (Token != -1)
            {
                scanner.Set("user", User);
                scanner.Set("token", Token);
                scanner.Set("UUIDToken", UuidToken);
                scanner.Set("doorID", DoorId);
            }

            scanner.Set("wk", Device.Wk);
            scanner.Set("zf", -1304962833);
        }

        return scanner.ToString();
    }

    [Obsolete("Growtopia no longer will support direct login.")]
    public string BuildLegacy(ENetClient client)
    {
        using var scanner = new TextScanner();

        if (Type == AccountType.Legacy)
        {
            scanner.Set("tankIDName", TankIdName);
            scanner.Set("tankIDPass", TankIdPass);
        }

        scanner.Set("requestedName", Device.GuestName);
        scanner.Set("f", "0");
        scanner.Set("protocol", App.Protocol);
        scanner.Set("game_version", App.Version);
        scanner.Set("fz", 40687656);
        scanner.Set("lmode", LMode);
        scanner.Set("cbits", 1024);
        scanner.Set("player_age", 28);
        scanner.Set("GDPR", 1);
        scanner.Set("category", "wotd_world");
        scanner.Set("totalPlaytime", 0);
        scanner.Set("klv", klv());
        scanner.Set("hash2", Device.Hash2);
        scanner.Set("meta", Meta);
        scanner.Set("fhash", -716928004);
        scanner.Set("rid", Device.Rid);
        scanner.Set("platformID", "0,1,1");
        scanner.Set("deviceVersion", 0);
        scanner.Set("country", Device.Country);
        scanner.Set("hash", Device.Hash);
        scanner.Set("mac", Device.Mac);

        if (Token != -1)
        {
            scanner.Set("user", User);
            scanner.Set("token", Token);
            scanner.Set("UUIDToken", UuidToken);
            scanner.Set("doorID", DoorId);
        }

        scanner.Set("wk", Device.Wk);
        scanner.Set("zf", -1304962833);

        if (LMode == 0)
        {
            client.ConsoleManager.Append($"Logging on {Name}...", client.FeatureFlags);
        }

        return scanner.ToString();
    }


    [Obsolete("Growtopia no longer supports guest accounts dont have to parse growid account / legacy")]
    public void Parse(string authData, string guestName)
    {
        string[] parts = authData.Split('|');
        string first = parts[0];
        string second = parts[1];
        if (first.Contains(':') || second.Contains(':'))
        {
            string rid = first.Contains(':') ? second : first;
            string mac = first.Contains(':') ? first : second;
           SetGuest(mac, rid, guestName);
        }
        else
        {
            SetLegacy(first, second, guestName);
        }
    }

    public void SetGoogle(string gmail, string password, string guestName)
    {
        Device = Device.CreateDevice();
        Type = AccountType.Google;
        TankIdName = gmail;
        TankIdPass = password;
        Device.GuestName = guestName;
        Name = gmail;
    }

    public void SetLegacy(string tankIdName, string tankIdPass, string guestName)
    {
        Device = Device.CreateDevice();
        Type = AccountType.Legacy;
        TankIdName = tankIdName;
        TankIdPass = tankIdPass;
        Device.GuestName = guestName;
        Name = tankIdName;
    }

    [Obsolete("Growtopia no longer supports guest accounts")]
    public void SetGuest(string mac, string rid, string guestName)
    {
        /*Device = Device.CreateDevice();
        Type = AccountType.Guest;
        Device.Mac = mac;
        Device.Rid = rid;
        Device.GuestName = guestName;
        Name = guestName;*/
    }

    private string klv()
    {
        string[] constantValues =
        [
            HashUtils.Sha256HashString(HashUtils.Md5HashString(HashUtils.Sha256HashString(App.Protocol.ToString()))),
            HashUtils.Sha256HashString(HashUtils.Sha256HashString(App.Version)),
            HashUtils.Sha256HashString(HashUtils.Sha256HashString(App.Protocol.ToString()) + GameConstants.KLV_SALTS[3])
        ];
        return HashUtils.Sha256HashString(ZString.Concat(constantValues[0], GameConstants.KLV_SALTS[0], constantValues[1], GameConstants.KLV_SALTS[1], HashUtils.Sha256HashString(HashUtils.Md5HashString(HashUtils.Sha256HashString(Device.Rid))), GameConstants.KLV_SALTS[2], constantValues[2]));
    }

    public static string GenerateGuestName()
    {
        return GameConstants.GUEST_NAMES[random.Next(0, GameConstants.GUEST_NAMES.Length - 1)] + GameConstants.GUEST_NAMES[random.Next(0, GameConstants.GUEST_NAMES.Length - 1)];
    }

    public static string GetRandomGuestName()
    {
        return GameConstants.GUEST_NAMES[random.Next(0, GameConstants.GUEST_NAMES.Length - 1)];
    }
}