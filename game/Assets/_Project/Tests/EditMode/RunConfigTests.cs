using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class RunConfigTests
    {
        [Test]
        public void NewRun_ExposesTheConfigItWasStartedWith()
        {
            var config = TestConfigs.Run();

            var run = new Run(config, new SeededRandom(1));

            Assert.That(run.Config, Is.SameAs(config));
        }

        [Test]
        public void MissingPlayer_IsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new RunConfig(null, TestConfigs.Chaser(), TestConfigs.StraightShot()));
        }

        [Test]
        public void MissingChaser_IsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new RunConfig(TestConfigs.Player(), null, TestConfigs.StraightShot()));
        }

        [Test]
        public void MissingStartingWeapon_IsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new RunConfig(TestConfigs.Player(), TestConfigs.Chaser(), null));
        }
    }
}
