using System.Text;
using Cysharp.Text;

namespace Sensum.Framework.Utils;

public static unsafe class Memory
{
    public static void Skip(ref int offset, int count) => offset += count;

    public static T Read<T>(byte* data, ref int offset, int dataSize) where T : unmanaged
    {
        if (dataSize < offset + sizeof(T))
        {
            throw new Exception(ZString.Concat("Memory read overflow. Expected: ", sizeof(T), " bytes, but only ", dataSize - offset, " bytes left."));
        }
        T value = *(T*)(data + offset);
        offset += sizeof(T);
        return value;
    }

    public static string ReadString(byte* data, ref int offset, int dataSize)
    {
        ushort length = Read<ushort>(data, ref offset,  dataSize);
        return ReadString(data, ref offset, length, dataSize);
    }

    public static string ReadString(byte* data, ref int offset, int length, int dataSize)
    {
        if (dataSize < offset + length)
        {
            throw new Exception(ZString.Concat("Memory read overflow. Expected: ", length, " bytes, but only ", dataSize - offset, " bytes left."));
        }
        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(data + offset, length);
        offset += length;
        return Encoding.ASCII.GetString(span);
    }
}