using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace DrivingCar
{
    public class Obstacle
    {
        private readonly Canvas _gameCanvas;
        private readonly Random _random = new Random();
        private readonly SoundManager _soundManager;
        public List<Car> ActiveCars { get; } = new List<Car>();

        // Difficulty parameters (using your exact values)
        private const int checkpoint = 250;
        private int targetScore = 0;
        private int levelCount = 0;
        public double _levelInterval;
        public double _levelSpeed;
        public const double BaseSpeed = 8.0;
        private double _speedMultiplier = 1.0;
        public const double MinSpeed = 1.0;
        public const double BaseSpawnInterval = 2.0;
        private double _timeIntervalMultiplier = 0.25;
        public const double MinSpawnInterval = 0.25;
        private int _currentDifficultyTier = 0;
       
        private double _spawnIntervalMultiplier = 1.0;

        public event Action<Car> CarSpawned;
        public event Action<Car> CarRemoved;

        public Obstacle(Canvas gameCanvas, SoundManager soundManager)
        {
            _gameCanvas = gameCanvas ?? throw new ArgumentNullException(nameof(gameCanvas));
            _soundManager = soundManager;
        }

        public void UpdateDifficulty(int currentScore)
        {
            // Difficulty increases every 250 points
            
            
            if (currentScore > targetScore)
            {
            
                double calculatedInterval = BaseSpawnInterval - (_timeIntervalMultiplier * levelCount);
                _levelInterval = Math.Max(MinSpawnInterval, calculatedInterval);

                double calculatedSpeed = BaseSpeed - (_speedMultiplier * levelCount);
                _levelSpeed = Math.Max(MinSpeed, calculatedSpeed);

                levelCount++;
                targetScore += checkpoint;


                try
                {
                    _soundManager?.PlayLevelUpSound();
                    
                }
                catch { /* Silently handle sound errors */ }

                
            }

            
        }


        public double GetCurrentSpeed() => _levelSpeed;

        public double GetSpawnInterval() => _levelInterval;

        public void SpawnRandomCar()
        {
            if (_gameCanvas.ActualWidth == 0 || _gameCanvas.ActualHeight == 0)
                return;

     

            try
            {
                int obstacleType = _random.Next(1, 5);
                Car obstacle = CreateObstacle(obstacleType);

                if (obstacle != null)
                {
                    // Let the Car class handle its own animation
                    obstacle.AddMovingImage(_gameCanvas);
                    ActiveCars.Add(obstacle);

                    // Set initial position (above canvas)
                    PositionCar(obstacle);

                    // Pass cleanup callback to Car
                    obstacle.StartAnimation((car) =>
                    {
                        CleanupObstacle(car);
                    }, GetCurrentSpeed());

                    CarSpawned?.Invoke(obstacle);
                }
            }
            catch { /* Error handling */ }
        }

        private void PositionCar(Car car)
        {
            double carWidth = car._carImage.ActualWidth;
            double carHeight = car._carImage.ActualHeight;

            Canvas.SetLeft(car._carImage, _random.Next(45, 295)); // Canvas boundary
            Canvas.SetTop(car._carImage, -carHeight); // Start above canvas
        }

        private Car CreateObstacle(int type)
        {
            int xPos = _random.Next(45, 295);

            switch (type)
            {
                case 1: return CreateCar("Assets/carObstacle1.png", xPos);
                case 2: return CreatePoliceCar(xPos);
                case 3: return CreateSpeedCar(xPos);
                case 4: return CreateCar("Assets/carObstacle2.png", xPos);
                default: return null;
            }
        }

        private Car CreateCar(string imagePath, int xPos)
        {
            return new Car(imagePath, xPos, 0, GetCurrentSpeed());
        }

        private PoliceCar CreatePoliceCar(int xPos)
        {
            var policeCar = new PoliceCar("Assets/carPolice.png", xPos, 0, GetCurrentSpeed());
            try
            {
                _soundManager?.PlaySirenSound();
            }
            catch { /* Silently handle sound errors */ }
            return policeCar;
        }

        private SpeedCar CreateSpeedCar(int xPos)
        {
            return new SpeedCar("Assets/speedCar.png", xPos, 380, GetCurrentSpeed() * 1.3);
        }


        private void CleanupObstacle(Car obstacle)
        {
            try
            {
                if (obstacle._carImage != null && _gameCanvas.Children.Contains(obstacle._carImage))
                {
                    _gameCanvas.Children.Remove(obstacle._carImage);
                }

                ActiveCars.Remove(obstacle);
                CarRemoved?.Invoke(obstacle);

                if (obstacle is PoliceCar)
                {
                    _soundManager?.StopSirenSound();
                }
            }
            catch { /* Silent cleanup */ }
        }

        public void ClearAllCars()
        {
            foreach (var car in ActiveCars.ToArray()) // ToArray() for safe iteration
            {
                CleanupObstacle(car);
            }
            ActiveCars.Clear();
        }

    }
}