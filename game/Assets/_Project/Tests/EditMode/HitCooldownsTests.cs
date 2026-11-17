using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class HitCooldownsTests
    {
        [Test]
        public void FirstHitOnATarget_Lands()
        {
            Assert.That(new HitCooldowns(0.5f).TryHit(1), Is.True);
        }

        [Test]
        public void HitOnTheSameTargetWithinTheInterval_IsIgnored()
        {
            var cooldowns = new HitCooldowns(0.5f);
            cooldowns.TryHit(1);

            cooldowns.Tick(0.4f);

            Assert.That(cooldowns.TryHit(1), Is.False);
        }

        [Test]
        public void HitOnTheSameTargetOnceTheIntervalHasPassed_Lands()
        {
            var cooldowns = new HitCooldowns(0.5f);
            cooldowns.TryHit(1);

            cooldowns.Tick(0.25f);
            cooldowns.Tick(0.25f);

            Assert.That(cooldowns.TryHit(1), Is.True);
        }

        [Test]
        public void IgnoredHit_DoesNotRestartTheInterval()
        {
            var cooldowns = new HitCooldowns(0.5f);
            cooldowns.TryHit(1);
            cooldowns.Tick(0.4f);
            cooldowns.TryHit(1);

            cooldowns.Tick(0.1f);

            Assert.That(cooldowns.TryHit(1), Is.True);
        }

        [Test]
        public void HitOnAnotherTarget_IsNotBlockedByTheFirstOne()
        {
            var cooldowns = new HitCooldowns(0.5f);
            cooldowns.TryHit(1);

            Assert.That(cooldowns.TryHit(2), Is.True);
        }

        [Test]
        public void ChangedInterval_AppliesFromTheNextHit()
        {
            var cooldowns = new HitCooldowns(0.5f);
            cooldowns.TryHit(1);

            cooldowns.Interval = 1f;
            cooldowns.Tick(0.5f);

            Assert.That(cooldowns.TryHit(1), Is.True);
            cooldowns.Tick(0.5f);
            Assert.That(cooldowns.TryHit(1), Is.False);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NonPositiveInterval_IsRejected(float interval)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HitCooldowns(interval));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HitCooldowns(0.5f).Interval = interval);
        }

        [Test]
        public void NegativeTick_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HitCooldowns(0.5f).Tick(-0.1f));
        }
    }
}
