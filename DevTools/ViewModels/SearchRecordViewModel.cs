using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using DevTools.Common;
using DevTools.Models;
using DevTools.Services;
using DevTools.Views.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace DevTools.ViewModels
{
    public partial class SearchRecordViewModel : ObservableObject
    {
        [ObservableProperty] List<SearchRecord> records = new List<SearchRecord>();
        [ObservableProperty] List<SearchRemark> remarks = new List<SearchRemark>();
        [ObservableProperty] string searchKeyWord;
        [ObservableProperty] int recordPageIndex = 1;
        [ObservableProperty] int recordPageSize = 30;
        [ObservableProperty] long recordTotal = 1;

        [ObservableProperty] int remarkPageIndex = 1;
        [ObservableProperty] int remarkPageSize = 30;
        [ObservableProperty] long remarkTotal = 1;

        int tabSelectedIndex = 0;
        public int TabSelectedIndex
        {
            get => tabSelectedIndex;
            set
            {
                SetProperty(ref tabSelectedIndex, value);
                TabSelectionChanged();
            }
        }

        private List<Dialog> dialogs = new List<Dialog>();
        private EnvEnum _env;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationService _applicationService;
        private readonly SqliteService _sqliteService;

        public SearchRecordViewModel(IServiceProvider serviceProvider, ApplicationService applicationService, SqliteService sqliteService)
        {
            _serviceProvider = serviceProvider;
            _applicationService = applicationService;
            _sqliteService = sqliteService;
        }

        public async void InitView(EnvEnum env)
        {
            _env = env;
            SearchKeyWord = string.Empty;
            await QueryDataList(SearchKeyWord);
        }

        public void CloseDialog()
        {
            foreach (var dialog in dialogs)
            {
                dialog?.Close();
            }
        }

        async void TabSelectionChanged()
        {
            SearchKeyWord = string.Empty;
            await QueryDataList(SearchKeyWord);
        }

        [RelayCommand]
        void RecordMouseDoubleClick(SearchRecord item)
        {
            var selectAction = _applicationService.GetCurrentLogAssignInputAction();
            selectAction?.Invoke(item);
        }

        [RelayCommand]
        void RemarkMouseDoubleClick(SearchRemark item)
        {
            var selectAction = _applicationService.GetCurrentLogAssignInputAction();
            var record = item?.ToRecord();
            selectAction?.Invoke(record);
        }

        [RelayCommand]
        async Task FilterList(string keyWord)
        {
            if (TabSelectedIndex == 0)
            {
                RecordPageIndex = 1;
            }
            else
            {
                RemarkPageIndex = 1;
            }

            await QueryDataList(keyWord);
        }

        [RelayCommand]
        async Task PageUpdated(FunctionEventArgs<int> page)
        {
            await QueryDataList(SearchKeyWord);
        }

        [RelayCommand]
        async Task SearchRecordToRemark(SearchRecord item)
        {
            var remark = new SearchRemark
            {
                ClientIp = item.ClientIp,
                ServiceName = item.ServiceName,
                KeyWord = item.KeyWord,
                Query = item.Query,
                CreateDate = DateTime.Now,
            };
            var query = await _sqliteService.QuerySearchRemarkAsync(remark);
            if (query != null)
            {
                Growl.Info("查询记录收藏已存在");
                return;
            }

            var dialog = new Dialog();
            var view = _serviceProvider.GetRequiredService<RemarkEditDialog>();
            view.Vm.InitRemark(remark);
            view.Vm.CloseAction = () => dialog.Close();
            dialog = Dialog.Show(view);
            dialogs.Add(dialog);
        }

        [RelayCommand]
        async Task DeleteRemark(SearchRemark item)
        {
            var res = await _sqliteService.DeleteSearchRemarksAsync(item.Id);
            if (!res)
            {
                Growl.Error("删除失败");
                return;
            }
            Remarks.Remove(item);
            Growl.Info("删除成功");
        }

        private async Task QueryDataList(string keyWord)
        {
            if (TabSelectedIndex == 0)
            {
                Records = await QuerySearchRecordsPageAsync(_env, keyWord);
            }
            else
            {
                Remarks = await QuerySearchRemarksPageAsync(_env, keyWord);
            }
        }
        private async Task<List<SearchRecord>> QuerySearchRecordsPageAsync(EnvEnum env, string keyWord)
        {
            var page = new SearchRecordPagingInfo
            {
                Env = env.GetHashCode(),
                KeyWord = keyWord,
                PageNumber = RecordPageIndex,
                PageSize = RecordPageSize
            };

            var res = await _sqliteService.QuerySearchRecordsPageAsync(page) ?? new List<SearchRecord>();
            RecordTotal = page.Count % RecordPageSize == 0 ? page.Count / RecordPageSize : (page.Count / RecordPageSize) + 1;
            return res;
        }

        private async Task<List<SearchRemark>> QuerySearchRemarksPageAsync(EnvEnum env, string keyWord)
        {
            var page = new SearchRemarkPagingInfo
            {
                Env = env.GetHashCode(),
                KeyWord = keyWord,
                PageNumber = RemarkPageIndex,
                PageSize = RemarkPageSize
            };

            var res = await _sqliteService.QuerySearchRemarksPageAsync(page) ?? new List<SearchRemark>();
            RemarkTotal = page.Count % RemarkPageSize == 0 ? page.Count / RemarkPageSize : (page.Count / RemarkPageSize) + 1;
            return res;
        }
    }
}
