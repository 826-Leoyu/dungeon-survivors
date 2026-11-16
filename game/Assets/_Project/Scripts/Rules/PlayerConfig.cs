using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>Tuning numbers for the player, as plain data.</summary>
    public sealed class PlayerConfig
    {
        public int MaxHealth { get; }

        /// <summary>Seconds after taking damage during which further damage is ignored.</summary>
        public float HurtCooldown { get; }

        /// <summary>World units per second at full stick.</summary>
        public float MoveSpeed { get; }

        /// <summary>World units: an XP gem closer than this to the player is picked up.</summary>
        public float PickupRadius { get; }

        public PlayerConfig(int maxHealth, float hurtCooldown, float moveSpeed, float pickupRadius)
        {
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "Must be positive.");
            if (hurtCooldown < 0f) throw new ArgumentOutOfRangeException(nameof(hurtCooldown), hurtCooldown, "Must not be negative.");
            if (moveSpeed < 0f) throw new ArgumentOutOfRangeException(nameof(moveSpeed), moveSpeed, "Must not be negative.");
            if (pickupRadius <= 0f) throw new ArgumentOutOfRangeException(nameof(pickupRadius), pickupRadius, "Must be positive.");

            MaxHealth = maxHealth;
            HurtCooldown = hurtCooldown;
            MoveSpeed = moveSpeed;
            PickupRadius = pickupRadius;
        }
    }
}
