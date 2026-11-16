using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class WeaponUpgradeConfigTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveDamagePerLevel_IsRejected(int damagePerLevel)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.Upgrades(damagePerLevel: damagePerLevel));
        }

        [Test]
        public void NegativeMaxDamageLevel_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.Upgrades(maxDamageLevel: -1));
        }

        [Test]
        public void NegativeMaxBulletCountLevel_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.Upgrades(maxBulletCountLevel: -1));
        }

        [Test]
        public void NegativeMaxFireRateLevel_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.Upgrades(maxFireRateLevel: -1));
        }

        [TestCase(0f)]
        [TestCase(1f)]
        [TestCase(1.2f)]
        [TestCase(-0.5f)]
        public void FireIntervalMultiplierOutsideZeroToOne_IsRejected(float multiplier)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.Upgrades(fireIntervalMultiplier: multiplier));
        }

        [Test]
        public void ZeroMaxLevel_IsAllowed()
        {
            Assert.DoesNotThrow(() => TestConfigs.Upgrades(maxDamageLevel: 0, maxBulletCountLevel: 0, maxFireRateLevel: 0));
        }
    }
}
