using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// A projectile that flies in a straight line, damages the first enemy it touches, and goes back to its pool on a hit or when its lifetime runs out.
    /// Pooled: the weapon hands it its flight with <see cref="Launch"/> each time it comes out of the pool.
    /// Its body is a kinematic trigger, like the enemies': it passes through walls and the player.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        private Rigidbody2D body;
        private PrefabPool pool;
        private int damage;
        private float lifetimeRemaining;
        private bool spent;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        // Runs every time the pool hands this bullet out: it has not hit anything yet.
        private void OnEnable()
        {
            spent = false;
        }

        /// <summary>Sends a freshly spawned bullet flying at <paramref name="velocity"/> (world units per second) for <paramref name="lifetime"/> seconds.</summary>
        public void Launch(Vector2 velocity, int damage, float lifetime, PrefabPool pool)
        {
            this.damage = damage;
            this.pool = pool;
            lifetimeRemaining = lifetime;
            body.linearVelocity = velocity;
        }

        private void Update()
        {
            lifetimeRemaining -= Time.deltaTime;
            if (lifetimeRemaining <= 0f) Expire();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Touching two enemies in one physics step sends two Enter messages; only the first one counts.
            if (spent || !other.TryGetComponent(out ChaserEnemy enemy)) return;

            enemy.TakeHit(damage);
            Expire();
        }

        private void Expire()
        {
            spent = true;
            pool.Release(gameObject);
        }

        // Called by the editor when the component is first added: configure the body as a kinematic mover.
        private void Reset()
        {
            ConfigureBody(GetComponent<Rigidbody2D>());
        }

        /// <summary>Kinematic: moved by its velocity, unaffected by forces or collisions. Interpolated so it renders smoothly between physics steps.</summary>
        public static void ConfigureBody(Rigidbody2D body)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }
}
