using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class FanSpreadTests
    {
        [Test]
        public void SingleBullet_FliesStraightAtTheTarget()
        {
            Assert.That(FanSpread.OffsetDegrees(0, 1, 15f), Is.EqualTo(0f));
        }

        [Test]
        public void TwoBullets_SplitEvenlyEitherSideOfTheAim()
        {
            Assert.That(FanSpread.OffsetDegrees(0, 2, 15f), Is.EqualTo(-7.5f));
            Assert.That(FanSpread.OffsetDegrees(1, 2, 15f), Is.EqualTo(7.5f));
        }

        [Test]
        public void ThreeBullets_KeepTheMiddleOneOnTheAim()
        {
            Assert.That(FanSpread.OffsetDegrees(0, 3, 15f), Is.EqualTo(-15f));
            Assert.That(FanSpread.OffsetDegrees(1, 3, 15f), Is.EqualTo(0f));
            Assert.That(FanSpread.OffsetDegrees(2, 3, 15f), Is.EqualTo(15f));
        }

        [TestCase(-1, 3)]
        [TestCase(3, 3)]
        [TestCase(0, 0)]
        public void IndexOutsideTheFan_IsRejected(int index, int count)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FanSpread.OffsetDegrees(index, count, 15f));
        }
    }
}
