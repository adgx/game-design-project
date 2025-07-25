using System.Collections.Generic;
using Animations;
using UnityEngine;

namespace Audio
{
    public class MaynardAudioManager : MonoBehaviour
    {
        public static MaynardAudioManager Instance { get; private set; }
    
        // List of all active MaynardEvents scripts
        private List<MaynardEvents> allMaynards = new List<MaynardEvents>();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Register(MaynardEvents maynard)
        {
            if (!allMaynards.Contains(maynard))
            {
                allMaynards.Add(maynard);
            }
        }

        public void Unregister(MaynardEvents maynard)
        {
            if (allMaynards.Contains(maynard))
            {
                allMaynards.Remove(maynard);
            }
        }

        public void PauseAllMaynardSounds()
        {
            foreach (var maynard in allMaynards)
            {
                maynard.PauseLoopingSounds();
            }
        }

        public void ResumeAllMaynardSounds()
        {
            foreach (var maynard in allMaynards)
            {
                maynard.ResumeLoopingSounds();
            }
        }
    }
}