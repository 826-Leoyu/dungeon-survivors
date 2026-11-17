using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class OrbitLayoutTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void SingleKnifeWithNoRotation_SitsToTheRight()
        {
            AssertOffset(OrbitLayout.Offset(0, 1, rotationDegrees: 0f, radius: 2f), 2f, 0f);
        }

        [Test]
        public void FourKnives_AreSpreadAQuarterTurnApart()
        {
            AssertOffset(OrbitLayout.Offset(0, 4, 0f, 2f), 2f, 0f);
            AssertOffset(OrbitLayout.Offset(1, 4, 0f, 2f), 0f, 2f);
            AssertOffset(OrbitLayout.Offset(2, 4, 0f, 2f), -2f, 0f);
            AssertOffset(OrbitLayout.Offset(3, 4, 0f, 2f), 0f, -2f);
        }

        [Test]
        public void Rotation_TurnsEveryKnifeCounterClockwise()
        {
            AssertOffset(OrbitLayout.Offset(0, 2, 90f, 1f), 0f, 1f);
            AssertOffset(OrbitLayout.Offset(1, 2, 90f, 1f), 0f, -1f);
        }

        [Test]
        public void Radius_SetsTheDistanceFromTheCentre()
        {
            AssertOffset(OrbitLayout.Offset(0, 1, 0f, 3.5f), 3.5f, 0f);
        }

        [TestCase(-1, 3)]
        [TestCase(3, 3)]
        [TestCase(0, 0)]
        public void IndexOutsideTheOrbit_IsRejected(int index, int count)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => OrbitLayout.Offset(index, count, 0f, 1f));
        }

        private static void AssertOffset((float X, float Y) offset, float x, float y)
        {
            Assert.That(offset.X, Is.EqualTo(x).Within(Tolerance));
            Assert.That(offset.Y, Is.EqualTo(y).Within(Tolerance));
        }
    }
}
