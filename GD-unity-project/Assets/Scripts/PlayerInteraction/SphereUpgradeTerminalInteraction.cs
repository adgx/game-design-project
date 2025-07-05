using System.Collections;
using Audio;
using UnityEngine;

namespace PlayerInteraction
{
    public class SphereUpgradeTerminalInteraction : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt => _powerUpObtained
            ? "You obtained a " + _obtainedPowerUp.ToString() + "!"
            : (_powerUp != null && _powerUp.spherePowerUps.Count <= 0)
                ? "Terminal is empty"
                : "Press E to interact with the terminal";

        public bool IsInteractable =>
            !_isBusy && (_powerUp != null && _powerUp.spherePowerUps.Count > 0);

        [Header("Timings")] [SerializeField] private float _interactionTime = 2.0f;
        [SerializeField] private float _postInteractionDelay = 1.5f;
        [SerializeField] private float _rotationDuration = 0.2f;

        [Header("UI Feedback")]
        [Tooltip("How long the 'You obtained a ...' message should display before resetting.")]
        [SerializeField]
        private float _feedbackMessageDuration = 3.0f;

        private PowerUp.SpherePowerUpTypes _obtainedPowerUp;
        private bool _powerUpObtained = false;
        private bool _isBusy;
        
        private Player _player;
        private PlayerShoot _playerShoot;
        private PowerUp _powerUp;
        private RotateSphere _rotateSphere;
        static System.Random _random = new System.Random();

        private void Start()
        {
            _player = Player.Instance;
            _playerShoot = PlayerShoot.Instance;
            _powerUp = PowerUp.Instance;
            _rotateSphere = RotateSphere.Instance;
        }

        public bool Interact(GameObject interactor)
        {
            if (!IsInteractable) return false;
            
            StartCoroutine(RotatePlayerTowards(transform, _rotationDuration));
            AnimationManager.Instance.Idle();

            StartCoroutine(UpgradeSequence());
            return true;
        }

        private IEnumerator UpgradeSequence()
        {
            _isBusy = true;
            _player.FreezeMovement(true);
            _playerShoot.DisableAttacks(true);

            _rotateSphere.positionSphere(new Vector3(_rotateSphere.DistanceFromPlayer, 1f, 0),
                RotateSphere.Animation.Linear);
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerTerminalInteraction,
                this.transform.position);

            yield return new WaitForSeconds(_interactionTime);

            int powerUpIndex = _random.Next(_powerUp.spherePowerUps.Count);
            _obtainedPowerUp = _powerUp.spherePowerUps[powerUpIndex];
            _powerUp.ObtainPowerUp(_obtainedPowerUp);
            _powerUp.spherePowerUps.RemoveAt(powerUpIndex);

            StartCoroutine(ShowFeedbackMessage());

            _player.FreezeMovement(false);
            _playerShoot.DisableAttacks(false);

            yield return new WaitForSeconds(_postInteractionDelay);

            _playerShoot.DecreaseStamina(1);
            _rotateSphere.isRotating = true;

            _isBusy = false;
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
            yield return new WaitForSeconds(_feedbackMessageDuration);
            _powerUpObtained = false;
            _isBusy = true;
        }
    }
}