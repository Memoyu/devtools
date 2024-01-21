using DevTools.Common;
using DevTools.Services;
using DevTools.ViewModels;
using System.ComponentModel;

namespace DevTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainViewModel Vm { get; }

        private readonly ApplicationService _applicationService;

        public MainWindow(MainViewModel vm, ApplicationService applicationService)
        {
            Vm = vm;
            DataContext = this;
            InitializeComponent();

            _applicationService = applicationService;
            applicationService.InitMainTab(TabView);
            // appSvc.AddTabItem(EnvEnum.Dev);
            applicationService.AddLogTabItem(EnvEnum.Prod);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _applicationService.DisposeAllWebView();
            base.OnClosing(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            _applicationService.RegisterLogGlobalShortcut();
            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            _applicationService.ClearRegisterGlobalShortcut();
            base.OnDeactivated(e);
        }
    }
}