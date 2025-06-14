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
		private bool wcIsTrigger = false;
		
		private MusicLoopIteration iteration = MusicLoopIteration.FIRST_ITERATION;
		
		private GameObject player;
		private IEnumerator PlayWakeUpAfterDelay(float delay) {
			yield return new WaitForSeconds(delay);
			AudioManager.instance.PlayOneShot(FMODEvents.instance.playerWakeUp, player.transform.position);
		}

		private void InitializeEventEmittersWithTag(string tagValue, EventReference eventRef, List<StudioEventEmitter> emitters)
		{
			GameObject[] objects = GameObject.FindGameObjectsWithTag(tagValue);
			foreach(GameObject obj in objects) {
				StudioEventEmitter emitter = AudioManager.instance.InitializeEventEmitter(eventRef, obj);
				if(emitter != null) {
					emitters.Add(emitter);
				}
				else {
					Debug.LogWarning($"Missing StudioEventEmitter on {obj.name}");
				}
			}
		}
		
		private void stopEventEmitters(List<StudioEventEmitter> emitters)
		{
			foreach(var emitter in emitters) {
				emitter.Stop();
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
		
		private void OnDestroy()
        {
            if (roomManager)
                roomManager.OnRunReady -= HandleRunReady;
        }

        private void Start()
        {
            if (roomManager)
            {
                roomManager.OnRunReady += HandleRunReady;
            }

			// Audio management
			player = GameObject.Find("Player");

			InitializeEventEmittersWithTag("AlarmSpeaker", FMODEvents.instance.alarm, alarmEmitters);
			InitializeEventEmittersWithTag("Server", FMODEvents.instance.serverNoise, serverEmitters);
			InitializeEventEmittersWithTag("FlickeringLED", FMODEvents.instance.flickeringLED, ledEmitters);
			InitializeEventEmittersWithTag("Refrigerator", FMODEvents.instance.refrigeratorNoise, refrigeratorEmitters);
			InitializeEventEmittersWithTag("HealthSnackDistributor", FMODEvents.instance.vendingMachineNoise, healthVendingMachineEmitters);
			InitializeEventEmittersWithTag("PowerUpSnackDistributor", FMODEvents.instance.vendingMachineNoise, powerUpVendingMachineEmitters);
			InitializeEventEmittersWithTag("SphereTerminal", FMODEvents.instance.terminalNoise, terminalEmitters);
			InitializeEventEmittersWithTag("Elevator", FMODEvents.instance.elevatorNoise, elevatorEmitters);
			InitializeEventEmittersWithTag("Ventilation", FMODEvents.instance.ventilationNoise, ventilationEmitters);
			InitializeEventEmittersWithTag("FlushingWC", FMODEvents.instance.flushingWCNoise, wcEmitters);
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

                ResetRun();

				// Audio management
				stopEventEmitters(alarmEmitters);
				stopEventEmitters(serverEmitters);
				stopEventEmitters(ledEmitters);
				stopEventEmitters(refrigeratorEmitters);
				stopEventEmitters(healthVendingMachineEmitters);
				stopEventEmitters(powerUpVendingMachineEmitters);
				stopEventEmitters(terminalEmitters);
				stopEventEmitters(elevatorEmitters);
				stopEventEmitters(ventilationEmitters);
				stopEventEmitters(wcEmitters);
				
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
				
				AudioManager.instance.SetMusicLoopIteration(iteration);
				StartCoroutine(PlayWakeUpAfterDelay(1.15f)); // 1.15 seconds delay
			}

            UpdateTimerUI();

			// Audio management
			// TODO: for the activation time for the alarm is 10 seconds, but it will be 60 seconds
			playEventEmitters(alarmEmitters, !alarmIsTriggered && currentTime <= 10f, ref alarmIsTriggered);
			playEventEmitters(serverEmitters, !serverIsTriggered, ref serverIsTriggered);
			playEventEmitters(ledEmitters, !ledIsTriggered, ref ledIsTriggered);
			playEventEmitters(refrigeratorEmitters, !refrigeratorIsTriggered, ref refrigeratorIsTriggered);
			playEventEmitters(healthVendingMachineEmitters, !HealthVendingMachineIsTriggered, ref HealthVendingMachineIsTriggered);
			playEventEmitters(powerUpVendingMachineEmitters, !PowerUpVendingMachineIsTriggered, ref PowerUpVendingMachineIsTriggered);
			playEventEmitters(terminalEmitters, !terminalIsTriggered, ref terminalIsTriggered);
			playEventEmitters(elevatorEmitters, !elevatorIsTriggered, ref elevatorIsTriggered);
			playEventEmitters(ventilationEmitters, !ventilationIsTriggered, ref ventilationIsTriggered);
			playEventEmitters(wcEmitters, !wcIsTrigger, ref wcIsTrigger);
		}

        private void HandleRunReady()
        {
	        currentTime = TimeLimit;
	        isRunning = true;

	        // Audio management
	        resetEventEmitters(alarmEmitters, ref alarmIsTriggered);
	        resetEventEmitters(serverEmitters, ref serverIsTriggered);
	        resetEventEmitters(ledEmitters, ref ledIsTriggered);
	        resetEventEmitters(refrigeratorEmitters, ref refrigeratorIsTriggered);
	        resetEventEmitters(healthVendingMachineEmitters, ref HealthVendingMachineIsTriggered);
	        resetEventEmitters(powerUpVendingMachineEmitters, ref PowerUpVendingMachineIsTriggered);
	        resetEventEmitters(terminalEmitters, ref terminalIsTriggered);
	        resetEventEmitters(elevatorEmitters, ref elevatorIsTriggered);
	        resetEventEmitters(ventilationEmitters, ref ventilationIsTriggered);
	        resetEventEmitters(wcEmitters, ref wcIsTrigger);

	        InitializeEventEmittersWithTag("AlarmSpeaker", FMODEvents.instance.alarm, alarmEmitters);
	        InitializeEventEmittersWithTag("Server", FMODEvents.instance.serverNoise, serverEmitters);
	        InitializeEventEmittersWithTag("FlickeringLED", FMODEvents.instance.flickeringLED, ledEmitters);
	        InitializeEventEmittersWithTag("Refrigerator", FMODEvents.instance.refrigeratorNoise, refrigeratorEmitters);
	        InitializeEventEmittersWithTag("HealthSnackDistributor", FMODEvents.instance.vendingMachineNoise, healthVendingMachineEmitters);
	        InitializeEventEmittersWithTag("PowerUpSnackDistributor", FMODEvents.instance.vendingMachineNoise, powerUpVendingMachineEmitters);
	        InitializeEventEmittersWithTag("SphereTerminal", FMODEvents.instance.terminalNoise, terminalEmitters);
	        InitializeEventEmittersWithTag("Elevator", FMODEvents.instance.elevatorNoise, elevatorEmitters);
	        InitializeEventEmittersWithTag("Ventilation", FMODEvents.instance.ventilationNoise, ventilationEmitters);
	        InitializeEventEmittersWithTag("FlushingWC", FMODEvents.instance.flushingWCNoise, wcEmitters);
	        
	        AudioManager.instance.SetMusicLoopIteration(iteration);
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