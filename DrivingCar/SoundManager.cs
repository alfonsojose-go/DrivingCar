using Windows.Media.Core;
using Windows.Media.Playback;
using System;

namespace DrivingCar
{
    public class SoundManager
    {
        private MediaPlayer enginePlayer;
        private MediaPlayer sirenPlayer;

        public SoundManager()
        {
            enginePlayer = new MediaPlayer();
            sirenPlayer = new MediaPlayer();
        }

        public void PlayEngineSound()
        {
            enginePlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/drivingsound.wav"));
            enginePlayer.IsLoopingEnabled = true;
            enginePlayer.Play();
        }

        public void StopEngineSound()
        {
            enginePlayer.Pause();
        }

        public void PlaySirenSound()
        {
            sirenPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/policesiren.wav"));
            sirenPlayer.Play();
        }

        public void StopSirenSound()
        {
            sirenPlayer.Pause(); // Stops the siren sound when the car disappears
        }

    }
}
