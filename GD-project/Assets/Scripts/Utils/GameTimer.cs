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

		private void InitalizeEventEmittersWithTag(string tagValue, EventReference eventRef, List<StudioEventEmitter> emitters)
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
			foreach(var emitter in alarmEmitters) {
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

			InitalizeEventEmittersWithTag("AlarmSpeaker", FMODEvents.instance.laboratoryAlarm, alarmEmitters);
			InitalizeEventEmittersWithTag("Server", FMODEvents.instance.serverNoise, serverEmitters);
			InitalizeEventEmittersWithTag("FlickeringLED", FMODEvents.instance.flickeringLED, ledEmitters);
			InitalizeEventEmittersWithTag("Refrigerator", FMODEvents.instance.refrigeratorNoise, refrigeratorEmitters);
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
			playEventEmitters(alarmEmitters, !alarmTriggered && currentTime <= 10f, ref alarmTriggered);
			playEventEmitters(serverEmitters, !serverTriggered, ref serverTriggered);
			playEventEmitters(ledEmitters, !ledTriggered, ref ledTriggered);
			playEventEmitters(refrigeratorEmitters, !refrigeratorTriggered, ref refrigeratorTriggered);
		}

        private void HandleRunReady()
        {
	        currentTime = TimeLimit;
	        isRunning = true;

	        // Audio management
	        resetEventEmitters(alarmEmitters, ref alarmTriggered);
	        resetEventEmitters(serverEmitters, ref serverTriggered);
	        resetEventEmitters(ledEmitters, ref ledTriggered);
	        resetEventEmitters(refrigeratorEmitters, ref refrigeratorTriggered);

	        InitalizeEventEmittersWithTag("AlarmSpeaker", FMODEvents.instance.laboratoryAlarm, alarmEmitters);
	        InitalizeEventEmittersWithTag("Server", FMODEvents.instance.serverNoise, serverEmitters);
	        InitalizeEventEmittersWithTag("FlickeringLED", FMODEvents.instance.flickeringLED, ledEmitters);
	        InitalizeEventEmittersWithTag("Refrigerator", FMODEvents.instance.refrigeratorNoise, refrigeratorEmitters);
	        
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