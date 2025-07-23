using System;
using Helper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Enemy.EnemyManager;
using System.Threading.Tasks;
using Animations;
using FMOD.Studio;

namespace Utils {
	public class GameTimer : MonoBehaviour
	{
		private const float TimeLimit = 15f; // 2 * 60f;
		public float currentTime;

		public TMP_Text timerText;
		[SerializeField] private Image timerOutlineImage;
		[SerializeField] private Sprite timerOutlineSpriteRed;
		[SerializeField] private Sprite timerOutlineSpriteNormal;

		public bool isRunning;

		public RoomManager.RoomManager roomManager;
		[SerializeField] private EnemyManager enemyManager;

		private GameObject player;

		private Player playerScript;
		private PlayerShoot playerShoot;
		private RickEvents rickEvents;

		[SerializeField] private string respawnSceneName = "RespawnScene";
		private bool sceneIsLoading = false;
		
		// Audio management
		public static event Action OnTimerLow;
		private bool lowTimeEventFired = false;
		public bool IsAlarmConditionActive { get; private set; } = false;
		[Header("FMOD Events")]
		[SerializeField] private List<FMODUnity.EventReference> ambientEventsToForceStop;

		private void OnDestroy() {
			if(roomManager) {
				roomManager.OnRunReady -= HandleRunReady;
			}
		}

		private void Awake() {
			GameStatus.loopIteration = GameStatus.LoopIteration.FIRST_ITERATION;
			if(roomManager) {
				roomManager.OnRunReady += HandleRunReady;
			}
		}

		private async void Start() {
			player = GameObject.FindWithTag("Player");
			playerScript = player.GetComponent<Player>();
			playerShoot = player.GetComponent<PlayerShoot>();
			rickEvents = player.GetComponent<RickEvents>();
			
			playerScript.FreezeMovement(true);
			playerShoot.DisableAttacks(true);

			await Task.Delay(50);
			AnimationManager.Instance.StandUp();

			GameStatus.gameEnded = false;
			GameStatus.gamePaused = false;

			roomManager.SetRoomsDifficulty();
			enemyManager.SetEnemyDifficulty();
		}

		private void Update() {
			if(!isRunning || GameStatus.gameEnded || sceneIsLoading)
				return;
			
			currentTime -= Time.deltaTime;
			
			// Audio management
			IsAlarmConditionActive = (isRunning && currentTime <= 10f);
			
			if (IsAlarmConditionActive && !lowTimeEventFired)
			{
				lowTimeEventFired = true;
				OnTimerLow?.Invoke();
			}

			if(currentTime <= 0f) {
				currentTime = 0f;
				isRunning = false;

				AnimationManager.Instance.Idle();
				rickEvents.DisableRickState();
				
				// Ambient light management
				GameEvents.current.TimerEnded(); 

				if(GameStatus.loopIteration == GameStatus.LoopIteration.THIRD_ITERATION) {
					GameStatus.gameEnded = true;
					
					// Audio management: clean the audio before changing scene
					ForceStopAllAmbientEvents();
					
					FadeManager.Instance.FadeOutIn(() => {
						StartCoroutine(LoadRespawnSceneAsync());
					});
				}
				else {
					switch(GameStatus.loopIteration) {
						case GameStatus.LoopIteration.FIRST_ITERATION:
							GameStatus.loopIteration = GameStatus.LoopIteration.SECOND_ITERATION;
							break;
						case GameStatus.LoopIteration.SECOND_ITERATION:
							GameStatus.loopIteration = GameStatus.LoopIteration.THIRD_ITERATION;
							break;
						case GameStatus.LoopIteration.THIRD_ITERATION:
							GameStatus.loopIteration = GameStatus.LoopIteration.FIRST_ITERATION;
							break;
						default:
							break;
					}

					GamePlayAudioManager.instance.SetMusicLoopIteration();

					ResetRun();
				}

				// Audio management: exit the Update for this frame, preventing sounds from being reactivated immediately afterward
				return;
			}

			UpdateTimerUI();
		}

		private void HandleRunReady() {
			currentTime = TimeLimit;
			isRunning = true;

			// Audio management
			GamePlayAudioManager.instance.SetMusicLoopIteration();
		}
		
		private void UpdateTimerUI() {
			var minutes = Mathf.FloorToInt(currentTime / 60f);
			var seconds = Mathf.FloorToInt(currentTime % 60f);

			if(currentTime <= 30f && timerOutlineImage.sprite != timerOutlineSpriteRed) {
				timerOutlineImage.sprite = timerOutlineSpriteRed;
				StartCoroutine(Pulse());
			}

			timerText.text = $"{minutes:00}:{seconds:00}";
		}

		// Coroutine that produces a pulse effect on the text
		IEnumerator Pulse() {
			while(currentTime <= 30f) {
				// Scale pulse
				float scale = Mathf.PingPong(Time.time * 0.7f, 1f);
				scale = Mathf.Lerp(0.9f, 1f, scale);
				timerText.transform.localScale = new Vector3(1 * scale, 1 * scale, 1f);
				yield return null;
			}
		}

		private void ResetRun() {
			if(!roomManager)
				return;
			
			// Audio management: first, stop the player sounds immediately
			rickEvents.StopAllLoopingSounds(); 
			
			// Audio management: clean the audio before starting the reset and resets the alarm logic state
			ForceStopAllAmbientEvents();
			IsAlarmConditionActive = false;
			lowTimeEventFired = false;

			FadeManager.Instance.FadeOutIn(() => {
				roomManager.RegenerateRooms();

				roomManager.SetRoomsDifficulty();
				enemyManager.SetEnemyDifficulty();
				enemyManager.DestroyEnemies(roomManager.CurrentRoomIndex);

				playerShoot.ResetAttack();

				// Deleting the snacks that Rick has in hand, if any
				Destroy(player.transform.Find("Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand/EnergyDrink(Clone)")?.gameObject);
				Destroy(player.transform.Find("Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand/Snack(Clone)")?.gameObject);
				Destroy(player.transform.Find("Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand/SpecialSnack(Clone)")?.gameObject);

				currentTime = TimeLimit;
				timerOutlineImage.sprite = timerOutlineSpriteNormal;
				StopCoroutine(Pulse());
				
				playerScript.FreezeMovement(true);
				playerShoot.DisableAttacks(true);
			
				AnimationManager.Instance.StandUp();
				
				isRunning = true;
			});
		}
		
		// Audio management
		private void ForceStopAllAmbientEvents()
		{
			foreach (var eventRef in ambientEventsToForceStop)
			{
				if (eventRef.IsNull)
				{
					continue;
				}
			
				EventDescription eventDescription = FMODUnity.RuntimeManager.GetEventDescription(eventRef);
    
				if (eventDescription.isValid())
				{
					// Release (stop and destroy) all instances of this event
					var result = eventDescription.releaseAllInstances();
					FMODUnity.RuntimeManager.StudioSystem.lookupPath(eventRef.Guid, out string path);
				}
				else
				{
					Debug.LogError("Could not find a valid description for the alarm event.");
				}
			}
		}

		private IEnumerator LoadRespawnSceneAsync() {
			sceneIsLoading = true;

			// Starts async loading of the scene
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(respawnSceneName);
			if (asyncLoad != null)
			{
				asyncLoad.allowSceneActivation = false;

				// Wait until the scene is almost ready (>= 0.9)
				while (asyncLoad.progress < 0.9f)
				{
					yield return null;
				}

				// Activate the scene
				asyncLoad.allowSceneActivation = true;
			}
		}
	}
}