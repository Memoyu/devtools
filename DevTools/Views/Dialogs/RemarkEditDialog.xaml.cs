using DevTools.ViewModels.Dialogs;

namespace DevTools.Views.Controls
{
    /// <summary>
    /// RemarkEditDialog.xaml 的交互逻辑
    /// </summary>
    public partial class RemarkEditDialog
    {
        public RemarkEditDialogViewModel Vm { get; }

        public RemarkEditDialog(RemarkEditDialogViewModel vm)
        {
            Vm = vm;
            DataContext = this;
            InitializeComponent();
        }
    }
}
