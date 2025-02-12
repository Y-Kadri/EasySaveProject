using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Pages;
using EasySave_Project.Views.Components;

namespace EasySave_Project.Views.Pages;

public partial class AddJobsPage : UserControl, IPage
{

    public AddJobsPage()
    {
        InitializeComponent();
        DataContext = new AddJobsPageViewModel(); 
    }
    
    public void reload()
    {
        string aucundossier = TranslationService.GetInstance().GetText("NoFolderSelected");
        
        JobName.Text = "";
        SelectedFileSourcePath.Text = aucundossier;
        SelectedFileTargetPath.Text = aucundossier;
        FirstType.IsChecked = false;
        SecondType.IsChecked = false;
        DataContext = new AddJobsPageViewModel();
    }


    private void valide(object sender, RoutedEventArgs e)
    {
        // Récupère le nom saisi
        string name = JobName.Text;

        // Vérifie quelle option radio est sélectionnée
        JobSaveTypeEnum type;
        if (FirstType.IsChecked == true)
        {
            type = JobSaveTypeEnum.COMPLETE;
        }
        else if (SecondType.IsChecked == true)
        {
            type = JobSaveTypeEnum.DIFFERENTIAL;
        }
        else
        {
            string message = TranslationService.GetInstance().GetText("ErrorNoTypeSelected");
            Toastr.ShowNotification(message, NotificationContainer);
            return;
        }

        // Récupère les chemins des dossiers
        string source = SelectedFileSourcePath.Text;
        string target = SelectedFileTargetPath.Text;

        // Vérifie que les champs ne sont pas vides
        if (string.IsNullOrWhiteSpace(name))
        {
            string message = TranslationService.GetInstance().GetText("ErrorNoName");
            Toastr.ShowNotification(message,NotificationContainer);
            return;
        }

        if (source == TranslationService.GetInstance().GetText("NoFolderSelected") || string.IsNullOrWhiteSpace(source))
        {
            string message = TranslationService.GetInstance().GetText("ErrorNoSourceFolder");
            Toastr.ShowNotification(message,NotificationContainer);
            return;
        }

        if (target == TranslationService.GetInstance().GetText("NoFolderSelected") || string.IsNullOrWhiteSpace(target))
        {
            string message = TranslationService.GetInstance().GetText("ErrorNoTargetFolder");
            Toastr.ShowNotification(message, NotificationContainer);
            return;
        }
        
        // Crée un nouvel objet Job
        JobManager.GetInstance().CreateAndAddJob(name, source, target, type);
        
        // Affiche une notification de succès
        string messageSuccess = TranslationService.GetInstance().GetText("JobAddedSuccess");
        Toastr.ShowNotification(messageSuccess, NotificationContainer, "Success");
    }

    private async void OnOpenFolderTargetDialogClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = TranslationService.GetInstance().GetText("ChooseFolder")
        };

        // Récupère la fenêtre parente du UserControl
        var window = this.VisualRoot as Window;

        if (window != null)
        {
            var result = await dialog.ShowAsync(window);

            if (!string.IsNullOrEmpty(result))
            {
                SelectedFileTargetPath.Text = result;  // Affiche le chemin du dossier dans le TextBlock
            }
            else
            {
                SelectedFileTargetPath.Text = TranslationService.GetInstance().GetText("NoFolderSelected");
            }
        }
    }
    
    private async void OnOpenFolderSourceDialogClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title =  TranslationService.GetInstance().GetText("ChooseFolder")
        };

        // Récupère la fenêtre parente du UserControl
        var window = this.VisualRoot as Window;

        if (window != null)
        {
            var result = await dialog.ShowAsync(window);

            if (!string.IsNullOrEmpty(result))
            {
                SelectedFileSourcePath.Text = result;  // Affiche le chemin du dossier dans le TextBlock
            }
            else
            {
                SelectedFileSourcePath.Text = TranslationService.GetInstance().GetText("NoFolderSelected");
            }
        }
    }
}