using System.Collections.Generic;
using Animations;
using UnityEngine;

namespace Audio
{
    public class DrakeAudioManager : MonoBehaviour
    {
        public static DrakeAudioManager Instance { get; private set; }
    
        // Lista di tutti gli script DrakeEvents attivi
        private List<DrakeEvents> allDrakes = new List<DrakeEvents>();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Register(DrakeEvents drake)
        {
            if (!allDrakes.Contains(drake))
            {
                allDrakes.Add(drake);
            }
        }

        public void Unregister(DrakeEvents drake)
        {
            if (allDrakes.Contains(drake))
            {
                allDrakes.Remove(drake);
            }
        }

        public void PauseAllDrakeSounds()
        {
            foreach (var drake in allDrakes)
            {
                drake.PauseAllSounds();
            }
        }

        public void ResumeAllDrakeSounds()
        {
            foreach (var drake in allDrakes)
            {
                drake.ResumeAllSounds();
            }
        }
    }
}