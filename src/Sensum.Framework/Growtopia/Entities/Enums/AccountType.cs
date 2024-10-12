namespace Sensum.Framework.Growtopia.Entities.Enums;

public enum AccountType : byte
{
    Legacy = 0, // Old account type (growid and password)
    Ubiconnect = 1,
    Google = 2,
    Apple = 3
}