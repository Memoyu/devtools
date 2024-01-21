using DevTools.ViewModels;
using System.Windows.Controls;

namespace DevTools.Views
{
    /// <summary>
    /// SettingView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingView : UserControl
    {
        public SettingViewModel Vm { get; }

        public SettingView(SettingViewModel vm)
        {
            Vm = vm;
            DataContext = this;

            InitializeComponent();
        }
    }
}
