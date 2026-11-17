using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>Tuning numbers for a weapon that fires bullets, as plain data: its starting stats and how upgrades raise them.</summary>
    public sealed class WeaponConfig
    {
        /// <summary>Damage each bullet deals to the enemy it hits.</summary>
        public int Damage { get; }

        /// <summary>Seconds between shots.</summary>
        public float FireInterval { get; }

        /// <summary>World units per second.</summary>
        public float BulletSpeed { get; }

        /// <summary>Seconds before a bullet that hit nothing goes back to the pool. Its range is speed x lifetime.</summary>
        public float BulletLifetime { get; }

        /// <summary>Degrees between neighbouring bullets when a volley fires several of them as a fan.</summary>
        public float SpreadDegrees { get; }

        /// <summary>How the upgrades picked at level-up raise these numbers.</summary>
        public WeaponUpgradeConfig Upgrades { get; }

        public WeaponConfig(int damage, float fireInterval, float bulletSpeed, float bulletLifetime, float spreadDegrees, WeaponUpgradeConfig upgrades)
        {
            if (damage <= 0) throw new ArgumentOutOfRangeException(nameof(damage), damage, "Must be positive.");
            if (fireInterval <= 0f) throw new ArgumentOutOfRangeException(nameof(fireInterval), fireInterval, "Must be positive.");
            if (bulletSpeed <= 0f) throw new ArgumentOutOfRangeException(nameof(bulletSpeed), bulletSpeed, "Must be positive.");
            if (bulletLifetime <= 0f) throw new ArgumentOutOfRangeException(nameof(bulletLifetime), bulletLifetime, "Must be positive.");
            if (spreadDegrees < 0f) throw new ArgumentOutOfRangeException(nameof(spreadDegrees), spreadDegrees, "Must not be negative.");

            Damage = damage;
            FireInterval = fireInterval;
            BulletSpeed = bulletSpeed;
            BulletLifetime = bulletLifetime;
            SpreadDegrees = spreadDegrees;
            Upgrades = upgrades ?? throw new ArgumentNullException(nameof(upgrades));
        }
    }
}
