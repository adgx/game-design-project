using System;

namespace CollectablePapers
{
    /// <summary>
    /// Represents a single collectible paper with an ID and associated text content.
    /// </summary>
    [Serializable]
    public class PaperEntry
    {
        /// <summary>
        /// Unique identifier for this paper entry.
        /// </summary>
        public int id;

        /// <summary>
        /// The textual content of the paper.
        /// </summary>
        public string content;
    }

    /// <summary>
    /// A container class holding a collection of paper entries.
    /// Used for serialization (e.g., loading from JSON).
    /// </summary>
    [Serializable]
    public class PaperCollection
    {
        /// <summary>
        /// Array of collectible paper entries.
        /// </summary>
        public PaperEntry[] messages;
    }
}