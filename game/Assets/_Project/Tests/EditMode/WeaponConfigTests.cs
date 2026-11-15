using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class WeaponConfigTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveDamage_IsRejected(int damage)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponConfig(damage, fireInterval: 0.5f, bulletSpeed: 12f, bulletLifetime: 2f));
        }

        [TestCase(0f)]
        [TestCase(-0.5f)]
        public void NonPositiveFireInterval_IsRejected(float fireInterval)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponConfig(damage: 5, fireInterval, bulletSpeed: 12f, bulletLifetime: 2f));
        }

        [TestCase(0f)]
        [TestCase(-12f)]
        public void NonPositiveBulletSpeed_IsRejected(float bulletSpeed)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponConfig(damage: 5, fireInterval: 0.5f, bulletSpeed, bulletLifetime: 2f));
        }

        [TestCase(0f)]
        [TestCase(-2f)]
        public void NonPositiveBulletLifetime_IsRejected(float bulletLifetime)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponConfig(damage: 5, fireInterval: 0.5f, bulletSpeed: 12f, bulletLifetime));
        }
    }
}
