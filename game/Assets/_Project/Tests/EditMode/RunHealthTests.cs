using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class RunHealthTests
    {
        private const int MaxHealth = 100;
        private const float HurtCooldown = 0.5f;

        private static Run NewRun() => new Run(new RunConfig(MaxHealth, HurtCooldown), new SeededRandom(1));

        [Test]
        public void TakingDamage_ReducesHealth()
        {
            var run = NewRun();

            run.TakeDamage(30);

            Assert.That(run.Health, Is.EqualTo(70));
        }

        [Test]
        public void DamageWithinHurtCooldown_IsIgnored()
        {
            var run = NewRun();
            run.TakeDamage(30);

            run.Tick(HurtCooldown * 0.5f);
            run.TakeDamage(30);

            Assert.That(run.Health, Is.EqualTo(70));
        }

        [Test]
        public void DamageAfterHurtCooldownElapses_Applies()
        {
            var run = NewRun();
            run.TakeDamage(30);

            run.Tick(HurtCooldown);
            run.TakeDamage(30);

            Assert.That(run.Health, Is.EqualTo(40));
        }

        [Test]
        public void NewRun_IsInProgress()
        {
            var run = NewRun();

            Assert.That(run.Outcome, Is.EqualTo(RunOutcome.InProgress));
        }

        [Test]
        public void HealthReachingZero_EndsRunInDefeat()
        {
            var run = NewRun();

            run.TakeDamage(MaxHealth);

            Assert.That(run.Outcome, Is.EqualTo(RunOutcome.Defeat));
        }

        [Test]
        public void OverkillDamage_LeavesHealthAtZero()
        {
            var run = NewRun();

            run.TakeDamage(MaxHealth + 50);

            Assert.That(run.Health, Is.EqualTo(0));
        }

        [Test]
        public void DamageAfterDefeat_IsIgnored()
        {
            var run = NewRun();
            run.TakeDamage(MaxHealth);
            run.Tick(HurtCooldown);

            var applied = run.TakeDamage(10);

            Assert.That(applied, Is.False);
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void NonPositiveDamage_IsRejected(int amount)
        {
            var run = NewRun();

            Assert.Throws<ArgumentOutOfRangeException>(() => run.TakeDamage(amount));
        }
    }
}
