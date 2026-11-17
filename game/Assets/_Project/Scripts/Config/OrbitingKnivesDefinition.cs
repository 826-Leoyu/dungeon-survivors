using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>The orbiting knives' tuning numbers and upgrades, edited in the Inspector and converted to an <see cref="OrbitingKnivesConfig"/> when a Run starts.</summary>
    [CreateAssetMenu(menuName = "Dungeon Survivors/Orbiting Knives Definition", fileName = "OrbitingKnives")]
    public class OrbitingKnivesDefinition : ScriptableObject
    {
        [Tooltip("Damage each hit deals to an enemy.")]
        [SerializeField, Min(1)] private int damage = 4;

        [Tooltip("Knives circling the player when the weapon is first picked.")]
        [SerializeField, Min(1)] private int knifeCount = 2;

        [Tooltip("World units from the player to each knife.")]
        [SerializeField, Min(0.1f)] private float radius = 1.6f;

        [Tooltip("Degrees per second the knives turn around the player.")]
        [SerializeField, Min(1f)] private float rotationSpeed = 180f;

        [Tooltip("Seconds after a hit before the knives can hit the same enemy again, however many knives touch it.")]
        [SerializeField, Min(0.01f)] private float hitInterval = 0.5f;

        [Header("Upgrades (a max level of 0 means never offered)")]
        [Tooltip("Damage each damage upgrade adds to every hit.")]
        [SerializeField, Min(1)] private int damagePerLevel = 3;

        [SerializeField, Min(0)] private int maxDamageLevel = 5;

        [Tooltip("Times the knife-count upgrade can be picked; each pick adds one knife.")]
        [SerializeField, Min(0)] private int maxKnifeCountLevel = 4;

        [Tooltip("World units each radius upgrade adds to the orbit.")]
        [SerializeField, Min(0.05f)] private float radiusPerLevel = 0.5f;

        [SerializeField, Min(0)] private int maxRadiusLevel = 3;

        [Tooltip("Degrees per second each rotation-speed upgrade adds.")]
        [SerializeField, Min(1f)] private float rotationSpeedPerLevel = 60f;

        [SerializeField, Min(0)] private int maxRotationSpeedLevel = 3;

        public OrbitingKnivesConfig ToConfig() => new OrbitingKnivesConfig(damage, knifeCount, radius, rotationSpeed, hitInterval,
            new OrbitingKnivesUpgradeConfig(
                damagePerLevel, maxDamageLevel, maxKnifeCountLevel, radiusPerLevel, maxRadiusLevel, rotationSpeedPerLevel, maxRotationSpeedLevel));
    }
}
