using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class RunLevelTests
    {
        // Costs 5, 8, 11, 14 ... XP to go from level 1 to 2, 2 to 3, 3 to 4, 4 to 5 ...
        private static Run NewRun() => new Run(TestConfigs.Run(firstLevelXp: 5, xpGrowth: 3), new SeededRandom(1));

        [Test]
        public void NewRun_StartsAtLevelOneWithNoXp()
        {
            var run = NewRun();

            Assert.That(run.Level, Is.EqualTo(1));
            Assert.That(run.Xp, Is.EqualTo(0));
            Assert.That(run.XpToNextLevel, Is.EqualTo(5));
        }

        [Test]
        public void XpNeededForEachLevel_GrowsByTheGrowth()
        {
            var run = NewRun();

            run.GainXp(5);
            Assert.That(run.XpToNextLevel, Is.EqualTo(8));

            run.GainXp(8);
            Assert.That(run.XpToNextLevel, Is.EqualTo(11));
        }

        [Test]
        public void EnoughXpForSeveralLevelsAtOnce_RaisesEachLevelAndQueuesEachLevelUp()
        {
            var run = NewRun();

            // 5 + 8 + 11 = 24 reaches level 4, with 2 XP left over towards level 5.
            run.GainXp(26);

            Assert.That(run.Level, Is.EqualTo(4));
            Assert.That(run.Xp, Is.EqualTo(2));
            Assert.That(run.PendingLevelUps, Is.EqualTo(3));
        }

        [Test]
        public void NewRun_HasNoPendingLevelUps()
        {
            Assert.That(NewRun().PendingLevelUps, Is.EqualTo(0));
        }

        [Test]
        public void ConsumingLevelUps_WorksThroughTheQueueOneAtATime()
        {
            var run = NewRun();
            run.GainXp(13); // Levels 2 and 3.

            Assert.That(run.ConsumeLevelUp(), Is.True);
            Assert.That(run.PendingLevelUps, Is.EqualTo(1));
            Assert.That(run.ConsumeLevelUp(), Is.True);
            Assert.That(run.PendingLevelUps, Is.EqualTo(0));
        }

        [Test]
        public void ConsumingWithNoPendingLevelUp_ReturnsFalse()
        {
            var run = NewRun();

            Assert.That(run.ConsumeLevelUp(), Is.False);
            Assert.That(run.PendingLevelUps, Is.EqualTo(0));
        }

        [Test]
        public void XpAfterDefeat_IsIgnored()
        {
            var run = NewRun();
            run.TakeDamage(100);

            run.GainXp(5);

            Assert.That(run.Level, Is.EqualTo(1));
            Assert.That(run.Xp, Is.EqualTo(0));
            Assert.That(run.PendingLevelUps, Is.EqualTo(0));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveXp_IsRejected(int amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => NewRun().GainXp(amount));
        }
    }
}
