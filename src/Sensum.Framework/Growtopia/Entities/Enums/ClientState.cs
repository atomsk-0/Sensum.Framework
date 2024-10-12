namespace Sensum.Framework.Growtopia.Entities.Enums;

public enum ClientState : byte
{
    Disconnected,
    RequestingServer,
    Connecting,
    Connected,
    Maintenance,
    ServerRequestFail,
    Suspended,
    Banned,
    Relogging,
    GameUpdate,
    GuestLimitReached,
    BadGuestName,
    WrongCredentials,
    SwitchingServer,
    UpdatingItems,
    TooManyPeopleLogging,
    FailedToGetLoginToken,
    RequestingToken,
    BadGateway,
    CaptchaNotSupportedYet,
    TextCaptchaFailed,
    CaptchaInProgress,
}