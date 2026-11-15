using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>One kind of enemy's tuning numbers, edited in the Inspector and converted to an <see cref="EnemyConfig"/> when a Run starts.</summary>
    [CreateAssetMenu(menuName = "Dungeon Survivors/Enemy Definition", fileName = "Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        [SerializeField, Min(1)] private int maxHealth = 10;

        [Tooltip("World units per second.")]
        [SerializeField, Min(0f)] private float moveSpeed = 2f;

        [Tooltip("Damage reported to the Run while touching the player; the player's hurt cooldown decides which hits land.")]
        [SerializeField, Min(1)] private int contactDamage = 10;

        public EnemyConfig ToConfig() => new EnemyConfig(maxHealth, moveSpeed, contactDamage);
    }
}
