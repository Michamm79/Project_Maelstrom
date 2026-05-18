using System.Collections.Generic;
using UnityEngine;

namespace OrbSystem
{
    /// <summary>
    /// Stateless lookup service for transmutation recipes.
    ///
    /// Holds a registry of all TransmutationRecipe assets (populated at startup
    /// via LoadAllRecipes or assigned in the inspector on a registry MonoBehaviour).
    ///
    /// This class is intentionally NOT a MonoBehaviour. It's pure logic: data in,
    /// data out. That makes it trivial to unit-test and reuse anywhere.
    /// </summary>
    public static class TransmutationSystem
    {
        private static List<TransmutationRecipe> recipes = new List<TransmutationRecipe>();
        private static bool initialized = false;

        /// <summary>
        /// Loads every TransmutationRecipe asset from a Resources folder.
        /// Place your recipe assets in: Assets/Resources/Recipes/Transmutation/
        ///
        /// Alternative: call RegisterRecipes() manually if you don't want to use Resources.
        /// </summary>
        public static void Initialize()
        {
            if (initialized) return;

            recipes.Clear();
            TransmutationRecipe[] loaded = Resources.LoadAll<TransmutationRecipe>("Recipes/Transmutation");
            recipes.AddRange(loaded);
            initialized = true;

            Debug.Log($"[TransmutationSystem] Loaded {recipes.Count} recipes.");
        }

        /// <summary>
        /// Manual registration alternative — useful if recipes are managed by a
        /// MonoBehaviour registry rather than the Resources folder.
        /// </summary>
        public static void RegisterRecipes(IEnumerable<TransmutationRecipe> incoming)
        {
            recipes.Clear();
            recipes.AddRange(incoming);
            initialized = true;
        }

        /// <summary>
        /// Find a recipe matching the two input materials (order-independent).
        /// Returns null if no recipe exists for this pair.
        ///
        /// If a level is provided, recipes above that level are filtered out.
        /// </summary>
        public static TransmutationRecipe FindRecipe(MaterialSO a, MaterialSO b, int playerLevel = int.MaxValue)
        {
            if (!initialized) Initialize();
            if (a == null || b == null) return null;

            foreach (var recipe in recipes)
            {
                if (recipe == null) continue;
                if (recipe.requiredLevel > playerLevel) continue;

                bool match =
                    (recipe.inputA == a && recipe.inputB == b) ||
                    (recipe.inputA == b && recipe.inputB == a);

                if (match) return recipe;
            }

            return null;
        }
    }
}
