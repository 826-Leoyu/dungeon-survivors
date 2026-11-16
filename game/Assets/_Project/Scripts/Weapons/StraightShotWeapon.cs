using System.Collections.Generic;
using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// The starting weapon: at a fixed rate, fires a volley of pooled bullets from the shooter at the nearest enemy, fanned out when there are several.
    /// Holds fire while there are no enemies, while the game is frozen, and once the Run is over.
    /// Damage, bullet count and fire rate come from the Run's <see cref="Rules.StraightShot"/>, read every frame so upgrades apply straight away;
    /// the other numbers come from the starting-weapon config.
    /// </summary>
    public class StraightShotWeapon : MonoBehaviour
    {
        [SerializeField] private RunHost runHost;
        [SerializeField] private PrefabPool bulletPool;
        [SerializeField] private Transform shooter;

        private readonly List<(float X, float Y)> targetPositions = new List<(float X, float Y)>();
        private WeaponConfig config;
        private StraightShot stats;
        private FireCooldown cooldown;

        private void Awake()
        {
            config = runHost.Run.Config.StartingWeapon;
            stats = runHost.Run.StraightShot;
            cooldown = new FireCooldown(stats.FireInterval);
        }

        private void Update()
        {
            // A frozen frame (level-up choice, pause) would otherwise fire a ready weapon and leave the bullets hanging in place.
            if (runHost.Run.IsOver || Time.deltaTime <= 0f) return;

            cooldown.Interval = stats.FireInterval;
            cooldown.Tick(Time.deltaTime);

            // Look for a target before spending the shot, so the weapon stays ready while there are no enemies.
            if (!TryFindNearestEnemy(out var enemy) || !cooldown.TryFire()) return;

            FireVolleyAt(enemy.transform.position);
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

        private void FireVolleyAt(Vector2 target)
        {
            var origin = (Vector2)shooter.position;
            // An enemy right on top of the shooter gives a zero direction: the bullets stay put, still overlap the enemy, and hit it.
            var aim = (target - origin).normalized;

            var count = stats.BulletCount;
            for (var i = 0; i < count; i++)
            {
                var offset = Quaternion.Euler(0f, 0f, FanSpread.OffsetDegrees(i, count, config.SpreadDegrees));
                if (!FireBullet(origin, offset * aim)) return;
            }
        }

        // When the pool is at its cap the bullet is skipped rather than creating more. Returns false then, so the rest of the volley is skipped too.
        private bool FireBullet(Vector2 origin, Vector2 direction)
        {
            if (!bulletPool.TryGet(origin, out var instance)) return false;

            instance.GetComponent<Bullet>().Launch(direction * config.BulletSpeed, stats.Damage, config.BulletLifetime, bulletPool);
            return true;
        }
    }
}
