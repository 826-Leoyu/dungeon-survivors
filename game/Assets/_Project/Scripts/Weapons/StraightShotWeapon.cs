using System.Collections.Generic;
using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// The starting weapon: at a fixed rate, fires a pooled bullet from the shooter straight at the nearest enemy.
    /// Holds fire while there are no enemies, and stops once the Run is over.
    /// </summary>
    public class StraightShotWeapon : MonoBehaviour
    {
        [SerializeField] private RunHost runHost;
        [SerializeField] private PrefabPool bulletPool;
        [SerializeField] private Transform shooter;

        [SerializeField, Min(1)] private int damage = 5;

        [Tooltip("Seconds between shots. Read once, at start.")]
        [SerializeField, Min(0.01f)] private float fireInterval = 0.5f;

        [Tooltip("World units per second.")]
        [SerializeField, Min(0.1f)] private float bulletSpeed = 12f;

        [Tooltip("Seconds before a bullet that hit nothing goes back to the pool. Its range is speed x lifetime.")]
        [SerializeField, Min(0.1f)] private float bulletLifetime = 2f;

        private readonly List<(float X, float Y)> targetPositions = new List<(float X, float Y)>();
        private FireCooldown cooldown;

        private void Awake()
        {
            cooldown = new FireCooldown(fireInterval);
        }

        private void Update()
        {
            if (runHost.Run.IsOver) return;

            cooldown.Tick(Time.deltaTime);

            // Look for a target before spending the shot, so the weapon stays ready while there are no enemies.
            if (!TryFindNearestEnemy(out var enemy) || !cooldown.TryFire()) return;

            FireAt(enemy.transform.position);
        }

        private bool TryFindNearestEnemy(out ChaserEnemy nearest)
        {
            var enemies = ChaserEnemy.Active;
            targetPositions.Clear();
            // Index loop: foreach over an IReadOnlyList boxes its enumerator, allocating garbage every frame.
            for (var i = 0; i < enemies.Count; i++)
            {
                var position = enemies[i].transform.position;
                targetPositions.Add((position.x, position.y));
            }

            var origin = shooter.position;
            var index = NearestTarget.IndexOf(origin.x, origin.y, targetPositions);
            nearest = index >= 0 ? enemies[index] : null;
            return nearest != null;
        }

        // When the pool is at its cap the shot is skipped rather than creating more bullets.
        private void FireAt(Vector2 target)
        {
            var origin = (Vector2)shooter.position;
            if (!bulletPool.TryGet(origin, out var instance)) return;

            // An enemy right on top of the shooter gives a zero direction: the bullet stays put, still overlaps the enemy, and hits it.
            var direction = (target - origin).normalized;
            instance.GetComponent<Bullet>().Launch(direction * bulletSpeed, damage, bulletLifetime, bulletPool);
        }
    }
}
