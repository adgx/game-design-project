using Helper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Audio management
using FMODUnity;

namespace Utils
{
	// Audio management
	[RequireComponent(typeof(StudioEventEmitter))]

	public class GameTimer : MonoBehaviour
    {
	    // TODO: For testing purpose we use a 30 seconds timer, but it should be of 10 minutes 
        private const float TimeLimit = 30f;
        private float currentTime;

        public TMP_Text timerText;

        private bool isRunning;

        public RoomManager.RoomManager roomManager;

		// Audio management
		private List<StudioEventEmitter> alarmEmitters = new List<StudioEventEmitter>();
		private bool alarmTriggered = false;
		private MusicLoopIteration iteration = MusicLoopIteration.FIRST_ITERATION;
		private GameObject player;

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
		}

        private void HandleRunReady()
        {
	        currentTime = TimeLimit;
	        isRunning = true;

	        // Audio management
	        alarmTriggered = false;
	        alarmEmitters.Clear();

	        GameObject[] alarmSpeakers = GameObject.FindGameObjectsWithTag("AlarmSpeaker");
	        foreach (GameObject speaker in alarmSpeakers)
	        {
		        StudioEventEmitter emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.laboratoryAlarm, speaker);
		        if (emitter != null)
		        {
			        alarmEmitters.Add(emitter);
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