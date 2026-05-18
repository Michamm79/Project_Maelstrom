using System.Collections.Generic;
using UnityEngine;

namespace OrbSystem
{
    /// <summary>
    /// An alchemy recipe: a set of elements (with required quantities) produces
    /// one output object.
    ///
    /// Unlike transmutation (which pairs exactly 2 materials), alchemy can
    /// combine any number of elements with any quantities — e.g., 2 Pyron + 1 Air
    /// → fireball.
    ///
    /// Create via: Right-click in Project window →
    /// Create → OrbSystem → Alchemy Recipe
    /// </summary>
    [CreateAssetMenu(fileName = "NewAlchemyRecipe", menuName = "OrbSystem/Alchemy Recipe", order = 3)]
    public class AlchemyRecipe : ScriptableObject
    {
        [Header("Inputs")]
        [Tooltip("Elements required (with quantities) to produce the output.")]
        public List<ElementQuantity> requiredElements = new List<ElementQuantity>();

        [Header("Output")]
        public GameObject resultPrefab;
        public string resultDisplayName;
        public Sprite resultIcon;

        [Header("Gating")]
        [Tooltip("Minimum player level required to perform this alchemy.")]
        public int requiredLevel = 5;
    }
}
