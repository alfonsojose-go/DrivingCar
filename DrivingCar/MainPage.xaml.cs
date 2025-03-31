using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.System;
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
       

        public MainPage()
        {
            this.InitializeComponent();
<<<<<<< HEAD
            // Usage Example (Ensure Canvas is Loaded)
            GameCanvas.Loaded += (s, e) => {
                AddMovingImage("Assets/carObstacle1.png", 150, 50);

            };
=======
>>>>>>> 434e8c425de772f22c667e78dd9b5e889cea29a3
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            tiltCar(-15);
            MoveCar(-CarMoveDistance);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            tiltCar(15);
            MoveCar(CarMoveDistance);
        }

        private void MoveCar(double distance)
        {
            double currentLeft = Canvas.GetLeft(PlayerCar);
            double newLeft = currentLeft + distance;

            if (newLeft < LeftBoundary || newLeft > RightBoundary)
            {
                EndGame();
                return;
            }

            Canvas.SetLeft(PlayerCar, newLeft);
            currentScore++;
            lblScore.Text = currentScore.ToString();
        }

        private void tiltCar(double angle)
        {
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.Angle = angle;
            PlayerCar.RenderTransform = rotateTransform;
        }

        private void EndGame()
        {
            btnStart.Content = "Start";
            lblCrashScore.Text = $"Score: {currentScore}";
            lblCrashScore.Visibility = Visibility.Visible;

            // Save and update score history
            scores.Add(currentScore);
            scores = scores.OrderByDescending(s => s).Take(5).ToList();  // Keep only top 5 scores
            SaveScores();
            UpdateScoreDisplay();

            // Reset game state
            currentScore = 0;
            lblScore.Text = "0";

            // Move car back to original position
            Canvas.SetLeft(PlayerCar, CarStartLeft);
            Canvas.SetTop(PlayerCar, CarStartTop);
            tiltCar(0);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.Content = "Playing..";
            lblCrashScore.Visibility = Visibility.Collapsed;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void SaveScores()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string scoreData = string.Join(",", scores);
            localSettings.Values["ScoreHistory"] = scoreData;
        }

        private void LoadScores()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("ScoreHistory"))
            {
                string scoreData = (string)localSettings.Values["ScoreHistory"];
                scores = scoreData.Split(',').Where(s => !string.IsNullOrWhiteSpace(s))
                                    .Select(int.Parse).OrderByDescending(s => s)
                                    .Take(5).ToList();  // Keep only top 5 scores
                UpdateScoreDisplay();
            }
        }

        private void UpdateScoreDisplay()
        {
            lstScores.Items.Clear();
            foreach (int score in scores)
            {
                lstScores.Items.Add($"Score: {score}");
            }
            lstScores.Visibility = scores.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
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
                case VirtualKey.Enter:
                    btnStart_Click(null, null);
                    break;
                case VirtualKey.Escape:
                    btnExit_Click(null, null);
                    break;
            }
            e.Handled = true;
        }
<<<<<<< HEAD


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

=======
    }
>>>>>>> 434e8c425de772f22c667e78dd9b5e889cea29a3
}
