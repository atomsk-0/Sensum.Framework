using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Cysharp.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Growtopia.Player;
using Sensum.Framework.Utils;
using HttpRequestError = Sensum.Framework.Entities.HttpRequestError;
using Proxy = Sensum.Framework.Entities.Proxy;

namespace Sensum.Framework.Growtopia.Authentications;

public static class GoogleAuth
{
    private const string too_many_logging_message = "Oops, too many people trying to login at once. Please try again in 30 sec.";
    private const string token_pattern = "\"token\":\"(.*?)\"";

    public static readonly Dictionary<string, DriverData> DRIVERS = [];

    public struct DriverData
    {
        public ChromeDriver Driver;
        public Process? ProxyProcess;
    }

    private static readonly List<Tuple<string, int>> target_urls =
    [
        Tuple.Create("https://accounts.google.com/v3/signin/challenge/recaptcha", 2),
        Tuple.Create("https://accounts.google.com/v3/signin/challenge/pwd", 2),
        Tuple.Create("https://accounts.google.com/signin/oauth/id?authuser", 3),
        Tuple.Create("https://login.growtopiagame.com/player/growid/logon-name", 3),
        Tuple.Create("https://accounts.google.com/v3/signin/identifier", 1),
        Tuple.Create("https://accounts.google.com/speedbump", 3)
    ];

    public static bool TryAuthenticate(ENetClient client, in Proxy proxy, out string token, out HttpRequestError error)
    {
        token = string.Empty;
        error = HttpRequestError.None;
        if (App.TryGetAuthOptionToken(AuthType.Google, client, proxy, out string firstToken, out error) == false) return false;
        DestroyDriver(client.LoginBuilder.TankIdName);
        var driver = setupDriver(client, proxy);

        driver.Navigate().GoToUrl($"https://login.growtopiagame.com/google/redirect?token={firstToken}");
        tryAgain1:
        if (waitForUrlChanged(driver, 1) == false)
        {
            try
            {
                var textElement = new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(ExpectedConditions.ElementExists(By.ClassName("text")));
                if (textElement?.Text.Contains(too_many_logging_message) ?? false)
                {
                    driver.Navigate().Refresh();
                    goto tryAgain1;
                }
            }
            catch
            {
                // Ignore
            }
            error = HttpRequestError.Timeout;
            DestroyDriver(client.LoginBuilder.TankIdName);
            return false;
        }
        if (handleTargetUrls(driver, client, out token, out GoogleAuthResult result0, true) == false)
        {
            error = HttpRequestError.Unknown;
            DestroyDriver(client.LoginBuilder.TankIdName);
            return false;
        }
        if (enterEmail(driver, client.LoginBuilder.TankIdName) == false)
        {
            DestroyDriver(client.LoginBuilder.TankIdName);
            return false;
        }
        if (waitForUrlChanged(driver, "https://accounts.google.com/v3/signin/identifier") == false)
        {
            client.State = ClientState.WrongCredentials;
            DestroyDriver(client.LoginBuilder.TankIdName);
            return false;
        }
        if (handleTargetUrls(driver, client, out token, out GoogleAuthResult result1, true) == false)
        {
            error = HttpRequestError.Unknown;
            DestroyDriver(client.LoginBuilder.TankIdName);
            return false;
        }
        if (waitForUrlChanged(driver, 2) == false)
        {
            error = HttpRequestError.Timeout;
            DestroyDriver(client.LoginBuilder.TankIdName);
            return false;
        }
        if (enterPassword(driver, client, client.LoginBuilder.TankIdPass) == false)
        {
            error = HttpRequestError.Unknown;
            DestroyDriver(client.LoginBuilder.TankIdName);
            return false;
        }
        if (waitForUrlChanged(driver, "accounts.google.com/v3/signin/challenge/pwd") == false)
        {
            client.State = ClientState.WrongCredentials;
            DestroyDriver(client.LoginBuilder.TankIdName);
            return false;
        }
        if (handleTargetUrls(driver, client, out token, out GoogleAuthResult result3) == false)
        {
            error = HttpRequestError.Unknown;
            DestroyDriver(client.LoginBuilder.TankIdName);
            return false;
        }


        DestroyDriver(client.LoginBuilder.TankIdName);
        error = HttpRequestError.None;
        return true;
    }

