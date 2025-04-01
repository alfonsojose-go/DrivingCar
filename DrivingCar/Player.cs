using System;
using System.ServiceModel.Channels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DrivingCar
{
    public class Player
    {
        public Image PlayerCar { get; private set; }
        private const double CarMoveDistance = 10;
        private const double LeftBoundary = 45;
        private const double RightBoundary = 300;
        private const double TopBoundary = 0;
        private const double BottomBoundary = 324;
        private const double CarStartLeft = 170;
        private const double CarStartTop = 324;

        public Player(Image car)
        {
            PlayerCar = car;

            if (double.IsNaN(Canvas.GetLeft(PlayerCar)))
            {
                Canvas.SetLeft(PlayerCar, CarStartLeft);
            }

            if (double.IsNaN(Canvas.GetTop(PlayerCar)))
            {
                Canvas.SetTop(PlayerCar, CarStartTop);
            }
        }

        public void MoveCar(double distance)
        {
            double currentLeft = Canvas.GetLeft(PlayerCar);

            if (double.IsNaN(currentLeft))
            {
                currentLeft = CarStartLeft;
            }

            double newLeft = currentLeft + distance;
            newLeft = Math.Max(LeftBoundary, Math.Min(RightBoundary, newLeft));
            Canvas.SetLeft(PlayerCar, newLeft);
        }

        public void MoveLeft() => MoveCar(-CarMoveDistance);
        public void MoveRight() => MoveCar(CarMoveDistance);

        public void MoveVertical(double distance)
        {
            double currentTop = Canvas.GetTop(PlayerCar);

            if (double.IsNaN(currentTop))
            {
                currentTop = CarStartTop;
            }

            double newTop = currentTop + distance;
            newTop = Math.Max(TopBoundary, Math.Min(BottomBoundary, newTop));
            Canvas.SetTop(PlayerCar, newTop);
        }

        public void MoveUp() => MoveVertical(-CarMoveDistance);
        public void MoveDown() => MoveVertical(CarMoveDistance);

        public void tiltCar(double angle)
        {
            RotateTransform rotateTransform = new RotateTransform
            {
                Angle = angle
            };

            PlayerCar.RenderTransform = rotateTransform;
        }

        public void tiltLeft() => tiltCar(-15);
        public void tiltRight() => tiltCar(15);
        public void resetTilt() => tiltCar(0);

        // Check for crash with another car
        public bool CheckCrash(Car otherCar)
        {
            double playerLeft = Canvas.GetLeft(PlayerCar);
            double playerTop = Canvas.GetTop(PlayerCar);
            double otherLeft = Canvas.GetLeft(otherCar._carImage);
            double otherTop = Canvas.GetTop(otherCar._carImage);

            double crashThreshold = 30;

            double distance = Math.Sqrt(Math.Pow(playerLeft - otherLeft, 2) + Math.Pow(playerTop - otherTop, 2));
            return distance < crashThreshold;
        }

        // Handle crash logic (reset player's car position and stop the game)
        public void Crash()
        {
            // Reset car position
            Canvas.SetLeft(PlayerCar, CarStartLeft);
            Canvas.SetTop(PlayerCar, CarStartTop);

            // Reset tilt to 0
            resetTilt();

            // Handle game over logic (e.g., notify user, stop the game)
            
                ContentDialog gameOverDialog = new ContentDialog
                {
                    Title = "Game Over",
                    Content = "You crashed!",
                    CloseButtonText = "OK"
                };

        

            // You might want to trigger other actions like stopping timers, etc.
            // Optionally, raise a game over event to handle in the main game loop
        }

        // Method to check for crash and handle game over
        public void CheckAndHandleCrash(Car otherCar)
        {
            if (CheckCrash(otherCar))
            {
                Crash();  // Trigger the crash logic
            }
        }
    }
}
