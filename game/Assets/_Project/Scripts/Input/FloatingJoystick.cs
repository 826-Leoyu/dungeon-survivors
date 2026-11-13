using DungeonSurvivors.Rules;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace DungeonSurvivors
{
    /// <summary>
    /// Touch joystick that appears wherever a finger presses on this (full-screen, invisible) UI element and feeds a
    /// virtual gamepad stick, so <c>Player/Move</c> reads it like any other device and movement code never knows the difference.
    /// Sits under all other UI: a press on a button hits the button first and never reaches this element.
    /// The mouse drives it too (left button), which is how it is tested in the editor.
    /// </summary>
    public class FloatingJoystick : OnScreenControl,
        IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler, IPointerUpHandler
    {
        [InputControl(layout = "Vector2")]
        [SerializeField] private string stickControlPath = "<Gamepad>/leftStick";

        [Tooltip("Drag distance for full speed, in canvas units (pixels at the canvas reference resolution). Read once, at enable.")]
        [SerializeField, Min(1f)] private float radius = 80f;

        [Tooltip("Shown at the press point while held.")]
        [SerializeField] private RectTransform stickBase;

        [Tooltip("Follows the finger, up to the radius. Usually a child of the base.")]
        [SerializeField] private RectTransform knob;

        private Canvas canvas;
        private FloatingStick stick;

        protected override string controlPathInternal
        {
            get => stickControlPath;
            set => stickControlPath = value;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            canvas = GetComponentInParent<Canvas>().rootCanvas;
            // A fresh stick also drops any finger that was still down when the joystick was disabled.
            stick = new FloatingStick(radius);
            ShowVisuals(false);
        }

        protected override void OnDisable()
        {
            // The base sends the stick back to its default (zero) value, so the player stops.
            base.OnDisable();
            ShowVisuals(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsStickButton(eventData)) return;

            var position = ToCanvasUnits(eventData.position);
            if (!stick.Press(eventData.pointerId, position.x, position.y)) return;

            SendValueToControl(Vector2.zero);
            ShowVisuals(true);
            UpdateVisuals();
        }

        // Without this, OnDrag only starts after the EventSystem's drag threshold, adding a dead spot on top of the stick's own.
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsStickButton(eventData)) return;

            var position = ToCanvasUnits(eventData.position);
            stick.Drag(eventData.pointerId, position.x, position.y);
            if (!stick.IsHeld) return;

            SendValueToControl(ToVector(stick.Value));
            UpdateVisuals();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsStickButton(eventData)) return;

            stick.Release(eventData.pointerId);
            if (stick.IsHeld) return; // another finger lifted; the owner is still down

            SendValueToControl(Vector2.zero);
            ShowVisuals(false);
        }

        /// <summary>
        /// Touches always report the left button. A mouse uses one pointer id for all its buttons, so without this a right
        /// click while dragging with the left would release the stick.
        /// </summary>
        private static bool IsStickButton(PointerEventData eventData) => eventData.button == PointerEventData.InputButton.Left;

        /// <summary>
        /// Screen pixels to canvas units, so the radius means the same physical size on every screen the canvas scaler adapts to.
        /// Only distances between positions matter to the stick, so no offset is needed.
        /// </summary>
        private Vector2 ToCanvasUnits(Vector2 screenPosition) => screenPosition / canvas.scaleFactor;

        private void UpdateVisuals()
        {
            // On a Screen Space - Overlay canvas, a UI element's world position is its screen position in pixels.
            var origin = ToVector(stick.Origin) * canvas.scaleFactor;
            if (stickBase != null) stickBase.position = origin;
            if (knob != null) knob.position = origin + ToVector(stick.Value) * (radius * canvas.scaleFactor);
        }

        private void ShowVisuals(bool visible)
        {
            if (stickBase != null) stickBase.gameObject.SetActive(visible);
            if (knob != null) knob.gameObject.SetActive(visible);
        }

        private static Vector2 ToVector((float X, float Y) value) => new Vector2(value.X, value.Y);
    }
}
