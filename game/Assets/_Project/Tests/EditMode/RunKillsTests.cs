using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class RunKillsTests
    {
        private static Run NewRun() => new Run(new RunConfig(maxHealth: 100, hurtCooldown: 0.5f), new SeededRandom(1));

        [Test]
        public void NewRun_HasNoKills()
        {
            Assert.That(NewRun().Kills, Is.EqualTo(0));
        }

        [Test]
        public void EachReportedKill_AddsOne()
        {
            var run = NewRun();

            run.RecordKill();
            run.RecordKill();
            run.RecordKill();

            Assert.That(run.Kills, Is.EqualTo(3));
        }

        [Test]
        public void KillAfterDefeat_IsIgnored()
        {
            var run = NewRun();
            run.RecordKill();
            run.TakeDamage(100);

            run.RecordKill();

            Assert.That(run.Kills, Is.EqualTo(1));
        }
    }
}
