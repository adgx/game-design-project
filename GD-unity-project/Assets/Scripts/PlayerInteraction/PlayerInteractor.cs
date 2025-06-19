using System;
using TMPro;
using UnityEngine;

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

        [Tooltip("Key used to trigger an interaction.")] [SerializeField]
        private KeyCode _interactionKey = KeyCode.E;

        [Tooltip("Layer mask to filter which objects can be interacted with.")] [SerializeField]
        private LayerMask _interactionLayer;

        [Header("UI References")] [Tooltip("UI container that displays the help prompt.")] [SerializeField]
        private GameObject _helpTextContainer;

        [Tooltip("Text component that shows the interaction prompt.")] [SerializeField]
        private TextMeshProUGUI _helpText;

        private IInteractable _currentTarget;
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

            if (Physics.SphereCast(ray, 0.5f, out RaycastHit hit, _interactionDistance, _interactionLayer) &&
                hit.collider.TryGetComponent(out IInteractable interactable))
            {
                if (interactable != _currentTarget)
                {
                    _currentTarget = interactable;
                    ShowPrompt();
                }
            }
            else if (_currentTarget != null)
            {
                ClearTarget();
            }
        }

        /// <summary>
        /// Checks for input and triggers interaction on the current target if available.
        /// </summary>
        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(_interactionKey) && _currentTarget != null)
            {
                _currentTarget.Interact(this.gameObject);
            }
        }

        /// <summary>
        /// Clears the current interactable target and hides the prompt UI.
        /// </summary>
        private void ClearTarget()
        {
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