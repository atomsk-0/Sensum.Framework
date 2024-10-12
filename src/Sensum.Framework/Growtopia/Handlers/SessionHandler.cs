using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Proton;
using static Sensum.Framework.Proton.ENet;

namespace Sensum.Framework.Growtopia.Handlers;

internal static unsafe class SessionHandler
{
    private const string event_type_str = "eventType";
    private const string event_name_str = "eventName";

    internal static void Handle(ENetClient client, ENetPacket* packet)
    {
        string text = PacketManager.GetTextPointerFromPacket(packet);

        using var scanner = new TextScanner(text);
        if (!scanner.Contains(event_type_str)) return;
        if (!scanner.Contains(event_name_str)) return;

        int eventType = scanner.Get<int>(event_type_str);
        string eventName = scanner.Get<string>(event_name_str);
        switch (eventType)
        {
            case 0:
            {
                switch (eventName)
                {
                    case "102_PLAYER.AUTHENTICATION":
                        playerAuthentication(scanner, client);
                        break;
                    case "100_MOBILE.START":
                        mobileStart(scanner, client);
                        break;
                }
                break;
            }
        }
    }

    private static void playerAuthentication(TextScanner scanner, ENetClient client)
    {
        int authenticated = scanner.Get<int>("Authenticated");
        if (authenticated == 1) return;
        var authError = (AuthenticationError)scanner.Get<int>("Authentication_error");
        switch (authError)
        {
            case AuthenticationError.SubServersSyncing:
                break;
            case AuthenticationError.Suspend:
                client.State = ClientState.Suspended;
                break;
            case AuthenticationError.Banned:
                client.State = ClientState.Banned;
                break;
            case AuthenticationError.TooManyPeopleLogging:
                client.State = ClientState.TooManyPeopleLogging;
                break;
            case AuthenticationError.Maintenance:
                client.State = ClientState.Maintenance;
                break;
            case AuthenticationError.GameUpdate:
                client.State = ClientState.GameUpdate;
                break;
            case AuthenticationError.BadGuestName:
                client.State = ClientState.BadGuestName;
                break;
            case AuthenticationError.WrongCredentials:
                client.State = ClientState.WrongCredentials;
                break;
            case AuthenticationError.GuestLimitReached:
                client.State = ClientState.GuestLimitReached;
                break;
        }
        client.AuthenticationErrorCallback?.Invoke(authError);
    }

    private static void mobileStart(TextScanner scanner, ENetClient client)
    {
        client.NetAvatar.Level = scanner.Get<byte>("Level");
        client.PlayerItems.Gems = scanner.Get<int>("Gems_balance");
    }
}