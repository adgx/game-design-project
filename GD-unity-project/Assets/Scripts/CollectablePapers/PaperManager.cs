using System.Collections.Generic;
using System.Linq;
using Animations;
using Audio;
using TMPro;
using UnityEngine;

namespace CollectablePapers
{
    public class PaperManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance of the PaperManager.
        /// </summary>
        public static PaperManager Instance { get; private set; }

        [Header("UI & Player References")]
        [Tooltip("UI text element where the paper's content will be displayed.")]
        [SerializeField]
        private TextMeshProUGUI _paperText;

        [Tooltip("Container GameObject for displaying paper UI.")] [SerializeField]
        private GameObject _paperTextContainer;

        [Tooltip("Reference to the player to freeze movement while reading.")] [SerializeField]
        private Player _player;

        private Dictionary<int, string> _paperMessages;
        private HashSet<int> _collectedPapers = new();
        private bool _isPaperUiOpen = false;

        [SerializeField] private PlayerInput playerInput;
		[SerializeField] private RickEvents _rickEvents;

		[SerializeField] StartTutorial _startTutorial;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            LoadPaperData();
        }

        private void Start()
        {
            if (_paperTextContainer != null)
                _paperTextContainer.SetActive(false);

            playerInput = Player.Instance.GetComponent<PlayerInput>();
        }

        /// <summary>
        /// Loads paper message data from a JSON file in the Resources folder.
        /// </summary>
        private void LoadPaperData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("PaperMessages");

            if (jsonFile == null)
            {
                Debug.LogError("PaperManager: Could not find 'PaperMessages.json' in any Resources folder!");
                return;
            }

            PaperCollection paperCollection = JsonUtility.FromJson<PaperCollection>(jsonFile.text);
            _paperMessages = paperCollection.messages.ToDictionary(p => p.id, p => p.content);
        }

        private void Update()
        {
            if (_isPaperUiOpen && playerInput.InteractionPressed())
            {
                ClosePaperUI();
            }
        }

        /// <summary>
        /// Displays a paper's content on screen and freezes the player.
        /// </summary>
        /// <param name="paperPosition">World position of the paper (used for sound).</param>
        public void ShowPaper(Vector3 paperPosition)
        {
            if (_isPaperUiOpen) return;

            if (_paperMessages.TryGetValue(_collectedPapers.Count, out string messageContent))
            {
                _isPaperUiOpen = true;
                _collectedPapers.Add(_collectedPapers.Count);

                AnimationManager.Instance.Idle();
                _rickEvents.SetIdleState();
				_player.isFrozen = true;

				_paperText.SetText(messageContent + "\n\n<color=yellow>[Press E to Close]</color>");
                _paperTextContainer.SetActive(true);

				GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerPaperInteraction, paperPosition);
            }
            else
            {
                Debug.LogWarning($"PaperManager: Tried to show paper with invalid ID: {_collectedPapers.Count}");
            }
        }

        /// <summary>
        /// Closes the paper UI and unfreezes the player.
        /// </summary>
        private void ClosePaperUI()
        {
            if(_collectedPapers.Count <= 4) {
                StartCoroutine(_startTutorial.ShowTip(_collectedPapers.Count - 1));
            }
            _isPaperUiOpen = false;
            _paperTextContainer.SetActive(false);
            _player.isFrozen = false;
        }

        /// <summary>
        /// Checks whether a paper with the given ID has been collected.
        /// </summary>
        public bool IsPaperCollected(int paperID)
        {
            return _collectedPapers.Contains(paperID);
        }

        /// <summary>
        /// Tries to get the content of a paper by ID.
        /// </summary>
        public bool TryGetPaperContent(int paperID, out string content)
        {
            return _paperMessages.TryGetValue(paperID, out content);
        }

        /// <summary>
        /// Returns the total number of paper messages loaded.
        /// </summary>
        public int GetTotalPaperCount()
        {
            return _paperMessages?.Count ?? 0;
        }
    }
}