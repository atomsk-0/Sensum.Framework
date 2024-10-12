namespace Sensum.Framework.Entities;

[Flags]
public enum ClientFeatureFlags : byte
{
    None,
    BotDetection,
    ConsoleManager,
}