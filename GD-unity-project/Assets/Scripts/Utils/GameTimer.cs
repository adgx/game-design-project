using Helper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Audio management
using FMODUnity;
using UnityEngine.UI;

namespace Utils
{
	// Audio management
	[RequireComponent(typeof(StudioEventEmitter))]

	public class GameTimer : MonoBehaviour
    {
        // TODO: the timer is set to 2 minutes for debugging. It should be of 10 minutes.
	    private const float TimeLimit = 2f * 60f;
        private float currentTime;

        public TMP_Text timerText;
        [SerializeField] private Image timerOutlineImage;
        [SerializeField] private Sprite timerOutlineSpriteRed;
        [SerializeField] private Sprite timerOutlineSpriteNormal;

        private bool isRunning;

        public RoomManager.RoomManager roomManager;

		// Audio management
		private List<StudioEventEmitter> alarmEmitters = new List<StudioEventEmitter>();
		private bool alarmIsTriggered = false;
		
		private List<StudioEventEmitter> serverEmitters = new List<StudioEventEmitter>();
		private bool serverIsTriggered = false;
		
		private List<StudioEventEmitter> ledEmitters = new List<StudioEventEmitter>();
		private bool ledIsTriggered = false;
		
		private List<StudioEventEmitter> refrigeratorEmitters = new List<StudioEventEmitter>();
		private bool refrigeratorIsTriggered = false;
		
		private List<StudioEventEmitter> healthVendingMachineEmitters = new List<StudioEventEmitter>();
		private bool HealthVendingMachineIsTriggered = false;
		
		private List<StudioEventEmitter> powerUpVendingMachineEmitters = new List<StudioEventEmitter>();
		private bool PowerUpVendingMachineIsTriggered = false;
		
		private List<StudioEventEmitter> terminalEmitters = new List<StudioEventEmitter>();
		private bool terminalIsTriggered= false;
		
		private List<StudioEventEmitter> elevatorEmitters = new List<StudioEventEmitter>();
		private bool elevatorIsTriggered = false;
		
		private List<StudioEventEmitter> ventilationEmitters = new List<StudioEventEmitter>();
		private bool ventilationIsTriggered = false;
		
		private List<StudioEventEmitter> wcEmitters = new List<StudioEventEmitter>();
		private bool wcIsTriggered = false;
		
		private MusicLoopIteration iteration = MusicLoopIteration.FIRST_ITERATION;
		
		private GameObject player;
		private IEnumerator PlayWakeUpAfterDelay(float delay) {
			yield return new WaitForSeconds(delay);
			GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerWakeUp, player.transform.position);
		}

