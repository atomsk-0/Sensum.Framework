using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Authentications;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Proton;
using HttpRequestError = Sensum.Framework.Entities.HttpRequestError;

namespace Sensum.Framework.Growtopia.Managers;

public static unsafe class ConnectionManager
{
    public static void Connect(ENetClient client, string host, ushort port, in Proxy proxy)
    {
        client.Reset();

        client.Host = ENet.Create(null, 1, 2, 0, 0);
        ENet.CompressWithRangeCoder(client.Host);
        ENet.UseNewPacket(client.Host);
        ENet.UseCrc32(client.Host);
        client.LastProxyUsed = proxy;

        if (proxy.Host != App.IGNORED_PROXY_HOST)
        {
            ENet.SetProxy(client.Host, proxy.Host, proxy.Port, proxy.Username, proxy.Password);
        }

        ENetAddress address;
        ENet.SetAddressHost(&address, host);
        address.port = port;

        client.State = ClientState.Connecting;

        client.Peer = ENet.Connect(client.Host, &address, 2, 0);
        client.ConnectionTimeout = MiscUtils.UtcUnixTimeStamp + 15;

        if (client.Running == false)
        {
            client.Running = true;
            new Thread(client.Poll).Start();
        }
    }

    public static bool Connect(ENetClient client, in Proxy proxy)
    {
        client.LastProxyUsed = proxy;
        client.State = ClientState.RequestingServer;
        client.ConsoleManager.Append("Getting `wserver address``..", client.FeatureFlags);
        if (App.TryGetServerData(out ServerData serverData, out HttpRequestError errorSData, proxy) == false)
        {
            client.State = ClientState.ServerRequestFail;
            client.ConsoleManager.Append("`4Error 403: Failed to reteive server data``..", client.FeatureFlags);
            client.FailedToGetServerDataCallback?.Invoke(errorSData);
            return false;
        }
        client.State = ClientState.RequestingToken;
        client.LoginBuilder.Meta = serverData.Meta;
        string? loginToken = client.GetLoginToken(client);
        if (loginToken != null)
        {
            if (App.TryCheckToken(ref loginToken, client, proxy, out _) == false)
            {
                loginToken = null;
            }
        }
        if (loginToken == null)
        {
            string authToken = "";
            switch (client.LoginBuilder.Type)
            {
                case AccountType.Legacy:
                    if (LegacyAuth.TryAuthenticate(client, proxy, out authToken, out HttpRequestError legacyAuthError) == false)
                    {
                        if (client.State is not (ClientState.WrongCredentials or ClientState.TooManyPeopleLogging))
                        {
                            client.State = ClientState.FailedToGetLoginToken;
                        }
                        client.ConsoleManager.Append("`4Error 403: Failed to reteive token``..", client.FeatureFlags);
                        client.FailedToGetLoginTokenCallback?.Invoke(legacyAuthError);
                    }
                    break;
                case AccountType.Ubiconnect:
                    break;
                case AccountType.Google:
                    if (GoogleAuth.TryAuthenticate(client, proxy, out authToken, out HttpRequestError googleAuthError) == false)
                    {
                        if (client.State is not (ClientState.WrongCredentials or ClientState.TooManyPeopleLogging or ClientState.CaptchaNotSupportedYet or ClientState.TextCaptchaFailed))
                        {
                            client.State = ClientState.FailedToGetLoginToken;
                        }
                        client.ConsoleManager.Append("`4Error 403: Failed to reteive token``..", client.FeatureFlags);
                        client.FailedToGetLoginTokenCallback?.Invoke(googleAuthError);
                        GoogleAuth.DestroyDriver(client.LoginBuilder.TankIdName);
                    }
                    break;
                case AccountType.Apple:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (string.IsNullOrEmpty(authToken)) return false;
            client.LoginBuilder.LoginToken = authToken;
        }
        else
        {
            client.LoginBuilder.LoginToken = loginToken;
        }

        client.SaveLoginTokenCallback?.Invoke(client.LoginBuilder.LoginToken);


        if (string.IsNullOrEmpty(serverData.Maintenance) == false)
        {
            client.State = ClientState.Maintenance;
            client.ConsoleManager.Append(serverData.Maintenance, client.FeatureFlags);
            return false;
        }

        client.SetConnectionData(-1, -1, 0, "", "");

        client.CanStartTimeout = true;

        client.ConsoleManager.Append("Located `wserver``, connecting...", client.FeatureFlags);
        client.Connect(serverData.Host, serverData.Port, proxy);
        return true;
    }

    public static bool ConnectSync(ENetClient client, in Proxy proxy)
    {
        if (Connect(client, proxy) == false) return false;
        while (client.CanStartTimeout == false)
        {
            Thread.Sleep(100);
        }
        client.CanStartTimeout = false;
        byte timeout = 25;
        byte time = 0;
        while (client.State != ClientState.Connected)
        {
            if (time >= timeout) return false;
            time++;
            Thread.Sleep(1000);
        }

        return true;
    }

    public static void Disconnect(this ENetClient client)
    {
        client.Reset();
        client.PlayerItems.Reset();
        client.DisconnectedCallback?.Invoke();
        client.State = ClientState.Disconnected;
    }
}