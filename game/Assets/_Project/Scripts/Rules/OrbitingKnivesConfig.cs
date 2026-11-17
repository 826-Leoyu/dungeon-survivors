using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>Tuning numbers for the orbiting knives, as plain data: their starting stats and how upgrades raise them.</summary>
    public sealed class OrbitingKnivesConfig
    {
        /// <summary>Damage each hit deals to an enemy.</summary>
        public int Damage { get; }

        /// <summary>Knives circling the player when the weapon is first picked.</summary>
        public int KnifeCount { get; }

        /// <summary>World units from the player to each knife.</summary>
        public float Radius { get; }

        /// <summary>Degrees per second the knives turn around the player (counter-clockwise).</summary>
        public float RotationSpeed { get; }

        /// <summary>Seconds after a hit before the knives can hit the same enemy again, however many knives touch it.</summary>
        public float HitInterval { get; }

        /// <summary>How the upgrades picked at level-up raise these numbers.</summary>
        public OrbitingKnivesUpgradeConfig Upgrades { get; }

        public OrbitingKnivesConfig(int damage, int knifeCount, float radius, float rotationSpeed, float hitInterval, OrbitingKnivesUpgradeConfig upgrades)
        {
            if (damage <= 0) throw new ArgumentOutOfRangeException(nameof(damage), damage, "Must be positive.");
            if (knifeCount <= 0) throw new ArgumentOutOfRangeException(nameof(knifeCount), knifeCount, "Must be positive.");
            if (radius <= 0f) throw new ArgumentOutOfRangeException(nameof(radius), radius, "Must be positive.");
            if (rotationSpeed <= 0f) throw new ArgumentOutOfRangeException(nameof(rotationSpeed), rotationSpeed, "Must be positive.");
            if (hitInterval <= 0f) throw new ArgumentOutOfRangeException(nameof(hitInterval), hitInterval, "Must be positive.");

            Damage = damage;
            KnifeCount = knifeCount;
            Radius = radius;
            RotationSpeed = rotationSpeed;
            HitInterval = hitInterval;
            Upgrades = upgrades ?? throw new ArgumentNullException(nameof(upgrades));
        }
    }
}
