using System;
using System.ServiceModel.Channels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DrivingCar
{
    public class Player
    {
        public Image PlayerCar { get; private set; } // The player's car represented as a UI Image

        // Constants to define movement behavior and screen boundaries
        private const double CarMoveDistance = 10;
        private const double LeftBoundary = 45;
        private const double RightBoundary = 300;
        private const double TopBoundary = 0;
        private const double BottomBoundary = 324;
        private const double CarStartLeft = 171;
        private const double CarStartTop = 228;

        // Constructor initializes the player's car image and sets its initial position if undefined
        public Player(Image car)
        {
            PlayerCar = car;

            if (double.IsNaN(Canvas.GetLeft(PlayerCar)))
            {
                Canvas.SetLeft(PlayerCar, CarStartLeft); // Set default horizontal position
            }

            if (double.IsNaN(Canvas.GetTop(PlayerCar)))
            {
                Canvas.SetTop(PlayerCar, CarStartTop); // Set default vertical position
            }
        }


        // Moves the car horizontally by the specified distance while respecting boundaries.
        public void MoveCar(double distance)
        {
            double currentLeft = Canvas.GetLeft(PlayerCar);

            if (double.IsNaN(currentLeft))
            {
                currentLeft = CarStartLeft; // Fallback if position is uninitialized
            }

            double newLeft = currentLeft + distance;
            newLeft = Math.Max(LeftBoundary, Math.Min(RightBoundary, newLeft)); // Clamp within boundaries
            Canvas.SetLeft(PlayerCar, newLeft);
        }

        // Moves car left or right using the default movement distance
        public void MoveLeft() => MoveCar(-CarMoveDistance);
        public void MoveRight() => MoveCar(CarMoveDistance);


        // Moves the car vertically by the specified distance while respecting boundaries.
        public void MoveVertical(double distance)
        {
            double currentTop = Canvas.GetTop(PlayerCar);

            if (double.IsNaN(currentTop))
            {
                currentTop = CarStartTop; // Fallback if position is uninitialized
            }

            double newTop = currentTop + distance;
            newTop = Math.Max(TopBoundary, Math.Min(BottomBoundary, newTop)); // Clamp within boundaries
            Canvas.SetTop(PlayerCar, newTop);
        }

        // Moves car up or down using the default movement distance
        public void MoveUp() => MoveVertical(-CarMoveDistance);
        public void MoveDown() => MoveVertical(CarMoveDistance);


        // Tilts the car to simulate turning effect using rotation.
        public void tiltCar(double angle)
        {
            RotateTransform rotateTransform = new RotateTransform
            {
                Angle = angle
            };

            PlayerCar.RenderTransform = rotateTransform;
        }

        // Quick tilt control shortcuts
        public void tiltLeft() => tiltCar(-15);
        public void tiltRight() => tiltCar(15);
        public void resetTilt() => tiltCar(0);


        // Checks whether the player's car has collided with an obstacle.
        public bool CheckCrash(Car obstacle)
        {
            // Get the player's position and half dimensions
            double playerLeft = Canvas.GetLeft(PlayerCar);
            double playerTop = Canvas.GetTop(PlayerCar);
            double playerRight = playerLeft + PlayerCar.Width / 2;
            double playerBottom = playerTop + PlayerCar.Height / 2;

            // Get the obstacle's position and half dimensions
            double obstacleLeft = Canvas.GetLeft(obstacle._carImage);
            double obstacleTop = Canvas.GetTop(obstacle._carImage);
            double obstacleRight = obstacleLeft + obstacle._carImage.Width / 2;
            double obstacleBottom = obstacleTop + obstacle._carImage.Height / 2;

            // Optional buffer to make collision slightly forgiving
            double collisionBuffer = 0.5;

            // Axis-Aligned Bounding Box (AABB) collision detection
            bool isCollision = !(playerRight + collisionBuffer < obstacleLeft ||
                                 playerLeft - collisionBuffer > obstacleRight ||
                                 playerBottom + collisionBuffer < obstacleTop ||
                                 playerTop - collisionBuffer > obstacleBottom);

            return isCollision;
        }

        // Resets the player's car to the starting position and shows a game over dialog.
        public void Crash()
        {
            // Reset position to starting coordinates
            Canvas.SetLeft(PlayerCar, CarStartLeft);
            Canvas.SetTop(PlayerCar, CarStartTop);

            // Remove any tilt effect
            resetTilt();

            // Display Game Over dialog to the player
            ContentDialog gameOverDialog = new ContentDialog
            {
                Title = "Game Over",
                Content = "You crashed!",
                CloseButtonText = "OK"
            };

        }


        // Checks for a crash with the given car and handles crash logic if one occurred.
        public void CheckAndHandleCrash(Car otherCar)
        {
            if (CheckCrash(otherCar))
            {
                Crash(); // Trigger crash behavior if collision detected
            }
        }
    }
}
