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
        private readonly Canvas _gameCanvas; // Reference to the game canvas for rendering obstacles
        private readonly Random _random = new Random(); // Random generator for obstacle positioning and selection
        private readonly SoundManager _soundManager; // Reference to sound manager for playing effects

        public List<Car> ActiveCars { get; } = new List<Car>(); // List to track active obstacles

        // Difficulty control constants
        private const int checkpoint = 250; // Score interval to increase difficulty
        private int targetScore = 0; // Score threshold to trigger difficulty increase
        private int levelCount = 0; // Track difficulty level progression

        // Speed and spawn rate adjustments
        public double _levelInterval; // Current spawn interval
        public double _levelSpeed; // Current speed of obstacles
        public const double BaseSpeed = 8.0; // Initial base speed
        private double _speedMultiplier = 1.0; // Speed reduction factor per level
        public const double MinSpeed = 1.0; // Minimum allowed speed for obstacles

        public const double BaseSpawnInterval = 2.0; // Default spawn interval
        private double _timeIntervalMultiplier = 0.25; // Reduction factor for spawn interval
        public const double MinSpawnInterval = 0.25; // Minimum spawn interval

        private double _spawnIntervalMultiplier = 1.0; // Dynamic spawn rate modifier

        // Events triggered when cars are spawned or removed
        public event Action<Car> CarSpawned;
        public event Action<Car> CarRemoved;

        // Constructor initializes obstacle manager with game canvas and sound manager
        public Obstacle(Canvas gameCanvas, SoundManager soundManager)
        {
            _gameCanvas = gameCanvas ?? throw new ArgumentNullException(nameof(gameCanvas));
            _soundManager = soundManager;
        }

        // Adjusts difficulty based on the player's current score.
        // Difficulty increases every 250 points by adjusting obstacle speed and spawn frequency.
        public void UpdateDifficulty(int currentScore)
        {
            if (currentScore > targetScore)
            {
                // Adjust spawn interval, ensuring it does not drop below the minimum limit
                double calculatedInterval = BaseSpawnInterval - (_timeIntervalMultiplier * levelCount);
                _levelInterval = Math.Max(MinSpawnInterval, calculatedInterval);

                // Adjust speed, ensuring it does not drop below the minimum limit
                double calculatedSpeed = BaseSpeed - (_speedMultiplier * levelCount);
                _levelSpeed = Math.Max(MinSpeed, calculatedSpeed);

                levelCount++; // Increase difficulty level count
                targetScore += checkpoint; // Set next score milestone for difficulty increase

                try
                {
                    _soundManager?.PlayLevelUpSound(); // Play sound to indicate difficulty increase
                }
                catch { /* Silently handle sound errors */ }
            }
        }


        // Returns the current speed for obstacles based on difficulty progression.
        public double GetCurrentSpeed() => _levelSpeed;


        // Returns the current obstacle spawn interval.
        public double GetSpawnInterval() => _levelInterval;


        // Creates and adds a random obstacle to the game.
        public void SpawnRandomCar()
        {
            // Ensure game canvas has valid dimensions before spawning
            if (_gameCanvas.ActualWidth == 0 || _gameCanvas.ActualHeight == 0)
                return;

            try
            {
                int obstacleType = _random.Next(1, 5); // Randomly select obstacle type
                Car obstacle = CreateObstacle(obstacleType);

                if (obstacle != null)
                {
                    // Let the Car class handle its own animation and rendering
                    obstacle.AddMovingImage(_gameCanvas);
                    ActiveCars.Add(obstacle);

                    // Set initial position above the canvas
                    PositionCar(obstacle);

                    // Start animation and assign cleanup method upon completion
                    obstacle.StartAnimation((car) =>
                    {
                        CleanupObstacle(car);
                    }, GetCurrentSpeed());

                    CarSpawned?.Invoke(obstacle); // Trigger event for car spawn
                }
            }
            catch { /* Error handling */ }
        }

        /// Positions the obstacle randomly within canvas boundaries.
        private void PositionCar(Car car)
        {
            double carWidth = car._carImage.ActualWidth;
            double carHeight = car._carImage.ActualHeight;

            Canvas.SetLeft(car._carImage, _random.Next(45, 295)); // Place car randomly within canvas width
            Canvas.SetTop(car._carImage, -carHeight); // Start position above the canvas
        }


        /// Creates a car obstacle based on the given type.
        private Car CreateObstacle(int type)
        {
            int xPos = _random.Next(45, 295); // Generate random x-position

            switch (type)
            {
                case 1: return CreateCar("Assets/carObstacle1.png", xPos);
                case 2: return CreatePoliceCar(xPos);
                case 3: return CreateSpeedCar(xPos);
                case 4: return CreateCar("Assets/carObstacle2.png", xPos);
                default: return null;
            }
        }

        // Creates a standard car obstacle.
        private Car CreateCar(string imagePath, int xPos)
        {
            return new Car(imagePath, xPos, 0, GetCurrentSpeed());
        }


        // Creates a police car obstacle with a siren sound effect.
        private PoliceCar CreatePoliceCar(int xPos)
        {
            var policeCar = new PoliceCar("Assets/carPolice.png", xPos, 0, GetCurrentSpeed());
            try
            {
                _soundManager?.PlaySirenSound(); // Play police siren sound
            }
            catch { /* Silently handle sound errors */ }
            return policeCar;
        }


        // Creates a speed car, which moves faster than regular obstacles.
        private SpeedCar CreateSpeedCar(int xPos)
        {
            return new SpeedCar("Assets/speedCar.png", xPos, 380, GetCurrentSpeed() * 1.3);
        }


        
        // Cleans up an obstacle after it moves off-screen or collides.
        private void CleanupObstacle(Car obstacle)
        {
            try
            {
                // Remove the obstacle image from the game canvas
                if (obstacle._carImage != null && _gameCanvas.Children.Contains(obstacle._carImage))
                {
                    _gameCanvas.Children.Remove(obstacle._carImage);
                }

                ActiveCars.Remove(obstacle); // Remove from active list
                CarRemoved?.Invoke(obstacle); // Trigger car removal event

                // Stop siren sound if the removed obstacle was a police car
                if (obstacle is PoliceCar)
                {
                    _soundManager?.StopSirenSound();
                }
            }
            catch { /* Silent cleanup */ }
        }


        public void ClearAllCars()
        {
            foreach (var car in ActiveCars.ToArray()) // Convert list to array to avoid modification issues
            {
                CleanupObstacle(car);
            }
            ActiveCars.Clear(); // Ensure all obstacles are removed
        }
    }
}
