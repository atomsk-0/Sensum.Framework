using System.Drawing;

namespace Sensum.Framework.Entities;

public struct Vector2Int(int x, int y)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;

    public Point Point => new(X, Y);

    public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2Int operator *(Vector2Int a, int b) => new(a.X * b, a.Y * b);
    public static Vector2Int operator /(Vector2Int a, int b) => new(a.X / b, a.Y / b);
    public static bool operator ==(Vector2Int a, Vector2Int b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Vector2Int a, Vector2Int b) => a.X != b.X || a.Y != b.Y;

    public static readonly Vector2Int NEGATIVE = new(-1, -1);

    public override bool Equals(object? obj) => obj is Vector2Int vector && this == vector;
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() =>  $"{X}:{Y}";
}