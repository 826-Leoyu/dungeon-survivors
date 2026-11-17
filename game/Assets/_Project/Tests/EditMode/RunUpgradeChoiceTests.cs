using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class RunUpgradeChoiceTests
    {
        // Every upgrade a new Run can offer: the straight shot's three and the orbiting knives' unlock.
        private static readonly Upgrade[] NewRunUpgrades =
            { Upgrade.StraightShotDamage, Upgrade.StraightShotBulletCount, Upgrade.StraightShotFireRate, Upgrade.OrbitingKnivesUnlock };

        // Level 1 to 2 costs 5 XP, 2 to 3 costs 10, 3 to 4 costs 15.
        private static Run NewRun(WeaponUpgradeConfig upgrades = null, int seed = 1) =>
            new Run(TestConfigs.Run(firstLevelXp: 5, xpGrowth: 5, upgrades: upgrades), new SeededRandom(seed));

        [Test]
        public void NewRun_OffersNoChoices()
        {
            Assert.That(NewRun().UpgradeChoices, Is.Empty);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void LevelUp_OffersThreeDifferentUpgrades(int seed)
        {
            var run = NewRun(seed: seed);

            run.GainXp(5);

            Assert.That(run.UpgradeChoices, Has.Count.EqualTo(3));
            Assert.That(run.UpgradeChoices, Is.Unique);
            Assert.That(run.UpgradeChoices, Is.SubsetOf(NewRunUpgrades));
        }

        [Test]
        public void XpShortOfALevel_OffersNoChoices()
        {
            var run = NewRun();

            run.GainXp(4);

            Assert.That(run.UpgradeChoices, Is.Empty);
        }

        [Test]
        public void NewRun_StartsWithTheStartingWeaponsNumbers()
        {
            var weapon = NewRun().StraightShot;

            Assert.That(weapon.Damage, Is.EqualTo(5));
            Assert.That(weapon.BulletCount, Is.EqualTo(1));
            Assert.That(weapon.FireInterval, Is.EqualTo(0.5f));
        }

        [Test]
        public void ChoosingDamage_AddsTheDamagePerLevel()
        {
            var run = LevelledRun(upgrades: TestConfigs.Upgrades(maxBulletCountLevel: 0));

            run.ChooseUpgrade(Upgrade.StraightShotDamage);

            Assert.That(run.StraightShot.Damage, Is.EqualTo(7));
            Assert.That(run.StraightShot.LevelOf(Upgrade.StraightShotDamage), Is.EqualTo(1));
        }

        [Test]
        public void ChoosingBulletCount_AddsABullet()
        {
            var run = LevelledRun(upgrades: TestConfigs.Upgrades(maxFireRateLevel: 0));

            run.ChooseUpgrade(Upgrade.StraightShotBulletCount);

            Assert.That(run.StraightShot.BulletCount, Is.EqualTo(2));
        }

        [Test]
        public void ChoosingFireRateTwice_MultipliesTheIntervalEachTime()
        {
            var run = LevelledRun(levels: 2, upgrades: TestConfigs.Upgrades(maxBulletCountLevel: 0));

            run.ChooseUpgrade(Upgrade.StraightShotFireRate);
            run.ChooseUpgrade(Upgrade.StraightShotFireRate);

            // 0.5 s x 0.5 x 0.5.
            Assert.That(run.StraightShot.FireInterval, Is.EqualTo(0.125f));
        }

        [Test]
        public void Choosing_HandlesTheLevelUpAndClearsTheChoices()
        {
            var run = LevelledRun();

            run.ChooseUpgrade(run.UpgradeChoices[0]);

            Assert.That(run.PendingLevelUps, Is.EqualTo(0));
            Assert.That(run.UpgradeChoices, Is.Empty);
        }

        [Test]
        public void SeveralLevelsAtOnce_OfferOneChoiceAfterAnother()
        {
            var run = LevelledRun(levels: 3);

            for (var remaining = 3; remaining > 0; remaining--)
            {
                Assert.That(run.PendingLevelUps, Is.EqualTo(remaining));
                Assert.That(run.UpgradeChoices, Has.Count.EqualTo(3));
                run.ChooseUpgrade(run.UpgradeChoices[0]);
            }

            Assert.That(run.PendingLevelUps, Is.EqualTo(0));
            Assert.That(run.UpgradeChoices, Is.Empty);
        }

        [Test]
        public void XpDuringAChoice_QueuesTheLevelUpWithoutChangingTheChoices()
        {
            var run = LevelledRun();
            var offered = new[] { run.UpgradeChoices[0], run.UpgradeChoices[1], run.UpgradeChoices[2] };

            run.GainXp(10); // Level 3.

            Assert.That(run.PendingLevelUps, Is.EqualTo(2));
            Assert.That(run.UpgradeChoices, Is.EqualTo(offered));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void MaxedUpgrade_IsNoLongerOffered(int seed)
        {
            var run = new Run(TestConfigs.Run(firstLevelXp: 5, xpGrowth: 5, upgrades: TestConfigs.Upgrades(maxDamageLevel: 1, maxFireRateLevel: 0)),
                new SeededRandom(seed));
            run.GainXp(15); // Levels 2 and 3.

            run.ChooseUpgrade(Upgrade.StraightShotDamage);

            // Only two upgrades are left, so the choice offers fewer than three.
            Assert.That(run.UpgradeChoices, Is.EquivalentTo(new[] { Upgrade.StraightShotBulletCount, Upgrade.OrbitingKnivesUnlock }));
        }

        [Test]
        public void UpgradeWithMaxLevelZero_IsNeverOffered()
        {
            var run = LevelledRun(upgrades: TestConfigs.Upgrades(maxBulletCountLevel: 0));

            Assert.That(run.UpgradeChoices,
                Is.EquivalentTo(new[] { Upgrade.StraightShotDamage, Upgrade.StraightShotFireRate, Upgrade.OrbitingKnivesUnlock }));
        }

        [Test]
        public void LevelUpWithEveryUpgradeMaxed_IsSkipped()
        {
            var upgrades = TestConfigs.Upgrades(maxDamageLevel: 1, maxBulletCountLevel: 0, maxFireRateLevel: 0);
            var knifeUpgrades = TestConfigs.KnifeUpgrades(maxDamageLevel: 0, maxKnifeCountLevel: 0, maxRadiusLevel: 0, maxRotationSpeedLevel: 0);
            var run = LevelledRun(levels: 3, upgrades: upgrades, knifeUpgrades: knifeUpgrades);

            run.ChooseUpgrade(Upgrade.StraightShotDamage);
            run.ChooseUpgrade(Upgrade.OrbitingKnivesUnlock);

            Assert.That(run.UpgradeChoices, Is.Empty);
            Assert.That(run.PendingLevelUps, Is.EqualTo(0));
        }

        [Test]
        public void ChoosingAnUpgradeNotOffered_IsRejected()
        {
            var run = LevelledRun(upgrades: TestConfigs.Upgrades(maxBulletCountLevel: 0));

            Assert.Throws<ArgumentException>(() => run.ChooseUpgrade(Upgrade.StraightShotBulletCount));
            Assert.That(run.PendingLevelUps, Is.EqualTo(1));
        }

        [Test]
        public void ChoosingWithNoLevelUpPending_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => NewRun().ChooseUpgrade(Upgrade.StraightShotDamage));
        }

        [Test]
        public void ChoosingAfterDefeat_IsIgnored()
        {
            var run = LevelledRun();
            run.TakeDamage(100);

            run.ChooseUpgrade(Upgrade.StraightShotDamage);

            Assert.That(run.StraightShot.Damage, Is.EqualTo(5));
            Assert.That(run.PendingLevelUps, Is.EqualTo(1));
        }

        [Test]
        public void ChoosingAnUpgradeNotOfferedAfterDefeat_IsIgnoredRatherThanRejected()
        {
            var run = LevelledRun(upgrades: TestConfigs.Upgrades(maxBulletCountLevel: 0));
            run.TakeDamage(100);

            Assert.DoesNotThrow(() => run.ChooseUpgrade(Upgrade.StraightShotBulletCount));
        }

        [Test]
        public void SameSeed_OffersTheSameChoices()
        {
            var first = LevelledRun();
            var second = LevelledRun();

            Assert.That(second.UpgradeChoices, Is.EqualTo(first.UpgradeChoices));
        }

        /// <summary>
        /// A Run that has just reached <paramref name="levels"/> more levels. Tests that pick a particular upgrade set a max level to 0
        /// so at most three are available: then every one of them is offered.
        /// </summary>
        private static Run LevelledRun(int levels = 1, WeaponUpgradeConfig upgrades = null, OrbitingKnivesUpgradeConfig knifeUpgrades = null)
        {
            var run = new Run(TestConfigs.Run(firstLevelXp: 5, xpGrowth: 5, upgrades: upgrades, knifeUpgrades: knifeUpgrades), new SeededRandom(1));
            // 5, 15, 30 XP reach levels 2, 3, 4.
            run.GainXp(new[] { 0, 5, 15, 30 }[levels]);
            Assert.That(run.PendingLevelUps, Is.EqualTo(levels));
            return run;
        }
    }
}
