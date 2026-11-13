using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// Says when to spawn at a fixed interval. The caller advances it with <see cref="Tick"/> each frame and spawns as many enemies as it returns.
    /// Placeholder until the spawn director (W2) varies the rate over time.
    /// </summary>
    public sealed class SpawnTimer
    {
        private readonly float interval;
        private float accumulated;

        /// <param name="interval">Seconds between spawns.</param>
        public SpawnTimer(float interval)
        {
            if (interval <= 0f) throw new ArgumentOutOfRangeException(nameof(interval), interval, "Must be positive.");
            this.interval = interval;
        }

        /// <summary>
        /// Advances the timer by <paramref name="deltaTime"/> seconds and returns how many spawns are due. Leftover time carries over,
        /// so a long frame returns several spawns instead of losing them.
        /// </summary>
        public int Tick(float deltaTime)
        {
            if (deltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Must not be negative.");

            accumulated += deltaTime;

            var due = 0;
            while (accumulated >= interval)
            {
                accumulated -= interval;
                due++;
            }

            return due;
        }
    }
}
