using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DrivingCar
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Add key down event handler
            this.KeyDown += MainPage_KeyDown;

            // Ensure the page can receive keyboard focus
            this.Loaded += (s, e) => this.Focus(FocusState.Programmatic);
        }

        private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Check what the key is pressed
            switch (e.Key)
            {
                case VirtualKey.Enter:
                    btnStart_Click(btnStart, null);
                    e.Handled = true;
                    break;

                case VirtualKey.Escape:
                    btnExit_Click(btnExit, null);
                    e.Handled = true;
                    break;

                case VirtualKey.Left:
                    btnLeft_Click(btnLeft, null);
                    e.Handled = true;
                    break;

                case VirtualKey.Right:
                    btnRight_Click(btnRight, null);
                    e.Handled = true;
                    break;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.Content = "Playing...";
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private async void btnRight_Click(object sender, RoutedEventArgs e)
        {

            //test code. Put code here to move the car to the right
            var dialog = new ContentDialog
            {
                Title = "Movement",
                Content = "Right button clicked",
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }

        private async void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            //test code. Put code here to move the car to the left
            var dialog = new ContentDialog
            {
                Title = "Movement",
                Content = "Left button clicked",
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }
    }
}
