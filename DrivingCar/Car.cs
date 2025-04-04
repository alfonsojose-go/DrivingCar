// Import required namespaces
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media;

namespace DrivingCar
{
    // Base class representing a car in the game
    public class Car
    {
        public const int DefaultWidth = 56;
        public const int DefaultHeight = 74;
        public const double DefaultSpeed = 8.0;

        public int XPos { get; set; } // X position of the car on the canvas
        public int YPos { get; set; } // Y position of the car on the canvas
        public double Speed { get; set; } // Movement speed of the ca
        public int CarWidth { get; } // Width of the car image
        public int CarHeight { get; } // Height of the car image
        public string ImagePath { get; } // Path to the car image asset

        public Image _carImage; // The car's Image control on the UI
        public Canvas _gameCanvas; // The game canvas where the car is rendered
        public Storyboard _storyboard; // Optional storyboard for car animation


        // Constructor to initialize a car with position, size, speed, and image path
        public Car(string imagePath, int xPos, int yPos, double speed = DefaultSpeed, int carWidth = DefaultWidth, int carHeight = DefaultHeight)
        {
            ImagePath = imagePath; // Set image path
            XPos = xPos; // Set starting X position
            YPos = yPos; // Set starting Y position
            Speed = speed; // Set movement speed
            CarWidth = carWidth; // Set width
            CarHeight = carHeight; // Set height
        }

        public virtual void AddMovingImage(Canvas gameCanvas)
        {
            _gameCanvas = gameCanvas;

            // Set Clip to hide anything outside the canvas
            gameCanvas.Clip = new RectangleGeometry
            {
                Rect = new Windows.Foundation.Rect(0, 0, gameCanvas.ActualWidth, gameCanvas.ActualHeight)
            };

            // Create Image
            _carImage = new Image
            {
                Width = CarWidth,
                Height = CarHeight
            };

            // Load Image Source
            string fullPath = $"ms-appx:///{ImagePath.TrimStart('/')}";
            BitmapImage bitmap = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
            _carImage.Source = bitmap;

            // Set initial position
            Canvas.SetLeft(_carImage, XPos);
            Canvas.SetTop(_carImage, YPos);

            // Add to Canvas
            _gameCanvas.Children.Add(_carImage);

            // Start Moving Animation
            //StartAnimation();
        }

        public virtual void StartAnimation(Action<Car> cleanupCallback, double _speed)
        {
            if (_gameCanvas == null || _carImage == null)
                return;

            double targetHeight = _gameCanvas.ActualHeight > 0 ? _gameCanvas.ActualHeight : 500;
            double beyondCanvas = targetHeight + CarHeight + 500; // Move beyond canvas

            // Create Animation
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = beyondCanvas,
                Duration = new Duration(TimeSpan.FromSeconds(_speed)),
                AutoReverse = false,
                RepeatBehavior = new RepeatBehavior(1)
            };

            // Apply Animation to Canvas.Top Property
            _storyboard = new Storyboard();
            Storyboard.SetTarget(animation, _carImage);
            Storyboard.SetTargetProperty(animation, "(Canvas.Top)");
            _storyboard.Children.Add(animation);

            // Remove the car when animation completes
            _storyboard.Completed += (s, e) =>
            {
                cleanupCallback?.Invoke(this);
                _gameCanvas.Children.Remove(_carImage);
                _carImage = null; // Prevent memory leaks
            };

            // Start Animation on UI Thread
            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    _storyboard.Begin();
                });
        }
    }
}
