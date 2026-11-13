using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>Picks spawn points on a ring just outside the camera's view, so enemies never pop into existence on screen.</summary>
    public static class SpawnRing
    {
        /// <summary>
        /// Returns a point relative to the view's centre, on the border of the view rectangle grown by <paramref name="margin"/> on every side.
        /// Points are spread evenly along that border, so the longer sides get proportionally more spawns.
        /// </summary>
        /// <param name="margin">Gap between the screen edge and the spawn point; at least the enemy's radius keeps it fully off-screen.</param>
        public static (float X, float Y) PickOffset(float viewHalfWidth, float viewHalfHeight, float margin, IRandom random)
        {
            if (viewHalfWidth < 0f) throw new ArgumentOutOfRangeException(nameof(viewHalfWidth), viewHalfWidth, "Must not be negative.");
            if (viewHalfHeight < 0f) throw new ArgumentOutOfRangeException(nameof(viewHalfHeight), viewHalfHeight, "Must not be negative.");
            if (margin < 0f) throw new ArgumentOutOfRangeException(nameof(margin), margin, "Must not be negative.");
            if (random == null) throw new ArgumentNullException(nameof(random));

            var halfWidth = viewHalfWidth + margin;
            var halfHeight = viewHalfHeight + margin;

            // Walk a random distance clockwise around the ring, starting at the top-left corner.
            var distance = random.NextFloat() * 4f * (halfWidth + halfHeight);

            if (distance < 2f * halfWidth) return (-halfWidth + distance, halfHeight); // top
            distance -= 2f * halfWidth;

            if (distance < 2f * halfHeight) return (halfWidth, halfHeight - distance); // right
            distance -= 2f * halfHeight;

            if (distance < 2f * halfWidth) return (halfWidth - distance, -halfHeight); // bottom
            distance -= 2f * halfWidth;

            // Clamped so float rounding in the subtractions above can never push the point past the top-left corner.
            return (-halfWidth, Math.Min(halfHeight, -halfHeight + distance)); // left
        }
    }
}
