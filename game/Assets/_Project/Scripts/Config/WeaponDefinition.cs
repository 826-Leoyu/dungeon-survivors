using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>A bullet weapon's tuning numbers and upgrades, edited in the Inspector and converted to a <see cref="WeaponConfig"/> when a Run starts.</summary>
    [CreateAssetMenu(menuName = "Dungeon Survivors/Weapon Definition", fileName = "Weapon")]
    public class WeaponDefinition : ScriptableObject
    {
        [Tooltip("Damage each bullet deals to the enemy it hits.")]
        [SerializeField, Min(1)] private int damage = 5;

        [Tooltip("Seconds between shots.")]
        [SerializeField, Min(0.01f)] private float fireInterval = 0.5f;

        [Tooltip("World units per second.")]
        [SerializeField, Min(0.1f)] private float bulletSpeed = 12f;

        [Tooltip("Seconds before a bullet that hit nothing goes back to the pool. Its range is speed x lifetime.")]
        [SerializeField, Min(0.1f)] private float bulletLifetime = 2f;

        [Tooltip("Degrees between neighbouring bullets when a volley fires several of them as a fan.")]
        [SerializeField, Range(0f, 90f)] private float spreadDegrees = 15f;

        [Header("Upgrades (a max level of 0 means never offered)")]
        [Tooltip("Damage each damage upgrade adds to every bullet.")]
        [SerializeField, Min(1)] private int damagePerLevel = 2;

        [SerializeField, Min(0)] private int maxDamageLevel = 5;

        [Tooltip("Times the bullet-count upgrade can be picked; each pick adds one bullet to every volley.")]
        [SerializeField, Min(0)] private int maxBulletCountLevel = 3;

        [Tooltip("Each fire-rate upgrade multiplies the seconds between shots by this.")]
        [SerializeField, Range(0.05f, 0.99f)] private float fireIntervalMultiplier = 0.85f;

        [SerializeField, Min(0)] private int maxFireRateLevel = 5;

        public WeaponConfig ToConfig() => new WeaponConfig(damage, fireInterval, bulletSpeed, bulletLifetime, spreadDegrees,
            new WeaponUpgradeConfig(damagePerLevel, maxDamageLevel, maxBulletCountLevel, fireIntervalMultiplier, maxFireRateLevel));
    }
}
