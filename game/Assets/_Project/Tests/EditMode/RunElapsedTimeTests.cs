using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class RunElapsedTimeTests
    {
        private const int MaxHealth = 100;

        private static Run NewRun() => new Run(new RunConfig(MaxHealth, hurtCooldown: 0.5f), new SeededRandom(1));

        [Test]
        public void Tick_AdvancesElapsedTime()
        {
            var run = NewRun();

            run.Tick(0.25f);
            run.Tick(0.5f);

            Assert.That(run.ElapsedTime, Is.EqualTo(0.75f));
        }

        [Test]
        public void TickAfterDefeat_DoesNotAdvanceElapsedTime()
        {
            var run = NewRun();
            run.Tick(2f);
            run.TakeDamage(MaxHealth);

            run.Tick(1f);

            Assert.That(run.ElapsedTime, Is.EqualTo(2f));
        }

        [Test]
        public void NegativeTick_IsRejected()
        {
            var run = NewRun();

            Assert.Throws<ArgumentOutOfRangeException>(() => run.Tick(-0.1f));
        }
    }
}
