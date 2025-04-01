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
        private const double CarStartLeft = 170;
        private const double CarStartTop = 324;
        private bool gameRunning;
        private Player player;
        private DispatcherTimer gameTimer;
        private DispatcherTimer spawnTimer;
        private List<Car> obstacles = new List<Car>();
        private Random random = new Random();

        public MainPage()
        {
            this.InitializeComponent();
            LoadScores();
            player = new Player(PlayerCar);
            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            gameTimer.Tick += GameLoop;

            // Set up the spawn timer to control obstacle spawning at a specific interval
            spawnTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) }; // Set the interval to 2 seconds or any desired value
            spawnTimer.Tick += SpawnObstacleTimed;

        }

        private void SpawnObstacleTimed(object sender, object e)
        {
            // Only spawn obstacles if the game is running
            if (gameRunning)
            {
                SpawnObstacles();
            }
        }


        private void SpawnObstacles()
        {
            int obstacleType = random.Next(1, 5); // Random number between 1 and 4
            Car obstacle = null;

            switch (obstacleType)
            {
                case 1:
                    obstacle = new Car("Assets/carObstacle1.png", random.Next(45, 300), 0);
                    break;
                case 2:
                    obstacle = new PoliceCar("Assets/carPolice.png", random.Next(45, 300), 0);
                    break;
                case 3:
                    obstacle = new SpeedCar("Assets/speedCar.png", random.Next(45, 300), 380);
                    break;
                case 4:
                    obstacle = new Car("Assets/carObstacle2.png", random.Next(45, 300), 0);
                    break;
            }

            if (obstacle != null)
            {
                obstacles.Add(obstacle);
                obstacle.AddMovingImage(GameCanvas);

                // Ensure the obstacle starts at the top of the screen
                Canvas.SetTop(obstacle._carImage, 0);

                // Get the height of the canvas and the height of the car image
                double canvasHeight = GameCanvas.ActualHeight;
                double carHeight = obstacle._carImage.ActualHeight;

                // Set the animation's 'To' value so it moves the car all the way to the bottom
                double targetPosition = canvasHeight - carHeight;

                // Create an animation to move the obstacle down
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = 0,          // Start at the top of the canvas
                    To = targetPosition, // Move to the bottom (adjusted for car's height)
                    Duration = TimeSpan.FromSeconds(3), // Adjust speed
                    FillBehavior = FillBehavior.Stop // Stops after completion
                };

                // Apply animation to Canvas.Top
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, obstacle._carImage);
                Storyboard.SetTargetProperty(animation, "(Canvas.Top)");

                // Remove obstacle when animation is complete
                storyboard.Completed += (s, e) =>
                {
                    // Remove from the canvas and from the obstacles list
                    GameCanvas.Children.Remove(obstacle._carImage);
                    obstacles.Remove(obstacle);
                };

                // Start the animation
                storyboard.Begin();
            }
        }







        private void GameLoop(object sender, object e)
        {
            if (!gameRunning) return;

            // Check for collisions before spawning new obstacles
            bool crashDetected = false;

            // Check for collisions with all obstacles
            foreach (var obstacle in obstacles)
            {
                if (player.CheckCrash(obstacle)) // Check for a collision with the player
                {
                    crashDetected = true; // If a crash is detected, stop spawning and handle the crash
                    break;
                }
            }

            if (crashDetected)
            {
                EndGame(); // Handle end game logic if a crash occurred
                return; // Stop further processing, no need to spawn new obstacles
            }

            // Update the score
            currentScore++;
            lblScore.Text = currentScore.ToString();
        }




        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            player.tiltLeft();
            player.MoveLeft();
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            player.tiltRight();
            player.MoveRight();
        }

        private void EndGame()
        {
            gameRunning = false;
            gameTimer.Stop();
            spawnTimer.Stop(); // Stop the spawn timer as well
            btnStart.Content = "Start";
            lblCrashScore.Text = $"Score: {currentScore}";
            lblCrashScore.Visibility = Visibility.Visible;

            scores.Add(currentScore);
            scores = scores.OrderByDescending(s => s).Take(5).ToList();
            SaveScores();
            UpdateScoreDisplay();

            // Clear obstacles from the canvas
            foreach (var obstacle in obstacles)
            {
                GameCanvas.Children.Remove(obstacle._carImage); // Assuming _carImage is the Image object
            }
            obstacles.Clear();

            currentScore = 0;
            lblScore.Text = "0";
            Canvas.SetLeft(PlayerCar, CarStartLeft);
            Canvas.SetTop(PlayerCar, CarStartTop);
            player.resetTilt();
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            gameRunning = true;
            btnStart.Content = "Playing..";
            lblCrashScore.Visibility = Visibility.Collapsed;
            currentScore = 0;
            lblScore.Text = "0";
            obstacles.Clear(); // Clear any previous obstacles before starting

            // Start the main game loop timer
            gameTimer.Start();

            // Start the obstacle spawning timer
            spawnTimer.Start();
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
                                    .Take(5).ToList();
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
                    player.resetTilt();
                    player.MoveUp();
                    break;
                case VirtualKey.Down:
                    player.resetTilt();
                    player.MoveDown();
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
