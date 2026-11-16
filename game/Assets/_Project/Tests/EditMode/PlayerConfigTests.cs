using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class PlayerConfigTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveMaxHealth_IsRejected(int maxHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerConfig(maxHealth, hurtCooldown: 0.5f, moveSpeed: 5f, pickupRadius: 1.5f));
        }

        [Test]
        public void NegativeHurtCooldown_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerConfig(maxHealth: 100, hurtCooldown: -0.1f, moveSpeed: 5f, pickupRadius: 1.5f));
        }

        [Test]
        public void NegativeMoveSpeed_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerConfig(maxHealth: 100, hurtCooldown: 0.5f, moveSpeed: -1f, pickupRadius: 1.5f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NonPositivePickupRadius_IsRejected(float pickupRadius)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerConfig(maxHealth: 100, hurtCooldown: 0.5f, moveSpeed: 5f, pickupRadius));
        }

        [Test]
        public void ZeroHurtCooldownAndMoveSpeed_AreAllowed()
        {
            Assert.DoesNotThrow(() => new PlayerConfig(maxHealth: 1, hurtCooldown: 0f, moveSpeed: 0f, pickupRadius: 1.5f));
        }
    }
}
