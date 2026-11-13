using System;

namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// The floating joystick's rules: a stick that appears wherever a finger presses and turns the drag into a direction and strength.
    /// Owned by one finger at a time: other fingers cannot press, drag or release it until that finger lets go.
    /// The origin stays where the finger first pressed; dragging past the radius keeps full strength.
    /// Positions and radius share any consistent unit (the caller uses canvas units).
    /// </summary>
    public sealed class FloatingStick
    {
        private readonly float radius;
        private int ownerId;
        private float originX, originY;

        /// <param name="radius">Drag distance from the origin that gives full strength.</param>
        public FloatingStick(float radius)
        {
            if (radius <= 0f) throw new ArgumentOutOfRangeException(nameof(radius), radius, "Must be positive.");
            this.radius = radius;
        }

        /// <summary>Direction times strength; its length is never more than 1. Zero while not held.</summary>
        public (float X, float Y) Value { get; private set; }

        public bool IsHeld { get; private set; }

        /// <summary>Where the owning finger first pressed. Meaningless while not held.</summary>
        public (float X, float Y) Origin => (originX, originY);

        /// <summary>Places the stick under the finger, at zero strength. Returns false, and changes nothing, if another finger holds it.</summary>
        public bool Press(int pointerId, float x, float y)
        {
            if (IsHeld) return false;

            IsHeld = true;
            ownerId = pointerId;
            originX = x;
            originY = y;
            Value = (0f, 0f);
            return true;
        }

        /// <summary>Updates the strength from the owning finger's position; ignored for any other finger.</summary>
        public void Drag(int pointerId, float x, float y)
        {
            if (!IsOwnedBy(pointerId)) return;

            var valueX = (x - originX) / radius;
            var valueY = (y - originY) / radius;
            var length = (float)Math.Sqrt(valueX * valueX + valueY * valueY);
            if (length > 1f)
            {
                valueX /= length;
                valueY /= length;
            }

            Value = (valueX, valueY);
        }

        /// <summary>Lets go and returns to zero when the owning finger lifts; ignored for any other finger.</summary>
        public void Release(int pointerId)
        {
            if (!IsOwnedBy(pointerId)) return;

            IsHeld = false;
            Value = (0f, 0f);
        }

        private bool IsOwnedBy(int pointerId) => IsHeld && pointerId == ownerId;
    }
}
