using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// Tuning numbers for the XP curve, as plain data. Going from level n to n + 1 costs
    /// <see cref="FirstLevelXp"/> + <see cref="XpGrowth"/> x (n - 1) XP, so each level costs a little more than the last.
    /// </summary>
    public sealed class ProgressionConfig
    {
        /// <summary>XP needed to go from level 1 to level 2.</summary>
        public int FirstLevelXp { get; }

        /// <summary>Extra XP each level costs over the one before it.</summary>
        public int XpGrowth { get; }

        public ProgressionConfig(int firstLevelXp, int xpGrowth)
        {
            if (firstLevelXp <= 0) throw new ArgumentOutOfRangeException(nameof(firstLevelXp), firstLevelXp, "Must be positive.");
            if (xpGrowth < 0) throw new ArgumentOutOfRangeException(nameof(xpGrowth), xpGrowth, "Must not be negative.");

            FirstLevelXp = firstLevelXp;
            XpGrowth = xpGrowth;
        }

        /// <summary>XP needed to go from <paramref name="level"/> to the next level.</summary>
        public int XpToLevelUpFrom(int level)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level), level, "Levels start at 1.");
            return FirstLevelXp + XpGrowth * (level - 1);
        }
    }
}
