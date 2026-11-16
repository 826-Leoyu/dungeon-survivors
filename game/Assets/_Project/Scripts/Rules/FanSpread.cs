using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// Aims a volley of several bullets as a fan centred on the target: neighbouring bullets are a fixed angle apart,
    /// split evenly either side of the aim, so a single bullet flies straight at the target.
    /// </summary>
    public static class FanSpread
    {
        /// <summary>
        /// Degrees to rotate bullet <paramref name="index"/> of <paramref name="count"/> away from the aim (negative is clockwise),
        /// when neighbouring bullets are <paramref name="spreadDegrees"/> apart.
        /// </summary>
        public static float OffsetDegrees(int index, int count, float spreadDegrees)
        {
            if (index < 0 || index >= count) throw new ArgumentOutOfRangeException(nameof(index), index, $"Must be in [0, {count}).");
            return (index - (count - 1) / 2f) * spreadDegrees;
        }
    }
}
