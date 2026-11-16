using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class ProgressionConfigTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveFirstLevelXp_IsRejected(int firstLevelXp)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressionConfig(firstLevelXp, xpGrowth: 5));
        }

        [Test]
        public void NegativeXpGrowth_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressionConfig(firstLevelXp: 5, xpGrowth: -1));
        }

        [Test]
        public void ZeroXpGrowth_IsAllowed()
        {
            Assert.DoesNotThrow(() => new ProgressionConfig(firstLevelXp: 5, xpGrowth: 0));
        }
    }
}
