using System.Runtime.InteropServices;
// ReSharper disable All

namespace Sensum.Framework.Proton;

#pragma warning disable CS8500
#pragma warning disable CS8618

public static unsafe partial class ENet
{
    private const string enet_lib = "enet";


    [LibraryImport(enet_lib, EntryPoint = "enet_initialize")]
    public static partial int Initialize();

    [LibraryImport(enet_lib, EntryPoint = "enet_linked_version")]
    public static partial ENetVersion GetLinkedVersion();

    [LibraryImport(enet_lib, EntryPoint = "enet_time_get")]
    public static partial uint GetENetTime();

    [LibraryImport(enet_lib, EntryPoint = "enet_address_set_host", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int SetAddressHost(ENetAddress* address, string hostName);

    [LibraryImport(enet_lib, EntryPoint = "enet_peer_get_id")]
    public static partial uint GetPeerId(nint peer);

    [LibraryImport(enet_lib, EntryPoint = "enet_peer_get_state")]
    public static partial ENetPeerState GetPeerState(nint peer);

    [LibraryImport(enet_lib, EntryPoint = "enet_packet_get_datas")]
    public static partial nint GetPacketData(ENetPacket packet);

    [LibraryImport(enet_lib, EntryPoint = "enet_packet_get_length")]
    public static partial uint GetPacketLength(ENetPacket packet);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_create")]
    public static partial nint Create(ENetAddress* address, nuint peerLimit, nuint channelLimit, uint incomingBandwidth = 0, uint outgoingBandwidth = 0);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_destroy")]
    public static partial void Destroy(nint host);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_connect")]
    public static partial nint Connect(nint host, ENetAddress* address, nuint channelCount, uint data);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_check_events")]
    public static partial int CheckEvents(nint host, ENetEvent* netEvent);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_service")]
    public static partial int Service(nint host, ENetEvent* netEvent, uint timeout);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_send_raw")]
    public static partial int SendRaw(nint host, ENetAddress* address, byte* data, nuint dataLength);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_flush")]
    public static partial void Flush(nint host);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_broadcast")]
    public static partial void Broadcast(nint host, byte channelId, ENetPacket* packet);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_random_seed")]
    public static partial ulong RandomHostSeed();

    [LibraryImport(enet_lib, EntryPoint = "enet_peer_send")]
    public static partial int Send(nint peer, byte channelId, ENetPacket* packet);

    [LibraryImport(enet_lib)]
    public static partial ENetPacket* PeerReceive(nint peer, byte* channelId);

    [LibraryImport(enet_lib, EntryPoint = "enet_peer_ping")]
    public static partial void PingPeer(nint peer);

    [LibraryImport(enet_lib, EntryPoint = "enet_peer_ping_interval")]
    public static partial void PeerPingInterval(nint peer, uint pingInterval);

    [LibraryImport(enet_lib, EntryPoint = "enet_peer_timeout")]
    public static partial void SetPeerTimeout(nint peer, uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum);

    [LibraryImport(enet_lib, EntryPoint = "enet_peer_reset")]
    public static partial void ResetPeer(nint peer);

    [LibraryImport(enet_lib, EntryPoint = "enet_peer_disconnect")]
    public static partial void Disconnect(nint peer, uint data);

    [LibraryImport(enet_lib, EntryPoint = "enet_peer_disconnect_now")]
    public static partial void DisconnectNow(nint peer, uint data);

    [LibraryImport(enet_lib, EntryPoint = "enet_peer_disconnect_later")]
    public static partial void DisconnectLater(nint peer, uint data);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_use_crc32")]
    public static partial void UseCrc32(nint host);

    [LibraryImport(enet_lib, EntryPoint = "enet_packet_destroy")]
    public static partial void DestroyPacket(ENetPacket* packet);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_compress_with_range_coder")]
    public static partial int CompressWithRangeCoder(nint host);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_use_new_packet")]
    public static partial void UseNewPacket(nint host);

    [LibraryImport(enet_lib, EntryPoint = "enet_host_use_proxy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void SetProxy(nint host, [MarshalAs(UnmanagedType.LPStr)] string ip, ushort port, [MarshalAs(UnmanagedType.LPStr)] string username, [MarshalAs(UnmanagedType.LPStr)] string password);

    [LibraryImport(enet_lib, EntryPoint = "enet_packet_create")]
    public static partial ENetPacket* CreatePacket(nint data, nuint dataLength, ENetPacketFlag flags);

    [LibraryImport(enet_lib, EntryPoint = "enet_peer_get_ping")]
    public static partial uint GetPeerPing(nint peer);
}

[StructLayout(LayoutKind.Explicit)]
public struct ENetAddress
{
    [FieldOffset(16)] public ushort port;
}

[Flags]
public enum ENetPacketFlag
{
    Reliable = 1 << 0,
    Unsequenced = 1 << 1,
    NoAllocate = 1 << 2,
    UnreliableFragment = 1 << 3,
    Sent = 1 << 8
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ENetPacket
{
    public UIntPtr referenceCount; //0x0
    public uint flags;             //0x8
    public IntPtr data;            //0xc
    public UIntPtr dataLength;     //0x14
    public IntPtr freeCallback;    //0x1c
    public IntPtr userData;        //0x24

    public Span<byte> GetDataAsSpan => new(Data, unchecked((int)dataLength));

    public byte* Data => (byte*)data.ToPointer();

    public int GetDataLength => unchecked((int)dataLength);
}

public enum ENetPeerState
{
    Disconnected = 0,
    Connecting = 1,
    AcknowledgingConnect = 2,
    ConnectionPending = 3,
    ConnectionSucceeded = 4,
    Connected = 5,
    DisconnectLater = 6,
    Disconnecting = 7,
    AcknowledgingDisconnect = 8,
    Zombie = 9
}

[StructLayout(LayoutKind.Sequential)]
public struct ENetBuffer
{
    public UIntPtr dataLength;
    public IntPtr data;
}


public enum ENetEventType
{
    None = 0,
    Connect = 1,
    Disconnect = 2,
    Receive = 3,
    DisconnectTimeout = 4
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ENetEvent
{
    public ENetEventType type;
    public IntPtr peer;
    public byte channelID;
    public uint data;
    public ENetPacket* packet;
}

[StructLayout(LayoutKind.Sequential)]
public struct ENetVersion
{
    public byte major;
    public byte minor;
    public byte patch;
}