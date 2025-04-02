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
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using System.IO;
using System.Threading.Tasks;

namespace DrivingCar
{
    public sealed partial class MainPage : Page
    {
        private int currentScore;
        private List<int> scores;
        private const double CarStartLeft = 171;
        private const double CarStartTop = 228;
        private bool gameRunning;
        private Player player;
        private DispatcherTimer gameTimer;
        private DispatcherTimer spawnTimer;
        private List<Car> obstacles;
        private Random random;
        private ApplicationView scoreboardView;
        private const string ScoreFile = "scores.json";

        private SoundManager soundManager; // Sound Manager instance

        public MainPage()
        {
            this.InitializeComponent();
            scores = new List<int>();
            obstacles = new List<Car>();
            random = new Random();
            LoadScores();
            
            player = new Player(PlayerCar);
            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            gameTimer.Tick += GameLoop;

            spawnTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            spawnTimer.Tick += SpawnObstacleTimed;

            soundManager = new SoundManager(); // Initialize Sound Manager

            // Hide crash score initially
            lblCrashScore.Visibility = Visibility.Collapsed;
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
                    soundManager.PlaySirenSound(); // Play siren when police car appears
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

                    // Stop siren sound when the police car disappears (after animation completes)
                    if (obstacle is PoliceCar)
                    {
                        soundManager.StopSirenSound();
                    }
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
                    if (obstacle is PoliceCar)
                    {
                        // Stop siren sound if PoliceCar is hit
                        soundManager.StopSirenSound();
                    }
                    break;
                }
            }

            if (crashDetected)
            {
                soundManager.PlayCrashSound(); // Play crash sound on collision
                GameOver(); // Handle end game logic if a crash occurred
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
            //await Task.Delay(100); // Small delay to simulate key press effect
            //player.resetTilt();
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            player.tiltRight();
            player.MoveRight();
            //await Task.Delay(100); // Small delay to simulate key press effect
            //player.resetTilt();
        }



        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            player.resetTilt();
            player.MoveUp();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            player.resetTilt();
            player.MoveDown();
        }


        private void GameOver()
        {
            gameRunning = false;
            gameTimer.Stop();
            spawnTimer.Stop();
            btnStart.Content = "Play Again?"; // Change button text to prompt user to play again
            lblCrashScore.Text = $"Score: {currentScore}";
            lblCrashScore.Visibility = Visibility.Visible;

            soundManager.StopEngineSound(); // Stop engine sound when game ends

            // Add new score and save it
            if (currentScore > 0)
            {
                scores.Add(currentScore);
                SaveScores();
            }

            // Clear obstacles
            foreach (var obstacle in obstacles)
            {
                GameCanvas.Children.Remove(obstacle._carImage);
            }

            obstacles.Clear();

            // Reset player position
            currentScore = 0;
            lblScore.Text = "0";
            Canvas.SetLeft(PlayerCar, CarStartLeft);
            Canvas.SetTop(PlayerCar, CarStartTop);
            player.resetTilt();

            // Re-enable scoreboard button
            btnScoreboard.IsEnabled = true;

            btnStart.IsEnabled = true;
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            gameRunning = true;
            btnStart.Content = "Playing..";
            btnStart.IsEnabled = false;
            lblCrashScore.Visibility = Visibility.Collapsed;
            currentScore = 0;
            lblScore.Text = "0";
            obstacles.Clear(); // Clear any previous obstacles before starting

            soundManager.PlayEngineSound(); // Start engine sound when game starts

            // Start the main game loop timer
            gameTimer.Start();

            // Start the obstacle spawning timer
            spawnTimer.Start();

            // Disable scoreboard button during gameplay
            btnScoreboard.IsEnabled = false;
        }


        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private async void SaveScores()
        {
            try
            {
                var storageFolder = ApplicationData.Current.LocalFolder;
                var file = await storageFolder.CreateFileAsync(ScoreFile, CreationCollisionOption.ReplaceExisting);

                var jsonArray = new JsonArray();
                foreach (var score in scores.OrderByDescending(s => s).Take(5))
                {
                    var jsonObject = new JsonObject();
                    jsonObject.SetNamedValue("PlayerName", JsonValue.CreateStringValue("Player"));
                    jsonObject.SetNamedValue("PlayerScore", JsonValue.CreateNumberValue(score));
                    jsonObject.SetNamedValue("Time", JsonValue.CreateStringValue(DateTime.Now.ToString("MM/dd/yyyy")));
                    jsonArray.Add(jsonObject);
                }

                await FileIO.WriteTextAsync(file, jsonArray.ToString());
                LoadScores();
            }
            catch (Exception ex)
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine($"Error saving scores: {ex}");
                #endif
            }
        }

        private class ScoreInfo
        {
            public int Score { get; set; }
            public string Time { get; set; }
        }

        private List<ScoreInfo> scoreInfos = new List<ScoreInfo>();

        private async void LoadScores()
        {
            try
            {
                var storageFolder = ApplicationData.Current.LocalFolder;
                var file = await storageFolder.GetFileAsync(ScoreFile);
                var json = await FileIO.ReadTextAsync(file);

                scores.Clear();
                scoreInfos.Clear();
                
                if (!string.IsNullOrEmpty(json))
                {
                    var jsonArray = JsonArray.Parse(json);
                    foreach (var item in jsonArray)
                    {
                        var obj = item.GetObject();
                        var score = (int)obj.GetNamedNumber("PlayerScore");
                        var time = obj.GetNamedString("Time", DateTime.Now.ToString("MM/dd/yyyy"));
                        scores.Add(score);
                        scoreInfos.Add(new ScoreInfo { Score = score, Time = time });
                    }
                }
            }
            catch (FileNotFoundException)
            {
                scores = new List<int>();
                scoreInfos = new List<ScoreInfo>();
            }
            catch (Exception ex)
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine($"Error loading scores: {ex}");
                #endif
                scores = new List<int>();
                scoreInfos = new List<ScoreInfo>();
            }

            UpdateScoreDisplay();
        }

        private void UpdateScoreDisplay()
        {
            lstScores.Items.Clear();
            
            var topScores = scoreInfos.OrderByDescending(s => s.Score).Take(5);
            int rank = 1;
            
            foreach (var score in topScores)
            {
                lstScores.Items.Add($"{rank}. {score.Score} ({score.Time})");
                rank++;
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
                case VirtualKey.A:
                    btnLeft_Click(null, null);
                    break;
                case VirtualKey.D:
                    btnRight_Click(null, null);
                    break;
                case VirtualKey.W:
                    player.resetTilt();
                    player.MoveUp();
                    break;
                case VirtualKey.S:
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

        private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Left || e.Key == VirtualKey.Right || e.Key == VirtualKey.A || e.Key == VirtualKey.D)
            {
                player.resetTilt(); // Reset tilt when key is released
            }
            e.Handled = true;
        }




        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus(FocusState.Programmatic); // Ensure the page gets keyboard focus
        }



        private async void btnScoreboard_Click(object sender, RoutedEventArgs e)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(typeof(ScoreBoard));
                Window.Current.Content = frame;
                Window.Current.Activate();
                newViewId = ApplicationView.GetForCurrentView().Id;
            });

            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }

        private void lstScores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
