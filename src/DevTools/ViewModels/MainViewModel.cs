using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using DevTools.Common;
using DevTools.Services;
using System.Windows;

namespace DevTools.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty] string title = "DevTool";

        private readonly ApplicationService _appSvc;

        public MainViewModel(ApplicationService appSvc)
        {
            _appSvc = appSvc;
        }

        [RelayCommand]
        void AddLogTabItem(EnvEnum env)
        {
            _appSvc.AddLogTabItem(env);
        }

        [RelayCommand]
        void AddMaintenanceHomeTabItem(EnvEnum env)
        {
            _appSvc.AddHomeTabItem(env);
        }

        [RelayCommand]
        void AddJsonFormatTabItem()
        {
            _appSvc.AddJsonFormatTabItem(string.Empty);
        }
        
        [RelayCommand]
        void AddSettingTabItem()
        {
            _appSvc.AddSettingTabItem();
        }

        [RelayCommand]
        void ClosedTabItem(RoutedEventArgs args)
        {
            var item = args.OriginalSource as TabItem;
            _appSvc.DisposeWebView(item);
        }
    }
}
