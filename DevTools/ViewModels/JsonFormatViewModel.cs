using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Threading;

namespace DevTools.ViewModels
{
    public partial class JsonFormatViewModel : ObservableObject
    {
        private volatile WebView2 _webView;
        private string content;


        [RelayCommand]
        async void JsonFormat()
        {
            await _webView.CoreWebView2.ExecuteScriptAsync("window.open(\"https://www.baidu.com\")");
        }


        public void SetWebView(WebView2 webView)
        {
            webView.NavigationCompleted += OnWebViewNavigationCompleted;
            _webView = webView;
            webView.Source = new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\json\index.html"));
        }

        public void SetContent(string content)
        {
            this.content = content;
        }

        private void OnWebViewNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            DispatchAsync(InitializeEditorAsync);
        }

        private async Task InitializeEditorAsync()
        {
            await SetContentAsync(content);
        }

        private async Task SetContentAsync(string content)
        {
            if (content == null) return;
            try
            {
                var jsonDocument = JsonDocument.Parse(content);
                content = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions()
                {
                    // 整齐打印
                    WriteIndented = true,
                    //重新编码，解决中文乱码问题
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });
            }
            catch
            {
                var i1 = content.IndexOf('{');
                var i2 = content.LastIndexOf('}') + 1;
                if (i1 > 0)
                {
                    content = content.Substring(i1, i2 - i1);
                }
                else
                {
                    content = $"'{content}'";
                }
            }

            await _webView.CoreWebView2.ExecuteScriptAsync($"setEditorContent({content});");
        }

        private DispatcherOperation<TResult> DispatchAsync<TResult>(Func<TResult> callback)
        {
            return Application.Current.Dispatcher.InvokeAsync(callback);
        }
    }
}
