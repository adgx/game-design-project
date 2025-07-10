using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace PlayerInteraction
{
    /// <summary>
    /// Handles detecting and interacting with IInteractable objects in the scene.
    /// Displays a help prompt when looking at interactable objects.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Interaction Configuration")]
        [Tooltip("Maximum distance at which the player can interact with objects.")]
        [SerializeField]
        private float _interactionDistance = 6f;

        [Tooltip("Layer mask to filter which objects can be interacted with.")] [SerializeField]
        private LayerMask _interactionLayer;

        [Header("UI References")] [Tooltip("UI container that displays the help prompt.")] [SerializeField]
        private GameObject _helpTextContainer;

        [Tooltip("Text component that shows the interaction prompt.")] [SerializeField]
        private TextMeshProUGUI _helpText;

        [Header("Forgiveness & Feel")]
        [Tooltip("How long (in seconds) the target will remain 'sticky' after looking away from it.")]
        [SerializeField]
        private float _interactionGracePeriod = 0.15f;

        /// <summary>
        /// Tracks the time since the last interactable was hit, used for grace period handling.
        /// </summary>
        private float _timeSinceLastHit = 0f;

        /// <summary>
        /// The current interactable object the player is targeting.
        /// </summary>
        private IInteractable _currentTarget;

        /// <summary>
        /// Reference to the player instance.
        /// </summary>
        private Player _player;

        /// <summary>
        /// Reference to the PlayerInput instance for handling inputs.
        /// </summary>
        private PlayerInput _playerInput;

        /// <summary>
        /// Cached reference to the main camera.
        /// </summary>
        private Camera _mainCamera;

        /// <summary>
        /// Gets the main camera reference on Awake.
        /// Logs an error if the main camera is not found.
        /// </summary>
        private void Awake()
        {
            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogError("PlayerInteractor: No camera tagged 'MainCamera' was found in the scene!");
            }
        }

        /// <summary>
        /// Initializes references to Player and PlayerInput singletons.
        /// </summary>
        private void Start()
        {
            _playerInput = PlayerInput.Instance;
            _player = Player.Instance;
        }

        /// <summary>
        /// Called once per frame. Handles detection and input logic.
        /// </summary>
        private void Update()
        {
            if (_mainCamera == null)
            {
                Debug.LogWarning("PlayerInteractor cannot run because _mainCamera is null!");
                return;
            }

            Debug.DrawRay(_mainCamera.transform.position, _mainCamera.transform.forward * _interactionDistance,
                Color.red);

            FindInteractable();
            HandleInteractionInput();
        }

        /// <summary>
        /// Performs multi-stage detection:
        /// - First checks a close-proximity sphere cast (for large objects).
        /// - Then performs a long-range raycast (for distant precision objects).
        /// - Finally checks a nearby sphere overlap (as fallback).
        /// </summary>
        private void FindInteractable()
        {
            float proximityRadius = 2f;
            float proximityDistance = 1.5f;

            if (Physics.SphereCast(_mainCamera.transform.position, proximityRadius, _mainCamera.transform.forward,
                    out RaycastHit proximityHit, proximityDistance, _interactionLayer) &&
                proximityHit.collider.TryGetComponent(out IInteractable proximityTarget) &&
                IsValidTarget(proximityTarget))
            {
                SetCurrentTarget(proximityTarget);
                _timeSinceLastHit = 0.0f;
                return;
            }

            Vector3 aimStartPoint = _mainCamera.transform.position - _mainCamera.transform.forward;
            Ray aimRay = new Ray(aimStartPoint, _mainCamera.transform.forward);

            if (Physics.Raycast(aimRay, out RaycastHit aimingHit, _interactionDistance, _interactionLayer) &&
                aimingHit.collider.TryGetComponent(out IInteractable aimingTarget) && IsValidTarget(aimingTarget))
            {
                SetCurrentTarget(aimingTarget);
                _timeSinceLastHit = 0.0f;
                return;
            }

            Vector3 sphereCenter = _mainCamera.transform.position + _mainCamera.transform.forward * 1.3f;
            Collider[] overlaps = Physics.OverlapSphere(sphereCenter, 1.0f, _interactionLayer);

            foreach (Collider col in overlaps)
            {
                if (col == null || col.gameObject == null) continue;

                if (col.TryGetComponent(out IInteractable nearbyTarget) && IsValidTarget(nearbyTarget))
                {
                    SetCurrentTarget(nearbyTarget);
                    _timeSinceLastHit = 0.0f;
                    return;
                }
            }

            _timeSinceLastHit += Time.deltaTime;

            if (_timeSinceLastHit > _interactionGracePeriod)
            {
                ClearTarget();
            }
        }

        /// <summary>
        /// Sets the current interactable target and updates the prompt.
        /// </summary>
        /// <param name="newTarget">The new interactable object to set as target.</param>
        private void SetCurrentTarget(IInteractable newTarget)
        {
            if (newTarget == _currentTarget) return;

            _currentTarget = newTarget;
            ShowPrompt();
        }

        /// <summary>
        /// Validates if a target is interactable and the player is within its interaction zone (if defined).
        /// </summary>
        /// <param name="target">The IInteractable object to validate.</param>
        /// <returns>True if the target is interactable and valid.</returns>
        private bool IsValidTarget(IInteractable target)
        {
            if (!target.IsInteractable) return false;

            Collider requiredZone = target.InteractionZone;

            if (requiredZone == null) return true;

            Collider playerCollider = _player.GetComponent<Collider>();

            if (playerCollider == null)
            {
                Debug.LogWarning("PlayerInteractor: Player is missing a Collider component for zone checks.");
                return false;
            }

            return requiredZone.bounds.Intersects(playerCollider.bounds);
        }

        /// <summary>
        /// Checks for interaction input and calls Interact() on the current target.
        /// </summary>
        private void HandleInteractionInput()
        {
            if (_playerInput.InteractionPressed() && _currentTarget != null)
            {
                _currentTarget.Interact(this.gameObject);
            }
        }

        /// <summary>
        /// Clears the current target and hides the interaction UI prompt.
        /// </summary>
        private void ClearTarget()
        {
            if (_currentTarget == null) return;

            _currentTarget = null;

            if (_helpTextContainer != null)
            {
                _helpTextContainer.SetActive(false);
            }
        }

        /// <summary>
        /// Shows the help prompt UI and updates it with the current target's interaction text.
        /// </summary>
        private void ShowPrompt()
        {
            if (_helpTextContainer != null && _currentTarget != null)
            {
                _helpTextContainer.SetActive(true);
                _helpText.text = _currentTarget.InteractionPrompt;
            }
        }

        /// <summary>
        /// Draws debug gizmos in the Scene view for interaction rays and target lines.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (_mainCamera != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(_mainCamera.transform.position, _mainCamera.transform.forward * _interactionDistance);

                if (_currentTarget != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(_mainCamera.transform.position, ((MonoBehaviour)_currentTarget).transform.position);
                }
            }
        }
    }
}