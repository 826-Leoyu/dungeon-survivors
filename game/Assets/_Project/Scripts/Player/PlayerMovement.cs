using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonSurvivors
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        private Rigidbody2D body;
        private InputAction moveAction;
        private Vector2 moveInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            moveAction = InputSystem.actions.FindAction("Player/Move", throwIfNotFound: true);
        }

        private void Update()
        {
            // Keyboard composites are already normalized; the clamp covers analog sticks and the future touch joystick.
            moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        }

        private void FixedUpdate()
        {
            body.linearVelocity = moveInput * moveSpeed;
        }

        // Called by the editor when the component is first added: configure the body for top-down movement.
        private void Reset()
        {
            ConfigureBody(GetComponent<Rigidbody2D>());
        }

        /// <summary>Top-down setup: no gravity, no spinning on collisions, interpolated so it renders smoothly between physics steps.</summary>
        public static void ConfigureBody(Rigidbody2D body)
        {
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }
}
