using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class EnemyConfigTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveMaxHealth_IsRejected(int maxHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyConfig(maxHealth, moveSpeed: 2f, contactDamage: 10));
        }

        [Test]
        public void NegativeMoveSpeed_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyConfig(maxHealth: 10, moveSpeed: -1f, contactDamage: 10));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveContactDamage_IsRejected(int contactDamage)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyConfig(maxHealth: 10, moveSpeed: 2f, contactDamage));
        }

        [Test]
        public void ZeroMoveSpeed_IsAllowed()
        {
            Assert.DoesNotThrow(() => new EnemyConfig(maxHealth: 1, moveSpeed: 0f, contactDamage: 1));
        }
    }
}
