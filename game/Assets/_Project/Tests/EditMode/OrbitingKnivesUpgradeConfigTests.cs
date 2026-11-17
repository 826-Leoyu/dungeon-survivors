using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class OrbitingKnivesUpgradeConfigTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveDamagePerLevel_IsRejected(int damagePerLevel)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.KnifeUpgrades(damagePerLevel: damagePerLevel));
        }

        [TestCase(0f)]
        [TestCase(-0.5f)]
        public void NonPositiveRadiusPerLevel_IsRejected(float radiusPerLevel)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.KnifeUpgrades(radiusPerLevel: radiusPerLevel));
        }

        [TestCase(0f)]
        [TestCase(-60f)]
        public void NonPositiveRotationSpeedPerLevel_IsRejected(float rotationSpeedPerLevel)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.KnifeUpgrades(rotationSpeedPerLevel: rotationSpeedPerLevel));
        }

        [Test]
        public void NegativeMaxLevels_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.KnifeUpgrades(maxDamageLevel: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.KnifeUpgrades(maxKnifeCountLevel: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.KnifeUpgrades(maxRadiusLevel: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.KnifeUpgrades(maxRotationSpeedLevel: -1));
        }

        [Test]
        public void MaxLevelsOfZero_AreAccepted()
        {
            Assert.DoesNotThrow(() => TestConfigs.KnifeUpgrades(maxDamageLevel: 0, maxKnifeCountLevel: 0, maxRadiusLevel: 0, maxRotationSpeedLevel: 0));
        }
    }
}
