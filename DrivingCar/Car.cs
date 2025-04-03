using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media;

namespace DrivingCar
{
    public class Car
    {
        public const int DefaultWidth = 56;
        public const int DefaultHeight = 74;
        public const double DefaultSpeed = 8.0;

        public int XPos { get; set; }
        public int YPos { get; set; }
        public double Speed { get; set; }
        public int CarWidth { get; }
        public int CarHeight { get; }
        public string ImagePath { get; }

        public Image _carImage;
        public Canvas _gameCanvas;
        public Storyboard _storyboard;

        public Car(string imagePath, int xPos, int yPos, double speed = DefaultSpeed, int carWidth = DefaultWidth, int carHeight = DefaultHeight)
        {
            ImagePath = imagePath;
            XPos = xPos;
            YPos = yPos;
            Speed = speed;
            CarWidth = carWidth;
            CarHeight = carHeight;
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
