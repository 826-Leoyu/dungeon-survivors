using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>Tuning numbers for one Run, as plain data (the presentation layer converts ScriptableObjects into this).</summary>
    public sealed class RunConfig
    {
        public int MaxHealth { get; }

        /// <summary>Seconds after taking damage during which further damage is ignored.</summary>
        public float HurtCooldown { get; }

        public RunConfig(int maxHealth, float hurtCooldown)
        {
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "Must be positive.");
            if (hurtCooldown < 0f) throw new ArgumentOutOfRangeException(nameof(hurtCooldown), hurtCooldown, "Must not be negative.");

            MaxHealth = maxHealth;
            HurtCooldown = hurtCooldown;
        }
    }
}
