using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using DevTools.Models;
using DevTools.Services;
using System.Windows;

namespace DevTools.ViewModels.Dialogs
{
    public partial class RemarkEditDialogViewModel : ObservableObject
    {
        public Action CloseAction;

        [ObservableProperty] SearchRemarkDto remark;

        private readonly SqliteService _sqliteService;

        public RemarkEditDialogViewModel(SqliteService sqliteService)
        {
            _sqliteService = sqliteService;
        }

        public void InitRemark(SearchRemark remark)
        {
            Remark = remark.ToDto();
        }

        [RelayCommand]
        async Task SaveRemark()
        {
            if (string.IsNullOrWhiteSpace(Remark.Desc))
            {
                Growl.Warning("请输入收藏记录描述");
                return;
            }

            await _sqliteService.AddSearchRemarksAsync(new List<SearchRemark> { Remark.ToEntity() });
            Growl.Info("添加收藏成功");
            Application.Current.Dispatcher.Invoke(() => CloseAction?.Invoke());
        }
    }
}
