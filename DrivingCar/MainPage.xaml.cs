using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace DrivingCar
{
    public sealed partial class MainPage : Page
    {
        // Car movement properties
        private const double CarMoveDistance = 10; // Pixels to move per click
        private const double LeftBoundary = 50;    // Left boundary (match your road position)
        private const double RightBoundary = 300;  // Right boundary (road width - car width)
       

        public MainPage()
        {
            this.InitializeComponent();
        }


        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            MoveCar(-CarMoveDistance); // Move left
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            MoveCar(CarMoveDistance); // Move right
        }

        private void MoveCar(double distance)
        {
            // Get current position
            double currentLeft = Canvas.GetLeft(PlayerCar);

            // Calculate new position
            double newLeft = currentLeft + distance;

            // Check boundaries (so car doesn't go off-road)
            if (newLeft < LeftBoundary)
                newLeft = LeftBoundary;
            else if (newLeft > RightBoundary)
                newLeft = RightBoundary;

            // Set new position
            Canvas.SetLeft(PlayerCar, newLeft);
        }

        
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.Content = "Playing..";
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }


        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //if (!_gameRunning) return;

            switch (e.Key)
            {
                case VirtualKey.Left:
                    btnLeft_Click(null, null);
                    break;
                case VirtualKey.Right:
                    btnRight_Click(null, null);
                    break;
                case VirtualKey.Up:
                case VirtualKey.Down:
                    
                    
                    break;
            }
            e.Handled = true;
        }
    }
}