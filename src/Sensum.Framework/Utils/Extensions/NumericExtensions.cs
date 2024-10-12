using System.Numerics;
using Sensum.Framework.Entities;

namespace Sensum.Framework.Utils.Extensions;

public static class NumericExtensions
{
    public static Vector2Int ToTilePosition(this Vector2 vec) => new((int)(vec.X / 32), (int)(vec.Y / 32));
    public static Vector2 ToWorldPosition(this Vector2Int vec) => new(vec.X * 32, vec.Y * 32);
}