using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonSurvivors
{
    /// <summary>Moves the player from the Player/Move action, at the speed in the Run's player config.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private RunHost runHost;

        private float moveSpeed;
        private Rigidbody2D body;
        private InputAction moveAction;
        private Vector2 moveInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            moveSpeed = runHost.Run.Config.Player.MoveSpeed;
            moveAction = InputSystem.actions.FindAction("Player/Move", throwIfNotFound: true);
        }

        private void Update()
        {
            // Keyboard composites are already normalized; the clamp is a guard for any device that is not (analog sticks, the floating joystick).
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