		private void InitializeEventEmittersWithTag(string tagValue, EventReference eventRef, List<StudioEventEmitter> emitters)
		{
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
		
		private void playEventEmitters(List<StudioEventEmitter> emitters, bool condition, ref bool isTriggered)
		{
			if(condition) {
				foreach (var emitter in emitters)
				{
					if (emitter != null && emitter.gameObject != null)
					{
						emitter.Play();
					}
				}
				isTriggered = true;
			}
		}
		
		private void resetEventEmitters(List<StudioEventEmitter> emitters, ref bool isTriggered)
		{
			isTriggered = false;
			emitters.Clear();
		}
		
		private void InitializeAmbientEmitters()
		{
			resetEventEmitters(alarmEmitters, ref alarmIsTriggered);
			resetEventEmitters(serverEmitters, ref serverIsTriggered);
			resetEventEmitters(ledEmitters, ref ledIsTriggered);
			resetEventEmitters(refrigeratorEmitters, ref refrigeratorIsTriggered);
			resetEventEmitters(healthVendingMachineEmitters, ref HealthVendingMachineIsTriggered);
			resetEventEmitters(powerUpVendingMachineEmitters, ref PowerUpVendingMachineIsTriggered);
			resetEventEmitters(terminalEmitters, ref terminalIsTriggered);
			resetEventEmitters(elevatorEmitters, ref elevatorIsTriggered);
			resetEventEmitters(ventilationEmitters, ref ventilationIsTriggered);
			resetEventEmitters(wcEmitters, ref wcIsTriggered);

			InitializeEventEmittersWithTag("AlarmSpeaker", FMODEvents.Instance.Alarm, alarmEmitters);
			InitializeEventEmittersWithTag("Server", FMODEvents.Instance.ServerNoise, serverEmitters);
			InitializeEventEmittersWithTag("FlickeringLED", FMODEvents.Instance.FlickeringLed, ledEmitters);
			InitializeEventEmittersWithTag("Refrigerator", FMODEvents.Instance.RefrigeratorNoise, refrigeratorEmitters);
			InitializeEventEmittersWithTag("HealthSnackDistributor", FMODEvents.Instance.VendingMachineNoise, healthVendingMachineEmitters);
			InitializeEventEmittersWithTag("PowerUpSnackDistributor", FMODEvents.Instance.VendingMachineNoise, powerUpVendingMachineEmitters);
			InitializeEventEmittersWithTag("SphereTerminal", FMODEvents.Instance.TerminalNoise, terminalEmitters);
			InitializeEventEmittersWithTag("Elevator", FMODEvents.Instance.ElevatorNoise, elevatorEmitters);
			InitializeEventEmittersWithTag("Ventilation", FMODEvents.Instance.VentilationNoise, ventilationEmitters);
			InitializeEventEmittersWithTag("FlushingWC", FMODEvents.Instance.FlushingWcNoise, wcEmitters);
		}
		
		private void StopAllAmbientSounds()
		{
			FMOD.Studio.Bus ambientBus = FMODUnity.RuntimeManager.GetBus("bus:/Ambience"); 
			if (ambientBus.isValid())
			{
				// Stop all events routed on this bus and its sub-buses
				ambientBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
			}
			else
			{
				Debug.LogWarning("FMOD Bus 'bus:/Ambience' not find!");
			}

			// Resets the Boolean flags as well, since all sounds have been stopped
			alarmIsTriggered = false;
			serverIsTriggered = false;
			ledIsTriggered = false;
			refrigeratorIsTriggered = false;
			HealthVendingMachineIsTriggered = false;
			PowerUpVendingMachineIsTriggered = false;
			terminalIsTriggered = false;
			elevatorIsTriggered = false;
			ventilationIsTriggered = false;
			wcIsTriggered = false;
		}
		
		private void OnDestroy()
        {
            if (roomManager)
            {
	            roomManager.OnRunReady -= HandleRunReady;
	            
	            // Audio management
	            roomManager.OnRoomFullyInstantiated -= InitializeAmbientEmitters;
            }
        }

        private void Awake()
        {
	        if (roomManager)
	        {
		        roomManager.OnRunReady += HandleRunReady;
		        
		        // Audio management
		        roomManager.OnRoomFullyInstantiated += InitializeAmbientEmitters;
	        }
        }

        private void Start()
        {
			// Audio management
			player = GameObject.FindWithTag("Player");
		}

        private void Update()
        {
            if (!isRunning)
                return;

            currentTime -= Time.deltaTime;

            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isRunning = false;

				// Audio management
				StopAllAmbientSounds();
				
				switch(iteration) {
					case MusicLoopIteration.FIRST_ITERATION:
						iteration = MusicLoopIteration.SECOND_ITERATION;
						break;
					case MusicLoopIteration.SECOND_ITERATION:
						iteration = MusicLoopIteration.THIRD_ITERATION;
						break;
					case MusicLoopIteration.THIRD_ITERATION:
						iteration = MusicLoopIteration.FIRST_ITERATION;
						break;
					default:
						break;
				}
				
				GamePlayAudioManager.instance.SetMusicLoopIteration(iteration);
				StartCoroutine(PlayWakeUpAfterDelay(1.15f)); // 1.15 seconds delay
				
				ResetRun();
				
				// Exit the Update for this frame, preventing sounds from being reactivated immediately afterwards.
				return; 
			}

            UpdateTimerUI();

			// Audio management
			playEventEmitters(alarmEmitters, !alarmIsTriggered && currentTime <= 10f, ref alarmIsTriggered);
			playEventEmitters(serverEmitters, !serverIsTriggered, ref serverIsTriggered);
			playEventEmitters(ledEmitters, !ledIsTriggered, ref ledIsTriggered);
			playEventEmitters(refrigeratorEmitters, !refrigeratorIsTriggered, ref refrigeratorIsTriggered);
			playEventEmitters(healthVendingMachineEmitters, !HealthVendingMachineIsTriggered, ref HealthVendingMachineIsTriggered);
			playEventEmitters(powerUpVendingMachineEmitters, !PowerUpVendingMachineIsTriggered, ref PowerUpVendingMachineIsTriggered);
			playEventEmitters(terminalEmitters, !terminalIsTriggered, ref terminalIsTriggered);
			playEventEmitters(elevatorEmitters, !elevatorIsTriggered, ref elevatorIsTriggered);
			playEventEmitters(ventilationEmitters, !ventilationIsTriggered, ref ventilationIsTriggered);
			playEventEmitters(wcEmitters, !wcIsTriggered, ref wcIsTriggered);
		}

        private void HandleRunReady()
        {
	        currentTime = TimeLimit;
	        isRunning = true;

	        // Audio management
	        InitializeAmbientEmitters();
	        
	        GamePlayAudioManager.instance.SetMusicLoopIteration(iteration);
        }


        private void UpdateTimerUI()
        {
            var minutes = Mathf.FloorToInt(currentTime / 60f);
            var seconds = Mathf.FloorToInt(currentTime % 60f);

            if (currentTime <= 30f && timerOutlineImage.sprite != timerOutlineSpriteRed)
            {
	            timerOutlineImage.sprite = timerOutlineSpriteRed;
	            StartCoroutine(Pulse());
            }

            timerText.text = $"{minutes:00}:{seconds:00}";
        }
        
        // Coroutine that produces a pulse effect on the text
        IEnumerator Pulse()
        {
	        while (currentTime <= 30f)
	        {
		        // Scale pulse
		        float scale = Mathf.PingPong(Time.time * 0.7f, 1f);
		        scale = Mathf.Lerp(0.9f, 1f, scale);
		        timerText.transform.localScale = new Vector3(1 * scale, 1 * scale, 1f);
		        yield return null;
	        }
        }

        private void ResetRun()
        {
            if (!roomManager)
                return;

            FadeManager.Instance.FadeOutIn(() =>
            {
                roomManager.RegenerateRooms();
                currentTime = TimeLimit;
                timerOutlineImage.sprite = timerOutlineSpriteNormal;
                StopCoroutine(Pulse());
                isRunning = true;
            });
        }
    }
}