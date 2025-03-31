using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;

namespace DrivingCar
{
    public sealed partial class MainPage : Page
    {
        // Car movement properties
        private const double CarMoveDistance = 10; // Pixels to move per click
        private const double LeftBoundary = 50;    // Left boundary (match your road position)
        private const double RightBoundary = 300;  // Right boundary (road width - car width)
        bool gameRunning;
       

        public MainPage()
        {
            this.InitializeComponent();
            // Usage Example (Ensure Canvas is Loaded)
            GameCanvas.Loaded += (s, e) => {
                AddMovingImage("Assets/carObstacle1.png", 150, 50);

            };
        }


        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            tiltCar(-15); // Tilt left
            MoveCar(-CarMoveDistance); // Move left
            
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            tiltCar(15); // Tilt right
            MoveCar(CarMoveDistance); // Move right
            
        }

        private void MoveCar(double distance)
        {
            // Get current position
            double currentLeft = Canvas.GetLeft(PlayerCar);

            // Calculate new position
            double newLeft = currentLeft + distance;

            // Check boundaries (so car doesn't go off-road)
            if (newLeft < LeftBoundary)
                newLeft = LeftBoundary;
            else if (newLeft > RightBoundary)
                newLeft = RightBoundary;

            // Set new position
            Canvas.SetLeft(PlayerCar, newLeft);
        }

        private void tiltCar(double angle)
        {
            // Rotate the car
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.Angle = angle;
            PlayerCar.RenderTransform = rotateTransform;
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.Content = "Playing..";
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }


        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //if (!_gameRunning) return;

            switch (e.Key)
            {
                case VirtualKey.Left:
                    btnLeft_Click(null, null);
                    break;
                case VirtualKey.Right:
                    btnRight_Click(null, null);
                    break;
                case VirtualKey.Up:
                case VirtualKey.Down:
                    tiltCar(0);
                    break;
                case VirtualKey.Enter:  // Handle Enter key press
                    btnStart_Click(null, null);
                    break;
                case VirtualKey.Escape:  // Handle ESC key press
                    btnExit_Click(null, null);
                    break;
            }
            e.Handled = true;
        }


        public void AddMovingImage(string imagePath, double width, double height)
        {
            // Create Image
            Image img = new Image
            {
                Width = width,
                Height = height
            };

            // Load Image Source
            string fullPath = $"ms-appx:///{imagePath.TrimStart('/')}";
            BitmapImage bitmap = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
            img.Source = bitmap;

            // Set initial position
            Canvas.SetLeft(img, 200); // Center horizontally
            Canvas.SetTop(img, 0);    // Start at the top

            // Add to Canvas
            GameCanvas.Children.Add(img);

            // Ensure Canvas Height is Available
            double targetHeight = GameCanvas.ActualHeight > 0 ? GameCanvas.ActualHeight : 500;

            // Create Animation
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = targetHeight - height,
                Duration = new Duration(TimeSpan.FromSeconds(3)),
                AutoReverse = false,
                RepeatBehavior = new RepeatBehavior(1)
            };

            // Apply Animation to Canvas.Top Property
            Storyboard storyboard = new Storyboard();
            Storyboard.SetTarget(animation, img);
            Storyboard.SetTargetProperty(animation, "(Canvas.Top)");
            storyboard.Children.Add(animation);

            // Start Animation on UI Thread
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                storyboard.Begin();
            });
        }

        
    };

}
