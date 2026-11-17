using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// Places the orbiting knives on their circle: evenly spaced around the centre, all turned by the same rotation,
    /// so the ring stays balanced whatever the knife count.
    /// </summary>
    public static class OrbitLayout
    {
        /// <summary>
        /// Offset from the centre of knife <paramref name="index"/> of <paramref name="count"/> on a circle of <paramref name="radius"/>,
        /// with the ring turned <paramref name="rotationDegrees"/> counter-clockwise. Knife 0 starts on the right.
        /// </summary>
        public static (float X, float Y) Offset(int index, int count, float rotationDegrees, float radius)
        {
            if (index < 0 || index >= count) throw new ArgumentOutOfRangeException(nameof(index), index, $"Must be in [0, {count}).");

            var radians = (rotationDegrees + 360.0 * index / count) * Math.PI / 180.0;
            return ((float)(radius * Math.Cos(radians)), (float)(radius * Math.Sin(radians)));
        }
    }
}
