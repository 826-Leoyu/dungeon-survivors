using System.Collections.Generic;
using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// An enemy that walks straight at the player and hurts it on contact. Pooled: the spawner hands it its scene references with
    /// <see cref="Init"/> each time it comes out of the pool, and it releases itself when it dies. Its numbers come from the Run's chaser config.
    /// Its body is a kinematic trigger: it overlaps other enemies, never pushes the player, and passes through walls.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ChaserEnemy : MonoBehaviour
    {
        private static readonly List<ChaserEnemy> active = new List<ChaserEnemy>();

        /// <summary>Every enemy currently out of the pool, for weapons to aim at. Kept by OnEnable / OnDisable, so it never holds a pooled enemy.</summary>
        public static IReadOnlyList<ChaserEnemy> Active => active;

        private Rigidbody2D body;
        private Transform target;
        private RunHost runHost;
        private PrefabPool pool;
        private EnemyConfig config;
        private EnemyHealth health;
        private bool touchingPlayer;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        // Runs every time the pool hands this enemy out, just before Init: start touching nothing.
        private void OnEnable()
        {
            touchingPlayer = false;
            active.Add(this);
        }

        private void OnDisable()
        {
            active.Remove(this);
        }

        /// <summary>
        /// Gives a freshly spawned enemy what it chases, which Run it reports to and takes its numbers from, and which pool to return to.
        /// Starts it at full health.
        /// </summary>
        public void Init(Transform target, RunHost runHost, PrefabPool pool)
        {
            this.target = target;
            this.runHost = runHost;
            this.pool = pool;
            config = runHost.Run.Config.Chaser;
            health = new EnemyHealth(config.MaxHealth);
        }

        /// <summary>Called by weapons. When the hit kills the enemy, reports the kill to the Run and returns the enemy to its pool.</summary>
        public void TakeHit(int damage)
        {
            if (!health.TakeDamage(damage)) return;

            runHost.Run.RecordKill();
            pool.Release(gameObject);
        }

        private void FixedUpdate()
        {
            if (target == null) return;

            // Straight line, no pathfinding: the simplest chase; performance work comes in W3 with profiler data.
            var toTarget = (Vector2)target.position - body.position;
            body.MovePosition(body.position + Vector2.ClampMagnitude(toTarget, config.MoveSpeed * Time.fixedDeltaTime));

            // Damage is reported every step while touching; the Run's hurt cooldown decides which hits land.
            if (touchingPlayer) runHost.Run.TakeDamage(config.ContactDamage);
        }

        // Enter/Exit instead of Stay: Stay stops firing when both bodies fall asleep, e.g. the player standing still under an enemy.
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) touchingPlayer = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) touchingPlayer = false;
        }

        [ContextMenu("Debug: Kill")]
        private void DebugKill() => TakeHit(int.MaxValue);

        // Called by the editor when the component is first added: configure the body as a kinematic mover.
        private void Reset()
        {
            ConfigureBody(GetComponent<Rigidbody2D>());
        }

        /// <summary>Kinematic: moved by code, unaffected by forces or collisions. Interpolated so it renders smoothly between physics steps.</summary>
        public static void ConfigureBody(Rigidbody2D body)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }
}
