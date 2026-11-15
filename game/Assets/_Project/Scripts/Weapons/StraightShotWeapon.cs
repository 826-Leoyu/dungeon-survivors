using System.Collections.Generic;
using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// The starting weapon: at a fixed rate, fires a pooled bullet from the shooter straight at the nearest enemy.
    /// Holds fire while there are no enemies, and stops once the Run is over. Its numbers come from the Run's starting-weapon config, read once at start.
    /// </summary>
    public class StraightShotWeapon : MonoBehaviour
    {
        [SerializeField] private RunHost runHost;
        [SerializeField] private PrefabPool bulletPool;
        [SerializeField] private Transform shooter;

        private readonly List<(float X, float Y)> targetPositions = new List<(float X, float Y)>();
        private WeaponConfig config;
        private FireCooldown cooldown;

        private void Awake()
        {
            config = runHost.Run.Config.StartingWeapon;
            cooldown = new FireCooldown(config.FireInterval);
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
            instance.GetComponent<Bullet>().Launch(direction * config.BulletSpeed, config.Damage, config.BulletLifetime, bulletPool);
        }
    }
}
