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
        private ObservableCollection<Score> scores;
        private readonly DispatcherTimer timer;
        private const string ScoreFile = "scores.json";

        public ScoreBoard()
        {
            InitializeComponent();
            scores = new ObservableCollection<Score>();
            ScoreListView.ItemsSource = scores;
            
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateScores();
            timer.Start();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            timer.Stop();
        }

        private void Timer_Tick(object sender, object e) => UpdateScores();

        private async void UpdateScores()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(ScoreFile);
                var json = await FileIO.ReadTextAsync(file);
                
                if (string.IsNullOrEmpty(json)) return;

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
                .OrderByDescending(s => s.Value)
                .Take(10)
                .Select((score, index) =>
                {
                    score.Rank = index + 1;
                    return score;
                })
                .ToList();

                var hasChanges = scores.Count != newScores.Count ||
                                newScores.Where((t, i) => !t.Equals(scores[i])).Any();

                if (hasChanges)
                {
                    scores.Clear();
                    foreach (var score in newScores)
                    {
                        scores.Add(score);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // First run, no scores yet
            }
            catch (Exception)
            {
                #if DEBUG
                throw;
                #endif
            }
        }

        public class Score : IEquatable<Score>
        {
            public string Time { get; set; }
            public int Value { get; set; }
            public int Rank { get; set; }

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
