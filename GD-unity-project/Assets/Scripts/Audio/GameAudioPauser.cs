using Animations;

namespace Audio
{
    public static class GameAudioPauser
    {
        /// <summary>
        /// Pauses all game sounds (optionally excluding music).
        /// </summary>
        /// <param name="pauseMusic">If true, the music will also be paused.</param>
        public static void PauseGameAudio(bool pauseMusic = true)
        {
            // Player
            if (Player.Instance != null)
            {
                RickEvents rickEvents = Player.Instance.GetComponent<RickEvents>();
                if (rickEvents != null)
                {
                    rickEvents.PauseAllRickSounds();
                }
            }
            
            if (GamePlayAudioManager.instance != null)
            {
                // Player's one-shot sounds
                GamePlayAudioManager.instance.PauseAllManagedOneShots();
                
                // Music (conditional)
                if (pauseMusic)
                {
                    GamePlayAudioManager.instance.PauseMusic();
                }
            }

            // Ambience
            AmbienceSystem.PauseAllRoomAmbience();

            // Enemies
            IncognitoAudioManager.Instance.PauseAllIncognitoSounds();
            MaynardAudioManager.Instance.PauseAllMaynardSounds();
            DrakeAudioManager.Instance.PauseAllDrakeSounds();
        }

        /// <summary>
        /// Shoots all game sounds (optionally excluding music).
        /// </summary>
        /// <param name="resumeMusic">Se true, anche la musica verrà ripresa.</param>
        public static void ResumeGameAudio(bool resumeMusic = true)
        {
            // Player
            if (Player.Instance != null)
            {
                RickEvents rickEvents = Player.Instance.GetComponent<RickEvents>();
                if (rickEvents != null)
                {
                    rickEvents.ResumeAllRickSounds();
                }
            }

            if (GamePlayAudioManager.instance != null)
            {
                // Player's one-shot sounds
                GamePlayAudioManager.instance.ResumeAllManagedOneShots();
                
                // Music (conditional)
                if (resumeMusic)
                {
                    GamePlayAudioManager.instance.ResumeMusic();
                }

            }
            
            // Ambience
            AmbienceSystem.ResumeAllRoomAmbience();

            // Enemies
            IncognitoAudioManager.Instance.ResumeAllIncognitoSounds();
            MaynardAudioManager.Instance.ResumeAllMaynardSounds();
            DrakeAudioManager.Instance.ResumeAllDrakeSounds();
        }
    }
}