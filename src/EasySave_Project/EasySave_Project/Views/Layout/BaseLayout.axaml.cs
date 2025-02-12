using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Layout;
using EasySave_Project.Views.Pages;

namespace EasySave_Project.Views.Layout;

public partial class BaseLayout : UserControl
{
    
    private List<IPage> pages = new List<IPage>();
    
    public BaseLayout()
    {
        InitializeComponent();
        DataContext = new BaseLayoutViewModel();
        
        // Ajout des pages à la liste
        pages.Add(new HomePage());
        pages.Add(new JobsPage());
        pages.Add(new LogsPage());
        pages.Add(new SettingPage(this));
        pages.Add(new AddJobsPage());
        
        ContentArea.Content = pages[0];
    }
    
    public void reload()
    {
        DataContext = new BaseLayoutViewModel();
    }
    
    public void LoadPage(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // Récupère l'index à partir de la propriété Tag
            if (int.TryParse(button.Tag?.ToString(), out int index))
            {
                pages[index].reload();
                ContentArea.Content = pages[index];
            }
        }
        
    }

    // Méthode pour changer dynamiquement le contenu
    public void SetContent(Control content)
    {
        ContentArea.Content = content;
    }
}