using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>The player's tuning numbers, edited in the Inspector and converted to a <see cref="PlayerConfig"/> when a Run starts.</summary>
    [CreateAssetMenu(menuName = "Dungeon Survivors/Player Definition", fileName = "Player")]
    public class PlayerDefinition : ScriptableObject
    {
        [SerializeField, Min(1)] private int maxHealth = 100;

        [Tooltip("Seconds after taking damage during which further damage is ignored.")]
        [SerializeField, Min(0f)] private float hurtCooldown = 0.5f;

        [Tooltip("World units per second at full stick.")]
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        [Tooltip("World units: an XP gem closer than this to the player is picked up.")]
        [SerializeField, Min(0.01f)] private float pickupRadius = 1.5f;

        public PlayerConfig ToConfig() => new PlayerConfig(maxHealth, hurtCooldown, moveSpeed, pickupRadius);
    }
}
