using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace EasySave_Project.Views.Components
{
    public class Toastr
    {
        public static async void ShowNotification(string message, StackPanel notificationContainer = null, string type = "Error")
        {
            if (notificationContainer == null)
                throw new ArgumentNullException(nameof(notificationContainer), "Notification container is required.");

            // Déclaration des couleurs par défaut
            Color backgroundColor = Colors.LightGray;
            Color foregroundColor = Colors.Black;
            Color borderColor = Colors.Gray;

            // Gestion des couleurs selon le type de notification
            switch (type)
            {
                case "Success":
                    backgroundColor = Color.Parse("#d4edda");
                    foregroundColor = Color.Parse("#155724");
                    borderColor = Color.Parse("#c3e6cb");
                    break;

                case "Warning":
                    backgroundColor = Color.Parse("#fff3cd");
                    foregroundColor = Color.Parse("#856404");
                    borderColor = Color.Parse("#ffeeba");
                    break;

                case "Info":
                    backgroundColor = Color.Parse("#d1ecf1");
                    foregroundColor = Color.Parse("#0c5460");
                    borderColor = Color.Parse("#bee5eb");
                    break;

                case "Error":
                default:
                    backgroundColor = Color.Parse("#f8d7da");
                    foregroundColor = Color.Parse("#721c24");
                    borderColor = Color.Parse("#f5c6cb");
                    break;
            }

            // Crée la NotificationCard avec le style adapté
            var notificationCard = new Border
            {
                Child = new TextBlock
                {
                    Text = message,
                    Foreground = new SolidColorBrush(foregroundColor),
                    FontSize = 14,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                },
                Background = new SolidColorBrush(backgroundColor),
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(10),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
            };

            // Ajoute la notification au conteneur
            notificationContainer.Children.Add(notificationCard);

            // Affiche la notification pendant 5 secondes
            await Task.Delay(5000);

            // Retire la notification après le délai
            notificationContainer.Children.Remove(notificationCard);
        }
    }
}
