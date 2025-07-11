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
        /// <summary>
        /// The prompt displayed to the player when they can collect the paper.
        /// </summary>
        public string InteractionPrompt => "Press E to collect paper";

        public Collider InteractionZone => null;

        /// <summary>
        /// Indicates whether this paper is currently interactable (not yet collected).
        /// </summary>
        public bool IsInteractable =>
            PaperManager.Instance != null;

        /// <summary>
        /// Called when the player interacts with the paper. Triggers paper collection.
        /// </summary>
        /// <param name="interactor">The GameObject that initiated the interaction.</param>
        /// <returns>True if the interaction was successful.</returns>
        public bool Interact(GameObject interactor)
        {
            PaperManager.Instance.ShowPaper(this.transform.position);
			Destroy(gameObject);
			return true;
        }
    }
}