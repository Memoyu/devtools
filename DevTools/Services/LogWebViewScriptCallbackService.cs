using DevTools.Common;
using DevTools.Models;
using DevTools.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;

namespace DevTools.Services
{
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class LogWebViewScriptCallbackService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SqliteService _sqliteService;
        private readonly ApplicationService _applicationService;
        private EnvEnum _env;
        private Dictionary<EnvEnum, SearchRecordView> _viewDic = new Dictionary<EnvEnum, SearchRecordView>();

        public LogWebViewScriptCallbackService(IServiceProvider serviceProvider, SqliteService sqliteService, ApplicationService applicationService)
        {
            _serviceProvider = serviceProvider;
            _sqliteService = sqliteService;
            _applicationService = applicationService;
        }

        public void SetEnv(EnvEnum env)
        {
            _env = env;
        }

        public async void SearchClickCallback(string input)
        {
            var record = JsonSerializer.Deserialize<SearchRecord>(input);
            if (record == null) return;
            record.RecordDate = DateTime.Now;
            record.Env = _env.GetHashCode();
            await _sqliteService.AddSearchRecordsAsync(new List<SearchRecord> { record });
        }

        public void GetSearchRecordCallback()
        {
            WindowsApi.GetCursorPos(out var p);

            var exist = _viewDic.TryGetValue(_env, out var view);
            if (!exist)
            {
                if (exist) _viewDic.Remove(_env);
                view = _serviceProvider.GetRequiredService<SearchRecordView>();
                view.Owner = Application.Current.MainWindow;
                view.WindowStartupLocation = WindowStartupLocation.Manual;
                view.Title = _env.GetDescription() + " 查询记录";
                _viewDic.Add(_env, view);
            }

            var areaHeight = SystemParameters.WorkArea.Size.Height;
            view.Height = areaHeight / 2;
            view.Left = p.X - (view.Width / 2);
            view.Top = p.Y + 20;
            view.Vm.InitView(_env);

            view.Show();
            view.Activate();
        }

        public void ToJsonFormatClickCallback(string input)
        {
            _applicationService.AddJsonFormatTabItem(input);
        }

        public void SignOutClickCallback()
        {
            _applicationService.SignOutCurrentLog();
        }
    }
}
