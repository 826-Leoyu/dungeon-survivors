using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>Health of one enemy. A pooled enemy gets a fresh instance each time it spawns.</summary>
    public sealed class EnemyHealth
    {
        public int Health { get; private set; }

        public bool IsDead => Health <= 0;

        public EnemyHealth(int maxHealth)
        {
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "Must be positive.");
            Health = maxHealth;
        }

        /// <summary>
        /// Returns true only for the hit that killed the enemy. Hits on an already-dead enemy are ignored and return false,
        /// so two bullets landing in the same frame count one kill.
        /// </summary>
        public bool TakeDamage(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Must be positive.");
            if (IsDead) return false;

            Health = Math.Max(0, Health - amount);
            return IsDead;
        }
    }
}
