using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>Tuning numbers for one kind of enemy, as plain data.</summary>
    public sealed class EnemyConfig
    {
        public int MaxHealth { get; }

        /// <summary>World units per second.</summary>
        public float MoveSpeed { get; }

        /// <summary>Damage reported to the Run while the enemy touches the player; the hurt cooldown decides which hits land.</summary>
        public int ContactDamage { get; }

        public EnemyConfig(int maxHealth, float moveSpeed, int contactDamage)
        {
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "Must be positive.");
            if (moveSpeed < 0f) throw new ArgumentOutOfRangeException(nameof(moveSpeed), moveSpeed, "Must not be negative.");
            if (contactDamage <= 0) throw new ArgumentOutOfRangeException(nameof(contactDamage), contactDamage, "Must be positive.");

            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            ContactDamage = contactDamage;
        }
    }
}
