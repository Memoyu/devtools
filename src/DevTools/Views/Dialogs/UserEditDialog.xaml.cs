using DevTools.ViewModels.Dialogs;

namespace DevTools.Views.Controls
{
    /// <summary>
    /// UserEditDialog.xaml 的交互逻辑
    /// </summary>
    public partial class UserEditDialog
    {
        public UserEditDialogViewModel Vm { get; }

        public UserEditDialog(UserEditDialogViewModel vm)
        {
            Vm = vm;
            DataContext = this;

            InitializeComponent();
        }
    }
}
