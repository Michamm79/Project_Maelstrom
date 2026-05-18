using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OrbSystem
{
    /// <summary>
    /// Runtime component attached to the player. Manages the two orb slots
    /// (Left/Right) and the player's alchemic element pool.
    ///
    /// Exposes three core actions:
    ///   - AddMaterialToOrb()      : pick up a material into a free orb
    ///   - TryTransmute()          : attempt to combine the two orb materials
    ///   - DecomposeMaterialAt()   : alchemically break a material into elements
    ///
    /// This script intentionally has no UI dependencies. UI listens to the
    /// UnityEvents and queries the public properties; the orb logic stays clean.
    /// </summary>
    public class OrbContainer : MonoBehaviour
    {
        public enum Hand { Left, Right }

        [Header("Player State")]
        [Tooltip("Player level — used to gate which recipes are usable.")]
        public int playerLevel = 1;

        [Tooltip("Level at which alchemic decomposition unlocks.")]
        public int alchemyUnlockLevel = 5;

        [Header("Spawn Settings")]
        [Tooltip("Where transmuted/alchemized objects spawn (e.g., in front of the player).")]
        public Transform spawnPoint;

        // ---- Runtime State ----
        private MaterialSO leftOrb;
        private MaterialSO rightOrb;
        private Dictionary<ElementSO, int> elementPool = new Dictionary<ElementSO, int>();

        // ---- Events for UI to subscribe to ----
        public UnityEvent OnOrbContentsChanged;
        public UnityEvent OnElementPoolChanged;

        // ---- Public Read-Only Accessors ----
        public MaterialSO LeftOrb => leftOrb;
        public MaterialSO RightOrb => rightOrb;
        public IReadOnlyDictionary<ElementSO, int> ElementPool => elementPool;
        public bool AlchemyUnlocked => playerLevel >= alchemyUnlockLevel;

        // ---------------------------------------------------------------
        // Adding materials
        // ---------------------------------------------------------------

        /// <summary>
        /// Place a material into the specified orb.
        /// Returns true if successful, false if the orb is already full.
        /// </summary>
        public bool AddMaterialToOrb(Hand hand, MaterialSO material)
        {
            if (material == null) return false;

            if (hand == Hand.Left)
            {
                if (leftOrb != null) return false;
                leftOrb = material;
            }
            else
            {
                if (rightOrb != null) return false;
                rightOrb = material;
            }

            OnOrbContentsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Auto-place into whichever orb is empty. Returns the hand used,
        /// or null if both orbs are full.
        /// </summary>
        public Hand? AddMaterialToFreeOrb(MaterialSO material)
        {
            if (leftOrb == null && AddMaterialToOrb(Hand.Left, material)) return Hand.Left;
            if (rightOrb == null && AddMaterialToOrb(Hand.Right, material)) return Hand.Right;
            return null;
        }

        public void ClearOrb(Hand hand)
        {
            if (hand == Hand.Left) leftOrb = null;
            else rightOrb = null;
            OnOrbContentsChanged?.Invoke();
        }

        // ---------------------------------------------------------------
        // Transmutation
        // ---------------------------------------------------------------

        /// <summary>
        /// Peek at what would result from transmuting the current orb contents.
        /// Useful for showing a preview in the UI before committing.
        /// Returns null if no valid recipe exists.
        /// </summary>
        public TransmutationRecipe PeekTransmutation()
        {
            if (leftOrb == null || rightOrb == null) return null;
            return TransmutationSystem.FindRecipe(leftOrb, rightOrb, playerLevel);
        }

        /// <summary>
        /// Commit to the transmutation: consumes both orb materials and spawns
        /// the result object at the spawn point.
        /// Returns the spawned GameObject, or null if no valid recipe.
        /// </summary>
        public GameObject TryTransmute()
        {
            var recipe = PeekTransmutation();
            if (recipe == null) return null;

            // Consume inputs
            leftOrb = null;
            rightOrb = null;
            OnOrbContentsChanged?.Invoke();

            // Spawn output
            return SpawnResult(recipe.resultPrefab);
        }

        // ---------------------------------------------------------------
        // Alchemy
        // ---------------------------------------------------------------

        /// <summary>
        /// Alchemically decompose the material in the specified orb into its
        /// constituent elements, which are added to the player's element pool.
        /// Locked behind alchemyUnlockLevel.
        /// </summary>
        public bool DecomposeMaterialAt(Hand hand)
        {
            if (!AlchemyUnlocked) return false;

            MaterialSO target = (hand == Hand.Left) ? leftOrb : rightOrb;
            if (target == null) return false;

            foreach (var comp in target.elementComposition)
            {
                if (comp.element == null || comp.quantity <= 0) continue;
                if (!elementPool.ContainsKey(comp.element)) elementPool[comp.element] = 0;
                elementPool[comp.element] += comp.quantity;
            }

            ClearOrb(hand);
            OnElementPoolChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Get every alchemy recipe the player can currently afford with their
        /// element pool. Use this to populate the alchemy menu.
        /// </summary>
        public List<AlchemyRecipe> GetAvailableAlchemyRecipes()
        {
            if (!AlchemyUnlocked) return new List<AlchemyRecipe>();
            return AlchemySystem.FindAvailableRecipes(elementPool, playerLevel);
        }

        /// <summary>
        /// Commit to an alchemy recipe: consumes the required elements and
        /// spawns the result. Returns the spawned GameObject, or null on failure.
        /// </summary>
        public GameObject TryAlchemize(AlchemyRecipe recipe)
        {
            if (recipe == null || !AlchemyUnlocked) return null;
            if (!AlchemySystem.ConsumeElements(recipe, elementPool)) return null;

            OnElementPoolChanged?.Invoke();
            return SpawnResult(recipe.resultPrefab);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private GameObject SpawnResult(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[OrbContainer] Recipe result prefab is null.");
                return null;
            }

            Transform t = spawnPoint != null ? spawnPoint : transform;
            return Instantiate(prefab, t.position, t.rotation);
        }
    }
}
