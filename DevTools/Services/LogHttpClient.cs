using HandyControl.Controls;
using DevTools.Common;
using DevTools.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Media.Imaging;

namespace DevTools.Services
{
    public class LogHttpClient
    {
        private Dictionary<EnvEnum, HttpClient> envHttpClients = new Dictionary<EnvEnum, HttpClient>();
        private Dictionary<EnvEnum, CookieContainer> envCookies = new Dictionary<EnvEnum, CookieContainer>();

        public LogHttpClient()
        {
        }

        public async Task<CookieCollection?> LoginAsync(User user, string captcha)
        {
            var env = (EnvEnum)user.Env;

            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>
            {
               new KeyValuePair<string, string>("username",user.UserName),
               new KeyValuePair<string, string>("password", user.Password),
               new KeyValuePair<string, string>("verifycode", captcha.ToUpper())
            };
            FormUrlEncodedContent content = new FormUrlEncodedContent(pairs);


            var httpClient = GetEnvHttpClient(env);
            var response = await httpClient.PostAsync("UserLogin/UserLogin", content);
            response.EnsureSuccessStatusCode();
            var res = response.Content.ReadFromJsonAsync<LogLoginResult>().GetAwaiter().GetResult() ?? LogLoginResult.Error();

            if (!res.State.Equals("success"))
            {
                Growl.Error(res.State + ": " + res.Msg);
                return null;
            }

            return GetCookies(env);
        }

        /// <summary>
        /// 获取响应验证码图片
        /// </summary>
        /// <returns></returns>
        public BitmapImage GetCaptchaImage(EnvEnum env)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var httpClient = GetEnvHttpClient(env);
            var response = httpClient.GetAsync($"UserLogin/VelidatedCode?rd={timestamp}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var imageSource = new BitmapImage();
            using (var stream = response.Content.ReadAsStream())
            {
                imageSource.BeginInit();
                imageSource.StreamSource = stream;
                imageSource.EndInit();
            }
            return imageSource;
        }


        private CookieCollection? GetCookies(EnvEnum env)
        {
            var exist = envCookies.TryGetValue(env, out var cookies);
            if (!exist) return null;
            var cols = cookies!.GetCookies(LogBaseUri(env));
            return cols;
        }

        private HttpClient GetEnvHttpClient(EnvEnum env)
        {
            var baseUri = LogBaseUri(env);
            var exist = envHttpClients.TryGetValue(env, out var httpClient);
            if (!exist)
            {
                var cookies = new CookieContainer();
                cookies.Add(baseUri, new Cookie("stplatform_id", $"52000293412280177{env.GetHashCode()}"));
                envCookies.Add(env, cookies);

                HttpClientHandler handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    CookieContainer = cookies!,
                    UseCookies = true,
                    ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
                    {
                        return true;
                    }
                };
                httpClient = new HttpClient(handler);
                httpClient.BaseAddress = baseUri;
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                envHttpClients.Add(env, httpClient);
            }

            return httpClient!;
        }

        private Uri LogBaseUri(EnvEnum env) => env switch
        {
            EnvEnum.Dev => new Uri("http://dev.com/"),
            EnvEnum.Prod => new Uri("https://prod.com/"),
            _ => throw new NotSupportedException("不支持的日志环境，请调整代码添加")
        };
    }
}
