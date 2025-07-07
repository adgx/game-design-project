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

        [Header("Item Meshes")] [SerializeField]
        private GameObject _specialSnackMeshPrefab;

        [Header("Timings")] [SerializeField] private float _hackingTime = 4.2f;
        [SerializeField] private float _itemHoldDelay = 1.0f;
        [SerializeField] private float _itemUseDuration = 5.0f;
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
        }

        public bool Interact(GameObject interactor)
        {
            if (_isBusy || _healthObtained) return false;

            StartCoroutine(RotatePlayerTowards(transform, _rotationDuration));
            //AnimationManager.Instance.Idle();

            StartCoroutine(_isHealthVendingMachineHacked ? GetItemSequence() : HackingSequence());

            return true;
        }

        private IEnumerator HackingSequence()
        {
            _isBusy = true;

            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineActivation,
                this.transform.position);
            _rotateSphere.positionSphere(new Vector3(_rotateSphere.DistanceFromPlayer, 1f, 0),
                RotateSphere.Animation.Linear);

            yield return new WaitForSeconds(_hackingTime);

            _playerShoot.DecreaseStamina(1);
            _rotateSphere.isRotating = true;
            _isHealthVendingMachineHacked = true;
            
            _isBusy = false;
        }

        private IEnumerator GetItemSequence()
        {
            _isBusy = true;
            _player.FreezeMovement(true);
            _playerShoot.DisableAttacks(true);

            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineItemPickUp,
                this.transform.position);
            _isHealthVendingMachineHacked = false;

            AnimationManager.Instance.EatSnack();

            yield return new WaitForSeconds(_itemHoldDelay);
            PlaceSpecialSnackInHand();

            yield return new WaitForSeconds(_itemUseDuration);

            if (_instantiatedItem != null) Destroy(_instantiatedItem);
            _playerShoot.RecoverHealth(_playerShoot.maxHealth);

            StartCoroutine(ShowFeedbackMessage());

            _player.FreezeMovement(false);
            _playerShoot.DisableAttacks(false);
            _isBusy = false;
        }

        private void PlaceSpecialSnackInHand()
        {
            _instantiatedItem = Instantiate(_specialSnackMeshPrefab, _leftHand);
            _instantiatedItem.transform.SetLocalPositionAndRotation(new Vector3(-5.51e-06f, 1.01e-05f, 3.32e-06f),
                Quaternion.Euler(-5.322f, 77.962f, -32.059f));
            _instantiatedItem.transform.localScale = new Vector3(0.00015f, 0.00015f, 0.00015f);
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