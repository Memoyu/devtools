using DevTools.ViewModels;
using System.ComponentModel;

namespace DevTools.Views
{
    /// <summary>
    /// SearchRecordDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SearchRecordView
    {
        public SearchRecordViewModel Vm { get; }

        public SearchRecordView(SearchRecordViewModel vm)
        {
            Vm = vm;
            DataContext = this;

            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            HideView();
            base.OnClosing(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            Owner = null;
            HideView();
            base.OnDeactivated(e);
        }

        void HideView()
        {
            Vm.CloseDialog();
            Hide();
        }
    }
}
