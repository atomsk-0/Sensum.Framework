using System.Text;

namespace Sensum.Framework.Utils.Extensions;

public static class StringExtensions
{
    public static string AddSpacesToSentence(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";
        var newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
                newText.Append(' ');
            newText.Append(text[i]);
        }

        return newText.ToString();
    }


    public static LineSplitEnumerator SplitLines(this string str)
    {
        // LineSplitEnumerator is a struct so there is no allocation here
        return new LineSplitEnumerator(str.AsSpan());
    }


// Must be a ref struct as it contains a ReadOnlySpan<char>
    public ref struct LineSplitEnumerator(ReadOnlySpan<char> str)
    {
        private ReadOnlySpan<char> str = str;


        // Needed to be compatible with the foreach operator
        public LineSplitEnumerator GetEnumerator()
        {
            return this;
        }


        public bool MoveNext()
        {
            ReadOnlySpan<char> span = str;
            if (span.Length == 0) // Reach the end of the string
                return false;

            int index = span.IndexOfAny('\r', '\n');
            if (index == -1) // The string is composed of only one line
            {
                str = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                Current = new LineSplitEntry(span, ReadOnlySpan<char>.Empty);
                return true;
            }

            if (index < span.Length - 1 && span[index] == '\r')
            {
                // Try to consume the '\n' associated to the '\r'
                char next = span[index + 1];
                if (next == '\n')
                {
                    Current = new LineSplitEntry(span[..index], span.Slice(index, 2));
                    str = span[(index + 2)..];
                    return true;
                }
            }

            Current = new LineSplitEntry(span[..index], span.Slice(index, 1));
            str = span[(index + 1)..];
            return true;
        }


        public LineSplitEntry Current { get; private set; } = default;
    }

    public readonly ref struct LineSplitEntry(ReadOnlySpan<char> line, ReadOnlySpan<char> separator)
    {
        public ReadOnlySpan<char> Line { get; } = line;
        public ReadOnlySpan<char> Separator { get; } = separator;


        // This method allow to deconstruct the type, so you can write any of the following code
        // foreach (var entry in str.SplitLines()) { _ = entry.Line; }
        // foreach (var (line, endOfLine) in str.SplitLines()) { _ = line; }
        // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct?WT.mc_id=DT-MVP-5003978#deconstructing-user-defined-types
        public void Deconstruct(out ReadOnlySpan<char> line, out ReadOnlySpan<char> separator)
        {
            line = Line;
            separator = Separator;
        }


        // This method allow to implicitly cast the type into a ReadOnlySpan<char>, so you can write the following code
        // foreach (ReadOnlySpan<char> entry in str.SplitLines())
        public static implicit operator ReadOnlySpan<char>(LineSplitEntry entry)
        {
            return entry.Line;
        }
    }
}