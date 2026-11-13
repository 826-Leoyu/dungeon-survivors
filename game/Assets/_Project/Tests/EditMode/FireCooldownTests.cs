using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class FireCooldownTests
    {
        private const float Interval = 0.5f;

        [Test]
        public void NewCooldown_IsReadyToFire()
        {
            var cooldown = new FireCooldown(Interval);

            Assert.That(cooldown.TryFire(), Is.True);
        }

        [Test]
        public void RightAfterFiring_IsNotReady()
        {
            var cooldown = new FireCooldown(Interval);
            cooldown.TryFire();

            cooldown.Tick(Interval * 0.5f);

            Assert.That(cooldown.TryFire(), Is.False);
        }

        [Test]
        public void AfterOneInterval_IsReadyAgain()
        {
            var cooldown = new FireCooldown(Interval);
            cooldown.TryFire();

            cooldown.Tick(Interval);

            Assert.That(cooldown.TryFire(), Is.True);
        }

        [Test]
        public void IdleTimeWithoutFiring_DoesNotStoreUpShots()
        {
            var cooldown = new FireCooldown(Interval);

            cooldown.Tick(Interval * 10f);
            cooldown.TryFire();

            Assert.That(cooldown.TryFire(), Is.False);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NonPositiveInterval_IsRejected(float interval)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FireCooldown(interval));
        }

        [Test]
        public void NegativeDeltaTime_IsRejected()
        {
            var cooldown = new FireCooldown(Interval);

            Assert.Throws<ArgumentOutOfRangeException>(() => cooldown.Tick(-0.1f));
        }
    }
}
