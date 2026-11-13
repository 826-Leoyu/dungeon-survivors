using System;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class FloatingStickTests
    {
        private const float Radius = 100f;
        private const int Finger = 0;

        [Test]
        public void DragWithinRadius_GivesStrengthProportionalToDistance()
        {
            var stick = new FloatingStick(Radius);
            stick.Press(Finger, 300f, 200f);

            stick.Drag(Finger, 350f, 200f);

            Assert.That(stick.Value, Is.EqualTo((0.5f, 0f)));
        }

        [Test]
        public void DragPastRadius_IsFullStrengthInTheSameDirection()
        {
            var stick = new FloatingStick(Radius);
            stick.Press(Finger, 300f, 200f);

            // A 3-4-5 triangle: 500 away, five radii, pointing (0.6, 0.8).
            stick.Drag(Finger, 600f, 600f);

            Assert.That(stick.Value.X, Is.EqualTo(0.6f).Within(1e-5f));
            Assert.That(stick.Value.Y, Is.EqualTo(0.8f).Within(1e-5f));
        }

        [Test]
        public void Release_ReturnsToZeroAndLetsGo()
        {
            var stick = new FloatingStick(Radius);
            stick.Press(Finger, 300f, 200f);
            stick.Drag(Finger, 350f, 200f);

            stick.Release(Finger);

            Assert.That(stick.Value, Is.EqualTo((0f, 0f)));
            Assert.That(stick.IsHeld, Is.False);
        }

        [Test]
        public void SecondFinger_CannotTakeOverTheStick()
        {
            const int OtherFinger = 1;
            var stick = new FloatingStick(Radius);
            stick.Press(Finger, 300f, 200f);
            stick.Drag(Finger, 350f, 200f);

            var tookOver = stick.Press(OtherFinger, 0f, 0f);
            stick.Drag(OtherFinger, 900f, 900f);
            stick.Release(OtherFinger);

            Assert.That(tookOver, Is.False);
            Assert.That(stick.IsHeld, Is.True);
            Assert.That(stick.Value, Is.EqualTo((0.5f, 0f)));
        }

        [Test]
        public void PressAfterRelease_StartsAtZeroWhereTheFingerLands()
        {
            var stick = new FloatingStick(Radius);
            stick.Press(Finger, 300f, 200f);
            stick.Drag(Finger, 400f, 200f);
            stick.Release(Finger);

            stick.Press(Finger, 50f, 60f);

            Assert.That(stick.Origin, Is.EqualTo((50f, 60f)));
            Assert.That(stick.Value, Is.EqualTo((0f, 0f)));

            stick.Drag(Finger, 50f, 10f);

            Assert.That(stick.Value, Is.EqualTo((0f, -0.5f)));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NonPositiveRadius_IsRejected(float radius)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FloatingStick(radius));
        }
    }
}
