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
            Assert.Throws<ArgumentNullException>(() => new RunConfig(null, TestConfigs.Chaser(), TestConfigs.StraightShot(), TestConfigs.Progression()));
        }

        [Test]
        public void MissingChaser_IsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new RunConfig(TestConfigs.Player(), null, TestConfigs.StraightShot(), TestConfigs.Progression()));
        }

        [Test]
        public void MissingStartingWeapon_IsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new RunConfig(TestConfigs.Player(), TestConfigs.Chaser(), null, TestConfigs.Progression()));
        }

        [Test]
        public void MissingProgression_IsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new RunConfig(TestConfigs.Player(), TestConfigs.Chaser(), TestConfigs.StraightShot(), null));
        }
    }
}
