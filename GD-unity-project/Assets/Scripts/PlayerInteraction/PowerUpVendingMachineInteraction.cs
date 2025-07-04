using System.Collections;
using UnityEngine;
using Audio;

namespace PlayerInteraction
{
    public class PowerUpVendingMachineInteraction : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt => _powerUpObtained
            ? "You obtained a " + _obtainedPowerUp + "!"
            : _isPowerUpVendingMachineHacked
                ? "Press E again to take a snack from the machine"
                : "Press E to interact with the snack distributor";

        public bool IsInteractable => !_isBusy;

        [SerializeField] private float hackingTime = 4.2f;
        [SerializeField] private float itemUseAnimationTime = 6.0f;

        static System.Random _random = new System.Random();

        private PowerUp.PlayerPowerUpTypes _obtainedPowerUp;
        private bool _isPowerUpVendingMachineHacked = false;
        private bool _powerUpObtained = false;
        private bool _isBusy = false;
        private PlayerShoot _playerShoot;
        private Player _player;
        private PowerUp _powerUp;
        private RotateSphere _rotateSphere;

        private void Start()
        {
            _playerShoot = PlayerShoot.Instance;
            _player = Player.Instance;
            _powerUp = PowerUp.Instance;
            _rotateSphere = RotateSphere.Instance;
        }

        public bool Interact(GameObject interactor)
        {
            if (_isBusy)
            {
                return false;
            }

            if (_powerUp.playerPowerUps.Count <= 0)
            {
                Debug.Log("Vending machine is empty.");
                return false;
            }

            if (_isPowerUpVendingMachineHacked)
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
            _rotateSphere.positionSphere(new Vector3(0.7f, 1f, 0), RotateSphere.Animation.Linear);

            yield return new WaitForSeconds(hackingTime);

            _playerShoot.DecreaseStamina(1);
            _rotateSphere.isRotating = true;

            _isPowerUpVendingMachineHacked = true;

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

            Debug.Log("Getting power up: taking power up from the machine");

            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineItemPickUp,
                this.transform.position);
            _isPowerUpVendingMachineHacked = false;

            int powerUpIndexPlayer = _random.Next(_powerUp.playerPowerUps.Count);
            _obtainedPowerUp = _powerUp.playerPowerUps[powerUpIndexPlayer];

            if (_obtainedPowerUp == PowerUp.PlayerPowerUpTypes.HealthBoost)
            {
                Debug.Log("Using power up: health boost (chips)");
                AnimationManager.Instance.EatChips();
            }
            else if (_obtainedPowerUp == PowerUp.PlayerPowerUpTypes.DamageReduction)
            {
                Debug.Log("Using power up: damage reduction (energy drink)");
                AnimationManager.Instance.Drink();
            }

            yield return new WaitForSeconds(itemUseAnimationTime);

            if (_obtainedPowerUp == PowerUp.PlayerPowerUpTypes.HealthBoost)
            {
                _playerShoot.maxHealth += 20;
                _playerShoot.health += 20;
            }
            else if (_obtainedPowerUp == PowerUp.PlayerPowerUpTypes.DamageReduction)
            {
                _playerShoot.damageReduction -= 0.2f;
            }

            _powerUp.ObtainPowerUp(_obtainedPowerUp);
            _powerUp.playerPowerUps.RemoveAt(powerUpIndexPlayer);
            _powerUpObtained = true;

            _player.FreezeMovement(false);
            _playerShoot.DisableAttacks(false);
            _isBusy = false;
        }
    }
}