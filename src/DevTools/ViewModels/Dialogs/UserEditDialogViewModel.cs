using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using DevTools.Common;
using DevTools.Models;
using DevTools.Services;
using System.Windows;

namespace DevTools.ViewModels.Dialogs
{
    public partial class UserEditDialogViewModel : ObservableObject
    {
        public Action CloseAction;

        [ObservableProperty] UserDto user = new UserDto();
        [ObservableProperty] List<CmbSourceItem> envs = new List<CmbSourceItem>();

        private readonly SqliteService _sqliteService;

        public UserEditDialogViewModel(SqliteService sqliteService)
        {
            _sqliteService = sqliteService;
            Envs = Enum.GetValues<EnvEnum>().Select(e => new CmbSourceItem { Id = e.GetHashCode(), Name = e.GetDescription() }).ToList();
        }

        public void InitUser(User? user = null)
        {
            if (user != null)
                User = user.ToDto();
        }

        [RelayCommand]
        async Task SaveUser()
        {
            if (string.IsNullOrWhiteSpace(User.UserName) || string.IsNullOrWhiteSpace(User.Password))
            {
                Growl.Warning("请输入账户和密码");
                return;
            }
            var entity = User.ToEntity();
            if (User.Id <= 0)
            {
                await _sqliteService.AddUserAsync(entity);
                Growl.Info("添加账户成功");
            }
            else
            {
                await _sqliteService.UpdateUserAsync(entity);
                Growl.Info("更新账户成功");
            }

            Application.Current.Dispatcher.Invoke(() => CloseAction?.Invoke());
        }
    }
}
