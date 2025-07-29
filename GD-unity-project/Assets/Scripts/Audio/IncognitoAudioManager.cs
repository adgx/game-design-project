using System.Collections.Generic;
using Animations;
using UnityEngine;

namespace Audio
{
    public class IncognitoAudioManager : MonoBehaviour
    {
        public static IncognitoAudioManager Instance { get; private set; }
    
        // List of all active IncognitoEvents scripts
        private List<IncognitoEvents> allIncognitos = new List<IncognitoEvents>();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Register(IncognitoEvents incognito)
        {
            if (!allIncognitos.Contains(incognito))
            {
                allIncognitos.Add(incognito);
            }
        }

        public void Unregister(IncognitoEvents incognito)
        {
            if (allIncognitos.Contains(incognito))
            {
                allIncognitos.Remove(incognito);
            }
        }

        public void PauseAllIncognitoSounds()
        {
            foreach (var incognito in allIncognitos)
            {
                incognito.PauseAllSounds();
            }
        }

        public void ResumeAllIncognitoSounds()
        {
            foreach (var incognito in allIncognitos)
            {
                incognito.ResumeAllSounds();
            }
        }
    }
}
