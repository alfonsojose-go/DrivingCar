using Windows.Media.Core;
using Windows.Media.Playback;
using System;

namespace DrivingCar
{
    public class SoundManager
    {
        private MediaPlayer enginePlayer;
        private MediaPlayer sirenPlayer;
        private MediaPlayer crashPlayer;
        private MediaPlayer levelUpPlayer;

        public SoundManager()
        {
            enginePlayer = new MediaPlayer();
            sirenPlayer = new MediaPlayer();
            crashPlayer = new MediaPlayer();
            levelUpPlayer = new MediaPlayer();

            // Set crash sound source once
            crashPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/explosion.wav"));
            levelUpPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/levelup.wav")); // Add level-up sound
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
            sirenPlayer.Pause();
        }

        public void PlayCrashSound()
        {
            crashPlayer.Play();
        }

        public void PlayLevelUpSound() // Method to play level-up sound
        {
            levelUpPlayer.Play();
        }
    }
}
