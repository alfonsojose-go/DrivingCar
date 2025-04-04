using Windows.Media.Core;
using Windows.Media.Playback;
using System;

namespace DrivingCar
{
    public class SoundManager
    {
        // MediaPlayer instances for different game sound effects
        private MediaPlayer enginePlayer;   // Background engine sound
        private MediaPlayer sirenPlayer;   // Police siren sound
        private MediaPlayer crashPlayer;   // Explosion/crash sound
        private MediaPlayer levelUpPlayer; // Level-up sound

        // Constructor initializes MediaPlayer objects and sets static sound sources
        public SoundManager()
        {
            enginePlayer = new MediaPlayer();
            sirenPlayer = new MediaPlayer();
            crashPlayer = new MediaPlayer();
            levelUpPlayer = new MediaPlayer();

            // Set static sound sources for crash and level-up
            crashPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/explosion.wav"));
            levelUpPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/levelup.wav"));
        }


        // Plays the looping engine sound from a file.
        public void PlayEngineSound()
        {
            enginePlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/drivingsound.wav"));
            enginePlayer.IsLoopingEnabled = true; // Loop the sound continuously
            enginePlayer.Play();
        }

        // Stops the engine sound.
        public void StopEngineSound()
        {
            enginePlayer.Pause(); // Pause instead of Stop to allow resume later if needed
        }

        // Plays the police siren sound.
        public void PlaySirenSound()
        {
            sirenPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/policesiren.wav"));
            sirenPlayer.Play();
        }

        // Stops the police siren sound.
        public void StopSirenSound()
        {
            sirenPlayer.Pause(); // Pause to retain source
        }

        // Plays the crash/explosion sound.
        public void PlayCrashSound()
        {
            crashPlayer.Play();
        }

        // Plays the level-up sound when score reaches a milestone.
        public void PlayLevelUpSound()
        {
            levelUpPlayer.Play();
        }
    }
}
