using System.Collections.Generic;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class NearestTargetTests
    {
        [Test]
        public void NoTargets_ReturnsMinusOne()
        {
            var targets = new List<(float X, float Y)>();

            Assert.That(NearestTarget.IndexOf(0f, 0f, targets), Is.EqualTo(-1));
        }

        [Test]
        public void SeveralTargets_PicksTheClosestToTheOrigin()
        {
            // Measured from (10, 10): index 0 is 5 away, index 1 is 1 away, index 2 is 10 away.
            var targets = new List<(float X, float Y)> { (13f, 14f), (10f, 11f), (0f, 10f) };

            Assert.That(NearestTarget.IndexOf(10f, 10f, targets), Is.EqualTo(1));
        }

        [Test]
        public void TargetsAtEqualDistance_PicksTheEarlierOne()
        {
            var targets = new List<(float X, float Y)> { (3f, 0f), (0f, 3f) };

            Assert.That(NearestTarget.IndexOf(0f, 0f, targets), Is.EqualTo(0));
        }
    }
}
