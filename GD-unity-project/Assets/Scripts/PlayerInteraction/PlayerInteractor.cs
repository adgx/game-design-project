using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private IInteractable _currentTarget;
        private Player _player;
        private PlayerInput _playerInput;
        private Camera _mainCamera;

        /// <summary>
        /// Gets the main camera reference on Awake.
        /// </summary>
        private void Awake()
        {
            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogError("PlayerInteractor: No camera tagged 'MainCamera' was found in the scene!");
			}
        }

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

            FindInteractable();
            HandleInteractionInput();
        }

        /// <summary>
        /// Performs a sphere cast from the camera forward to detect interactable objects.
        /// Updates the current target and shows the prompt if a new interactable is found.
        /// </summary>
        private void FindInteractable()
        {
            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);

            if (Physics.SphereCast(ray, 1.5f, out RaycastHit hit, _interactionDistance, _interactionLayer) &&
                hit.collider.TryGetComponent(out IInteractable interactable))
            {
                if (IsValidTarget(interactable))
                {
                    if (interactable == _currentTarget) return;
                    
                    _currentTarget = interactable;
                    ShowPrompt();
                }
                else
                {
                    ClearTarget();
                }

                return;
            }
            
            Vector3 behindStartPoint = _mainCamera.transform.position - _mainCamera.transform.forward * 1.0f; 
            Ray behindRay = new Ray(behindStartPoint, _mainCamera.transform.forward);
            
            if (Physics.Raycast(behindRay, out RaycastHit behindHit, _interactionDistance + 1.0f, _interactionLayer) &&
                behindHit.collider.TryGetComponent(out IInteractable secondInteractable))
            {
                if (secondInteractable.IsInteractable)
                {
                    if (secondInteractable == _currentTarget) return;
            
                    _currentTarget = secondInteractable;
                    ShowPrompt();
                }
                else
                {
                    ClearTarget();
                }
                
                return;
            }
            
            ClearTarget();
        }

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
        /// Checks for input and triggers interaction on the current target if available.
        /// </summary>
        private void HandleInteractionInput()
        {
            if (_playerInput.InteractionPressed() && _currentTarget != null)
            {
                _currentTarget.Interact(this.gameObject);
            }
        }

        /// <summary>
        /// Clears the current interactable target and hides the prompt UI.
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
        /// Displays the help prompt UI with the current target's interaction text.
        /// </summary>
        private void ShowPrompt()
        {
            if (_helpTextContainer != null && _currentTarget != null)
            {
                _helpTextContainer.SetActive(true);
                _helpText.text = _currentTarget.InteractionPrompt;
            }
        }
    }
}