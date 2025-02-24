using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Pages;
using EasySave_Project.Views.Components;

namespace EasySave_Project.Views.Pages
{
    public partial class AddJobsPage : UserControl, IPage
    {
        private readonly AddJobsPageViewModel _viewModel;

        public AddJobsPage()
        {
            InitializeComponent();
            _viewModel = new AddJobsPageViewModel();
            DataContext = _viewModel;
        }
        
        public void Reload()
        {
            JobName.Text = "";
            SelectedFileSourcePath.Text = _viewModel.NoFolderSelected;
            SelectedFileTargetPath.Text = _viewModel.NoFolderSelected;
            FirstType.IsChecked = false;
            SecondType.IsChecked = false;
            
            _viewModel.RefreshTranslations();
        }

        private void Valide(object sender, RoutedEventArgs e)
        {
            string name = JobName.Text;

            JobSaveTypeEnum? type = FirstType.IsChecked == true ? JobSaveTypeEnum.COMPLETE
                                : SecondType.IsChecked == true ? JobSaveTypeEnum.DIFFERENTIAL
                                : (JobSaveTypeEnum?)null;

            if (type == null)
            {
                ShowError("ErrorNoTypeSelected");
                return;
            }

            string source = SelectedFileSourcePath.Text;
            string target = SelectedFileTargetPath.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError("ErrorNoName");
                return;
            }

            if (IsFolderNotSelected(source))
            {
                ShowError("ErrorNoSourceFolder");
                return;
            }

            if (IsFolderNotSelected(target))
            {
                ShowError("ErrorNoTargetFolder");
                return;
            }

            JobManager.GetInstance().CreateAndAddJob(name, source, target, type.Value);
            ShowSuccess("JobAddedSuccess");
        }

        private void ShowError(string messageKey)
        {
            string message = TranslationService.GetInstance().GetText(messageKey);
            Toastr.ShowNotification(message, NotificationContainer);
        }

        private void ShowSuccess(string messageKey)
        {
            string message = TranslationService.GetInstance().GetText(messageKey);
            Toastr.ShowNotification(message, NotificationContainer, "Success");
        }

        private bool IsFolderNotSelected(string path)
        {
            return string.IsNullOrWhiteSpace(path) || path == _viewModel.NoFolderSelected;
        }

        private async void OnOpenFolderTargetDialogClick(object sender, RoutedEventArgs e)
        {
            await OpenFolderDialog(SelectedFileTargetPath);
        }

        private async void OnOpenFolderSourceDialogClick(object sender, RoutedEventArgs e)
        {
            await OpenFolderDialog(SelectedFileSourcePath);
        }

        private async System.Threading.Tasks.Task OpenFolderDialog(TextBlock targetTextBlock)
        {
            var dialog = new OpenFolderDialog
            {
                Title = TranslationService.GetInstance().GetText("ChooseFolder")
            };

            if (this.VisualRoot is Window window)
            {
                var result = await dialog.ShowAsync(window);
                targetTextBlock.Text = !string.IsNullOrEmpty(result) ? result : _viewModel.NoFolderSelected;
            }
        }
    }
}
