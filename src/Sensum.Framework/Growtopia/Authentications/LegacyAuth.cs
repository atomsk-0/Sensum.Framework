using System.Net;
using System.Text.Json.Nodes;
using AngleSharp;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Proton;
using HttpRequestError = Sensum.Framework.Entities.HttpRequestError;

namespace Sensum.Framework.Growtopia.Authentications;

internal static class LegacyAuth
{
    internal static bool TryAuthenticate(ENetClient client, in Proxy proxy, out string token, out HttpRequestError error)
    {
        token = string.Empty;
        try
        {
            if (App.TryGetAuthOptionToken(AuthType.Legacy, client, proxy, out string firstToken, out error) == false) return false;

            using var httpClient = new NetHttp(proxy, App.EDGE_WEBVIEW_USER_AGENT);

            var response = httpClient.GetAsync($"https://login.growtopiagame.com/player/growid/login?token={firstToken}").Result;
            if (response.IsSuccessStatusCode == false)
            {
                error = response.StatusCode == HttpStatusCode.Forbidden ? HttpRequestError.Forbidden : HttpRequestError.Unknown;
                return false;
            }

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            // ReSharper disable once AccessToModifiedClosure
            var doc = context.OpenAsync(req => req.Content(response.Content.ReadAsStringAsync().Result)).Result;

            string? secondToken = null;
            if (doc.QuerySelector("input[name='_token']") is { } tokenElement)
            {
                secondToken = tokenElement.GetAttribute("value");
            }

            if (secondToken is null)
            {
                error = HttpRequestError.Unknown;
                return false;
            }

            var values = new Dictionary<string, string>
            {
                { "growId", client.LoginBuilder.TankIdName },
                { "password", client.LoginBuilder.TankIdPass },
                { "_token", secondToken }
            };

            response = httpClient.PostAsync("https://login.growtopiagame.com/player/growid/login/validate", new FormUrlEncodedContent(values)).Result;
            if (response.IsSuccessStatusCode == false)
            {
                error = response.StatusCode == HttpStatusCode.Forbidden ? HttpRequestError.Forbidden : HttpRequestError.Unknown;
                return false;
            }

            string responseStr = response.Content.ReadAsStringAsync().Result;

            if (responseStr.Contains("Account credentials mismatched."))
            {
                client.State = ClientState.WrongCredentials;
                error = HttpRequestError.None;
                return false;
            }

            JsonNode? resultNode = JsonNode.Parse(responseStr);
            if (resultNode == null)            {
                error = HttpRequestError.Unknown;
                return false;
            }

            string status = resultNode["status"]!.ToString();
            if (status != "success")
            {
                error = HttpRequestError.Unknown;
                return false;
            }

            token = resultNode["token"]!.ToString();
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
}