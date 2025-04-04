using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;

namespace DrivingCar
{
    public class SpeedCar : Car
    {
        private const double SpeedMultiplier = 0.5; // ✅ 2x Faster
        private new Canvas _gameCanvas;
        


        public SpeedCar(string imagePath, int xPos, int yPos, double speed = DefaultSpeed * SpeedMultiplier,
                        int carWidth = DefaultWidth, int carHeight = DefaultHeight)
            : base(imagePath, xPos, yPos, speed, carWidth, carHeight)
        {
        }

        public override void AddMovingImage(Canvas gameCanvas)
        {
            _gameCanvas = gameCanvas;

            // Set initial YPos to the bottom of the canvas
            YPos = (int)(_gameCanvas.ActualHeight - CarHeight); // Starting at the bottom


            // Add SpeedCar Image
            base.AddMovingImage(_gameCanvas);

            
        }


        public override void StartAnimation(Action<Car> cleanupCallback, double _speed)
        {
            if (_gameCanvas == null || _carImage == null)
                return;

            // Set targetY to move upwards, which is off-screen at the top
            double targetY = -CarHeight - 450; // Moves off-screen at the top



            // Create Animation
            DoubleAnimation animation = new DoubleAnimation
            {
                From = YPos,  // Starts from the bottom
                To = targetY, // Moves to the top
                Duration = new Duration(TimeSpan.FromSeconds(_speed * SpeedMultiplier)),
                AutoReverse = false,
                RepeatBehavior = new RepeatBehavior(1)
            };

            // Apply Animation to Canvas.Top Property
            _storyboard = new Storyboard();
            Storyboard.SetTarget(animation, _carImage);
            Storyboard.SetTargetProperty(animation, "(Canvas.Top)");
            _storyboard.Children.Add(animation);

            // Remove SpeedCar and Warning Triangle when animation completes
            _storyboard.Completed += (s, e) =>
            {
                cleanupCallback?.Invoke(this);
                _gameCanvas.Children.Remove(_carImage);

                _carImage = null;
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
