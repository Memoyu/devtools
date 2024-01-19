using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using DevTools.Common;
using DevTools.Models;
using DevTools.Services;
using DevTools.Views.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace DevTools.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        [ObservableProperty] List<UserDto> users = new List<UserDto>();
        [ObservableProperty] long devSelected = 0;
        [ObservableProperty] long prodSelected = 0;

        private readonly IServiceProvider _serviceProvider;
        private readonly SqliteService _sqliteService;
        public SettingViewModel(IServiceProvider serviceProvider, SqliteService sqliteService)
        {
            _serviceProvider = serviceProvider;
            _sqliteService = sqliteService;

            QueryUsers();
        }

        [RelayCommand]
        async Task AddUser()
        {
            var dialog = new Dialog();
            var view = _serviceProvider.GetRequiredService<UserEditDialog>();
            view.Vm.InitUser();
            view.Vm.CloseAction = () =>
            {
                QueryUsers();
                dialog.Close();
            };
            dialog = Dialog.Show(view);
            
        }

        [RelayCommand]
        async Task EditUser(UserDto item)
        {
            var dialog = new Dialog();
            var view = _serviceProvider.GetRequiredService<UserEditDialog>();
            view.Vm.InitUser(item.ToEntity());
            view.Vm.CloseAction = () =>
            {
                QueryUsers();
                dialog.Close();
            };
            dialog = Dialog.Show(view);
       
        }

        [RelayCommand]
        async Task DeleteUser(UserDto item)
        {
            await _sqliteService.DeleteUserAsync(item.ToEntity());
            QueryUsers();
        }

        [RelayCommand]
        async Task SetUserDefault(UserDto item)
        {
            foreach (var user in Users)
            {
                if (user.Env != item.Env) continue;
                if (user.Id == item.Id) continue;
                user.Default = false;
            }
            await _sqliteService.UpdateUserDefaultAsync(item.ToEntity());
        }

        [RelayCommand]
        async Task SaveSetting()
        {
        }

        private async void QueryUsers()
        {
            var users = await _sqliteService.QueryUsersAsync(null) ?? new List<User>();
            var dtos = users.Select(u => u.ToDto()).ToList();
            Users = dtos;
        }

    }
}
