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
    }
}
