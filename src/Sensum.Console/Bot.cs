using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Growtopia.Network;
using ZeroLog;

namespace Sensum.Console;

public class Bot : ENetClient
{
    internal static readonly Log LOGGER = LogManager.GetLogger(typeof(Bot));

    private readonly Proxy proxy;

    public Bot(Proxy proxy)
    {
        this.proxy = proxy;
        SaveLoginTokenCallback = onSaveLoginToken;

    }

    private void onSaveLoginToken(string loginToken)
    {
        //File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{LoginBuilder.Name}-login_token.txt"), loginToken);
    }

    public override void Connect()
    {
        ConnectionManager.Connect(this, proxy);
    }

    public override void Connect(string ip, ushort port, in Proxy proxy)
    {
        ConnectionManager.Connect(this, ip, port, proxy);
    }

    public override void Connect(string ip, ushort port)
    {
        ConnectionManager.Connect(this, ip, port, proxy);
    }

    public override void Disconnect()
    {
        ConnectionManager.Disconnect(this);
    }

    public override string? GetLoginToken(ENetClient client)
    {
        return null;
        if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{LoginBuilder.Name}-login_token.txt")))
        {
            return File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{LoginBuilder.Name}-login_token.txt"));
        }
        return null;
    }
}