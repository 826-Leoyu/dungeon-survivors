using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// The rules state of one Run, from start until it ends in victory or defeat. The caller advances time with <see cref="Tick"/> and reports events;
    /// the Run never reads a clock or Unity APIs itself.
    /// </summary>
    public sealed class Run
    {
        private readonly RunConfig config;
        private readonly IRandom random;

        private float hurtCooldownRemaining;

        public int Health { get; private set; }

        /// <summary>Seconds of play so far: the player's survival time.</summary>
        public float ElapsedTime { get; private set; }

        public RunOutcome Outcome { get; private set; } = RunOutcome.InProgress;

        /// <summary>Once over, the Run ignores every further event.</summary>
        public bool IsOver => Outcome != RunOutcome.InProgress;

        public Run(RunConfig config, IRandom random)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            Health = config.MaxHealth;
        }

        /// <summary>Advances the Run's clock by <paramref name="deltaTime"/> seconds.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Must not be negative.");
            if (IsOver) return;

            ElapsedTime += deltaTime;
            hurtCooldownRemaining -= deltaTime;
        }

        /// <summary>Reports a hit on the player. Returns whether the damage was applied (false during the hurt cooldown or once the Run is over).</summary>
        public bool TakeDamage(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Must be positive.");
            if (IsOver || hurtCooldownRemaining > 0f) return false;

            Health = Math.Max(0, Health - amount);
            hurtCooldownRemaining = config.HurtCooldown;

            if (Health <= 0) Outcome = RunOutcome.Defeat;
            return true;
        }
    }
}
