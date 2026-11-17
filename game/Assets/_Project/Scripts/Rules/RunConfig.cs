using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// Tuning numbers for one Run, as plain data: the presentation layer converts ScriptableObjects into this at startup.
    /// The Run uses the player's numbers itself; the presentation systems read the rest from <see cref="Run.Config"/>.
    /// </summary>
    public sealed class RunConfig
    {
        public PlayerConfig Player { get; }

        /// <summary>The chaser, the first kind of enemy.</summary>
        public EnemyConfig Chaser { get; }

        /// <summary>The weapon the player starts with: the straight shot.</summary>
        public WeaponConfig StartingWeapon { get; }

        /// <summary>The orbiting knives, a weapon picked at a level-up.</summary>
        public OrbitingKnivesConfig OrbitingKnives { get; }

        /// <summary>The XP curve: how much XP each level costs.</summary>
        public ProgressionConfig Progression { get; }

        public RunConfig(PlayerConfig player, EnemyConfig chaser, WeaponConfig startingWeapon, OrbitingKnivesConfig orbitingKnives, ProgressionConfig progression)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Chaser = chaser ?? throw new ArgumentNullException(nameof(chaser));
            StartingWeapon = startingWeapon ?? throw new ArgumentNullException(nameof(startingWeapon));
            OrbitingKnives = orbitingKnives ?? throw new ArgumentNullException(nameof(orbitingKnives));
            Progression = progression ?? throw new ArgumentNullException(nameof(progression));
        }
    }
}
