using System.Drawing;
using System.Text.RegularExpressions;
using Sensum.Framework.Entities;
using Sensum.Framework.Proton;

namespace Sensum.Framework.Growtopia.Managers;

public class ConsoleManager : IResourceLifecycle
{
    public readonly List<ConnsoleMessage> Messages = [];

    public void Append(string text, ClientFeatureFlags featureFlags)
    {
        if (featureFlags.HasFlag(ClientFeatureFlags.ConsoleManager) == false) return;
        if (Messages.Count >= 175) Messages.RemoveAt(0);
        Messages.Add(new ConnsoleMessage(text));
        MessageAddedCallback?.Invoke(text);
    }

    public void Reset()
    {
        Messages.Clear();
    }

    public void Destroy()
    {
        Reset();
        MessageAddedCallback = null;
    }

    public Action<string>? MessageAddedCallback;
}

// Kinda old shit code and was designed just to be placeholder, but works fine tho...
public readonly partial struct ConnsoleMessage
{
    // ReSharper disable once CollectionNeverQueried.Global
    public readonly List<MessagePart> MessageParts = [];

    public ConnsoleMessage(string text) => tokenize(text);

    private void tokenize(string text)
    {
        var colorCodesAndColors = extractColorCodes(text);
        string cleanedString = removeColorCodes(text);
        string[] aw = cleanedString.Split(":X:X:X:");
        if (aw.Length != colorCodesAndColors.Count)
        {
            MessageParts.Add(new MessagePart {Text = aw[0], Color = RtColor.COLORS['v']});
            for (int i = 1; i < aw.Length; i++)
                MessageParts.Add(new MessagePart {Text = aw[i], Color = colorCodesAndColors[i - 1].Item2});
        }
        else
        {
            for (int i = 0; i < aw.Length; i++)
                MessageParts.Add(new MessagePart {Text = aw[i], Color = colorCodesAndColors[i].Item2});
        }
    }

    private static List<(char, Color)> extractColorCodes(string input)
    {
        List<(char, Color)> colorCodesAndColors = [];
        var regex = messageRegex();
        var matches = regex.Matches(input);

        foreach (Match match in matches)
        {
            char colorCode = match.Groups[1].Value.ToCharArray()[0];
            var color = RtColor.COLORS[colorCode];
            colorCodesAndColors.Add((colorCode, color));
        }

        return colorCodesAndColors;
    }

    private static string removeColorCodes(string input)
    {
        string cleanedString = Regex.Replace(input, "`([0-9`!@#$^&wobpqertascìv])", ":X:X:X:");
        cleanedString = cleanedString.Replace("<", "").Replace(">", "");
        return cleanedString;
    }

    [GeneratedRegex("`([0-9`!@#$^&wobpqertascìv])")]
    private static partial Regex messageRegex();
}

public struct MessagePart
{
    public string Text;
    public Color Color;
}