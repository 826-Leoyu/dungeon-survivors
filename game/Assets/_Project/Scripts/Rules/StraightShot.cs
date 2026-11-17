using System;
using System.Collections.Generic;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// The straight shot's current numbers: its starting numbers raised by the upgrades picked so far.
    /// The weapon reads these every frame, so a pick takes effect straight away.
    /// </summary>
    public sealed class StraightShot : IUpgradeable
    {
        /// <summary>Every upgrade this weapon offers.</summary>
        public static readonly IReadOnlyList<Upgrade> Upgrades = new[] { Upgrade.StraightShotDamage, Upgrade.StraightShotBulletCount, Upgrade.StraightShotFireRate };

        private readonly WeaponConfig config;
        private int damageLevel;
        private int bulletCountLevel;
        private int fireRateLevel;

        internal StraightShot(WeaponConfig config)
        {
            this.config = config;
        }

        /// <summary>Damage each bullet deals.</summary>
        public int Damage => config.Damage + config.Upgrades.DamagePerLevel * damageLevel;

        /// <summary>Bullets fired in each volley, spread as a fan.</summary>
        public int BulletCount => 1 + bulletCountLevel;

        /// <summary>Seconds between volleys.</summary>
        public float FireInterval => config.FireInterval * (float)Math.Pow(config.Upgrades.FireIntervalMultiplier, fireRateLevel);

        bool IUpgradeable.Owns(Upgrade upgrade)
        {
            foreach (var candidate in Upgrades)
            {
                if (candidate == upgrade) return true;
            }

            return false;
        }

        void IUpgradeable.Apply(Upgrade upgrade)
        {
            switch (upgrade)
            {
                case Upgrade.StraightShotDamage: damageLevel++; break;
                case Upgrade.StraightShotBulletCount: bulletCountLevel++; break;
                case Upgrade.StraightShotFireRate: fireRateLevel++; break;
                default: throw new ArgumentOutOfRangeException(nameof(upgrade), upgrade, "Not a straight-shot upgrade.");
            }
        }

        /// <summary>Upgrades of this weapon that are not yet at their max level.</summary>
        void IUpgradeable.AddAvailableUpgrades(List<Upgrade> pool)
        {
            // Only this weapon's own upgrades: other weapons' upgrades share the enum.
            foreach (var upgrade in Upgrades)
            {
                if (LevelOf(upgrade) < MaxLevelOf(upgrade)) pool.Add(upgrade);
            }
        }

        /// <summary>Times <paramref name="upgrade"/> has been picked.</summary>
        public int LevelOf(Upgrade upgrade) => upgrade switch
        {
            Upgrade.StraightShotDamage => damageLevel,
            Upgrade.StraightShotBulletCount => bulletCountLevel,
            Upgrade.StraightShotFireRate => fireRateLevel,
            _ => throw new ArgumentOutOfRangeException(nameof(upgrade), upgrade, "Not a straight-shot upgrade."),
        };

        /// <summary>Times <paramref name="upgrade"/> can be picked in total.</summary>
        public int MaxLevelOf(Upgrade upgrade) => upgrade switch
        {
            Upgrade.StraightShotDamage => config.Upgrades.MaxDamageLevel,
            Upgrade.StraightShotBulletCount => config.Upgrades.MaxBulletCountLevel,
            Upgrade.StraightShotFireRate => config.Upgrades.MaxFireRateLevel,
            _ => throw new ArgumentOutOfRangeException(nameof(upgrade), upgrade, "Not a straight-shot upgrade."),
        };
    }
}
