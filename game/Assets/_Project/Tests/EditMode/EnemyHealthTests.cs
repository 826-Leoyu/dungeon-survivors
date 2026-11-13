using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class EnemyHealthTests
    {
        [Test]
        public void TakingDamage_ReducesHealth()
        {
            var enemy = new EnemyHealth(10);

            enemy.TakeDamage(3);

            Assert.That(enemy.Health, Is.EqualTo(7));
        }

        [Test]
        public void KillingBlow_ReportsDeath()
        {
            var enemy = new EnemyHealth(10);
            enemy.TakeDamage(4);

            var killed = enemy.TakeDamage(6);

            Assert.That(killed, Is.True);
            Assert.That(enemy.IsDead, Is.True);
        }

        [Test]
        public void NonLethalHit_DoesNotReportDeath()
        {
            var enemy = new EnemyHealth(10);

            var killed = enemy.TakeDamage(9);

            Assert.That(killed, Is.False);
            Assert.That(enemy.IsDead, Is.False);
        }

        [Test]
        public void HitOnDeadEnemy_DoesNotReportDeathAgain()
        {
            var enemy = new EnemyHealth(10);
            enemy.TakeDamage(10);

            var killedAgain = enemy.TakeDamage(5);

            Assert.That(killedAgain, Is.False);
        }

        [Test]
        public void OverkillDamage_LeavesHealthAtZero()
        {
            var enemy = new EnemyHealth(10);

            enemy.TakeDamage(25);

            Assert.That(enemy.Health, Is.EqualTo(0));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void NonPositiveDamage_IsRejected(int amount)
        {
            var enemy = new EnemyHealth(10);

            Assert.Throws<ArgumentOutOfRangeException>(() => enemy.TakeDamage(amount));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveMaxHealth_IsRejected(int maxHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyHealth(maxHealth));
        }
    }
}
