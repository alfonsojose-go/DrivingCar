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
        private Obstacle obstacleManager;
        private Random random;
        private ApplicationView scoreboardView;
        private const string ScoreFile = "scores.json";

        private SoundManager soundManager; // Sound Manager instance

        public MainPage()
        {
            this.InitializeComponent();
            scores = new List<int>();
            random = new Random();
            LoadScores();

            player = new Player(PlayerCar);
            soundManager = new SoundManager();
            obstacleManager = new Obstacle(GameCanvas, soundManager); // Initialize obstacle manager

            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            gameTimer.Tick += GameLoop;

            spawnTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            spawnTimer.Tick += SpawnObstacleTimed;

            lblCrashScore.Visibility = Visibility.Collapsed;
        }

        private void SpawnObstacleTimed(object sender, object e)
        {
            if (gameRunning)
            {
                obstacleManager.SpawnRandomCar();
                spawnTimer.Interval = TimeSpan.FromSeconds(obstacleManager.GetSpawnInterval());
            }
        }




        private void GameLoop(object sender, object e)
        {
            if (!gameRunning) return;

            // Update score and difficulty
            currentScore++;
            lblScore.Text = currentScore.ToString();
            obstacleManager.UpdateDifficulty(currentScore);

            // Check for collisions
            foreach (var obstacle in obstacleManager.ActiveCars)
            {
                if (player.CheckCrash(obstacle))
                {
                    if (obstacle is PoliceCar)
                    {
                        soundManager.StopSirenSound();
                    }
                    soundManager.PlayCrashSound();
                    GameOver();
                    return;
                }
            }
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

            btnStart.Content = "Play Again?";
            lblCrashScore.Text = $"Score: {currentScore}";
            lblCrashScore.Visibility = Visibility.Visible;

            soundManager.StopEngineSound();
            obstacleManager.ClearAllCars(); // Clear all obstacles

            if (currentScore > 0)
            {
                scores.Add(currentScore);
                SaveScores();
            }

            currentScore = 0;
            lblScore.Text = "0";
            Canvas.SetLeft(PlayerCar, CarStartLeft);
            Canvas.SetTop(PlayerCar, CarStartTop);
            player.resetTilt();
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

            obstacleManager.ClearAllCars(); // Clear previous obstacles
            soundManager.PlayEngineSound();

            gameTimer.Start();
            spawnTimer.Start();
            spawnTimer.Interval = TimeSpan.FromSeconds(obstacleManager.GetSpawnInterval());

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
                foreach (var score in scores.OrderByDescending(s => s).Take(10))
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
            
            var topScores = scoreInfos.OrderByDescending(s => s.Score).Take(3);
            int rank = 1;
            
            foreach (var score in topScores)
            {
                lstScores.Items.Add($"{rank}. {score.Score} | {score.Time}");
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
                    btnUp_Click(null, null);
                    break;
                case VirtualKey.Down:
                    btnDown_Click(null, null);
                    break;
                case VirtualKey.A:
                    btnLeft_Click(null, null);
                    break;
                case VirtualKey.D:
                    btnRight_Click(null, null);
                    break;
                case VirtualKey.W:
                    btnUp_Click(null, null);
                    break;
                case VirtualKey.S:
                    btnDown_Click(null, null);
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
