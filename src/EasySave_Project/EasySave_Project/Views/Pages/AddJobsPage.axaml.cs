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

        /// <summary>
        /// Initializes the AddJobsPage, sets up the ViewModel, and assigns the DataContext.
        /// </summary>
        public AddJobsPage()
        {
            InitializeComponent();
            _viewModel = new AddJobsPageViewModel();
            DataContext = _viewModel;
        }
        
        /// <summary>
        /// Resets the form fields and refreshes translations.
        /// </summary>
        public void Reload()
        {
            JobName.Text = "";
            SelectedFileSourcePath.Text = _viewModel.NoFolderSelected;
            SelectedFileTargetPath.Text = _viewModel.NoFolderSelected;
            FirstType.IsChecked = false;
            SecondType.IsChecked = false;
            
            _viewModel.RefreshTranslations();
        }

        /// <summary>
        /// Validates user input and creates a new job if all fields are correctly filled.
        /// </summary>
        /// <param name="sender">The button triggering the event.</param>
        /// <param name="e">Event arguments.</param>
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

        /// <summary>
        /// Displays an error notification based on the given message key.
        /// </summary>
        /// <param name="messageKey">The translation key for the error message.</param>
        private void ShowError(string messageKey)
        {
            string message = TranslationService.GetInstance().GetText(messageKey);
            Toastr.ShowNotification(message, NotificationContainer);
        }

        /// <summary>
        /// Displays a success notification based on the given message key.
        /// </summary>
        /// <param name="messageKey">The translation key for the success message.</param>
        private void ShowSuccess(string messageKey)
        {
            string message = TranslationService.GetInstance().GetText(messageKey);
            Toastr.ShowNotification(message, NotificationContainer, "Success");
        }

        /// <summary>
        /// Checks if the given folder path is invalid or not selected.
        /// </summary>
        /// <param name="path">The folder path to check.</param>
        /// <returns>True if the folder is not selected; otherwise, false.</returns>
        private bool IsFolderNotSelected(string path)
        {
            return string.IsNullOrWhiteSpace(path) || path == _viewModel.NoFolderSelected;
        }

        /// <summary>
        /// Opens a folder selection dialog for the target path.
        /// </summary>
        /// <param name="sender">The button triggering the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnOpenFolderTargetDialogClick(object sender, RoutedEventArgs e)
        {
            await OpenFolderDialog(SelectedFileTargetPath);
        }

        /// <summary>
        /// Opens a folder selection dialog for the source path.
        /// </summary>
        /// <param name="sender">The button triggering the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnOpenFolderSourceDialogClick(object sender, RoutedEventArgs e)
        {
            await OpenFolderDialog(SelectedFileSourcePath);
        }

        /// <summary>
        /// Opens a folder selection dialog and updates the target TextBlock with the selected path.
        /// </summary>
        /// <param name="targetTextBlock">The TextBlock to update with the selected folder path.</param>
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
