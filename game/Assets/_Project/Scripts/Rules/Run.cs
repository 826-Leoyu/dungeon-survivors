using System;
using System.Collections.Generic;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// The rules state of one Run, from start until it ends in victory or defeat. The caller advances time with <see cref="Tick"/> and reports events;
    /// the Run never reads a clock or Unity APIs itself.
    /// </summary>
    public sealed class Run
    {
        /// <summary>How many options a level-up offers when that many are available.</summary>
        public const int ChoicesPerLevelUp = 3;

        private readonly IRandom random;
        // Every weapon a level-up can offer upgrades for, owned or not; each decides what it offers.
        private readonly IUpgradeable[] weapons;
        private readonly List<Upgrade> availableUpgrades = new List<Upgrade>();
        private List<Upgrade> choices = new List<Upgrade>();

        private float hurtCooldownRemaining;

        /// <summary>The tuning numbers this Run was started with; presentation systems read their own numbers from here.</summary>
        public RunConfig Config { get; }

        public int Health { get; private set; }

        /// <summary>Seconds of play so far: the player's survival time.</summary>
        public float ElapsedTime { get; private set; }

        /// <summary>Enemies killed so far.</summary>
        public int Kills { get; private set; }

        /// <summary>The player's level, starting at 1. Rises as soon as enough XP is gained, before the level-up is handled.</summary>
        public int Level { get; private set; } = 1;

        /// <summary>XP gained towards the next level.</summary>
        public int Xp { get; private set; }

        /// <summary>XP the current level needs in total to reach the next one; it grows each level.</summary>
        public int XpToNextLevel => Config.Progression.XpToLevelUpFrom(Level);

        /// <summary>
        /// Level-ups reached but not yet handled. Gaining enough XP for several levels at once queues one per level,
        /// so none is lost; the level-up choice works through them one at a time with <see cref="ChooseUpgrade"/>.
        /// </summary>
        public int PendingLevelUps { get; private set; }

        /// <summary>
        /// The options offered for the oldest pending level-up: up to <see cref="ChoicesPerLevelUp"/> different upgrades, none at its max level.
        /// Empty when no level-up is waiting for a choice.
        /// </summary>
        public IReadOnlyList<Upgrade> UpgradeChoices => choices;

        /// <summary>The starting weapon's current numbers, raised by the upgrades picked so far.</summary>
        public StraightShot StraightShot { get; }

        /// <summary>The orbiting knives' current numbers. Not owned until a level-up's choice picks their unlock.</summary>
        public OrbitingKnives OrbitingKnives { get; }

        public RunOutcome Outcome { get; private set; } = RunOutcome.InProgress;

        /// <summary>Once over, the Run ignores every further event.</summary>
        public bool IsOver => Outcome != RunOutcome.InProgress;

        public Run(RunConfig config, IRandom random)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            Health = config.Player.MaxHealth;
            StraightShot = new StraightShot(config.StartingWeapon);
            OrbitingKnives = new OrbitingKnives(config.OrbitingKnives);
            weapons = new IUpgradeable[] { StraightShot, OrbitingKnives };
        }

        /// <summary>Advances the Run's clock by <paramref name="deltaTime"/> seconds.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Must not be negative.");
            if (IsOver) return;

            ElapsedTime += deltaTime;
            hurtCooldownRemaining -= deltaTime;
        }

        /// <summary>Reports that an enemy died. Ignored once the Run is over.</summary>
        public void RecordKill()
        {
            if (IsOver) return;
            Kills++;
        }

        /// <summary>
        /// Reports XP the player picked up. Enough XP for several levels raises each of them and queues one pending level-up per level.
        /// Ignored once the Run is over.
        /// </summary>
        public void GainXp(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Must be positive.");
            if (IsOver) return;

            Xp += amount;
            while (Xp >= XpToNextLevel)
            {
                Xp -= XpToNextLevel;
                Level++;
                PendingLevelUps++;
            }

            if (PendingLevelUps > 0 && choices.Count == 0) DrawChoices();
        }

        /// <summary>
        /// Handles the oldest pending level-up with <paramref name="upgrade"/>, which must be one of the <see cref="UpgradeChoices"/>,
        /// then offers the choices for the next pending level-up, if any. Ignored once the Run is over.
        /// </summary>
        public void ChooseUpgrade(Upgrade upgrade)
        {
            if (IsOver) return;
            if (!choices.Contains(upgrade)) throw new ArgumentException($"{upgrade} is not one of the current choices.", nameof(upgrade));

            WeaponOf(upgrade).Apply(upgrade);
            PendingLevelUps--;
            choices.Clear();
            if (PendingLevelUps > 0) DrawChoices();
        }

        // Draws the options for the oldest pending level-up. With every upgrade maxed there is nothing to offer,
        // so the pending level-ups are dropped rather than showing an empty choice.
        private void DrawChoices()
        {
            availableUpgrades.Clear();
            foreach (var weapon in weapons) weapon.AddAvailableUpgrades(availableUpgrades);
            if (availableUpgrades.Count == 0)
            {
                PendingLevelUps = 0;
                return;
            }

            choices = DistinctDraw.Pick(availableUpgrades, ChoicesPerLevelUp, random);
        }

        private IUpgradeable WeaponOf(Upgrade upgrade)
        {
            foreach (var weapon in weapons)
            {
                foreach (var candidate in weapon.AllUpgrades)
                {
                    if (candidate == upgrade) return weapon;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(upgrade), upgrade, "No weapon offers this upgrade.");
        }

        /// <summary>Reports a hit on the player. Returns whether the damage was applied (false during the hurt cooldown or once the Run is over).</summary>
        public bool TakeDamage(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Must be positive.");
            if (IsOver || hurtCooldownRemaining > 0f) return false;

            Health = Math.Max(0, Health - amount);
            hurtCooldownRemaining = Config.Player.HurtCooldown;

            if (Health <= 0) Outcome = RunOutcome.Defeat;
            return true;
        }
    }
}
