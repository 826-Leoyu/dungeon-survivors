using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class ViewFitTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void SquareScreen_ShowsSquareOfTheGivenArea()
        {
            // A 10 x 10 view: half its height is 5.
            Assert.That(ViewFit.OrthographicSize(100f, 1f), Is.EqualTo(5f).Within(Tolerance));
        }

        // Worked examples: a 20 x 5 view, a 5 x 20 view, then the default area (16:9 at size 5, 17.78 x 10) on the issue's screen ratios:
        // portrait 10 x 17.78, 4:3 at 15.40 x 11.55, and a long 19.5:9 phone at 19.63 x 9.06.
        [TestCase(100f, 4f, 2.5f)]
        [TestCase(100f, 0.25f, 10f)]
        [TestCase(1600f / 9f, 16f / 9f, 5f)]
        [TestCase(1600f / 9f, 9f / 16f, 80f / 9f)]
        [TestCase(1600f / 9f, 4f / 3f, 5.7735f)]
        [TestCase(1600f / 9f, 19.5f / 9f, 4.5291f)]
        public void WideOrTallScreen_KeepsTheGivenArea(float visibleArea, float aspect, float expectedSize)
        {
            Assert.That(ViewFit.OrthographicSize(visibleArea, aspect), Is.EqualTo(expectedSize).Within(Tolerance));
        }

        [TestCase(0f, 1f)]
        [TestCase(-1f, 1f)]
        [TestCase(100f, 0f)]
        [TestCase(100f, -1f)]
        public void NonPositiveAreaOrAspect_IsRejected(float visibleArea, float aspect)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ViewFit.OrthographicSize(visibleArea, aspect));
        }
    }
}
