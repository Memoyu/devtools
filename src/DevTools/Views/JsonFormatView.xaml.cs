using DevTools.Services;
using DevTools.ViewModels;
using System.Windows.Controls;

namespace DevTools.Views
{
    /// <summary>
    /// JsonFormatView.xaml 的交互逻辑
    /// </summary>
    public partial class JsonFormatView : UserControl, IBaseWebView
    {
        public JsonFormatViewModel Vm { get;}

        public JsonFormatView(JsonFormatViewModel vm)
        {
            Vm = vm;
            DataContext = this;
            InitializeComponent();
            Vm.SetWebView(WebView);
        }

        public void Dispose()
        {
            WebView.Dispose();
        }
    }
}
