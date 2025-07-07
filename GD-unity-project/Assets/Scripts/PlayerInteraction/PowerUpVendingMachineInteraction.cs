using System.Collections;
using UnityEngine;
using Audio;

namespace PlayerInteraction
{
    public class PowerUpVendingMachineInteraction : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt => _powerUpObtained
            ? "You obtained a " + _obtainedPowerUp.ToString().Replace("Boost", " Boost") + "!"
            : _isPowerUpVendingMachineHacked
                ? "Press E again to take a snack from the machine"
                : "Press E to interact with the snack distributor";

        public bool IsInteractable => !_isBusy;

        [Header("Item Meshes")]
        [SerializeField] private GameObject _energyDrinkMeshPrefab;
        [SerializeField] private GameObject _snackMeshPrefab;
        
        [Header("Timings")]
        [SerializeField] private float _hackingTime = 4.2f;
        [SerializeField] private float _itemHoldDelay = 1.0f;
        [SerializeField] private float _itemUseDuration = 5.0f;
        [SerializeField] private float _rotationDuration = 0.2f;

        [Header("UI Feedback")]
        [Tooltip("How long the 'You obtained a ...' message should display before resetting.")]
        [SerializeField] private float _feedbackMessageDuration = 3.0f;

        static System.Random _random = new System.Random();

        private PowerUp.PlayerPowerUpTypes _obtainedPowerUp;
        private bool _isPowerUpVendingMachineHacked = false;
        private bool _powerUpObtained = false;
        private bool _isBusy = false;
        
        private PlayerShoot _playerShoot;
        private Player _player;
        private PowerUp _powerUp;
        private RotateSphere _rotateSphere;
        private Transform _leftHand;
        private Transform _rightHand;
        private GameObject _instantiatedItem;

        private void Start()
        {
            _playerShoot = PlayerShoot.Instance;
            _player = Player.Instance;
            _powerUp = PowerUp.Instance;
            _rotateSphere = RotateSphere.Instance;
            
            _leftHand = GameObject.Find("Player/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand").transform;
            _rightHand = GameObject.Find("Player/Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand").transform;
        }

        public bool Interact(GameObject interactor)
        {
            if (_isBusy || _powerUpObtained) return false;
            if (_powerUp.playerPowerUps.Count <= 0 && _isPowerUpVendingMachineHacked)
            {
                Debug.Log("Vending machine is empty.");
                return false;
            }
            
            StartCoroutine(RotatePlayerTowards(transform, _rotationDuration));

            StartCoroutine(_isPowerUpVendingMachineHacked ? GetItemSequence() : HackingSequence());

            return true;
        }

        private IEnumerator HackingSequence()
        {
            _isBusy = true;

            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineActivation, this.transform.position);
            _rotateSphere.positionSphere(new Vector3(_rotateSphere.DistanceFromPlayer, 1f, 0), RotateSphere.Animation.Linear);

            yield return new WaitForSeconds(_hackingTime);

            _playerShoot.DecreaseStamina(1);
            _rotateSphere.isRotating = true;
            _isPowerUpVendingMachineHacked = true;

            _isBusy = false;
        }

        private IEnumerator GetItemSequence()
        {
            _isBusy = true;
            _player.FreezeMovement(true);
            _playerShoot.DisableAttacks(true);
            
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineItemPickUp, this.transform.position);
            _isPowerUpVendingMachineHacked = false;

            int powerUpIndexPlayer = _random.Next(_powerUp.playerPowerUps.Count);
            _obtainedPowerUp = _powerUp.playerPowerUps[powerUpIndexPlayer];

            if (_obtainedPowerUp == PowerUp.PlayerPowerUpTypes.HealthBoost)
            {
                AnimationManager.Instance.EatChips();
                yield return new WaitForSeconds(_itemHoldDelay);
                PlaceSnackInHand();
                yield return new WaitForSeconds(_itemUseDuration);
                _playerShoot.maxHealth += 20;
                _playerShoot.health += 20;
            }
            else if (_obtainedPowerUp == PowerUp.PlayerPowerUpTypes.DamageReduction)
            {
                AnimationManager.Instance.Drink();
                yield return new WaitForSeconds(_itemHoldDelay);
                PlaceDrinkInHand();
                yield return new WaitForSeconds(_itemUseDuration);
                _playerShoot.damageReduction -= 0.2f;
            }
            
            if(_instantiatedItem != null) Destroy(_instantiatedItem);

            _powerUp.ObtainPowerUp(_obtainedPowerUp);
            _powerUp.playerPowerUps.RemoveAt(powerUpIndexPlayer);

            StartCoroutine(ShowFeedbackMessage());

            _player.FreezeMovement(false);
            _playerShoot.DisableAttacks(false);
            _isBusy = false;
        }

        private void PlaceDrinkInHand() {
            _instantiatedItem = Instantiate(_energyDrinkMeshPrefab, _leftHand);
            _instantiatedItem.transform.SetLocalPositionAndRotation(new Vector3(-7.91e-06f, 7.33e-06f, 3.93e-06f), Quaternion.Euler(-3.593f, 99.3f, 0));
            _instantiatedItem.transform.localScale = new Vector3(0.0002f, 0.0002f, 0.0002f);
        }

        private void PlaceSnackInHand() {
            _instantiatedItem = Instantiate(_snackMeshPrefab, _rightHand);
            _instantiatedItem.transform.SetLocalPositionAndRotation(new Vector3(3.59e-06f, 7.72e-06f, 1.022e-05f), Quaternion.Euler(-85.337f, 90, 0));
            _instantiatedItem.transform.localScale = new Vector3(1.6e-05f, 1.6e-05f, 1.6e-05f);
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
            _isBusy = true;
        }
    }
}