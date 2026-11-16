using System;
using System.Collections.Generic;

namespace DungeonSurvivors.Rules
{
    /// <summary>Draws items from a pool at random without repeats, using the injected <see cref="IRandom"/> so a seed reproduces the draw.</summary>
    public static class DistinctDraw
    {
        /// <summary>
        /// Returns <paramref name="count"/> different items from <paramref name="pool"/>, in random order;
        /// every item when the pool has fewer than that. The pool is left untouched.
        /// </summary>
        public static List<T> Pick<T>(IReadOnlyList<T> pool, int count, IRandom random)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), count, "Must not be negative.");

            // Partial Fisher-Yates shuffle: each step swaps a random not-yet-drawn item into the next drawn slot.
            var items = new List<T>(pool);
            var drawn = Math.Min(count, items.Count);
            for (var i = 0; i < drawn; i++)
            {
                var j = random.Next(i, items.Count);
                (items[i], items[j]) = (items[j], items[i]);
            }

            items.RemoveRange(drawn, items.Count - drawn);
            return items;
        }
    }
}
