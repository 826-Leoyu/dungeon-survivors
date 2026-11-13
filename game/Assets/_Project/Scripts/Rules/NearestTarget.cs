using System;
using System.Collections.Generic;

namespace DungeonSurvivors.Rules
{
    /// <summary>Picks what a weapon aims at: the target closest to the shooter.</summary>
    public static class NearestTarget
    {
        /// <summary>
        /// Returns the index of the target in <paramref name="targets"/> closest to the origin, or -1 when there are none.
        /// On a tie the earlier target wins.
        /// </summary>
        public static int IndexOf(float originX, float originY, IReadOnlyList<(float X, float Y)> targets)
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));

            var nearest = -1;
            var nearestDistanceSquared = float.PositiveInfinity;
            for (var i = 0; i < targets.Count; i++)
            {
                // Squared distance orders the same as distance and skips the square root.
                var dx = targets[i].X - originX;
                var dy = targets[i].Y - originY;
                var distanceSquared = dx * dx + dy * dy;
                if (distanceSquared >= nearestDistanceSquared) continue;

                nearest = i;
                nearestDistanceSquared = distanceSquared;
            }

            return nearest;
        }
    }
}
