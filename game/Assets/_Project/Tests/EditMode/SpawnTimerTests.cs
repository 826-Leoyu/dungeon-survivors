using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class SpawnTimerTests
    {
        private const float Interval = 0.5f;

        [Test]
        public void LessThanOneInterval_SpawnsNothing()
        {
            var timer = new SpawnTimer(Interval);

            Assert.That(timer.Tick(0.25f), Is.EqualTo(0));
        }

        [Test]
        public void OneInterval_SpawnsOne()
        {
            var timer = new SpawnTimer(Interval);

            Assert.That(timer.Tick(Interval), Is.EqualTo(1));
        }

        [Test]
        public void ShortFramesAddingUpToOneInterval_SpawnOne()
        {
            var timer = new SpawnTimer(Interval);

            var spawned = timer.Tick(0.25f) + timer.Tick(0.25f);

            Assert.That(spawned, Is.EqualTo(1));
        }

        [Test]
        public void LeftoverTime_CarriesIntoNextInterval()
        {
            var timer = new SpawnTimer(Interval);
            timer.Tick(0.75f);

            Assert.That(timer.Tick(0.25f), Is.EqualTo(1));
        }

        [Test]
        public void LongFrame_SpawnsOncePerElapsedInterval()
        {
            var timer = new SpawnTimer(Interval);

            Assert.That(timer.Tick(1.75f), Is.EqualTo(3));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NonPositiveInterval_IsRejected(float interval)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpawnTimer(interval));
        }

        [Test]
        public void NegativeDeltaTime_IsRejected()
        {
            var timer = new SpawnTimer(Interval);

            Assert.Throws<ArgumentOutOfRangeException>(() => timer.Tick(-0.1f));
        }
    }
}
