using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;

namespace DrivingCar
{
    public sealed partial class ScoreBoard : Page
    {
        // Observable collection bound to the UI to display the list of scores
        private ObservableCollection<Score> scores;

        // Timer to periodically refresh the scoreboard
        private readonly DispatcherTimer timer;

        // Filename for storing/retrieving scores
        private const string ScoreFile = "scores.json";

        public ScoreBoard()
        {
            InitializeComponent();

            // Initialize the scores collection and bind to UI ListView
            scores = new ObservableCollection<Score>();
            ScoreListView.ItemsSource = scores;

            // Initialize the timer and set tick interval to 1 second
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
        }

        // Called when the page is navigated to
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UpdateScores(); // Load scores immediately
            timer.Start();  // Start the periodic update timer
        }

        // Called when the page is navigated away from
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            timer.Stop(); // Stop the timer to save resources
        }

        // Timer tick event handler: refresh scores every second
        private void Timer_Tick(object sender, object e) => UpdateScores();

        // Loads scores from the local file and updates the UI
        private async void UpdateScores()
        {
            try
            {
                // Try to access the saved scores file
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(ScoreFile);
                var json = await FileIO.ReadTextAsync(file);

                if (string.IsNullOrEmpty(json)) return;

                // Parse JSON array of score entries
                var jsonArray = JsonArray.Parse(json);
                var newScores = jsonArray.Select(token =>
                {
                    var obj = token.GetObject();
                    return new Score
                    {
                        Value = (int)obj.GetNamedNumber("PlayerScore"),
                        Time = obj.GetNamedString("Time", DateTime.Now.ToString("MM/dd/yyyy"))
                    };
                })
                .OrderByDescending(s => s.Value) // Sort scores descending
                .Take(10) // Keep top 10 scores
                .Select((score, index) =>
                {
                    score.Rank = index + 1; // Assign rank
                    return score;
                })
                .ToList();

                // Check if there are any changes to avoid unnecessary UI updates
                var hasChanges = scores.Count != newScores.Count ||
                                newScores.Where((t, i) => !t.Equals(scores[i])).Any();

                if (hasChanges)
                {
                    scores.Clear();
                    foreach (var score in newScores)
                    {
                        scores.Add(score); // Update UI-bound collection
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // No scores file exists yet – probably the first run
            }
            catch (Exception)
            {
                #if DEBUG
                throw; // Re-throw in debug mode for troubleshooting
                #endif
            }
        }

        // Inner class representing a score entry
        public class Score : IEquatable<Score>
        {
            public string Time { get; set; }  // Time the score was achieved
            public int Value { get; set; }    // Numeric score value
            public int Rank { get; set; }     // Position on the leaderboard

            // Compare two Score objects for value-based equality
            public bool Equals(Score other)
            {
                if (other == null) return false;
                return Value == other.Value &&
                       Rank == other.Rank &&
                       Time == other.Time;
            }
        }
    }
}
