using System.Collections;
using Animations;
using Audio;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlayerInteraction
{
    public class SphereUpgradeTerminalInteraction : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt
        {
            get
            {
                if (_powerUpObtained)
                {
                    string message = "You obtained a ";
                    if (_obtainedPowerUp.ToString() == "DistanceAttackPowerUp")
                        message += "Distance Attack power-up!";
                    else if(_obtainedPowerUp.ToString() == "CloseAttackPowerUp")
                        message += "Close Attack power-up!";
                    else 
                        message += "Defense power-up!";
                    return message;
                }
                if (_powerUp != null && _powerUp.spherePowerUps.Count <= 0)
                {
                    return "You have already collected all sphere power-ups!";
                }
                if (!_feedbackMessageActive && RoomManager.RoomManager.Instance.IsSphereUpgradeTerminalUsedInCurrentRoom())
                {
                    return "The terminal has already been hacked";
                }
                return "Press E to interact with the terminal";
            }
        }
        
        public bool IsInteractable => !_isBusy && (_powerUp != null && _powerUp.spherePowerUps.Count > 0);
        
        public Collider InteractionZone => _interactionZone;

        public GameObject GameObject => this.gameObject;


        [Header("Interaction Zone")]
        [Tooltip("An optional trigger collider that defines the area the player must be in to use this.")]
        [SerializeField]
        private Collider _interactionZone;

        [Header("Timings")] [SerializeField] private float _interactionTime = 2.0f;
        [SerializeField] private float _postInteractionDelay = 1.5f;
        [SerializeField] private float _rotationDuration = 0.2f;
        
        [Header("UI Feedback")]
        [Tooltip("How long the 'You obtained a ...' message should display before resetting.")]
        [SerializeField] private float _feedbackMessageDuration = 5.0f;
        private bool _feedbackMessageActive = false;
        private PlayerInteractor _playerInteractor;

        private PowerUp.SpherePowerUpTypes _obtainedPowerUp;
        private bool _powerUpObtained = false;
        private bool _isBusy;
		private bool _noMorePowerUp = false;

		private Player _player;
        private PlayerShoot _playerShoot;
        private PowerUp _powerUp;
        private RickEvents _rickEvents;
        private RotateSphere _rotateSphere;
        static System.Random _random = new System.Random();

        private void Start()
        {
            _player = Player.Instance;
            _playerShoot = PlayerShoot.Instance;
            _powerUp = PowerUp.Instance;
            _rotateSphere = RotateSphere.Instance;
            _rickEvents = _player.GetComponent<RickEvents>();
            _playerInteractor = FindObjectOfType<PlayerInteractor>();
        }

        public bool Interact(GameObject interactor)
        {
            if (!IsInteractable || RoomManager.RoomManager.Instance.IsSphereUpgradeTerminalUsedInCurrentRoom() || !_playerShoot.CheckStamina(1)) 
                return false;

            AnimationManager.Instance.Idle();
			StartCoroutine(RotatePlayerTowards(transform, _rotationDuration));
            
            // Mark the upgrade sphere terminal as used (not interactable anymore)
            RoomManager.RoomManager.Instance.MarkSphereUpgradeTerminalAsUsedInCurrentRoom();

            StartCoroutine(UpgradeSequence());
            return true;
        }

        private IEnumerator UpgradeSequence()
        {
            _isBusy = true;
            _playerShoot.isInteracting = true;
            
            // Make the sphere return to its default position with a linear movement
            _rotateSphere.positionSphere(new Vector3(_rotateSphere.DistanceFromPlayer, 1f, 0), RotateSphere.Animation.Linear);
            
            GamePlayAudioManager.instance.PlayManagedOneShot(FMODEvents.Instance.PlayerTerminalInteraction, transform.position);

            _playerShoot.DecreaseStamina(1);
            
            yield return new WaitForSeconds(_interactionTime);
            
            _playerShoot.lastStaminaUseTime = Time.time;
            
            int powerUpIndex = _random.Next(_powerUp.spherePowerUps.Count);
            _obtainedPowerUp = _powerUp.spherePowerUps[powerUpIndex];
            _powerUp.ObtainPowerUp(_obtainedPowerUp);
            _powerUp.spherePowerUps.RemoveAt(powerUpIndex);

            StartCoroutine(ShowFeedbackMessage());
            
            yield return new WaitForSeconds(_postInteractionDelay);
            
			_rotateSphere.isRotating = true;
            _isBusy = false;
            _playerShoot.isInteracting = false;
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
            _powerUpObtained = true;
            _feedbackMessageActive = true;

            // Force the PlayerInteractor to show the power-up prompt
            if (_playerInteractor != null)
            {
                _playerInteractor.ShowForcedPrompt(this.InteractionPrompt);
            }

            yield return new WaitForSeconds(_feedbackMessageDuration);

            _powerUpObtained = false;
            _feedbackMessageActive = false;
            _noMorePowerUp = true;

            // Once the feedback message is gone, make sure the UI prompt updates
            if (_playerInteractor != null)
            {
                _playerInteractor.ClearForcedPrompt(); // Send the signal to PlayerInteractor
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (_playerInteractor != null && !_feedbackMessageActive)
                {
                    // If the feedback message is not active, force the PlayerInteractor to consider this terminal
                    _playerInteractor.SetCurrentTarget(this); 
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (_playerInteractor != null && _playerInteractor.GetCurrentTarget() == this)
                {
                    _playerInteractor.ClearTarget();
                }
            }
        }
    }
}