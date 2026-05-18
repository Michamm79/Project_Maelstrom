using System.Collections.Generic;
using UnityEngine;

namespace OrbSystem
{
    /// <summary>
    /// Defines a collectible material in the world (stick, stone, iron ore, etc.).
    /// Each material can be transmuted with other materials, or alchemically
    /// broken down into its constituent elements.
    ///
    /// Create new materials via: Right-click in Project window →
    /// Create → OrbSystem → Material
    /// </summary>
    [CreateAssetMenu(fileName = "NewMaterial", menuName = "OrbSystem/Material", order = 0)]
    public class MaterialSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name shown in UI.")]
        public string displayName;

        [Tooltip("Stable unique ID. Do NOT change after creation — recipes reference this.")]
        public string materialID;

        [TextArea(2, 4)]
        public string description;

        [Header("Visuals")]
        public Sprite icon;
        public GameObject worldPrefab; // prefab for the pickup in-world

        [Header("Tags")]
        [Tooltip("Free-form tags for categorization: Wood, Metal, Organic, etc.")]
        public List<string> tags = new List<string>();

        [Header("Alchemic Composition")]
        [Tooltip("What elements this material breaks down into when alchemically decomposed.")]
        public List<ElementQuantity> elementComposition = new List<ElementQuantity>();
    }

    /// <summary>
    /// Pairs an element with a quantity. Used inside MaterialSO to describe
    /// what a material breaks down into.
    /// </summary>
    [System.Serializable]
    public struct ElementQuantity
    {
        public ElementSO element;
        public int quantity;
    }
}
