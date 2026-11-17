using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// One of the orbiting knives: reports every enemy it touches to its weapon, which decides whether the hit lands.
    /// The weapon moves it; its body is a kinematic trigger, like the bullets': it passes through walls and the player and is never used up.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Knife : MonoBehaviour
    {
        private OrbitingKnivesWeapon weapon;

        /// <summary>The physics body the weapon moves.</summary>
        public Rigidbody2D Body { get; private set; }

        private void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
        }

        /// <summary>Sets the weapon this knife reports its touches to.</summary>
        public void Init(OrbitingKnivesWeapon weapon)
        {
            this.weapon = weapon;
        }

        // Stay rather than Enter: an enemy pressed against a knife keeps getting hit once per hit interval, not only on first contact.
        // The knife moves every physics step, so it never falls asleep and Stay keeps firing.
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent(out ChaserEnemy enemy)) weapon.Hit(enemy);
        }

        // Called by the editor when the component is first added: configure the body as a kinematic mover.
        private void Reset()
        {
            ConfigureBody(GetComponent<Rigidbody2D>());
        }

        /// <summary>Kinematic: moved by the weapon, unaffected by forces or collisions. Interpolated so it renders smoothly between physics steps.</summary>
        public static void ConfigureBody(Rigidbody2D body)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }
}
