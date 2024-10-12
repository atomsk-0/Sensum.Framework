using System.Globalization;
using Cysharp.Text;
using Sensum.Framework.Utils.Extensions;

namespace Sensum.Framework.Proton;

public class TextScanner : IDisposable
{
    private const char separator = '|';
    private const char comment = '#';
    private readonly Dictionary<string, object> values = new();

    public TextScanner() {}

    public TextScanner(string payload)
    {
        Load(payload);
    }

    public void Dispose()
    {
        values.Clear();
        GC.SuppressFinalize(this);
    }

    public T Get<T>(string key)
    {
        return (T)Convert.ChangeType(values[key], typeof(T), CultureInfo.InvariantCulture);
    }

    public void Remove(string key)
    {
        values.Remove(key);
    }

    public void Set(string key, object value)
    {
        if (Contains(key))
            values[key] = value;
        else
            values.Add(key, value);
    }

    public bool Contains(string key)
    {
        return values.ContainsKey(key);
    }

    public void Load(string payload)
    {
        values.Clear();
        foreach (ReadOnlySpan<char> line in payload.SplitLines())
        {
            if (line.IsWhiteSpace()) continue;
            int separatorIndex = line.IndexOf(separator);
            if (separatorIndex == -1) continue;
            ReadOnlySpan<char> key = line[..separatorIndex];
            ReadOnlySpan<char> value = line[(separatorIndex + 1)..];
            if (key[0] == comment) continue;
            values.TryAdd(key.ToString(), value.ToString());
        }
    }


    public string ToEncodedString()
    {
        string unencoded = ToString();
        using var sb = ZString.CreateUtf8StringBuilder(true);
        foreach (char c in unencoded)
        {
            sb.AppendFormat("%{0:X2}", (byte)c);
        }
        return sb.ToString();
    }

    public override string ToString()
    {
        using var sb = ZString.CreateUtf8StringBuilder(true);
        foreach ((string key, object value) in values)
        {
            sb.Append(key);
            sb.Append(separator);
            sb.Append(value);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    ~TextScanner()
    {
        Dispose();
    }
}