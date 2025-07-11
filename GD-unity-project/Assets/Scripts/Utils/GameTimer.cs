using Helper;
using System.Collections;
using Audio;
using TMPro;
using UnityEngine;

// Audio management
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Enemy.EnemyManager;
using System.Threading.Tasks;
using Animations;

namespace Utils {
	public class GameTimer : MonoBehaviour {
		private const float TimeLimit = 10f * 60f;
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

		private void OnDestroy() {
			if(roomManager) {
				roomManager.OnRunReady -= HandleRunReady;

				// Audio management
				roomManager.OnRoomFullyInstantiated -= AmbienceEmitters.Instance.InitializeAmbientEmitters;
			}
		}

		private void Awake() {
			GameStatus.loopIteration = GameStatus.LoopIteration.FIRST_ITERATION;
			if(roomManager) {
				roomManager.OnRunReady += HandleRunReady;

				// Audio management
				roomManager.OnRoomFullyInstantiated += AmbienceEmitters.Instance.InitializeAmbientEmitters;
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

			if(currentTime <= 0f) {
				currentTime = 0f;
				isRunning = false;

				AnimationManager.Instance.Idle();
				rickEvents.SetIdleState();
				
				// Ambient light management
				GameEvents.current.TimerEnded(); 

				if(GameStatus.loopIteration == GameStatus.LoopIteration.THIRD_ITERATION) {
					GameStatus.gameEnded = true;
					FadeManager.Instance.FadeOutIn(() => {
						StartCoroutine(LoadRespawnSceneAsync());
					});
				}
				else {
					// Audio management
					AmbienceEmitters.Instance.StopAmbientEmitters();

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

				// Exit the Update for this frame, preventing sounds from being reactivated immediately afterward.
				return;
			}

			UpdateTimerUI();

			// Audio management
			AmbienceEmitters.Instance.PlayAmbientEmitters(currentTime);
		}

		private void HandleRunReady() {
			currentTime = TimeLimit;
			isRunning = true;

			// Audio management
			AmbienceEmitters.Instance.InitializeAmbientEmitters();
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