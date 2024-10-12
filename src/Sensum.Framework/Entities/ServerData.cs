namespace Sensum.Framework.Entities;

public readonly struct ServerData(string host, ushort port, string meta, string loginUrl, string maintenance)
{
    public readonly string Host = host;
    public readonly ushort Port = port;
    public readonly string Meta = meta;
    public readonly string LoginUrl = loginUrl;
    public readonly string Maintenance = maintenance;
}