    private static bool enterPassword(IWebDriver driver, ENetClient client, string password)
    {
        var passwordField = new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementIsVisible(By.Name("Passwd")));
        passwordField?.SendKeys(password + Keys.Enter);
        if (waitForUrlChanged(driver, "https://accounts.google.com/v3/signin/challenge/pwd") == false)
        {
            try
            {
                string? captchaImg = new WebDriverWait(driver, TimeSpan.FromSeconds(4)).Until(getCaptchaSrc);
                if (string.IsNullOrEmpty(captchaImg) == false)
                {
                    client.State = ClientState.CaptchaInProgress;
                    if (string.IsNullOrEmpty(App.CapMonsterApiKey)) return false;
                    using var httpClient = new HttpClient();
                    var response = httpClient.GetAsync(captchaImg).Result;
                    if (response.IsSuccessStatusCode == false) return false;

                    byte[] captchaBytes = response.Content.ReadAsByteArrayAsync().Result;
                    string captchaBase64 = Convert.ToBase64String(captchaBytes);
                    string? code = solveTextCaptcha(App.CapMonsterApiKey, captchaBase64);
                    if (string.IsNullOrEmpty(code)) return false;

                    new WebDriverWait(driver, TimeSpan.FromSeconds(20)).Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id=\"ca\"]"))).SendKeys(code + Keys.Enter);

