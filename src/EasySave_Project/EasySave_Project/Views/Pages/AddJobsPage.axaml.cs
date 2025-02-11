using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave_Project.Manager;
using EasySave_Project.Model;
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
        JobName.Text = "";
        SelectedFileSourcePath.Text = "Aucun dossier sélectionné";
        SelectedFileTargetPath.Text = "Aucun dossier sélectionné";
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
            Toastr.ShowNotification("Erreur : Aucune option de type sélectionnée.", NotificationContainer);
            return;
        }

        // Récupère les chemins des dossiers
        string source = SelectedFileSourcePath.Text;
        string target = SelectedFileTargetPath.Text;

        // Vérifie que les champs ne sont pas vides
        if (string.IsNullOrWhiteSpace(name))
        {
            Toastr.ShowNotification("Erreur : Veuillez remplir le nom.",NotificationContainer);
            return;
        }

        if (source == "Aucun dossier sélectionné" || string.IsNullOrWhiteSpace(source))
        {
            Toastr.ShowNotification("Erreur : Le dossier source n'a pas été sélectionné.",NotificationContainer);
            return;
        }

        if (target == "Aucun dossier sélectionné" || string.IsNullOrWhiteSpace(target))
        {
            Toastr.ShowNotification("Erreur : Le dossier cible n'a pas été sélectionné.",NotificationContainer);
            return;
        }
        
        // Crée un nouvel objet Job
        JobManager.GetInstance().CreateAndAddJob(name, source, target, type);
        
        // Affiche une notification de succès
        Toastr.ShowNotification("Le job a été ajouté avec succès.",NotificationContainer, "Success");
    }

    private async void OnOpenFolderTargetDialogClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Sélectionner un dossier"
        };

        // Récupère la fenêtre parente du UserControl
        var window = this.VisualRoot as Window;

        if (window != null)
        {
            var result = await dialog.ShowAsync(window);

            if (!string.IsNullOrEmpty(result))
            {
                SelectedFileTargetPath.Text = result;  // Affiche le chemin du dossier dans le TextBlock
                Console.WriteLine($"Dossier sélectionné : {result}");  // Log le chemin absolu dans la console
            }
            else
            {
                SelectedFileTargetPath.Text = "Aucun dossier sélectionné";
                Console.WriteLine("Aucun dossier sélectionné.");
            }
        }
    }
    
    private async void OnOpenFolderSourceDialogClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Sélectionner un dossier"
        };

        // Récupère la fenêtre parente du UserControl
        var window = this.VisualRoot as Window;

        if (window != null)
        {
            var result = await dialog.ShowAsync(window);

            if (!string.IsNullOrEmpty(result))
            {
                SelectedFileSourcePath.Text = result;  // Affiche le chemin du dossier dans le TextBlock
                Console.WriteLine($"Dossier sélectionné : {result}");  // Log le chemin absolu dans la console
            }
            else
            {
                SelectedFileSourcePath.Text = "Aucun dossier sélectionné";
                Console.WriteLine("Aucun dossier sélectionné.");
            }
        }
    }
}