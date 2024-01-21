using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using DevTools.Common;
using DevTools.Models;
using DevTools.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace DevTools.ViewModels
{
    public partial class MaintenanceViewModel : ObservableObject
    {
        [ObservableProperty] bool isShowWeb;
        [ObservableProperty] string captcha = string.Empty;
        [ObservableProperty] ImageSource captchaImage;
        [ObservableProperty] ObservableCollection<User> users = new ObservableCollection<User>();
        [ObservableProperty] long selectedUserId = 0;

        private WebView2 _webView;

        private EnvEnum _env;
        private bool _isLogWeb;
        private string _cookieDomain;
        private string _devOpsUri;
        private readonly IServiceProvider _serviceProvider;
        private readonly SqliteService _sqliteService;
        private readonly LogHttpClient _logHttpClient;
        private readonly ApplicationService _applicationService;
        public MaintenanceViewModel(IServiceProvider serviceProvider, SqliteService sqliteService, LogHttpClient logHttpClient, ApplicationService applicationService)
        {
            _serviceProvider = serviceProvider;
            _sqliteService = sqliteService;
            _logHttpClient = logHttpClient;
            _applicationService = applicationService;
        }

        public void InitViewModel(EnvEnum env, bool isLogWeb, string path = "")
        {
            _env = env;
            if (isLogWeb)
            {
                path = "Monitor/LogManage/LogList";
            }
            _devOpsUri = GetDevOpsUri(_env, path);
            _cookieDomain = GetCookieDomain(_env);
            _isLogWeb = isLogWeb;
        }

        public WebView2 CreateWebView()
        {
            var suffix = _env == EnvEnum.Prod ?  "prod" : "dev";
            var browserArg = _env == EnvEnum.Prod ? "--disable-web-security" : "--disable-web-security --proxy-server=http://192.168.90.200 --proxy-bypass-list=192.168.80.214:9090;";
            var userDataFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location) + suffix);
            return new WebView2
            {
                CreationProperties = new CoreWebView2CreationProperties()
                {
                    UserDataFolder = userDataFolder,
                    AdditionalBrowserArguments = browserArg
                }
            };
        }

        public async Task InitWebView(WebView2 webView)
        {
            _webView = webView;
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested; // 添加自定义选项
            if (_isLogWeb)
            {
                webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            }
            var callbackService = _serviceProvider.GetRequiredService<LogWebViewScriptCallbackService>();
            callbackService.SetEnv(_env);
            webView.CoreWebView2.AddHostObjectToScript("logcallback", callbackService);
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("var logcallback= window.chrome.webview.hostObjects.logcallback;");

            InitView();
        }

        #region Webview2 EventHandler

        private async void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            // 注入jq
            //string script = await File.ReadAllTextAsync(@"Assets\jq\jquery-3.7.1.min.js");
            //await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);

            _webView.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;
            await Task.CompletedTask;
        }

        private void CoreWebView2_ContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs e)
        {
            IList<CoreWebView2ContextMenuItem> menuList = e.MenuItems;
            var coreWebView = sender as CoreWebView2 ?? throw new ArgumentNullException(nameof(sender));
            CoreWebView2ContextMenuItem newItem = coreWebView.Environment.CreateContextMenuItem("复制并跳转Json格式化", null, CoreWebView2ContextMenuItemKind.Command);
            newItem.CustomItemSelected += async delegate (object? send, object ex)
            {
                await coreWebView.ExecuteScriptAsync(@"document.execCommand(""Copy"")");
                var copy = Clipboard.GetText();
                string pageUri = e.ContextMenuTarget.PageUri;
                SynchronizationContext.Current?.Post((_) =>
                {
                    // Growl.Info("复制内容: " + copy);
                    _applicationService.AddJsonFormatTabItem(copy);
                }, null);
            };
            menuList.Insert(0, newItem);
        }

        private void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var coreWebView = sender as CoreWebView2 ?? throw new ArgumentNullException(nameof(sender));
            DispatchAsync(async () =>
            {
                await Task.Delay(500);

                // 移除多余组件
                await coreWebView.ExecuteScriptAsync($@"
document.getElementsByClassName('sidebar-container')[0].remove();
document.getElementsByClassName('fixed-header')[0].remove();
document.getElementsByClassName('main-container')[0].setAttribute('style', 'margin-left: 0px;');
");
                // 初始化查询按钮
                await coreWebView.ExecuteScriptAsync(
                 $@"
var MutationObserver = window.MutationObserver || window.WebKitMutationObserver || window.MozMutationObserver;

// 添加登出按钮
var signOutBtn=document.createElement('button'); 
signOutBtn.type = 'button'; 
signOutBtn.innerText = '登出';
signOutBtn.setAttribute('style', 'position: absolute; z-index:99; right:10px; top:10px;');
signOutBtn.setAttribute('class', 'el-button el-button--primary el-button--mini');
signOutBtn.addEventListener('click',function(e) {{
    logcallback.SignOutClickCallback();
}} );
document.getElementById('app').appendChild(signOutBtn);

// 收缩菜单栏（已经在移除组件中移除掉了）
//const hamburgerContainer = document.getElementsByClassName('hamburger-container')[0];
//const hamburgerClass = document.getElementsByClassName('hamburger')[0].className.baseVal;
//if(hamburgerClass.includes('is-active')) {{
//    hamburgerContainer.click();
//}}

// 按钮
const elems = document.getElementsByTagName('button');
const btns = [].slice.call(elems);
const searchBtn = btns.find(function (b) {{
    return b.innerText.trim() === '查 询';
}});
// console.log('searchBtn:', searchBtn);
// searchBtn.click(); 

// 获取查询输入框
const inputs = document.getElementsByTagName('input');
// 客户端IP
const clientIpInput = inputs[3];
// 起始时间
const beginDateInput = inputs[4];
// 终止时间
const endDateInput = inputs[5];
// 服务名
const serviceNameInput = inputs[8];
// 关键字
const keyWordInput = inputs[9];
// 查询语法
const queryInput = inputs[13];

// 添加查询记录按钮
var container=document.getElementsByClassName('el-form-item__content')[13];
const recordBtn=document.createElement('button'); 
recordBtn.type = 'button'; 
recordBtn.innerText = '查询记录';
recordBtn.setAttribute('style', 'margin-left: 20px');
recordBtn.setAttribute('class', 'el-button el-button--primary el-button--mini');
container.appendChild(recordBtn);

// 查询监听事件
searchBtn.addEventListener('click', function() {{
    let input = {{
        ClientIp: clientIpInput.value,
        BeginDate: beginDateInput.value,
        EndDate: endDateInput.value,
        ServiceName: serviceNameInput.value,
        KeyWord: keyWordInput.value,
        Query: queryInput.value,
    }}
    logcallback.SearchClickCallback(JSON.stringify(input));
    // console.log('msg:', msg);
}});

// 监控查询按钮的查询事件，确定是否做内容处理
var observer = new MutationObserver(function(mutations) {{
    mutations.forEach(function(mutation) {{
    if (mutation.type == 'attributes') {{
        const dis = mutation.target.disabled;
        // console.log('变更了！！！！！', dis)
        if(!dis) {{
            setTimeout(addJsonFormatBtnForLogContent ,100);
        }}
    }}
    }});
}});
observer.observe(searchBtn, {{
    attributes: true,  //configure it to listen to attribute changes,
    attributeFilter: ['class']
}});

// 查询记录监听事件
recordBtn.addEventListener('click', function() {{
    logcallback.GetSearchRecordCallback();
}});

// 构建事件
const eventInput = new Event('input');
const eventChange = new Event('change');

// 生成日期选中picker,这样更改日期才有效果
beginDateInput.click();
// 延时隐藏，毕竟不能一直展示
setTimeout(function(){{
    const dateRangePicker = document.getElementsByClassName('el-picker-panel el-date-range-picker el-popper has-time');
    dateRangePicker[0].style.display = 'none';
}},100);

// 为日志内容添加Json格式化按钮
function addJsonFormatBtnForLogContent(){{
    // console.log('触发事件')
    const titleElems = document.getElementsByClassName('flex0 title')
    const titles = [].slice.call(titleElems);
    const logTitles = titles.filter(function (b) {{
        return b.innerText.trim() === '日志内容：';
    }});
    // console.log('触发事件', logTitles)
    const formatBtn=document.createElement('button'); 
    formatBtn.type = 'button'; 
    formatBtn.innerText = '格式化';
    formatBtn.setAttribute('class', 'el-button el-button--primary el-button--mini');
    for(i = 0; i<logTitles.length; i++) {{
        var toJsonFormat = formatBtn.cloneNode(true);
        logTitles[i].appendChild(toJsonFormat);
        const titleNode = logTitles[i];
        toJsonFormat.addEventListener('click', function(evt) {{ jsonFormatLogContent(evt, titleNode)}});
    }}   
}}

function jsonFormatLogContent(evt, titleNode) {{
    // console.log('格式化', evt, titleNode);
    let contentNode = titleNode.parentNode.lastChild.lastChild;
    let content = contentNode.innerText;
    // console.log('内容', content);

    // 如果不是大日志文件下载，直接跳转
    if(content.indexOf('文件内容太大，提供URL方式查看：') === -1) {{
        logcallback.ToJsonFormatClickCallback(content);
        return;
    }}
    // 否则，点击下载，跳转下载后进行拦截，再跳转json格式化
    let downloadBtn = contentNode.lastChild;
    // console.log('内容组件', downloadBtn);
    downloadBtn.click();
}}
             ");

            });
        }

        private void CoreWebView2_DownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
        {
            var s = e.ResultFilePath;

            e.Cancel = true;
        }

        private async void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();
            var url = e.Uri.ToString();
            // 文件内容太大了，页面都卡住了，需要限制文件大小，太大的文件就放弃
            // 如果是下载日志文件，则获取内容，跳转json格式化页面
            if (url.Contains("logcontent_"))
            {
                var content = string.Empty;
                // 获取文件
                using (HttpClient httpClient = new HttpClient())
                {
                    // 应该是未授权HEAD method，导致403
                    // 确定文件大小
                    //using (HttpRequestMessage headReq = new HttpRequestMessage(HttpMethod.Head, url))
                    //{
                    //    using (HttpResponseMessage headResp = await httpClient.SendAsync(headReq))
                    //    {
                    //        headResp.EnsureSuccessStatusCode();
                    //        long length = headResp.Content.Headers.ContentLength ?? 0;

                    //        if (length > 1024 * 1000)
                    //        {
                    //            Growl.Warning("文件大于1M，无法进行JSON格式化");
                    //            deferral.Complete();
                    //            return;
                    //        }
                    //    }
                    //}

                    // 下载文件
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    long length = response.Content.Headers.ContentLength ?? 0;
                    if (length > 1024 * 1000)
                    {
                        Growl.Warning("文件大于1M，无法进行JSON格式化");
                        deferral.Complete();
                        return;
                    }

                    content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        Growl.Warning("下载的文件内容为空");
                        deferral.Complete();
                        return;
                    }
                }

                // 取下下载弹窗
                e.Handled = true;
                _applicationService.AddJsonFormatTabItem(content);
            }
            deferral.Complete();
            await Task.CompletedTask;
        }

        #endregion

        public async void AssignInput(SearchRecord record)
        {
            if (record == null) return;
            await _webView.CoreWebView2.ExecuteScriptAsync($@"
// 赋值，并触发变更
clientIpInput.value = '{record.ClientIp}';
clientIpInput.dispatchEvent(eventInput);

beginDateInput.value = '{record.BeginDate}';
beginDateInput.dispatchEvent(eventInput);
beginDateInput.dispatchEvent(eventChange);

endDateInput.value = '{record.EndDate}';
endDateInput.dispatchEvent(eventInput);
endDateInput.dispatchEvent(eventChange);

serviceNameInput.value = '{record.ServiceName}';
serviceNameInput.dispatchEvent(eventInput);

keyWordInput.value = '{record.KeyWord}';
keyWordInput.dispatchEvent(eventInput);

queryInput.value = '{record.Query}';
queryInput.dispatchEvent(eventInput);

searchBtn.click(); 
            ");
        }

        public void ToSignOut()
        {
            IsShowWeb = false;
            InitUsers();
            RefreshCaptcha();
            _webView.Source = new Uri("about:blank");
        }

        private async Task ToLoginAsync()
        {
            var user = Users.FirstOrDefault(u => u.Id == SelectedUserId);
            if (user == null || string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password))
            {
                Growl.Error("请先在设置中配置账户信息");
                return;
            }

            if (string.IsNullOrWhiteSpace(Captcha))
            {
                Growl.Error("请输入验证码");
                return;
            }

            var cookies = await _logHttpClient.LoginAsync(user, Captcha);
            if (cookies == null || cookies.Count <= 0)
            {
                Growl.Error($"登录失败，请重新登录");
                return;
            }

            // 记录登录cookies
            var now = DateTime.Now;
            var loginCookie = cookies.Select(c => new UserLoginCookie { Name = c.Name, Value = c.Value }).ToList();
            await _sqliteService.AddUserLoginRecordAsync(new UserLoginRecord
            {
                Env = _env.GetHashCode(),
                Cookies = JsonSerializer.Serialize(loginCookie),
                LoginDate = now,
                ExpiredDate = now.AddHours(6).AddMinutes(-10), // 1天有效期，提前10分钟过期
            });

            NavigationToLog(cookies);
        }

        private async void NavigationToLog(CookieCollection? cookies)
        {
            if (cookies == null)
            {
                // Growl.Error("请先进行登录");
                return;
            }

            foreach (Cookie cookie in cookies)
            {
                var wb2Cookie = _webView.CoreWebView2.CookieManager.CreateCookie(cookie.Name, cookie.Value, _cookieDomain, "/");
                _webView.CoreWebView2.CookieManager.AddOrUpdateCookie(wb2Cookie);
            }

            // 导航到页面
            _webView.Source = new Uri(_devOpsUri);

            IsShowWeb = true;
        }

        private async void InitView()
        {
            var loginRecord = await _sqliteService.QueryLatestUserLoginRecordAsync(_env);
            if (loginRecord != null)
            {
                var loginCookies = JsonSerializer.Deserialize<List<UserLoginCookie>>(loginRecord.Cookies);
                if (loginCookies != null && loginCookies.Count > 0)
                {
                    var cookies = new CookieCollection();
                    loginCookies.ForEach(c => cookies.Add(new Cookie { Name = c.Name, Value = c.Value }));
                    NavigationToLog(cookies);
                    return;
                }
            }

            InitUsers();
            RefreshCaptcha();
        }

        private async void InitUsers()
        {
            SelectedUserId = -1;
            Users.Clear();
            var users = await _sqliteService.QueryUsersAsync(_env);
            if (users == null || users.Count == 0)
            {
                users = new List<User> { new User { Id = 0, UserName = "未配置账户信息" } };
            }

            users.ForEach(u => Users.Add(u));
            var defaultUser = Users.FirstOrDefault(u => u.Default);
            SelectedUserId = defaultUser == null ? Users.First().Id : defaultUser.Id;
        }

        private DispatcherOperation<TResult> DispatchAsync<TResult>(Func<TResult> callback)
        {
            return Application.Current.Dispatcher.InvokeAsync(callback);
        }

        private string GetCookieDomain(EnvEnum env) => env switch
        {
            EnvEnum.Dev => ".dev.com",
            EnvEnum.Prod => ".prod.com",
            _ => throw new NotImplementedException()
        };

        private string GetDevOpsUri(EnvEnum env, string path)
        {
            return GetBaseUri(env) + path;

            string GetBaseUri(EnvEnum env) => env switch
            {
                EnvEnum.Dev => "http://dev.com",
                EnvEnum.Prod => "http://prod.com",
                _ => throw new NotImplementedException()
            };
        }

        #region Command

        [RelayCommand]
        void RefreshCaptcha()
        {
            CaptchaImage = _logHttpClient.GetCaptchaImage(_env);
        }

        [RelayCommand]
        async Task Login()
        {
            await ToLoginAsync();
        }

        [RelayCommand]
        async Task EnterKeyDown()
        {
            if (IsShowWeb)
            {
                await _webView.CoreWebView2.ExecuteScriptAsync(@"searchBtn.click();");
            }
            else
            {
                await ToLoginAsync();
            }
        }

        [RelayCommand]
        void UserCmbDropDown()
        {
            InitUsers();
        }

        [RelayCommand]
        async Task JsonFormat()
        {
            var e = await _webView.CoreWebView2.ExecuteScriptAsync($"window.open('{_devOpsUri}')");
        }

        #endregion
    }
}
