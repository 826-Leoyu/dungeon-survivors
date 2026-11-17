using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class OrbitingKnivesConfigTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveDamage_IsRejected(int damage)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.OrbitingKnives(damage: damage));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveKnifeCount_IsRejected(int knifeCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.OrbitingKnives(knifeCount: knifeCount));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NonPositiveRadius_IsRejected(float radius)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.OrbitingKnives(radius: radius));
        }

        [TestCase(0f)]
        [TestCase(-90f)]
        public void NonPositiveRotationSpeed_IsRejected(float rotationSpeed)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.OrbitingKnives(rotationSpeed: rotationSpeed));
        }

        [TestCase(0f)]
        [TestCase(-0.5f)]
        public void NonPositiveHitInterval_IsRejected(float hitInterval)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestConfigs.OrbitingKnives(hitInterval: hitInterval));
        }

        [Test]
        public void MissingUpgrades_IsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new OrbitingKnivesConfig(4, 2, 1.5f, 180f, 0.5f, null));
        }

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
