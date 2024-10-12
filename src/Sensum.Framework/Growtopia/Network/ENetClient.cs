using System.Diagnostics;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Growtopia.Features;
using Sensum.Framework.Growtopia.Handlers;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Growtopia.Player;
using Sensum.Framework.Growtopia.World;
using Sensum.Framework.Proton;
using HttpRequestError = Sensum.Framework.Entities.HttpRequestError;

namespace Sensum.Framework.Growtopia.Network;

public abstract unsafe class ENetClient: IResourceLifecycle
{
    public readonly NetObjectManager NetObjectManager = new();
    public readonly ConsoleManager ConsoleManager = new();
    public readonly LoginBuilder LoginBuilder = new();
    public readonly BotDetector BotDetector = new();
    public readonly PlayerItems PlayerItems = new();
    public readonly NetAvatar NetAvatar = new();
    public readonly WorldMap World = new();
    public readonly Dialog Dialog = new();

    public ClientFeatureFlags FeatureFlags;
    public StoreResult LastStoreResult;
    public Proxy? LastProxyUsed;

    public ClientState State;
    public uint ConnectionTimeout;
    public uint TimeSinceLastGeigerSignal;
    public bool LoggedUsingCheck;
    public bool CanStartTimeout;
    public bool Running;

    public nint Host;
    public nint Peer;

    #if DEBUG
    public readonly LinkedList<long> PollTimes = [];
    #endif

    public uint Ping => Peer == 0 ? 500 : ENet.GetPeerPing(Peer) / 2;

    public abstract void Connect();
    public abstract void Connect(string ip, ushort port, in Proxy proxy);
    public abstract void Connect(string ip, ushort port);
    public abstract void Disconnect();
    public abstract string? GetLoginToken(ENetClient client);

    internal void SetConnectionData(int userId, int token, byte mode, string doorId, string uuid)
    {
        if (token != -1)
        {
            LoginBuilder.Token = token;
            LoginBuilder.UuidToken = uuid;
        }
        LoginBuilder.User = userId;
        LoginBuilder.LMode = mode;
        LoginBuilder.DoorId = doorId;
        if (LoginBuilder.DoorId.Length > 1)
        {
            World.DoorId = doorId;
        }
    }

    internal void Poll()
    {
        #if DEBUG
        Stopwatch stopwatch = new();
        #endif
        while (Running)
        {
            #if DEBUG
            stopwatch.Restart();
            #endif
            if (Host == 0 || Peer == 0)
            {
                Thread.Sleep(500);
                continue;
            }
            service();
            connectionTimeoutCheck();
            Thread.Sleep(1);
            #if DEBUG
            PollTimes.AddLast(stopwatch.ElapsedMilliseconds);
            if (PollTimes.Count > 500) PollTimes.RemoveFirst();
            #endif
        }
    }

    private void service()
    {
        ENetEvent netEvent;
        if (ENet.Service(Host, &netEvent, 0) <= 0) return;
        switch (netEvent.type)
        {
            case ENetEventType.Connect:
                ENetConnectedCallback?.Invoke();
                onConnect();
                break;
            case ENetEventType.Receive:
                onReceive(netEvent.packet);
                ENet.DestroyPacket(netEvent.packet);
                break;
            case ENetEventType.Disconnect or ENetEventType.DisconnectTimeout:
                onDisconnect();
                break;
        }
    }

    private void onConnect()
    {
        ConnectionTimeout = 0;
        ConnectedCallback?.Invoke();
    }

    private void onReceive(ENetPacket* packet)
    {
        NetMessageHandler.Handle(this, packet);
    }

    private void onDisconnect()
    {
        Reset();
        PlayerItems.Reset();
        DisconnectedCallback?.Invoke();
        State = ClientState.Disconnected;
    }

    private void connectionTimeoutCheck()
    {
        if (ConnectionTimeout == 0) return;
        if (MiscUtils.UtcUnixTimeStamp < ConnectionTimeout) return;
        ConnectionTimeoutCallback?.Invoke();
        Reset();
    }

    public virtual void Reset()
    {
        if (Peer != 0)
        {
            ENet.Disconnect(Peer, 0);
            Peer = 0;
        }

        if (Host != 0)
        {
            ENet.Destroy(Host);
            Host = 0;
        }

        ConnectionTimeout = 0;
        State = ClientState.Disconnected;

        NetObjectManager.Reset();
        BotDetector.Reset();
        Dialog.Reset();
        World.Reset();
    }

    public virtual void Destroy()
    {
        Reset();
        NetObjectManager.Destroy();
        ConsoleManager.Destroy();
        BotDetector.Destroy();
        PlayerItems.Destroy();
        Dialog.Destroy();
        World.Destroy();

        BannedCallback = null;
        ConnectedCallback = null;
        DisconnectedCallback = null;
        DialogRequestCallback = null;
        ENetConnectedCallback = null;
        SaveLoginTokenCallback = null;
        VariantReceivedCallback = null;
        NetMessageReceivedCallback = null;
        FailedToGetLoginTokenCallback = null;
        FailedToGetServerDataCallback = null;
        GameUpdatePacketReceivedCallback = null;
        NetMessageSentCallback = null;
        GamePacketSentCallback = null;
    }

    ~ENetClient() => Destroy();

    public Action<NetMessageType, string>? NetMessageSentCallback;
    public Action<GameUpdatePacket>? GamePacketSentCallback;
    public Action? ConnectedCallback;
    public Action? DisconnectedCallback;
    public Action? ConnectionTimeoutCallback;
    public Action? ENetConnectedCallback;
    public Action<int>? BannedCallback;
    public Action<string>? SaveLoginTokenCallback;
    public Action<HttpRequestError>? FailedToGetServerDataCallback;
    public Action<HttpRequestError>? FailedToGetLoginTokenCallback;
    public Action<string>? DialogRequestCallback;
    public Action<Variant>? VariantReceivedCallback;
    public Action<NetMessageDelegate>? NetMessageReceivedCallback;
    public Action<AuthenticationError>? AuthenticationErrorCallback;
    public Action<GameUpdatePacketDelegate>? GameUpdatePacketReceivedCallback;
    public Action<GeigerSignal>? GeigerSignalChangedCallback;
}