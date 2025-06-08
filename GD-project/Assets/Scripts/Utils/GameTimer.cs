using Helper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Audio management
using FMODUnity;
using RoomManager;

namespace Utils
{
	// Audio management
	[RequireComponent(typeof(StudioEventEmitter))]

	public class GameTimer : MonoBehaviour
    {
        // TODO: the timer is set to 2 minutes for debugging. It should be of 10 minutes.
	    private const float TimeLimit = 2 * 60f;
        private float currentTime;

        public TMP_Text timerText;

        private bool isRunning;

        public RoomManager.RoomManager roomManager;

		// Audio management
		private List<StudioEventEmitter> alarmEmitters = new List<StudioEventEmitter>();
		private bool alarmTriggered = false;
		
		private List<StudioEventEmitter> serverEmitters = new List<StudioEventEmitter>();
		private bool serverTriggered = false;
		
		private List<StudioEventEmitter> ledEmitters = new List<StudioEventEmitter>();
		private bool ledTriggered = false;
		
		private List<StudioEventEmitter>  refrigeratorEmitters = new List<StudioEventEmitter>();
		private bool refrigeratorTriggered = false;
		
		private MusicLoopIteration iteration = MusicLoopIteration.FIRST_ITERATION;
		
		private GameObject player;
		private IEnumerator PlayWakeUpAfterDelay(float delay) {
			yield return new WaitForSeconds(delay);
			AudioManager.instance.PlayOneShot(FMODEvents.instance.playerWakeUp, player.transform.position);
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
			
			GameObject[] alarmSpeakers = GameObject.FindGameObjectsWithTag("AlarmSpeaker");
			foreach(GameObject speaker in alarmSpeakers) {
				StudioEventEmitter emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.laboratoryAlarm, speaker);
				if(emitter != null) {
					alarmEmitters.Add(emitter);
				}
				else {
					Debug.LogWarning($"Missing StudioEventEmitter on {speaker.name}");
				}
			}
			
			GameObject[] servers = GameObject.FindGameObjectsWithTag("Server");
			foreach(GameObject server in servers) {
				StudioEventEmitter emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.serverNoise, server);
				if(emitter != null) {
					alarmEmitters.Add(emitter);
				}
				else {
					Debug.LogWarning($"Missing StudioEventEmitter on {server.name}");
				}
			}
			
			GameObject[] leds = GameObject.FindGameObjectsWithTag("FlickeringLED");
			foreach(GameObject led in leds) {
				StudioEventEmitter emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.flickeringLED, led);
				if(emitter != null) {
					ledEmitters.Add(emitter);
				}
				else {
					Debug.LogWarning($"Missing StudioEventEmitter on {led.name}");
				}
			}
			
			GameObject[] refrigerators = GameObject.FindGameObjectsWithTag("Refrigerator");
			foreach(GameObject refrigerator in refrigerators) {
				StudioEventEmitter emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.refrigeratorNoise, refrigerator);
				if(emitter != null) {
					ledEmitters.Add(emitter);
				}
				else {
					Debug.LogWarning($"Missing StudioEventEmitter on {refrigerator.name}");
				}
			}
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
				foreach(var emitter in alarmEmitters) {
					emitter.Stop();
				}
				
				foreach(var emitter in serverEmitters) {
					emitter.Stop();
				}
				
				foreach(var emitter in ledEmitters) {
					emitter.Stop();
				}
				
				foreach(var emitter in refrigeratorEmitters) {
					emitter.Stop();
				}
				
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
			if(!alarmTriggered && currentTime <= 10f) {
				foreach (var emitter in alarmEmitters)
				{
					if (emitter != null && emitter.gameObject != null)
					{
						emitter.Play();
					}
				}
				alarmTriggered = true;
			}
			
			if(!serverTriggered) {
				foreach (var emitter in serverEmitters)
				{
					if (emitter != null && emitter.gameObject != null)
					{
						emitter.Play();
					}
				}
				serverTriggered = true;
			}
			
			if(!ledTriggered) {
				foreach (var emitter in ledEmitters)
				{
					if (emitter != null && emitter.gameObject != null)
					{
						emitter.Play();
					}
				}
				ledTriggered = true;
			}
			
			if(!refrigeratorTriggered) {
				foreach (var emitter in refrigeratorEmitters)
				{
					if (emitter != null && emitter.gameObject != null)
					{
						emitter.Play();
					}
				}
				refrigeratorTriggered = true;
			}
		}

        private void HandleRunReady()
        {
	        currentTime = TimeLimit;
	        isRunning = true;

	        // Audio management
	        alarmTriggered = false;
	        alarmEmitters.Clear();
	        
	        serverTriggered = false;
	        serverEmitters.Clear();
	        
	        ledTriggered = false;
	        ledEmitters.Clear();
	        
	        refrigeratorTriggered = false;
	        refrigeratorEmitters.Clear();

	        GameObject[] alarmSpeakers = GameObject.FindGameObjectsWithTag("AlarmSpeaker");
	        foreach (GameObject speaker in alarmSpeakers)
	        {
		        StudioEventEmitter emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.laboratoryAlarm, speaker);
		        if (emitter != null)
		        {
			        alarmEmitters.Add(emitter);
		        }
	        }
	        
	        GameObject[] servers = GameObject.FindGameObjectsWithTag("Server");
	        foreach (GameObject server in servers)
	        {
		        StudioEventEmitter emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.serverNoise, server);
		        if (emitter != null)
		        {
			        serverEmitters.Add(emitter);
		        }
	        }
	        
	        GameObject[] leds = GameObject.FindGameObjectsWithTag("FlickeringLED");
	        foreach (GameObject led in leds)
	        {
		        StudioEventEmitter emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.flickeringLED, led);
		        if (emitter != null)
		        {
			        ledEmitters.Add(emitter);
		        }
	        }
	        
	        GameObject[] refrigerators = GameObject.FindGameObjectsWithTag("Refrigerator");
	        foreach (GameObject refrigerator  in refrigerators)
	        {
		        StudioEventEmitter emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.refrigeratorNoise, refrigerator);
		        if (emitter != null)
		        {
			        refrigeratorEmitters.Add(emitter);
		        }
	        }

	        AudioManager.instance.SetMusicLoopIteration(iteration);
        }


        private void UpdateTimerUI()
        {
            var minutes = Mathf.FloorToInt(currentTime / 60f);
            var seconds = Mathf.FloorToInt(currentTime % 60f);

            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void ResetRun()
        {
            if (!roomManager)
                return;

            FadeManager.Instance.FadeOutIn(() =>
            {
                roomManager.RegenerateRooms();
                currentTime = TimeLimit;
                isRunning = true;
            });
        }
    }
}