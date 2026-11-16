using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// Tuning numbers for a bullet weapon's upgrades, as plain data: how much each pick raises a stat and how many times it can be picked.
    /// A max level of 0 means the upgrade is never offered.
    /// </summary>
    public sealed class WeaponUpgradeConfig
    {
        /// <summary>Damage each damage upgrade adds to every bullet.</summary>
        public int DamagePerLevel { get; }

        public int MaxDamageLevel { get; }

        /// <summary>Times the bullet-count upgrade can be picked; each pick adds one bullet to every volley.</summary>
        public int MaxBulletCountLevel { get; }

        /// <summary>Each fire-rate upgrade multiplies the seconds between shots by this, so it is between 0 and 1.</summary>
        public float FireIntervalMultiplier { get; }

        public int MaxFireRateLevel { get; }

        public WeaponUpgradeConfig(int damagePerLevel, int maxDamageLevel, int maxBulletCountLevel, float fireIntervalMultiplier, int maxFireRateLevel)
        {
            if (damagePerLevel <= 0) throw new ArgumentOutOfRangeException(nameof(damagePerLevel), damagePerLevel, "Must be positive.");
            if (maxDamageLevel < 0) throw new ArgumentOutOfRangeException(nameof(maxDamageLevel), maxDamageLevel, "Must not be negative.");
            if (maxBulletCountLevel < 0) throw new ArgumentOutOfRangeException(nameof(maxBulletCountLevel), maxBulletCountLevel, "Must not be negative.");
            if (fireIntervalMultiplier <= 0f || fireIntervalMultiplier >= 1f)
                throw new ArgumentOutOfRangeException(nameof(fireIntervalMultiplier), fireIntervalMultiplier, "Must be between 0 and 1, exclusive.");
            if (maxFireRateLevel < 0) throw new ArgumentOutOfRangeException(nameof(maxFireRateLevel), maxFireRateLevel, "Must not be negative.");

            DamagePerLevel = damagePerLevel;
            MaxDamageLevel = maxDamageLevel;
            MaxBulletCountLevel = maxBulletCountLevel;
            FireIntervalMultiplier = fireIntervalMultiplier;
            MaxFireRateLevel = maxFireRateLevel;
        }
    }
}
