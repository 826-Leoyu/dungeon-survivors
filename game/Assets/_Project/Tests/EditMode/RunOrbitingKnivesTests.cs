using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class RunOrbitingKnivesTests
    {
        // The straight shot offers only its damage upgrade, so a level-up offers exactly that and the knives' unlock.
        private static readonly WeaponUpgradeConfig StraightShotDamageOnly = TestConfigs.Upgrades(maxBulletCountLevel: 0, maxFireRateLevel: 0);

        // The straight shot offers nothing, so the knives are the only weapon a level-up can offer.
        private static readonly WeaponUpgradeConfig NoStraightShotUpgrades = TestConfigs.Upgrades(maxDamageLevel: 0, maxBulletCountLevel: 0, maxFireRateLevel: 0);

        [Test]
        public void NewRun_DoesNotOwnTheKnives()
        {
            Assert.That(NewRun().OrbitingKnives.IsOwned, Is.False);
        }

        [Test]
        public void LevelUpBeforeOwningTheKnives_OffersTheUnlockButNotTheirStatUpgrades()
        {
            var run = NewRun(StraightShotDamageOnly);

            run.GainXp(5);

            Assert.That(run.UpgradeChoices, Is.EquivalentTo(new[] { Upgrade.StraightShotDamage, Upgrade.OrbitingKnivesUnlock }));
        }

        [Test]
        public void ChoosingTheUnlock_GivesTheKnivesAtTheirStartingNumbers()
        {
            var run = NewRun(NoStraightShotUpgrades);
            run.GainXp(5);

            run.ChooseUpgrade(Upgrade.OrbitingKnivesUnlock);

            var knives = run.OrbitingKnives;
            Assert.That(knives.IsOwned, Is.True);
            Assert.That(knives.Damage, Is.EqualTo(4));
            Assert.That(knives.KnifeCount, Is.EqualTo(2));
            Assert.That(knives.Radius, Is.EqualTo(1.5f));
            Assert.That(knives.RotationSpeed, Is.EqualTo(180f));
            Assert.That(knives.HitInterval, Is.EqualTo(0.5f));
        }

        [Test]
        public void OnceOwned_TheUnlockIsNoLongerOfferedAndTheStatUpgradesAre()
        {
            var run = KnivesRun(levels: 2, TestConfigs.KnifeUpgrades(maxRotationSpeedLevel: 0));

            Assert.That(run.UpgradeChoices,
                Is.EquivalentTo(new[] { Upgrade.OrbitingKnivesDamage, Upgrade.OrbitingKnivesCount, Upgrade.OrbitingKnivesRadius }));
        }

        [Test]
        public void Unlock_CanBePickedOnce()
        {
            var run = KnivesRun();

            Assert.That(run.OrbitingKnives.LevelOf(Upgrade.OrbitingKnivesUnlock), Is.EqualTo(1));
            Assert.That(run.OrbitingKnives.MaxLevelOf(Upgrade.OrbitingKnivesUnlock), Is.EqualTo(1));
        }

        [Test]
        public void ChoosingDamage_AddsTheDamagePerLevel()
        {
            var run = KnivesRun(levels: 2, TestConfigs.KnifeUpgrades(maxRotationSpeedLevel: 0));

            run.ChooseUpgrade(Upgrade.OrbitingKnivesDamage);

            Assert.That(run.OrbitingKnives.Damage, Is.EqualTo(7));
        }

        [Test]
        public void ChoosingCount_AddsAKnife()
        {
            var run = KnivesRun(levels: 2, TestConfigs.KnifeUpgrades(maxRotationSpeedLevel: 0));

            run.ChooseUpgrade(Upgrade.OrbitingKnivesCount);

            Assert.That(run.OrbitingKnives.KnifeCount, Is.EqualTo(3));
        }

        [Test]
        public void ChoosingRadiusTwice_AddsTheRadiusPerLevelEachTime()
        {
            var run = KnivesRun(levels: 3, TestConfigs.KnifeUpgrades(maxRotationSpeedLevel: 0));

            run.ChooseUpgrade(Upgrade.OrbitingKnivesRadius);
            run.ChooseUpgrade(Upgrade.OrbitingKnivesRadius);

            Assert.That(run.OrbitingKnives.Radius, Is.EqualTo(2.5f));
        }

        [Test]
        public void ChoosingRotationSpeed_AddsTheRotationSpeedPerLevel()
        {
            var run = KnivesRun(levels: 2, TestConfigs.KnifeUpgrades(maxDamageLevel: 0));

            run.ChooseUpgrade(Upgrade.OrbitingKnivesRotationSpeed);

            Assert.That(run.OrbitingKnives.RotationSpeed, Is.EqualTo(240f));
        }

        [Test]
        public void MaxedKnifeUpgrade_IsNoLongerOffered()
        {
            var run = KnivesRun(levels: 3, TestConfigs.KnifeUpgrades(maxDamageLevel: 1, maxRotationSpeedLevel: 0));

            run.ChooseUpgrade(Upgrade.OrbitingKnivesDamage);

            Assert.That(run.UpgradeChoices, Is.EquivalentTo(new[] { Upgrade.OrbitingKnivesCount, Upgrade.OrbitingKnivesRadius }));
        }

        private static Run NewRun(WeaponUpgradeConfig straightShotUpgrades = null, OrbitingKnivesUpgradeConfig knifeUpgrades = null) =>
            new Run(TestConfigs.Run(firstLevelXp: 5, xpGrowth: 5, upgrades: straightShotUpgrades, knifeUpgrades: knifeUpgrades), new SeededRandom(1));

        /// <summary>
        /// A Run that has reached <paramref name="levels"/> more levels and spent the first on the knives' unlock, the only option then,
        /// leaving the rest pending. The straight shot offers nothing, so later choices are all knife upgrades.
        /// </summary>
        private static Run KnivesRun(int levels = 1, OrbitingKnivesUpgradeConfig knifeUpgrades = null)
        {
            var run = NewRun(NoStraightShotUpgrades, knifeUpgrades);
            // 5, 15, 30 XP reach levels 2, 3, 4.
            run.GainXp(new[] { 0, 5, 15, 30 }[levels]);
            run.ChooseUpgrade(Upgrade.OrbitingKnivesUnlock);
            Assert.That(run.PendingLevelUps, Is.EqualTo(levels - 1));
            return run;
        }
    }
}
