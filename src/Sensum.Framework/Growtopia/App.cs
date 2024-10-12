using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using AngleSharp;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Proton;
using HttpRequestError = Sensum.Framework.Entities.HttpRequestError;

namespace Sensum.Framework.Growtopia;

public static class App
{
    public static string Version = "4.65";
    public static byte Protocol = 209;

    public const string IGNORED_PROXY_HOST = "0.0.0.0";

    internal const string EDGE_WEBVIEW_USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36 Edg/126.0.0.0";
    internal const string UBI_SERVICES_SDK_USER_AGENT = "UbiServices_SDK_2022.Release.9_PC64_ansi_static";

    private const string primary_server_data_uri = "https://www.growtopia1.com/growtopia/server_data.php";
    private const string alternative_server_data_uri = "https://www.growtopia2.com/growtopia/server_data.php";

    internal static string CapMonsterApiKey = "";

    public static bool TryGetAuthOptionToken(AuthType authType, ENetClient client, in Proxy proxy, out string token, out HttpRequestError error)
    {
        token = string.Empty;
        error = HttpRequestError.None;
        try
        {
            using var httpClient = new NetHttp(in proxy, EDGE_WEBVIEW_USER_AGENT);
            HttpResponseMessage response = httpClient.PostAsync("https://login.growtopiagame.com/player/login/dashboard", new StringContent(MiscUtils.EncodeToUrlString(client.LoginBuilder.BuildNew()))).Result;
            if (response.IsSuccessStatusCode == false)
            {
                if (response.StatusCode == HttpStatusCode.BadGateway)
                {
                    client.State = ClientState.BadGateway;
                }
                error = response.StatusCode == HttpStatusCode.Forbidden ? HttpRequestError.Forbidden : HttpRequestError.Unknown;
                return false;
            }

            string rs1 = response.Content.ReadAsStringAsync().Result;
            if (rs1.Contains("\"status\":\"failed\""))
            {
                JsonNode node = JsonNode.Parse(rs1)!;
                string errorMessage = node["message"]!.ToString();
                client.State = errorMessage switch
                {
                    "Oops, too many people logging at once." => ClientState.TooManyPeopleLogging,
                    _ => ClientState.FailedToGetLoginToken
                };
                error = HttpRequestError.None;
                return false;
            }

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            // ReSharper disable once AccessToModifiedClosure
            var doc = context.OpenAsync(req => req.Content(response.Content.ReadAsStringAsync().Result)).Result;

            var links = doc.QuerySelectorAll("a");
            string targetStr = authType switch
            {
                AuthType.Google => "/google/redirect?token=",
                AuthType.Apple => "/apple/redirect?token=",
                AuthType.Legacy => "/player/growid/login?token=",
                _ => "sumsum"
            };

            foreach (var link in links)
            {
                if (link.GetAttribute("href")?.Contains(targetStr) == true)
                {
                    token = link.GetAttribute("href")!.Split("token=")[1];
                    break;
                }
            }

            if (string.IsNullOrEmpty(token))
            {
                error = HttpRequestError.Unknown;
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            if (e.Message.Contains("The proxy tunnel request to proxy"))
            {
                error = HttpRequestError.DeadPorxy;
                return false;
            }
            error = HttpRequestError.Unknown;
            return false;
        }
    }

    public static bool TryCheckToken(ref string token, ENetClient client, in Proxy proxy, out HttpRequestError error)
    {
        try
        {
            using NetHttp httpClient = new NetHttp(proxy);
            var postData = new Dictionary<string, string> { { "refreshToken", token } };
            var response = httpClient.PostAsync("https://login.growtopiagame.com/player/growid/checktoken", new FormUrlEncodedContent(postData)).Result;
            if (response.IsSuccessStatusCode == false)
            {
                if (response.StatusCode == HttpStatusCode.BadGateway)
                {
                    client.State = ClientState.BadGateway;
                }
                error = response.StatusCode == HttpStatusCode.Forbidden ? HttpRequestError.Forbidden : HttpRequestError.Unknown;
                return false;
            }
            JsonNode resultNode = JsonNode.Parse(response.Content.ReadAsStringAsync().Result)!;
            string status = resultNode["status"]!.ToString();
            if (status != "success")
            {
                error = HttpRequestError.Unknown;
                return false;
            }

            token = resultNode["token"]!.ToString().Replace("\\/", "/");

            error = HttpRequestError.None;
            return true;

        }
        catch (Exception e)
        {
            if (e.Message.Contains("The proxy tunnel request to proxy"))
            {
                error = HttpRequestError.DeadPorxy;
                return false;
            }
            error = HttpRequestError.Unknown;
            return false;
        }
    }


    public static bool TryGetServerData(out ServerData serverData, out HttpRequestError error, in Proxy proxy)
    {
        serverData = default;
        error = HttpRequestError.None;

        HttpResponseMessage? response = TryGetServerData(out HttpRequestError err, in proxy);
        if (response == null)
        {
            error = err;
            return false;
        }

        using var scanner = new TextScanner(response.Content.ReadAsStringAsync().Result);
        serverData = new ServerData(scanner.Get<string>("server"), scanner.Get<ushort>("port"), scanner.Get<string>("meta"), scanner.Contains("loginurl") ? scanner.Get<string>("loginurl") : string.Empty,scanner.Contains("maint") ? scanner.Get<string>("maint") : string.Empty);
        error = HttpRequestError.None;
        return true;
    }

    public static HttpResponseMessage? TryGetServerData(out HttpRequestError error, in Proxy proxy)
    {
        try
        {
            using NetHttp httpClient = new NetHttp(proxy);
            var postData = new Dictionary<string, string>
            {
                { "version", Version },
                { "platform", "0" },
                { "protocol", Protocol.ToString() }
            };
            bool usePrimary = true;
            tryAlternative:
            HttpResponseMessage result = httpClient.PostAsync(usePrimary ? primary_server_data_uri : alternative_server_data_uri, new FormUrlEncodedContent(postData)).Result;
            if (result.IsSuccessStatusCode)
            {
                error = HttpRequestError.None;
                return result;
            }
            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                if (usePrimary)
                {
                    usePrimary = false;
                    goto tryAlternative;
                }
                error = HttpRequestError.Forbidden;
                return null;
            }
            if (usePrimary)
            {
                usePrimary = false;
                goto tryAlternative;
            }
            error = HttpRequestError.Unknown;
            return null;
        }
        catch (Exception e)
        {
            if (e.Message.Contains("The proxy tunnel request to proxy"))
            {
                error = HttpRequestError.DeadPorxy;
                return null;
            }
            error = HttpRequestError.Unknown;
            return null;
        }
    }
}

public delegate void RefAction<T>(ref T arg);