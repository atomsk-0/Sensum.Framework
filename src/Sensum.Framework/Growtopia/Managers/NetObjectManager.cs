using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Player;

namespace Sensum.Framework.Growtopia.Managers;

public class NetObjectManager : IResourceLifecycle
{
    public readonly LinkedList<NetAvatar> Players = [];

    public NetAvatar? GetPlayerByNetId(int netId)
    {
        return Players.FirstOrDefault(c => c.NetId == netId);
    }

    public NetAvatar? GetPlayerAt(Vector2Int tilePos)
    {
        return Players.FirstOrDefault(c => c.TilePos == tilePos);
    }

    public void Add(NetAvatar netAvatar)
    {
        Players.AddLast(netAvatar);
        PlayerJoinedCallback?.Invoke(netAvatar);
    }

    public void Remove(int netId)
    {
        if (Players.Any(c => c.NetId == netId) == false) return;
        var netAvatar = Players.First(c => c.NetId == netId);
        Remove(netAvatar);
    }

    public void Remove(NetAvatar netAvatar)
    {
        PlayerLeftCallback?.Invoke(netAvatar);
        Players.Remove(netAvatar);
    }

    public void Reset()
    {
        Players.Clear();
    }

    public void Destroy()
    {
        Reset();
        PlayerJoinedCallback = null;
        PlayerLeftCallback = null;
    }

    public Action<NetAvatar>? PlayerJoinedCallback;
    public Action<NetAvatar>? PlayerLeftCallback;
}