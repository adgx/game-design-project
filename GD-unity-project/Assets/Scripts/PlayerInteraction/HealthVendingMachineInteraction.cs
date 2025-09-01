using System.Collections;
using Animations;
using Audio;
using UnityEngine;

namespace PlayerInteraction
{
    public class HealthVendingMachineInteraction : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt
        {
            get
            {
                if (_healthObtained)
                {
                    return "Your health was recovered!";
                }
                if (_isHealthVendingMachineHacked)
                {
                    return "Press E again to take a snack from the machine";
                }
                return RoomManager.RoomManager.Instance.IsHealthVendingMachineUsedInCurrentRoom() 
                    ? "The snack distributor is now empty" 
                    : "Press E to interact with the snack distributor";
            }
        }
        
        public bool IsInteractable => !_isBusy;
        
        public Collider InteractionZone => _interactionZone;

        public GameObject GameObject => this.gameObject;


        [Header("Interaction Zone")]
        [Tooltip("An optional trigger collider that defines the area the player must be in to use this.")]
        [SerializeField]
        private Collider _interactionZone;

        [Header("Item Meshes")] [SerializeField]
        private GameObject _specialSnackMeshPrefab;

        [Header("Timings")]
		[SerializeField] private float _hackingTime = 3.7f;
        [SerializeField] private float _rotationDuration = 0.2f;

        [Header("UI Feedback")]
        [Tooltip("How long the 'Health Recovered' message should display before resetting.")]
        [SerializeField]
        private float _feedbackMessageDuration = 3.0f;

        private bool _isHealthVendingMachineHacked = false;
        private bool _healthObtained = false;
        private bool _isBusy = false;
        
        private PlayerShoot _playerShoot;
        private Player _player;
        private RotateSphere _rotateSphere;
		private RickEvents _rickEvents;
		private Transform _leftHand;
        private GameObject _instantiatedItem;

        private void Start()
        {
            _playerShoot = PlayerShoot.Instance;
            _player = Player.Instance;
            _rotateSphere = RotateSphere.Instance;
            _leftHand = GameObject
                .Find(
                    "Player/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand")
                .transform;
			_rickEvents = _player.GetComponent<RickEvents>();
		}

        public bool Interact(GameObject interactor)
        {
            if (_isBusy || _healthObtained || RoomManager.RoomManager.Instance.IsHealthVendingMachineUsedInCurrentRoom())
            {
                return false; 
            }

            StartCoroutine(RotatePlayerTowards(transform, _rotationDuration));
            //AnimationManager.Instance.Idle();

            if (_isHealthVendingMachineHacked)
            {
                GetItemSequence();
                
                // Mark the health vending machine as used (not interactable anymore)
                RoomManager.RoomManager.Instance.MarkHealthVendingMachineAsUsedInCurrentRoom();
            }
                
            else 
                StartCoroutine(HackingSequence());

            return true;
        }

        private IEnumerator HackingSequence()
        {
            _isBusy = true;
            _playerShoot.isInteracting = true;
            
            _rotateSphere.positionSphere(new Vector3(_rotateSphere.DistanceFromPlayer, 1f, 0), RotateSphere.Animation.Linear);
            GamePlayAudioManager.instance.PlayManagedOneShot(FMODEvents.Instance.PlayerVendingMachineActivation, transform.position);

            _playerShoot.DecreaseStamina(1);
            
			yield return new WaitForSeconds(_hackingTime);
            
            _rotateSphere.isRotating = true;
            _isHealthVendingMachineHacked = true;
            _isBusy = false;
            _playerShoot.isInteracting = false;
        }

        private void GetItemSequence()
        {
            _isBusy = true;
            _player.FreezeMovement(true);
            _playerShoot.DisableAttacks(true);
            
            _isHealthVendingMachineHacked = false;

            AnimationManager.Instance.EatSnack();
			_rickEvents.HealthVendingMachineInteraction = this;
            _rickEvents.MachineType = "health";
        }

        public void PlaceSpecialSnackInHand()
        {
            _instantiatedItem = Instantiate(_specialSnackMeshPrefab, _leftHand);
            _instantiatedItem.transform.SetLocalPositionAndRotation(new Vector3(-5.51e-06f, 1.01e-05f, 3.32e-06f),
                Quaternion.Euler(-5.322f, 77.962f, -32.059f));
            _instantiatedItem.transform.localScale = new Vector3(0.00015f, 0.00015f, 0.00015f);
        }

		public void TerminateHealthRecovery() {
			if(_instantiatedItem != null) Destroy(_instantiatedItem);
			_playerShoot.RecoverHealth(_playerShoot.maxHealth);

			StartCoroutine(ShowFeedbackMessage());

			_isBusy = false;
			_playerShoot.FreePlayer();
		}

		private IEnumerator RotatePlayerTowards(Transform target, float duration)
        {
            Quaternion startRotation = _player.transform.rotation;
            Quaternion endRotation = Quaternion.LookRotation(target.forward, Vector3.up);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                _player.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _player.transform.rotation = endRotation;
        }

        private IEnumerator ShowFeedbackMessage()
        {
            _healthObtained = true;
            yield return new WaitForSeconds(_feedbackMessageDuration);
            _healthObtained = false;
            _isBusy = true;
        }
    }
}