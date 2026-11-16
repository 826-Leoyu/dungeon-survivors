using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// The rules state of one Run, from start until it ends in victory or defeat. The caller advances time with <see cref="Tick"/> and reports events;
    /// the Run never reads a clock or Unity APIs itself.
    /// </summary>
    public sealed class Run
    {
        private readonly IRandom random;

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
        /// so none is lost; the level-up choice works through them one at a time with <see cref="ConsumeLevelUp"/>.
        /// </summary>
        public int PendingLevelUps { get; private set; }

        public RunOutcome Outcome { get; private set; } = RunOutcome.InProgress;

        /// <summary>Once over, the Run ignores every further event.</summary>
        public bool IsOver => Outcome != RunOutcome.InProgress;

        public Run(RunConfig config, IRandom random)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            Health = config.Player.MaxHealth;
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
        }

        /// <summary>Takes the oldest pending level-up off the queue once it has been handled. Returns false when none is pending.</summary>
        public bool ConsumeLevelUp()
        {
            if (PendingLevelUps == 0) return false;
            PendingLevelUps--;
            return true;
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
