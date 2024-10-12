namespace Sensum.Framework.Entities;

public readonly struct Proxy(string host, ushort port, string username = "", string password = "")
{
    public readonly string Host = host;
    public readonly ushort Port = port;
    public readonly string Username = username, Password = password;
}