using System.Collections;
using UnityEngine;
using Audio;
using Animations;

namespace PlayerInteraction
{
    public class PowerUpVendingMachineInteraction : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt => _powerUpObtained
            ? "You obtained a " + _obtainedPowerUp.ToString().Replace("Boost", " Boost") + "!"
            : (_noMorePowerUp
                ? "You have already collected a Power Up from this machine"
				: (_isPowerUpVendingMachineHacked
                    ? "Press E again to take a snack from the machine"
                    : "Press E to interact with the snack distributor"));

        public bool IsInteractable => !_isBusy;
        
        public Collider InteractionZone => _interactionZone;

        [Header("Interaction Zone")]
        [Tooltip("An optional trigger collider that defines the area the player must be in to use this.")]
        [SerializeField]
        private Collider _interactionZone;

        [Header("Item Meshes")]
        [SerializeField] private GameObject _energyDrinkMeshPrefab;
        [SerializeField] private GameObject _snackMeshPrefab;
        
        [Header("Timings")]
		[SerializeField] private float _freeSphere = 0.5f;
		[SerializeField] private float _hackingTime = 3.7f;
        [SerializeField] private float _rotationDuration = 0.2f;

        [Header("UI Feedback")]
        [Tooltip("How long the 'You obtained a ...' message should display before resetting.")]
        [SerializeField] private float _feedbackMessageDuration = 3.0f;

        static System.Random _random = new System.Random();

        private PowerUp.PlayerPowerUpTypes _obtainedPowerUp;
        private int powerUpIndexPlayer;
		private bool _isPowerUpVendingMachineHacked = false;
        private bool _powerUpObtained = false;
        private bool _isBusy = false;
        private bool _noMorePowerUp = false;
        
        private PlayerShoot _playerShoot;
        private Player _player;
        private PowerUp _powerUp;
        private RotateSphere _rotateSphere;
        private RickEvents _rickEvents;
        private Transform _leftHand;
        private Transform _rightHand;
        private GameObject _instantiatedItem;

		private enum ItemToPick {
			Drink,
			Snack
		}
		private ItemToPick itemToPick;

		private void Start()
        {
            _playerShoot = PlayerShoot.Instance;
            _player = Player.Instance;
            _powerUp = PowerUp.Instance;
            _rotateSphere = RotateSphere.Instance;
            _rickEvents = _player.GetComponent<RickEvents>();
            
            _leftHand = GameObject.Find("Player/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand").transform;
            _rightHand = GameObject.Find("Player/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand").transform;
        }

        public bool Interact(GameObject interactor)
        {
            if (_isBusy || _powerUpObtained || _noMorePowerUp) return false;
            if (_powerUp.playerPowerUps.Count <= 0 && _isPowerUpVendingMachineHacked)
            {
                Debug.Log("Vending machine is empty.");
                return false;
            }

            StartCoroutine(RotatePlayerTowards(transform, _rotationDuration));

			if(_isPowerUpVendingMachineHacked)
				GetItemSequence();
			else
				StartCoroutine(HackingSequence());

			return true;
        }

        private IEnumerator HackingSequence()
        {
            _isBusy = true;

            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineActivation, this.transform.position);
            _rotateSphere.positionSphere(new Vector3(_rotateSphere.DistanceFromPlayer, 1f, 0), RotateSphere.Animation.Linear);

			yield return new WaitForSeconds(_freeSphere);
			_playerShoot.DecreaseStamina(1);
			_rotateSphere.isRotating = true;

			yield return new WaitForSeconds(_hackingTime);
			
            _isPowerUpVendingMachineHacked = true;

            _isBusy = false;
        }

        private void GetItemSequence()
        {
            _isBusy = true;
            _player.FreezeMovement(true);
            _playerShoot.DisableAttacks(true);
            
            _isPowerUpVendingMachineHacked = false;

            powerUpIndexPlayer = _random.Next(_powerUp.playerPowerUps.Count);
            _obtainedPowerUp = _powerUp.playerPowerUps[powerUpIndexPlayer];

            if (_obtainedPowerUp == PowerUp.PlayerPowerUpTypes.HealthBoost)
            {
                AnimationManager.Instance.EatChips();
				itemToPick = ItemToPick.Snack;
            }
            else if (_obtainedPowerUp == PowerUp.PlayerPowerUpTypes.DamageReduction)
            {
                AnimationManager.Instance.Drink();
				itemToPick = ItemToPick.Drink;
            }

            _rickEvents.powerUpVendingMachineInteraction = this;
            _rickEvents.machineType = "playerPowerUp";
        }

		public void PlaceItemInHand() {
			switch(itemToPick) {
				case ItemToPick.Drink:
					PlaceDrinkInHand();
					break;
				case ItemToPick.Snack:
					PlaceSnackInHand();
					break;
				default:
					break;
			}
		}

		private void PlaceDrinkInHand() {
            _instantiatedItem = Instantiate(_energyDrinkMeshPrefab, _leftHand);
            _instantiatedItem.transform.SetLocalPositionAndRotation(new Vector3(-7.91e-06f, 7.33e-06f, 3.93e-06f), Quaternion.Euler(-3.593f, 99.3f, 0));
            _instantiatedItem.transform.localScale = new Vector3(0.0002f, 0.0002f, 0.0002f);
        }

        private void PlaceSnackInHand() {
            _instantiatedItem = Instantiate(_snackMeshPrefab, _rightHand);
            _instantiatedItem.transform.SetLocalPositionAndRotation(new Vector3(8.85e-06f, 1.039e-05f, 7.75e-06f), Quaternion.Euler(-85.337f, 90, 0));
            _instantiatedItem.transform.localScale = new Vector3(1.6e-05f, 1.6e-05f, 1.6e-05f);
        }

		public void TerminatePlayerPowerUp() {
			if(_obtainedPowerUp == PowerUp.PlayerPowerUpTypes.HealthBoost) {
				_playerShoot.maxHealth += 20;
				_playerShoot.health += 20;
			}

			if(_obtainedPowerUp == PowerUp.PlayerPowerUpTypes.DamageReduction) {
				_playerShoot.damageReduction -= 0.2f;
			}

			if(_instantiatedItem != null) Destroy(_instantiatedItem);


			// Show a message to the player
			StartCoroutine(ShowFeedbackMessage());

			// Insert the power up in the dictionary of the obtained ones
			_powerUp.ObtainPowerUp(_obtainedPowerUp);

			// Remove the power up from the list of power ups
			_powerUp.playerPowerUps.RemoveAt(powerUpIndexPlayer);

			_isBusy = false;
			_playerShoot.FreePlayer();
		}

		private IEnumerator RotatePlayerTowards(Transform target, float duration) {
            Quaternion startRotation = _player.transform.rotation;
            Quaternion endRotation = Quaternion.LookRotation(target.forward, Vector3.up);
            float elapsed = 0f;
            while(elapsed < duration) {
                _player.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _player.transform.rotation = endRotation;
        }
        
        private IEnumerator ShowFeedbackMessage()
        {
            _powerUpObtained = true;
            yield return new WaitForSeconds(_feedbackMessageDuration);
            _powerUpObtained = false;
            _noMorePowerUp = true;
        }
    }
}