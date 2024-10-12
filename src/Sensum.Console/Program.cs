using System.Runtime.InteropServices;
using System.Web;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia;
using Sensum.Framework.Growtopia.Authentications;
using Sensum.Framework.Growtopia.Managers;
using Sensum.Framework.Growtopia.Player;
using Sensum.Framework.Proton;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZeroLog;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using HttpRequestError = Sensum.Framework.Entities.HttpRequestError;

namespace Sensum.Console;

internal static class Program
{
    // ReSharper disable once InconsistentNaming
    private static void Main()
    {
        ENet.Initialize();
        LogManager.Initialize(new ZeroLogConfiguration { RootLogger = { Appenders = {new ConsoleAppender()} } });
        Bot bot = new Bot(new Proxy(App.IGNORED_PROXY_HOST, 0))
        {
            ConnectedCallback = () => Bot.LOGGER.Debug("Connected to server"),
            DisconnectedCallback = () => Bot.LOGGER.Debug("Disconnected from server"),
            ConnectionTimeoutCallback = () => Bot.LOGGER.Debug("Connection timeout"),
            //NetMessageReceivedCallback = netMessageDelegate => Bot.LOGGER.Debug($"Received netMessage: {netMessageDelegate.Type.ToString()}"),
            //GameUpdatePacketReceivedCallback = gameUpdatePacket => Bot.LOGGER.Debug($"Received gameUpdatePacket: {gameUpdatePacket.Packet->Type.ToString()}"),
            //VariantReceivedCallback = variant => Bot.LOGGER.Debug($"Received variant: {variant.Function.ToString()}, RAW: {variant.UnknownName}"),
            AuthenticationErrorCallback = error => Bot.LOGGER.Debug($"Received authentication error: {error.ToString()}"),
            FeatureFlags = ClientFeatureFlags.ConsoleManager | ClientFeatureFlags.BotDetection
        };
        bot.LoginBuilder.SetLegacy("growid", "password", LoginBuilder.GenerateGuestName()); // Legacy login
        //bot.LoginBuilder.SetGoogle("example@gmail.com", "password", LoginBuilder.GenerateGuestName()); // Google login
        bot.World.JoinedWorldCallback = () => Bot.LOGGER.Debug($"Joined world: {bot.World.Name}");
        bot.World.LoadFailedCallback = reason => Bot.LOGGER.Debug($"Load failed: {reason}");
        bot.Connect();
    }
}