                    if (waitForUrlChanged(driver, 2) == false) return false;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                // ignore
            }
        }
        return true;
    }


    private static string? solveTextCaptcha(string apiKey, string imageBase64)
    {
        const string create_task_url = "https://api.capmonster.cloud/createTask";
        JsonNode taskPayload = new JsonObject
        {
            ["clientKey"] = apiKey,
            ["task"] = new JsonObject
            {
                ["type"] = "ImageToTextTask",
                ["body"] = imageBase64
            }
        };

        using var httpClient = new HttpClient();
        var response = httpClient.PostAsync(create_task_url, new StringContent(taskPayload.ToJsonString(), Encoding.UTF8, "application/json")).Result;
        if (response.IsSuccessStatusCode == false) return null;

        JsonNode? taskResponse = JsonNode.Parse(response.Content.ReadAsStringAsync().Result);
        if (taskResponse == null) return null;
        string? taskId = taskResponse["taskId"]?.ToString();
        if (string.IsNullOrEmpty(taskId)) return null;

        const string get_task_result_url = "https://api.capmonster.cloud/getTaskResult";
        JsonNode taskResultPayload = new JsonObject
        {
            ["clientKey"] = apiKey,
            ["taskId"] = taskId
        };

        while (true)
        {
            response = httpClient.PostAsync(get_task_result_url, new StringContent(taskResultPayload.ToJsonString(), Encoding.UTF8, "application/json")).Result;
            if (response.IsSuccessStatusCode == false) return null;

            JsonNode? taskResultResponse = JsonNode.Parse(response.Content.ReadAsStringAsync().Result);
            if (taskResultResponse == null) return null;
            string? status = taskResultResponse["status"]?.ToString();
            if (string.IsNullOrEmpty(status)) return null;
            if (status == "ready")
            {
                return taskResultResponse["solution"]?["text"]?.ToString();
            }
            if (status == "processing")
            {
                Thread.Sleep(2000);
            }
            else
            {
                return null;
            }
            Thread.Sleep(1000);
        }
    }

    private static bool enterEmail(IWebDriver driver, string email)
    {
        var emailField = new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementIsVisible(By.Id("identifierId")));
        emailField?.SendKeys(email + Keys.Enter);

        return true;
    }

    private static string? getCaptchaSrc(IWebDriver driver)
    {
        var captchaImg = driver.FindElement(By.Id("captchaimg"));
        if (captchaImg == null) return null;
        return captchaImg.GetAttribute("src");
    }


    private static bool handleTargetUrls(IWebDriver driver, ENetClient client, out string token, out GoogleAuthResult result, bool skipTooManyCheck = false)
    {
        result = GoogleAuthResult.None;
        token = "";
        if (skipTooManyCheck == false)
        {
            try
            {
                var textElement = new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(ExpectedConditions.ElementExists(By.ClassName("text")));
                if (textElement?.Text.Contains(too_many_logging_message) ?? false)
                {
                    driver.Navigate().Refresh();
                    Thread.Sleep(5000);
                    return handleTargetUrls(driver, client, out token, out result);
                }
            }
            catch
            {
                // Ignore
            }
        }

        try
        {
            if (driver.Url.Contains("accounts.google.com/signin/oauth/id?authuser"))
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"yDmH0d\"]/c-wiz/div/div[3]/div/div/div[2]/div/div/button/span"))).Click();
                waitForUrlChanged(driver, "accounts.google.com/signin/oauth/id?authuser");
                return handleTargetUrls(driver, client, out token, out result);
            }
            if (driver.Url.Contains("accounts.google.com/speedbump"))
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"confirm\"]"))).Click();
                if (waitForUrlChanged(driver, 3) == false) return false;
                return handleTargetUrls(driver, client, out token, out result);
            }
            if (driver.Url.Contains("accounts.google.com/v3/signin/challenge/recaptcha"))
            {
                client.State = ClientState.CaptchaInProgress;
                if (solveCaptcha(driver) == false)
                {
                    result = GoogleAuthResult.CaptchaFailed;
                    return false;
                }
                byte time = 0;
                while (driver.Url.Contains("accounts.google.com/v3/signin/challenge/recaptcha"))
                {
                    if (time >= 30) return false;
                    Thread.Sleep(1000);
                    time++;
                }
                client.State = ClientState.CaptchaInProgress;
                return handleTargetUrls(driver, client, out token, out result, true);
            }
            if (driver.Url.Contains("https://login.growtopiagame.com/player/growid/logon-name"))
            {
                try
                {
                    if (driver.PageSource.Contains("Choose your name in Growtopia") == false)
                    {
                        Regex tokenRegex = new Regex(token_pattern);
                        Match match = tokenRegex.Match(driver.PageSource);
                        if (match.Success)
                        {
                            token = match.Groups[1].Value.Replace("\\/", "/");
                            return true;
                        }
                        return false;
                    }
                    generateAndEnterUsername(driver);
                    if (waitForToken(driver, out token))
                    {
                        return true;
                    }
                }
                catch
                {
                    generateAndEnterUsername(driver);
                    if (waitForToken(driver, out token))
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    private static bool waitForToken(IWebDriver driver, out string token)
    {
        token = "";
        var responseElement = new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(ExpectedConditions.ElementExists(By.XPath("/html/body")));
        Regex tokenRegex = new Regex(token_pattern);
        Match match = tokenRegex.Match(responseElement.Text);
        if (match.Success)
        {
            token = match.Groups[1].Value.Replace("\\/", "/");
            return true;
        }

        return false;
    }


    private static string generateName()
    {
        return ZString.Concat(GeneralUtils.RandomStringWithNumbers(3), LoginBuilder.GetRandomGuestName(), GeneralUtils.RandomStringWithNumbers(3));
    }

    private static void generateAndEnterUsername(IWebDriver driver)
    {
        while (true)
        {
            string username = generateName();
            var loginNameField = new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(ExpectedConditions.ElementIsVisible(By.Id("login-name")));
            loginNameField?.SendKeys(username);

            var submitButton = new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(ExpectedConditions.ElementToBeClickable(By.ClassName("grow-button")));
            submitButton?.Click();
            Thread.Sleep(5000);

            var elements = driver.FindElements(By.XPath("//*[@id=\"modalShow\"]/div/div/div/div/section/div/div[2]/ul/li"));
            if (elements.Any(c => c.Text == "What kind of name is that? Kids play this too, ya know.") == false) break;
        }
    }


    private static bool solveCaptcha(IWebDriver driver)
    {
        if (driver.Url.Contains("accounts.google.com/v3/signin/challenge/recaptcha"))
        {
            new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(By.XPath("//iframe[@title=\"reCAPTCHA\"]")));
            for (int i = 0; i < 100; i++)
            {
                if (driver.PageSource.Contains("You are verified"))
                {
                    driver.SwitchTo().DefaultContent();
                    new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[text()=\"Next\"]"))).Click();
                    return true;
                }
                Thread.Sleep(1000);
            }
        }
        return true;
    }

    private static bool waitForUrlChanged(IWebDriver driver, string url)
    {
        byte time = 0;
        while (driver.Url.Contains(url))
        {
            if (time >= 50) return false;
            Thread.Sleep(100);
            time++;
        }
        return true;
    }


    private static bool waitForUrlChanged(IWebDriver driver, int cond)
    {
        try
        {
            new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => hasTargetUrlChanged(d, cond));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool hasTargetUrlChanged(IWebDriver driver, int expectedCondition)
    {
        foreach (var url in target_urls)
        {
            if (driver.Url.Contains(url.Item1) && url.Item2 == expectedCondition)
            {
                return true;
            }
        }
        return false;
    }

    private static IWebDriver setupDriver(ENetClient client, in Proxy proxy)
    {
        Process? proxyProcess = null;
        ChromeOptions options = new ChromeOptions();
        options.AddArgument("--disable-blink-features=AutomationControlled");
        #if RELEASE
        options.AddArgument("--headless");
        options.AddArgument("--log-level=3");
        options.AddArgument("--silent");
        options.AddArgument("--disable-logging");
        #endif

        if (OperatingSystem.IsLinux())
        {
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
        }

        if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extension")))
        {
            options.AddArgument($"--load-extension={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extension")}");
        }

        if (proxy.Host != App.IGNORED_PROXY_HOST)
        {
            if (string.IsNullOrEmpty(proxy.Username) && string.IsNullOrEmpty(proxy.Password))
            {
                options.AddArgument($"--proxy-server={proxy.Host}:{proxy.Port}");
            }
            else
            {
                int localPort = findAvailablePort(8001, 65535);

                if (OperatingSystem.IsWindows())
                {
                    proxyProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "gost-windows-amd64.exe",
                            Arguments = $"-L socks5://:{localPort} -F socks5://{proxy.Username}:{proxy.Password}@{proxy.Host}:{proxy.Port}",
#if DEBUG
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
#else
                        RedirectStandardInput = false,
                        RedirectStandardOutput = false,
#endif
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                }
                else if (OperatingSystem.IsLinux())
                {
                    proxyProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "gost-linux-amd64",
                            Arguments = $"-L socks5://:{localPort} -F socks5://{proxy.Username}:{proxy.Password}@{proxy.Host}:{proxy.Port}",
#if DEBUG
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
#else
                        RedirectStandardInput = false,
                        RedirectStandardOutput = false,
#endif
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
                proxyProcess.Start();
                options.AddArgument($"--proxy-server=socks5://127.0.0.1:{localPort}");
            }
        }

        ChromeDriverService service = ChromeDriverService.CreateDefaultService();

        #if RELEASE
        service.SuppressInitialDiagnosticInformation = true;
        service.EnableVerboseLogging = false;
        service.EnableAppendLog = false;
        #endif

        //new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser, Architecture.X64);
        var driver = new ChromeDriver(service, options);

        DRIVERS.Add(client.LoginBuilder.TankIdName, new DriverData { Driver = driver, ProxyProcess = proxyProcess });
        return driver;
    }

    private static int findAvailablePort(int startPort, int endPort)
    {
        for (int port = startPort; port <= endPort; port++)
        {
            try
            {
                using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return port;
            }
            catch (System.Net.Sockets.SocketException)
            {
                // ignore
            }
        }
        throw new Exception("No available ports found.");
    }

    public static void DestroyDriver(string id)
    {
        if (DRIVERS.TryGetValue(id, out DriverData driverData) == false) return;
        try
        {
            driverData.Driver.Close();
        }
        catch
        {
            // ignore
        }
        driverData.Driver.Quit();
        driverData.Driver.Dispose();
        if (driverData.ProxyProcess != null)
        {
            driverData.ProxyProcess.Kill();
            driverData.ProxyProcess.Dispose();
        }
        DRIVERS.Remove(id);
    }

    public enum GoogleAuthResult
    {
        None,
        CaptchaFailed,
    }
}