using UnityEngine;

namespace PlayerInteraction
{
    /// <summary>
    /// Defines the contract for any object that can be interacted with by the player.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// The message to display to the player when this object can be interacted with.
        /// </summary>
        string InteractionPrompt { get; }
        
        bool IsInteractable { get; }

        /// <summary>
        /// The method to be called when the player interacts with this object.
        /// </summary>
        /// <param name="interactor">The GameObject that initiated the interaction.</param>
        /// <returns>True if the interaction was successful, false otherwise.</returns>
        bool Interact(GameObject interactor);
    }
}