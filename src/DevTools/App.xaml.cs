using System.Text;
using System.Windows;
using System.Windows.Threading;
using FreeSql;
using DevTools.Services;
using DevTools.ViewModels;
using DevTools.ViewModels.Dialogs;
using DevTools.Views;
using DevTools.Views.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace DevTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private Window mainWindow;
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            Startup += new StartupEventHandler(AppStartupEvent);
            Exit += new ExitEventHandler(AppExitEvent);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            AddSqlite(services);

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainViewModel>();

            services.AddTransient<SettingView>();
            services.AddTransient<SettingViewModel>();
            services.AddTransient<UserEditDialog>();
            services.AddTransient<UserEditDialogViewModel>();
            services.AddTransient<SearchRecordView>();
            services.AddTransient<SearchRecordViewModel>();
            services.AddTransient<RemarkEditDialog>();
            services.AddTransient<RemarkEditDialogViewModel>();
            services.AddTransient<MaintenanceView>();
            services.AddTransient<MaintenanceViewModel>();
            services.AddTransient<JsonFormatView>();
            services.AddTransient<JsonFormatViewModel>();

            services.AddTransient<LogWebViewScriptCallbackService>();
            services.AddSingleton<ApplicationService>();
            services.AddSingleton<LogHttpClient>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            mainWindow = _serviceProvider.GetService<MainWindow>() ?? throw new ArgumentNullException(nameof(MainWindow));
            mainWindow.Show();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            mainWindow.Close();
        }

        private void AddSqlite(IServiceCollection services)
        {
            var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, "data source=devtools.db")
                // .UseMonitorCommand(cmd => Trace.WriteLine($"线程：{cmd.CommandText}\r\n"))
                .UseAutoSyncStructure(true) //自动创建、迁移实体表结构
                .UseNoneCommandParameter(true)
                .Build();

            services.AddSingleton(fsql);
            services.AddSingleton<SqliteService>();
        }

        #region 事件捕获


        private void AppStartupEvent(object sender, StartupEventArgs e)
        {
            //UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);

            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        private void AppExitEvent(object sender, ExitEventArgs e)
        {
        }


        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true; //把 Handled 属性设为true，表示此异常已处理，程序可以继续运行，不会强制退出      
                MessageBox.Show("捕获未处理异常:" + e.Exception.Message);
            }
            catch (Exception ex)
            {
                //此时程序出现严重异常，将强制结束退出
                MessageBox.Show("程序发生致命错误，将终止！");
            }

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StringBuilder sbEx = new StringBuilder();
            if (e.IsTerminating)
            {
                sbEx.Append("程序发生致命错误，将终止！");
            }
            sbEx.Append("捕获未处理异常：");
            if (e.ExceptionObject is Exception)
            {
                sbEx.Append(((Exception)e.ExceptionObject).Message);
            }
            else
            {
                sbEx.Append(e.ExceptionObject);
            }
            MessageBox.Show(sbEx.ToString());
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            //task线程内未处理捕获
            MessageBox.Show("捕获线程内未处理异常：" + e.Exception.Message);
            e.SetObserved();//设置该异常已察觉（这样处理后就不会引起程序崩溃）
        }

        #endregion
    }

}
