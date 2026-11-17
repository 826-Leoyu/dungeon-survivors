using System;
using System.Collections.Generic;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// The orbiting knives' current numbers: not owned until their unlock is picked at a level-up, then their starting numbers
    /// raised by the upgrades picked since. The weapon reads these every frame, so a pick takes effect straight away.
    /// </summary>
    public sealed class OrbitingKnives : IUpgradeable
    {
        /// <summary>Every upgrade this weapon offers: the unlock first, then its stat upgrades.</summary>
        public static readonly IReadOnlyList<Upgrade> Upgrades = new[]
        {
            Upgrade.OrbitingKnivesUnlock, Upgrade.OrbitingKnivesDamage, Upgrade.OrbitingKnivesCount,
            Upgrade.OrbitingKnivesRadius, Upgrade.OrbitingKnivesRotationSpeed,
        };

        private readonly OrbitingKnivesConfig config;
        private int unlockLevel;
        private int damageLevel;
        private int countLevel;
        private int radiusLevel;
        private int rotationSpeedLevel;

        internal OrbitingKnives(OrbitingKnivesConfig config)
        {
            this.config = config;
        }

        /// <summary>Whether the player has the knives. Until then the weapon shows nothing and hits nothing.</summary>
        public bool IsOwned => unlockLevel > 0;

        /// <summary>Damage each hit deals.</summary>
        public int Damage => config.Damage + config.Upgrades.DamagePerLevel * damageLevel;

        /// <summary>Knives circling the player.</summary>
        public int KnifeCount => config.KnifeCount + countLevel;

        /// <summary>World units from the player to each knife.</summary>
        public float Radius => config.Radius + config.Upgrades.RadiusPerLevel * radiusLevel;

        /// <summary>Degrees per second the knives turn around the player.</summary>
        public float RotationSpeed => config.RotationSpeed + config.Upgrades.RotationSpeedPerLevel * rotationSpeedLevel;

        /// <summary>Seconds after a hit before the knives can hit the same enemy again.</summary>
        public float HitInterval => config.HitInterval;

        bool IUpgradeable.Owns(Upgrade upgrade) => IsKnifeUpgrade(upgrade);

        void IUpgradeable.Apply(Upgrade upgrade)
        {
            switch (upgrade)
            {
                case Upgrade.OrbitingKnivesUnlock: unlockLevel++; break;
                case Upgrade.OrbitingKnivesDamage: damageLevel++; break;
                case Upgrade.OrbitingKnivesCount: countLevel++; break;
                case Upgrade.OrbitingKnivesRadius: radiusLevel++; break;
                case Upgrade.OrbitingKnivesRotationSpeed: rotationSpeedLevel++; break;
                default: throw NotAKnifeUpgrade(upgrade);
            }
        }

        /// <summary>Before the knives are owned, only their unlock; after, the stat upgrades not yet at their max level.</summary>
        void IUpgradeable.AddAvailableUpgrades(List<Upgrade> pool)
        {
            foreach (var upgrade in Upgrades)
            {
                var isUnlock = upgrade == Upgrade.OrbitingKnivesUnlock;
                if (isUnlock == IsOwned) continue;
                if (LevelOf(upgrade) < MaxLevelOf(upgrade)) pool.Add(upgrade);
            }
        }

        /// <summary>Times <paramref name="upgrade"/> has been picked.</summary>
        public int LevelOf(Upgrade upgrade) => upgrade switch
        {
            Upgrade.OrbitingKnivesUnlock => unlockLevel,
            Upgrade.OrbitingKnivesDamage => damageLevel,
            Upgrade.OrbitingKnivesCount => countLevel,
            Upgrade.OrbitingKnivesRadius => radiusLevel,
            Upgrade.OrbitingKnivesRotationSpeed => rotationSpeedLevel,
            _ => throw NotAKnifeUpgrade(upgrade),
        };

        /// <summary>Times <paramref name="upgrade"/> can be picked in total. The unlock can be picked once.</summary>
        public int MaxLevelOf(Upgrade upgrade) => upgrade switch
        {
            Upgrade.OrbitingKnivesUnlock => 1,
            Upgrade.OrbitingKnivesDamage => config.Upgrades.MaxDamageLevel,
            Upgrade.OrbitingKnivesCount => config.Upgrades.MaxKnifeCountLevel,
            Upgrade.OrbitingKnivesRadius => config.Upgrades.MaxRadiusLevel,
            Upgrade.OrbitingKnivesRotationSpeed => config.Upgrades.MaxRotationSpeedLevel,
            _ => throw NotAKnifeUpgrade(upgrade),
        };

        private static bool IsKnifeUpgrade(Upgrade upgrade)
        {
            foreach (var candidate in Upgrades)
            {
                if (candidate == upgrade) return true;
            }

            return false;
        }

        private static ArgumentOutOfRangeException NotAKnifeUpgrade(Upgrade upgrade) =>
            new ArgumentOutOfRangeException(nameof(upgrade), upgrade, "Not an orbiting-knives upgrade.");
    }
}
