using System.Security.Cryptography;
using Cysharp.Text;

namespace Sensum.Framework.Entities;

public struct Device
{
    private const byte mac_address_length = 6;
    private const string chars = "ABCDEF0123456789";
    private const byte rid_length = 32;

    private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
    private static readonly Random random = new();
    public string Country, Hash, Hash2, Rid, Mac, Wk, GuestName;

    public static Device CreateDevice()
    {
        return new Device
        {
            Country = "CA",
            Hash = random.Next(-777777776, 777777776).ToString(),
            Hash2 = random.Next(-777777776, 777777776).ToString(),
            Rid = generateRid(),
            Mac = generateMacAddress(),
            Wk = generateUniqueWinKey()
        };
    }

    private static string generateMacAddress()
    {
        Span<byte> macAddress = stackalloc byte[mac_address_length];
        rng.GetNonZeroBytes(macAddress);
        using var macAddressBuilder = ZString.CreateStringBuilder(true);
        for (byte i = 0; i < mac_address_length; i++) macAddressBuilder.Append(macAddress[i].ToString("X2"));
        return string.Join(':', macAddressBuilder.ToString());
    }

    private static string generateRid()
    {
        Span<byte> buffer = stackalloc byte[rid_length];
        Span<char> result = stackalloc char[rid_length];
        rng.GetBytes(buffer);
        for (byte i = 0; i < rid_length; i++) result[i] = chars[buffer[i] % chars.Length];
        return new string(result);
    }

    private static string generateUniqueWinKey()
    {
        using var builder = ZString.CreateStringBuilder(true);
        Span<byte> buffer = stackalloc byte[31];
        rng.GetBytes(buffer);
        for (byte i = 0; i < 31; i++) builder.Append(chars[buffer[i] % chars.Length]);
        return builder.ToString();
    }
}