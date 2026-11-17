using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// The orbiting knives: once the Run owns them, knives circle the player, evenly spaced, and hurt the enemies they touch.
    /// One enemy is hit at most once per hit interval, however many knives touch it. Shows no knives until they are owned.
    /// Knife count, damage, radius and rotation speed come from the Run's <see cref="Rules.OrbitingKnives"/>, read every physics step
    /// so upgrades apply straight away. Moves in FixedUpdate, so it stops while the game is frozen.
    /// </summary>
    public class OrbitingKnivesWeapon : MonoBehaviour
    {
        [SerializeField] private RunHost runHost;
        [SerializeField] private Knife knifePrefab;

        [Tooltip("The body the knives circle: the player.")]
        [SerializeField] private Rigidbody2D center;

        private OrbitingKnives current;
        private HitCooldowns cooldowns;
        private Knife[] knives;
        private int shownCount;
        private float rotationDegrees;

        private void Awake()
        {
            current = runHost.Run.OrbitingKnives;
            cooldowns = new HitCooldowns(current.HitInterval);
            CreateKnives();
        }

        // Every knife the weapon can ever show, created once and hidden: there are only a few and they are never used up, so no pool is needed.
        // Children of this object, not of the player: colliders under the player's body would become part of it.
        private void CreateKnives()
        {
            var upgrades = runHost.Run.Config.OrbitingKnives;
            knives = new Knife[upgrades.KnifeCount + upgrades.Upgrades.MaxKnifeCountLevel];
            for (var i = 0; i < knives.Length; i++)
            {
                knives[i] = Instantiate(knifePrefab, transform);
                knives[i].Init(this);
                knives[i].gameObject.SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            var count = runHost.Run.IsOver || !current.IsOwned ? 0 : current.KnifeCount;
            if (count == 0)
            {
                ShowKnives(0);
                return;
            }

            cooldowns.Interval = current.HitInterval;
            cooldowns.Tick(Time.fixedDeltaTime);
            rotationDegrees = Mathf.Repeat(rotationDegrees + current.RotationSpeed * Time.fixedDeltaTime, 360f);

            var newlyShown = shownCount;
            ShowKnives(count);
            PlaceKnives(count, newlyShown);
        }

        private void ShowKnives(int count)
        {
            if (count == shownCount) return;
            for (var i = 0; i < knives.Length; i++) knives[i].gameObject.SetActive(i < count);
            shownCount = count;
        }

        // Knives from firstNew on were hidden until now: they jump to their place instead of sweeping across the screen from where they were left.
        private void PlaceKnives(int count, int firstNew)
        {
            // The player moves by velocity, so its body only reaches this step's position after this FixedUpdate: aim there, or the ring trails behind it.
            var centre = center.position + center.linearVelocity * Time.fixedDeltaTime;
            for (var i = 0; i < count; i++)
            {
                var (x, y) = OrbitLayout.Offset(i, count, rotationDegrees, current.Radius);
                var position = centre + new Vector2(x, y);
                // Blade points along the orbit: the knife sprite is long along its local y axis, and the orbit's tangent is the radius turned 90 degrees.
                var angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

                var body = knives[i].Body;
                if (i >= firstNew)
                {
                    body.position = position;
                    body.rotation = angle;
                }
                else
                {
                    body.MovePositionAndRotation(position, angle);
                }
            }
        }

        /// <summary>Called by a knife touching <paramref name="enemy"/>: damages it unless the knives hit it less than a hit interval ago.</summary>
        public void Hit(ChaserEnemy enemy)
        {
            if (runHost.Run.IsOver || !cooldowns.TryHit(enemy.SpawnId)) return;
            enemy.TakeHit(current.Damage);
        }
    }
}
