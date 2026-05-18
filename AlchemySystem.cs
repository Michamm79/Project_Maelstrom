using System.Collections.Generic;
using UnityEngine;

namespace OrbSystem
{
    /// <summary>
    /// Stateless lookup service for alchemy recipes.
    ///
    /// Alchemy is harder than transmutation: instead of matching exactly 2 inputs,
    /// it checks whether the player's available element pool *contains* all the
    /// required elements (with required quantities) for a given recipe.
    /// </summary>
    public static class AlchemySystem
    {
        private static List<AlchemyRecipe> recipes = new List<AlchemyRecipe>();
        private static bool initialized = false;

        /// <summary>
        /// Loads every AlchemyRecipe asset from a Resources folder.
        /// Place your recipe assets in: Assets/Resources/Recipes/Alchemy/
        /// </summary>
        public static void Initialize()
        {
            if (initialized) return;

            recipes.Clear();
            AlchemyRecipe[] loaded = Resources.LoadAll<AlchemyRecipe>("Recipes/Alchemy");
            recipes.AddRange(loaded);
            initialized = true;

            Debug.Log($"[AlchemySystem] Loaded {recipes.Count} recipes.");
        }

        public static void RegisterRecipes(IEnumerable<AlchemyRecipe> incoming)
        {
            recipes.Clear();
            recipes.AddRange(incoming);
            initialized = true;
        }

        /// <summary>
        /// Given an available pool of elements (mapped element → quantity available),
        /// returns every alchemy recipe the player can currently perform.
        ///
        /// Returning a list (not a single recipe) makes sense for alchemy because
        /// the same element pool can often produce multiple different outputs —
        /// that's exactly the choice the UI menu should present to the player.
        /// </summary>
        public static List<AlchemyRecipe> FindAvailableRecipes(
            Dictionary<ElementSO, int> availableElements,
            int playerLevel = int.MaxValue)
        {
            if (!initialized) Initialize();

            var matches = new List<AlchemyRecipe>();
            if (availableElements == null || availableElements.Count == 0) return matches;

            foreach (var recipe in recipes)
            {
                if (recipe == null) continue;
                if (recipe.requiredLevel > playerLevel) continue;
                if (CanFulfill(recipe, availableElements)) matches.Add(recipe);
            }

            return matches;
        }

        /// <summary>
        /// Checks if the available pool contains enough of every element
        /// required by the recipe.
        /// </summary>
        private static bool CanFulfill(AlchemyRecipe recipe, Dictionary<ElementSO, int> available)
        {
            foreach (var req in recipe.requiredElements)
            {
                if (req.element == null) continue;
                if (!available.TryGetValue(req.element, out int have)) return false;
                if (have < req.quantity) return false;
            }
            return true;
        }

        /// <summary>
        /// Consumes the elements required by a recipe from the provided pool.
        /// Call this when the player confirms an alchemy combination.
        /// Returns true if consumption succeeded; false if the pool didn't have enough.
        /// </summary>
        public static bool ConsumeElements(AlchemyRecipe recipe, Dictionary<ElementSO, int> pool)
        {
            if (!CanFulfill(recipe, pool)) return false;

            foreach (var req in recipe.requiredElements)
            {
                pool[req.element] -= req.quantity;
                if (pool[req.element] <= 0) pool.Remove(req.element);
            }
            return true;
        }
    }
}
