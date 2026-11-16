using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>A bullet weapon's tuning numbers, edited in the Inspector and converted to a <see cref="WeaponConfig"/> when a Run starts.</summary>
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

        public WeaponConfig ToConfig() => new WeaponConfig(damage, fireInterval, bulletSpeed, bulletLifetime);
    }
}
