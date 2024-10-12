namespace Sensum.Framework.Utils.Extensions;

public static class ArrayExtensions
{
    public static int IndexOf<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i]!.Equals(value))
                return i;
        }
        return -1;
    }

    public static int FindIndex<T>(this T[] array, Predicate<T> match)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (match(array[i]))
                return i;
        }
        return -1;
    }
}