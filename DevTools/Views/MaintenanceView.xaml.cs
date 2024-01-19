using DevTools.Services;
using DevTools.ViewModels;
using Microsoft.Web.WebView2.Wpf;
using System.Windows.Controls;

namespace DevTools.Views
{
    /// <summary>
    /// ProdView.xaml 的交互逻辑
    /// </summary>
    public partial class MaintenanceView : UserControl,IBaseWebView
    {
        public MaintenanceViewModel Vm { get; }
        private WebView2 webView;

        public MaintenanceView(MaintenanceViewModel vm)
        {
            Vm = vm;
            DataContext = this;

            InitializeComponent();
            InitControl();
        }

        private async void InitControl()
        {
            // 输入框获得焦点
            CaptchaInput.Focus();
            
            webView = Vm.CreateWebView();
            WebViewPanel.Children.Add(webView);
            await Vm.InitWebView(webView);
        }


        public void Dispose()
        {
            webView.Dispose();
        }
    }
}
