using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

namespace Audio
{
    [RequireComponent(typeof(StudioEventEmitter))]
    
    public class AmbienceSound : Component
    {
        public readonly List<StudioEventEmitter> Emitters;
        public bool IsTriggered;
		
        public AmbienceSound(List<StudioEventEmitter> emitters)
        {
            this.Emitters = emitters;
            this.IsTriggered = false;
        }
    }
    
    public class AmbienceEmitters : MonoBehaviour
    {
	    public static AmbienceEmitters Instance { get; private set; }
	    
        private Dictionary<string, AmbienceSound> ambienceSounds;
        
        private static void InitializeEventEmittersWithTag(string tagValue, EventReference eventRef, List<StudioEventEmitter> emitters) { // TODO: moved
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tagValue);
            foreach(GameObject obj in objects) {
                StudioEventEmitter emitter = GamePlayAudioManager.instance.InitializeEventEmitter(eventRef, obj);
                if(emitter != null) {
                    emitters.Add(emitter);
                }
                else {
                    Debug.LogWarning($"Missing StudioEventEmitter on {obj.name}");
                }
            }
        }
        
        private void AddAmbienceSound(string objectTag, EventReference evRef, string key) {
			var emitters = new List<StudioEventEmitter>();
			InitializeEventEmittersWithTag(objectTag, evRef, emitters);
			ambienceSounds[key] = new AmbienceSound(emitters);
		}

		public void InitializeAmbientEmitters() {
			ambienceSounds = new Dictionary<string, AmbienceSound>();
			
			AddAmbienceSound("AlarmSpeaker", FMODEvents.Instance.Alarm, "alarm");
			AddAmbienceSound("Server", FMODEvents.Instance.ServerNoise, "server");
			AddAmbienceSound("FlickeringLED", FMODEvents.Instance.FlickeringLed, "led");
			AddAmbienceSound("Refrigerator", FMODEvents.Instance.RefrigeratorNoise, "refrigerator");
			AddAmbienceSound("HealthSnackDistributor", FMODEvents.Instance.VendingMachineNoise, "healthVendingMachine");
			AddAmbienceSound("PowerUpSnackDistributor", FMODEvents.Instance.VendingMachineNoise, "powerUpVendingMachine");
			AddAmbienceSound("SphereTerminal", FMODEvents.Instance.TerminalNoise, "terminal");
			AddAmbienceSound("Elevator", FMODEvents.Instance.ElevatorNoise, "elevator");
			AddAmbienceSound("Ventilation", FMODEvents.Instance.VentilationNoise, "ventilation");
			AddAmbienceSound("FlushingWC", FMODEvents.Instance.FlushingWcNoise, "wc");
		}
		
		public void PlayAmbientEmitters(float currentTime)
		{
			foreach (var element in ambienceSounds)
			{
				bool condition;
				
				if (element.Key == "alarm")
				{
					condition = currentTime <= 10f && !element.Value.IsTriggered;
				}
				else
				{
					condition = !element.Value.IsTriggered;
				}

				List<StudioEventEmitter> emitters = element.Value.Emitters;
				
				if(condition) {
					foreach(var emitter in emitters) {
						if(emitter != null && emitter.gameObject != null) {
							emitter.Play();
						}
					}
					element.Value.IsTriggered = true;
				}
			}
		}

		public void StopAmbientEmitters() {
			FMOD.Studio.Bus ambientBus = RuntimeManager.GetBus("bus:/Ambience");
			if(ambientBus.isValid()) {
				// Stop all events routed on this bus and its sub-buses
				ambientBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
			}
			else {
				Debug.LogWarning("FMOD Bus 'bus:/Ambience' not find!");
			}

			// Resets the Boolean flags as well, since all sounds have been stopped
			foreach (var sound in ambienceSounds.Values)
			{
				sound.IsTriggered = false;
			}
		}

		private void Awake()
		{
			if (Instance != null)
			{
				Debug.LogError("Found more than one Ambience Emitters Manager instance in the scene.");
			}
			Instance = this;
		}
    }
}
