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
        private int currentScore;
        private List<int> scores = new List<int>();

        private const double CarMoveDistance = 10;
        private const double LeftBoundary = 50;
        private const double RightBoundary = 300;
        private const double CarStartLeft = 170;  // Original car position
        private const double CarStartTop = 324;
        private bool gameRunning;


        public MainPage()
        {
            this.InitializeComponent();
            LoadScores();

            // Ensure the GameCanvas is properly loaded
            if (GameCanvas != null)
            {
                GameCanvas.Loaded += GameCanvas_Loaded;
            }
        }

        // Separate method for handling the Loaded event
        private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Car myCar = new Car("Assets/highwayCar.png", 200, 0); // Position at (200, 0)
            myCar.AddMovingImage(GameCanvas); // Add and animate the car

            // Create a PoliceCar (twice as fast)
            PoliceCar police = new PoliceCar("Assets/highwayCar.png", 100, 0);
            police.AddMovingImage(GameCanvas);

            // Create a SpeedCar (faster)
            SpeedCar speed = new SpeedCar("Assets/highwayCar.png", 250, 0);
            speed.AddMovingImage(GameCanvas);


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


    }
}
