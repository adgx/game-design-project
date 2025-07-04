using System.Collections;
using Audio;
using UnityEngine;

namespace PlayerInteraction
{
    public class HealthVendingMachineInteraction : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt => _healthObtained
            ? "Your health was recovered!"
            : _isHealthVendingMachineHacked
                ? "Press E again to take a snack from the machine"
                : "Press E to interact with the snack distributor";

        public bool IsInteractable => !_isBusy;
        
        [Header("Timings")] [SerializeField] private float _hackingTime = 4.2f;
        [SerializeField] private float _itemUseAnimationTime = 6.0f;

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

        private void Start()
        {
            _playerShoot = PlayerShoot.Instance;
            _player = Player.Instance;
            _rotateSphere = RotateSphere.Instance;
        }

        public bool Interact(GameObject interactor)
        {
            if (_isBusy)
            {
                return false;
            }

            if (_isHealthVendingMachineHacked)
            {
                StartCoroutine(GetItemSequence());
            }
            else
            {
                StartCoroutine(HackingSequence());
            }

            return true;
        }

        /// <summary>
        /// Coroutine for the initial "hacking" interaction with the vending machine.
        /// </summary>
        private IEnumerator HackingSequence()
        {
            _isBusy = true;
            _player.FreezeMovement(true);
            _playerShoot.DisableAttacks(true);

            Debug.Log("Getting power up: machine activation");

            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineActivation,
                this.transform.position);
            _rotateSphere.positionSphere(new Vector3(_rotateSphere.DistanceFromPlayer, 1f, 0),
                RotateSphere.Animation.Linear);

            yield return new WaitForSeconds(_hackingTime);

            _playerShoot.DecreaseStamina(1);
            _rotateSphere.isRotating = true;

            _isHealthVendingMachineHacked = true;

            _player.FreezeMovement(false);
            _playerShoot.DisableAttacks(false);
            _isBusy = false;
        }

        /// <summary>
        /// Coroutine for getting the item after the machine has been hacked.
        /// </summary>
        private IEnumerator GetItemSequence()
        {
            _isBusy = true;
            _player.FreezeMovement(true);
            _playerShoot.DisableAttacks(true);

            Debug.Log("Recovering health: taking snack from the machine");

            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineItemPickUp,
                this.transform.position);
            _isHealthVendingMachineHacked = false;

            AnimationManager.Instance.EatSnack();

            Debug.Log("Your health was recovered!");

            yield return new WaitForSeconds(_itemUseAnimationTime);

            _playerShoot.RecoverHealth(_playerShoot.maxHealth);
            
            StartCoroutine(ShowFeedbackMessage());

            _player.FreezeMovement(false);
            _playerShoot.DisableAttacks(false);
            _isBusy = false;
        }

        /// <summary>
        /// A small coroutine dedicated to showing the feedback message for a few seconds.
        /// </summary>
        private IEnumerator ShowFeedbackMessage()
        {
            _healthObtained = true;
            Debug.Log("UI: Showing 'Health Recovered' message.");
            
            yield return new WaitForSeconds(_feedbackMessageDuration);
            
            _healthObtained = false;
            Debug.Log("UI: Hiding feedback message. Machine is ready to be hacked again.");
        }
    }
}