using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>The XP curve's tuning numbers, edited in the Inspector and converted to a <see cref="ProgressionConfig"/> when a Run starts.</summary>
    [CreateAssetMenu(menuName = "Dungeon Survivors/Progression Definition", fileName = "Progression")]
    public class ProgressionDefinition : ScriptableObject
    {
        [Tooltip("XP needed to go from level 1 to level 2.")]
        [SerializeField, Min(1)] private int firstLevelXp = 5;

        [Tooltip("Extra XP each level costs over the one before it. 0 makes every level cost the same.")]
        [SerializeField, Min(0)] private int xpGrowth = 5;

        public ProgressionConfig ToConfig() => new ProgressionConfig(firstLevelXp, xpGrowth);
    }
}
