using CollectablePapers;
using UnityEngine;

namespace PlayerInteraction
{
    /// <summary>
    /// Handles player interaction with collectible papers in the game world.
    /// Implements the IInteractable interface to allow interaction via PlayerInteractor.
    /// </summary>
    public class PaperInteraction : MonoBehaviour, IInteractable
    {
        // Flag used to track whether the paper has already been collected
        private bool isCollected = false;
        
        [Tooltip("To be set to 0 for the first paper and 1 for the second, only in the initial room. Leave -1 for everyone else.")]
        [SerializeField]
        public int paperIndex = -1;
        
        /// <summary>
        /// The prompt displayed to the player when they can collect the paper.
        /// </summary>
        public string InteractionPrompt => "Press E to collect paper";

        public Collider InteractionZone => null;

        /// <summary>
        /// Indicates whether this paper is currently interactable (i.e. not yet collected).
        /// </summary>
        public bool IsInteractable =>
            PaperManager.Instance != null && !isCollected;

        /// <summary>
        /// Called when the player interacts with the paper. Triggers paper collection.
        /// </summary>
        /// <param name="interactor">The GameObject that initiated the interaction.</param>
        /// <returns>True if the interaction was successful.</returns>
        public bool Interact(GameObject interactor)
        {
            // If the paper has already been collected, we will stop execution immediately
            if (isCollected)
            {
                return false;
            }

            // As soon as the valid interaction begins, we lock the paper, preventing further calls to this method
            // from taking effect, even in the same frame or in those immediately following
            isCollected = true;
            
            if (RoomManager.RoomManager.Instance != null)
            {
                RoomManager.RoomManager.Instance.MarkPaperAsCollectedInCurrentRoom(paperIndex);
            }
            
            PaperManager.Instance.ShowPaper(this.transform.position);
			gameObject.SetActive(false);
			return true;
        }
    }
}