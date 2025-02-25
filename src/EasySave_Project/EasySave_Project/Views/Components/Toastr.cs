using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace EasySave_Project.Views.Components
{
    public class Toastr
    {
        /// <summary>
        /// Displays a notification message in the specified container with different styles based on the type (Success, Warning, Info, Error).
        /// </summary>
        /// <param name="message">The notification message to display.</param>
        /// <param name="notificationContainer">The container where the notification will be displayed.</param>
        /// <param name="type">The type of notification (Success, Warning, Info, Error).</param>
        /// <exception cref="ArgumentNullException">Thrown when notificationContainer is null.</exception>
        public static async void ShowNotification(string message, StackPanel notificationContainer = null, string type = "Error")
        {
            if (notificationContainer == null)
                throw new ArgumentNullException(nameof(notificationContainer), "Notification container is required.");

            Color backgroundColor = Colors.LightGray;
            Color foregroundColor = Colors.Black;
            Color borderColor = Colors.Gray;

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

                default:
                    backgroundColor = Color.Parse("#f8d7da");
                    foregroundColor = Color.Parse("#721c24");
                    borderColor = Color.Parse("#f5c6cb");
                    break;
            }
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
            notificationContainer.Children.Add(notificationCard);
            await Task.Delay(5000);
            notificationContainer.Children.Remove(notificationCard);
        }
        
        /// <summary>
        /// Displays a notification message in the specified container with a predefined style. Use for server notification
        /// </summary>
        /// <param name="message">The notification message to display.</param>
        /// <param name="notificationContainer">The container where the notification will be displayed.</param>
        /// <exception cref="ArgumentNullException">Thrown when notificationContainer is null.</exception>
        public static async void ShowServeurNotification(string message, StackPanel notificationContainer = null)
        {
            if (notificationContainer == null)
                throw new ArgumentNullException(nameof(notificationContainer), "Notification container is required.");

            Color backgroundColor, foregroundColor, borderColor;
            backgroundColor = Color.Parse("#d1ecf1");
            foregroundColor = Color.Parse("#0c5460");
            borderColor = Color.Parse("#bee5eb");
            
            Border notificationCard = null;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                notificationCard = new Border
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

                notificationContainer.Children.Add(notificationCard);
            });

            await Task.Delay(5000);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                notificationContainer.Children.Remove(notificationCard);
            });
        }
    }
}
