using System.Security.Cryptography;
using System.Text;

namespace Sensum.Framework.Proton;

public static class HashUtils
{
    public static string GetFileMd5Hash(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public static string Md5HashString(string input)
    {
        ReadOnlySpan<byte> inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);
        char[] result = new char[hashBytes.Length * 2];
        for (int i = 0; i < hashBytes.Length; i++)
        {
            byte b = hashBytes[i];
            result[i * 2] = getHexValue(b / 16);
            result[i * 2 + 1] = getHexValue(b % 16);
        }
        return new string(result).ToUpper();
    }

    public static string Sha256HashString(string input)
    {
        ReadOnlySpan<byte> inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        char[] result = new char[hashBytes.Length * 2];
        for (int i = 0; i < hashBytes.Length; i++)
        {
            byte b = hashBytes[i];
            result[i * 2] = getHexValue(b / 16);
            result[i * 2 + 1] = getHexValue(b % 16);
        }
        return new string(result);
    }

    private static char getHexValue(int i)
    {
        return i < 10 ? (char)(i + '0') : (char)(i - 10 + 'a');
    }
}