using System.Net;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia;

namespace Sensum.Framework.Proton;

public class NetHttp : IDisposable
{
    private readonly HttpClient httpClient;

    public NetHttp(in Proxy proxy, string? overwriteAgent = null)
    {
        var socketHandler = new SocketsHttpHandler
        {
            AllowAutoRedirect = true,
            UseProxy = proxy.Host is not App.IGNORED_PROXY_HOST,
            Proxy = new WebProxy
            {
                Address = new Uri($"socks5://{proxy.Host}:{proxy.Port}"),
            }
        };
        if (string.IsNullOrEmpty(proxy.Username) == false)
        {
            socketHandler.Proxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
        }
        httpClient = new HttpClient(socketHandler);
        httpClient.Timeout = TimeSpan.FromSeconds(15);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(overwriteAgent ?? App.UBI_SERVICES_SDK_USER_AGENT);
    }

    public async Task<HttpResponseMessage> PostAsync(string url, StringContent content) => await httpClient.PostAsync(url, content);

    public async Task<HttpResponseMessage> PostAsync(string url, FormUrlEncodedContent content) => await httpClient.PostAsync(url, content);

    public async Task<HttpResponseMessage> GetAsync(string url) => await httpClient.GetAsync(url);

    public void Dispose()
    {
        httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}