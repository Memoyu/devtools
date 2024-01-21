using HandyControl.Controls;
using HandyControl.Tools;
using DevTools.Common;
using DevTools.Models;
using DevTools.ViewModels;
using DevTools.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mime;
using System.Windows.Input;

namespace DevTools.Services
{
    public class ApplicationService
    {
        private TabControl mainTab;
        private readonly IServiceProvider _serviceProvider;

        public ApplicationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void InitMainTab(TabControl tab)
        {
            mainTab = tab;
            mainTab.SelectionChanged += MainTab_SelectionChanged;
        }

        public Action<SearchRecord>? GetCurrentLogAssignInputAction()
        {
            var contentView = GetSelectedContent();
            var vm = (contentView as MaintenanceView)?.Vm;
            if (vm == null) return null;
            return vm.AssignInput;
        }

        public void SignOutCurrentLog()
        {
            var contentView = GetSelectedContent();
            var vm = (contentView as MaintenanceView)?.Vm;
            if (vm == null) return;
            vm.ToSignOut();
        }

        public void RegisterLogGlobalShortcut()
        {
            var contentView = GetSelectedContent();
            // 每次都初始化回车事件
            if (contentView is not MaintenanceView)
            {
                ClearRegisterGlobalShortcut();
                return;
            }
            var vm = (contentView as MaintenanceView)?.Vm;
            if (vm == null) return;
            GlobalShortcut.Init(new List<KeyBinding>
            {
                new (vm.EnterKeyDownCommand, Key.Enter, ModifierKeys.None)
            });
        }

        public void ClearRegisterGlobalShortcut()
        {
            GlobalShortcut.Init(new List<KeyBinding>());
        }

        public void AddHomeTabItem(EnvEnum env)
        {
            var title = $"{env.GetDescription()}主页";
            var view = (MaintenanceView)(_serviceProvider.GetRequiredService(typeof(MaintenanceView)) ?? throw new ArgumentNullException($"{nameof(MaintenanceView)} 未注入"));
            AddTabItem(title, view);
            view.Vm?.InitViewModel(env, false);
        }

        public void AddLogTabItem(EnvEnum env)
        {
            var title = $"{env.GetDescription()}日志";
            var view = (MaintenanceView)(_serviceProvider.GetRequiredService(typeof(MaintenanceView)) ?? throw new ArgumentNullException($"{nameof(MaintenanceView)} 未注入"));
            AddTabItem(title, view);
            view.Vm?.InitViewModel(env, true);
        }

        public void AddJsonFormatTabItem(string content)
        {
            var view = _serviceProvider.GetRequiredService<JsonFormatView>();
            AddTabItem("Json格式化", view);

            if (!string.IsNullOrWhiteSpace(content))
            {
                view.Vm.SetContent(content);
            }
        }

        public void AddSettingTabItem()
        {
            for (int i = 0; i < mainTab.Items.Count; i++)
            {
                var item = (TabItem)mainTab.Items[i];
                // 如果已经存在，则直接跳到设置页面
                if (item != null && item.Content is SettingView)
                {
                    mainTab.SelectedIndex = i;
                    return;
                }
            }

            // 否则，创建setting tab item
            var view = _serviceProvider.GetRequiredService<SettingView>();
            AddTabItem("设置", view);
        }

        public void AddTabItem(string title, object contentView)
        {
            mainTab.Items.Add(new TabItem
            {
                Header = title,
                Content = contentView,
            });
            mainTab.SelectedIndex = mainTab.Items.Count - 1;
        }

        public void DisposeAllWebView()
        {
            foreach (TabItem item in mainTab.Items)
            {
                var contentView = item?.Content;
                if (contentView == null || contentView is not IBaseWebView view) return;
                view.Dispose();
            }
        }

        public void DisposeWebView(TabItem? item)
        {
            var contentView = item?.Content;
            if (contentView == null || contentView is not IBaseWebView view) return;
            view.Dispose();
        }

        private object? GetSelectedContent()
        {
            var item = mainTab.SelectedItem as TabItem;
            return item?.Content;
        }

        #region Event

        private void MainTab_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RegisterLogGlobalShortcut();
        }

        #endregion
    }
}
