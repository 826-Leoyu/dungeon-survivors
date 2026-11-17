using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// Tuning numbers for the orbiting knives' upgrades, as plain data: how much each pick raises a stat and how many times it can be picked.
    /// A max level of 0 means the upgrade is never offered.
    /// </summary>
    public sealed class OrbitingKnivesUpgradeConfig
    {
        /// <summary>Damage each damage upgrade adds to every hit.</summary>
        public int DamagePerLevel { get; }

        public int MaxDamageLevel { get; }

        /// <summary>Times the knife-count upgrade can be picked; each pick adds one knife.</summary>
        public int MaxKnifeCountLevel { get; }

        /// <summary>World units each radius upgrade adds to the orbit.</summary>
        public float RadiusPerLevel { get; }

        public int MaxRadiusLevel { get; }

        /// <summary>Degrees per second each rotation-speed upgrade adds.</summary>
        public float RotationSpeedPerLevel { get; }

        public int MaxRotationSpeedLevel { get; }

        public OrbitingKnivesUpgradeConfig(
            int damagePerLevel, int maxDamageLevel, int maxKnifeCountLevel, float radiusPerLevel, int maxRadiusLevel,
            float rotationSpeedPerLevel, int maxRotationSpeedLevel)
        {
            if (damagePerLevel <= 0) throw new ArgumentOutOfRangeException(nameof(damagePerLevel), damagePerLevel, "Must be positive.");
            if (maxDamageLevel < 0) throw new ArgumentOutOfRangeException(nameof(maxDamageLevel), maxDamageLevel, "Must not be negative.");
            if (maxKnifeCountLevel < 0) throw new ArgumentOutOfRangeException(nameof(maxKnifeCountLevel), maxKnifeCountLevel, "Must not be negative.");
            if (radiusPerLevel <= 0f) throw new ArgumentOutOfRangeException(nameof(radiusPerLevel), radiusPerLevel, "Must be positive.");
            if (maxRadiusLevel < 0) throw new ArgumentOutOfRangeException(nameof(maxRadiusLevel), maxRadiusLevel, "Must not be negative.");
            if (rotationSpeedPerLevel <= 0f)
                throw new ArgumentOutOfRangeException(nameof(rotationSpeedPerLevel), rotationSpeedPerLevel, "Must be positive.");
            if (maxRotationSpeedLevel < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRotationSpeedLevel), maxRotationSpeedLevel, "Must not be negative.");

            DamagePerLevel = damagePerLevel;
            MaxDamageLevel = maxDamageLevel;
            MaxKnifeCountLevel = maxKnifeCountLevel;
            RadiusPerLevel = radiusPerLevel;
            MaxRadiusLevel = maxRadiusLevel;
            RotationSpeedPerLevel = rotationSpeedPerLevel;
            MaxRotationSpeedLevel = maxRotationSpeedLevel;
        }
    }
}
