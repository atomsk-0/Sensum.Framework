using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Text;
using Sensum.Framework.Entities;

namespace Sensum.Framework.Proton;

public static class MiscUtils
{
    public static uint UtcUnixTimeStamp => (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public static uint HashBytes(IEnumerable<byte> b) => b.Aggregate<byte, uint>(0x55555555, (current, t) => (current >> 27) + (current << 5) + t);
    public static bool IsInside(in Vector2 circle, int rad, Vector2 circle2) => (circle2.X - circle.X) * (circle2.X - circle.X) + (circle2.Y - circle.Y) * (circle2.Y - circle.Y) <= rad * rad;
    public static bool IsInside(in Vector2Int circle, int rad, Vector2Int circle2) => (circle2.X - circle.X) * (circle2.X - circle.X) + (circle2.Y - circle.Y) * (circle2.Y - circle.Y) <= rad * rad;
    public static string Between(string str, string firstString, string lastString)
    {
        int pos1 = str.IndexOf(firstString, StringComparison.Ordinal) + firstString.Length;
        int pos2 = str.IndexOf(lastString, StringComparison.Ordinal);
        string finalString = str.AsSpan().Slice(pos1, pos2 - pos1).ToString();
        return finalString;
    }

    public static string EncodeToUrlString(string input)
    {
        Regex asciiLetterRegex = new(@"[a-zA-Z0-9]");
        var encodedString = ZString.CreateStringBuilder();
        foreach (char c in input)
        {
            if (asciiLetterRegex.IsMatch(c.ToString()))
            {
                encodedString.Append(c);
                continue;
            }
            encodedString.Append('%');
            encodedString.Append(((int)c).ToString("X2"));
            /*if (c == '|') encodedString.Append("%7C");
            else if (c == '\n') encodedString.Append("%0A");
            else if (c == '-') encodedString.Append("%2D");
            else if (c == ',') encodedString.Append("%2C");
            else if (c == '_') encodedString.Append("%5F");
            else if (c == '.') encodedString.Append("%2E");
            else if (c == '=') encodedString.Append("%3D");
            else if (c == '/') encodedString.Append("%2F"); //? \/
            else if (c == '\\') encodedString.Append("%5C");
            else if (c == '+') encodedString.Append("%2B");
            else encodedString.Append(c);*/
        }

        return encodedString.ToString();
    }
}