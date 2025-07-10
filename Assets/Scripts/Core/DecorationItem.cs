using UnityEngine;

namespace LifeCraft.Core
{
    /// <summary>
    /// Represents a decoration item that can be owned by the player and placed in their city.
    /// This is a data-only class (not a MonoBehaviour or ScriptableObject).
    /// </summary>
    [System.Serializable]
    public class DecorationItem
    {
        [Header("Basic Info")]
        public string id; // Unique identifier for this instance of the decoration
        public string displayName; // Name shown to the player
        public string description; // Optional: description for tooltips or details
        
        [Header("Visual")]
        public Sprite icon; // Sprite for the decoration (can be null for now)
        public Color iconColor = Color.white; // Tint for the icon (for rarity, etc.)
        
        [Header("Properties")]
        public DecorationRarity rarity = DecorationRarity.Common; // Rarity for sorting/filtering
        public Vector2Int size = Vector2Int.one; // Size in grid cells (2x2 for now, can be changed later)
        public bool isPremium = false; // True if won from a premium chest
        
        [Header("Metadata")]
        public System.DateTime dateAcquired; // When the player got this item
        public string source; // Where the item came from ("DecorChest", "PremiumDecorChest", etc.)
        
        /// <summary>
        /// Create a new decoration item (used when adding to inventory)
        /// </summary>
        public DecorationItem(string name, string desc = "", DecorationRarity rar = DecorationRarity.Common, bool premium = false)
        {
            id = System.Guid.NewGuid().ToString(); // Generate a unique ID
            displayName = name;
            description = desc;
            rarity = rar;
            isPremium = premium;
            dateAcquired = System.DateTime.Now;
            source = "Unknown";
        }
        
        /// <summary>
        /// Clone this decoration item (for safe copies)
        /// </summary>
        public DecorationItem Clone()
        {
            return new DecorationItem(displayName, description, rarity, isPremium)
            {
                icon = this.icon,
                iconColor = this.iconColor,
                size = this.size,
                source = this.source
            };
        }
    }
    
    /// <summary>
    /// Rarity levels for decorations (affects color, filtering, etc.)
    /// </summary>
    public enum DecorationRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
} 