using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class SpawnRingTests
    {
        // A 16:9 view with orthographic size 5, and a 1-unit margin: the ring is the border of a 20 x 12 rectangle.
        private const float HalfWidth = 8.9f;
        private const float HalfHeight = 5f;
        private const float Margin = 1f;
        private const float Tolerance = 1e-4f;

        [Test]
        public void PickedPoint_LiesOnRingJustOutsideView()
        {
            for (var seed = 0; seed < 500; seed++)
            {
                var (x, y) = SpawnRing.PickOffset(HalfWidth, HalfHeight, Margin, new SeededRandom(seed));

                var onVerticalEdge = Within(Math.Abs(x), 9.9f) && Math.Abs(y) <= 6f + Tolerance;
                var onHorizontalEdge = Within(Math.Abs(y), 6f) && Math.Abs(x) <= 9.9f + Tolerance;
                Assert.That(onVerticalEdge || onHorizontalEdge, $"seed {seed}: ({x}, {y}) is not on the ring");
            }
        }

        [Test]
        public void ManyPicks_ComeFromEverySide()
        {
            const int picks = 2000;
            int left = 0, right = 0, top = 0, bottom = 0;
            var random = new SeededRandom(42);

            for (var i = 0; i < picks; i++)
            {
                var (x, y) = SpawnRing.PickOffset(HalfWidth, HalfHeight, Margin, random);
                if (Within(x, -9.9f)) left++;
                else if (Within(x, 9.9f)) right++;
                else if (Within(y, 6f)) top++;
                else if (Within(y, -6f)) bottom++;
            }

            // Spread evenly along the ring, each short side gets about 19% and each long side about 31%; 10% catches a side being skipped.
            const int atLeast = picks / 10;
            Assert.That(new[] { left, right, top, bottom }, Is.All.GreaterThan(atLeast), $"left {left}, right {right}, top {top}, bottom {bottom}");
        }

        [TestCase(-1f, 5f, 1f)]
        [TestCase(8.9f, -1f, 1f)]
        [TestCase(8.9f, 5f, -1f)]
        public void NegativeSize_IsRejected(float halfWidth, float halfHeight, float margin)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SpawnRing.PickOffset(halfWidth, halfHeight, margin, new SeededRandom(1)));
        }

        [Test]
        public void MissingRandom_IsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => SpawnRing.PickOffset(HalfWidth, HalfHeight, Margin, null));
        }

        private static bool Within(float actual, float expected) => Math.Abs(actual - expected) <= Tolerance;
    }
}
