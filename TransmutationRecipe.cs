using UnityEngine;

namespace OrbSystem
{
    /// <summary>
    /// A transmutation recipe: two input materials produce one output object.
    ///
    /// Order-independent: (stick, stone) and (stone, stick) match the same recipe.
    /// The lookup in TransmutationSystem handles this normalization.
    ///
    /// For v1, each pair produces exactly one result. When you're ready to support
    /// up to 3 results per pair, change `resultPrefab` to a List<GameObject> and
    /// update the lookup to return the list.
    ///
    /// Create via: Right-click in Project window →
    /// Create → OrbSystem → Transmutation Recipe
    /// </summary>
    [CreateAssetMenu(fileName = "NewTransmutationRecipe", menuName = "OrbSystem/Transmutation Recipe", order = 2)]
    public class TransmutationRecipe : ScriptableObject
    {
        [Header("Inputs")]
        public MaterialSO inputA;
        public MaterialSO inputB;

        [Header("Output")]
        [Tooltip("Prefab of the object produced by this combination.")]
        public GameObject resultPrefab;

        [Tooltip("Display name of the resulting object (e.g., 'Hammer').")]
        public string resultDisplayName;

        public Sprite resultIcon;

        [Header("Gating")]
        [Tooltip("Minimum player level required to perform this transmutation.")]
        public int requiredLevel = 1;
    }
}
