using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// Says when a weapon may fire. The caller advances it with <see cref="Tick"/> each frame and calls <see cref="TryFire"/> when it has a target.
    /// Unlike <see cref="SpawnTimer"/>, readiness does not stack: time spent ready but without a target does not store up a burst of shots.
    /// </summary>
    public sealed class FireCooldown
    {
        private readonly float interval;
        private float remaining;

        /// <param name="interval">Seconds between shots.</param>
        public FireCooldown(float interval)
        {
            if (interval <= 0f) throw new ArgumentOutOfRangeException(nameof(interval), interval, "Must be positive.");
            this.interval = interval;
        }

        /// <summary>Advances the cooldown by <paramref name="deltaTime"/> seconds.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Must not be negative.");
            remaining -= deltaTime;
        }

        /// <summary>
        /// Fires if ready: returns true and restarts the cooldown. A new cooldown starts ready, so the first shot goes out as soon as there is a target.
        /// At most one shot per call, so a long frame fires once rather than in a burst.
        /// </summary>
        public bool TryFire()
        {
            if (remaining > 0f) return false;

            remaining = interval;
            return true;
        }
    }
}
