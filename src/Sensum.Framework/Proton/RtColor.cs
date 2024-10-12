using System.Drawing;

namespace Sensum.Framework.Proton;

internal static class RtColor
{
    internal static readonly Dictionary<char, Color> COLORS = new()
    {
        { '0', Color.FromArgb(255, 255, 255) },
        { '1', Color.FromArgb(173, 244, 255) },
        { '2', Color.FromArgb(73, 252, 0) },
        { '3', Color.FromArgb(191, 218, 255) },
        { '4', Color.FromArgb(255, 39, 29) },
        { '5', Color.FromArgb(235, 183, 255) },
        { '6', Color.FromArgb(255, 202, 111) },
        { '7', Color.FromArgb(230, 230, 230) },
        { '8', Color.FromArgb(255, 148, 69) },
        { '9', Color.FromArgb(255, 238, 125) },
        { '!', Color.FromArgb(209, 255, 249) },
        { '@', Color.FromArgb(255, 205, 201) },
        { '#', Color.FromArgb(255, 143, 243) },
        { '$', Color.FromArgb(255, 252, 197) },
        { '^', Color.FromArgb(181, 255, 151) },
        { '&', Color.FromArgb(254, 235, 255) },
        { 'w', Color.FromArgb(255, 255, 255) },
        { 'o', Color.FromArgb(252, 230, 186) },
        { 'b', Color.FromArgb(0, 0, 0) },
        { 'p', Color.FromArgb(255, 223, 241) },
        { 'q', Color.FromArgb(12, 96, 164) },
        { 'e', Color.FromArgb(25, 185, 255) },
        { 'r', Color.FromArgb(111, 211, 87) },
        { 't', Color.FromArgb(47, 131, 13) },
        { 'a', Color.FromArgb(81, 81, 81) },
        { 's', Color.FromArgb(158, 158, 158) },
        { 'c', Color.FromArgb(80, 255, 255) },
        { 'ì', Color.FromArgb(255, 225, 25) },
        { '`', Color.FromArgb(255, 252, 197) },
        { 'v', Color.FromArgb(255, 252, 197) }
    };
}