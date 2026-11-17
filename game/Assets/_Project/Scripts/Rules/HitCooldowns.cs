using System;
using System.Collections.Generic;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// A weapon's hit interval per target: after a hit lands on a target, further hits on that same target are ignored for
    /// <see cref="Interval"/> seconds, while other targets can still be hit. Lets a weapon that keeps touching enemies (the orbiting knives)
    /// report every contact without dealing damage every frame. Targets are plain ids chosen by the caller; the caller advances time with <see cref="Tick"/>.
    /// </summary>
    public sealed class HitCooldowns
    {
        // When each target on cooldown can be hit again, on this object's own clock. Entries are dropped once they expire, so it only holds recent targets.
        private readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        private readonly List<int> expired = new List<int>();
        private float interval;
        private float now;

        public HitCooldowns(float interval)
        {
            Interval = interval;
        }

        /// <summary>Seconds a hit target is safe from the next hit. A change applies from the next hit that lands.</summary>
        public float Interval
        {
            get => interval;
            set
            {
                if (value <= 0f) throw new ArgumentOutOfRangeException(nameof(value), value, "Must be positive.");
                interval = value;
            }
        }

        /// <summary>Advances the clock by <paramref name="deltaTime"/> seconds and forgets targets whose interval has passed.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Must not be negative.");
            now += deltaTime;

            // Collected first: a Dictionary cannot be changed while it is being enumerated. The list is reused, so this allocates nothing.
            expired.Clear();
            foreach (var entry in readyAt)
            {
                if (entry.Value <= now) expired.Add(entry.Key);
            }

            foreach (var target in expired) readyAt.Remove(target);
        }

        /// <summary>
        /// Reports a contact with <paramref name="target"/>. Returns true when the hit lands, which starts that target's interval,
        /// and false while the target is still within the interval of its last hit.
        /// </summary>
        public bool TryHit(int target)
        {
            if (readyAt.TryGetValue(target, out var ready) && ready > now) return false;

            readyAt[target] = now + interval;
            return true;
        }
    }
}
