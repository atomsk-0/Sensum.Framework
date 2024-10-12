using System.Globalization;
using System.Numerics;
using Cysharp.Text;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Utils;

namespace Sensum.Framework.Growtopia.Entities.Structs;

public readonly struct Variant(VariantFunction function, string unknownName, object[] @params)
{
    public readonly VariantFunction Function = function;
    public readonly string UnknownName = unknownName;
    public readonly object[] Params = @params;

    public T Get<T>(byte index)
    {
        return (T)Convert.ChangeType(Params[index], typeof(T), CultureInfo.InvariantCulture);
    }

    public string GetString(byte index)
    {
        return (string)Params[index];
    }

    public int GetInt(byte index)
    {
        return (int)Params[index];
    }

    internal static unsafe Variant Serialize(byte* data, int dataSize)
    {
        int offset = 0;
        byte count = Memory.Read<byte>(data, ref offset, dataSize);

        object[] variantParams = new object[count]; // removed -1.. caused index out of range not sure why
        string unknownFunc = null!;

        /* Reads variant func name */
        Memory.Skip(ref offset, 2);
        string str = Memory.ReadString(data, ref offset, Memory.Read<int>(data, ref offset, dataSize), dataSize);
        if (Enum.TryParse(str, out VariantFunction function) == false)
        {
            function = VariantFunction.Unknown;
            unknownFunc = str;
        }

        for (byte i = 1; i < count; i++)
        {
            byte index = Memory.Read<byte>(data, ref offset, dataSize);
            if (index != 0) index--;
            var type = (VariantType)Memory.Read<byte>(data, ref offset, dataSize);
            switch (type)
            {
                case VariantType.Float:
                    variantParams[index] = Memory.Read<float>(data, ref offset, dataSize);
                    break;
                case VariantType.String:
                    variantParams[index] = Memory.ReadString(data, ref offset, Memory.Read<int>(data, ref offset, dataSize), dataSize);
                    break;
                case VariantType.Vector2:
                    variantParams[index] = Memory.Read<Vector2>(data, ref offset, dataSize);
                    break;
                case VariantType.Vector3:
                    variantParams[index] = Memory.Read<Vector3>(data, ref offset, dataSize);
                    break;
                case VariantType.Uint32:
                    variantParams[index] = Memory.Read<uint>(data, ref offset, dataSize);
                    break;
                case VariantType.Rect:
                    variantParams[index] = Memory.Read<Vector4>(data, ref offset, dataSize);
                    break;
                case VariantType.Int32:
                    variantParams[index] = Memory.Read<int>(data, ref offset, dataSize);
                    break;
            }
        }
        return new Variant(function, unknownFunc, variantParams);
    }


    public override string ToString()
    {
        var sb = ZString.CreateStringBuilder();
        for (int i = 0; i < Params.Length; i++)
        {
            sb.Append($"Param {i}: {Params[i] ?? "undefined"}");
        }
        return sb.ToString();
    }
}