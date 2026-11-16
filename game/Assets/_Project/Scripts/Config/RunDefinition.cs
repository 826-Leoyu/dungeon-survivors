using System;
using DungeonSurvivors.Rules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DungeonSurvivors
{
    /// <summary>
    /// Bundles the definitions one Run uses. <see cref="RunHost"/> converts it to a <see cref="RunConfig"/> once, when the Run starts,
    /// so edits to any of these assets take effect on the next play without code changes.
    /// </summary>
    [CreateAssetMenu(menuName = "Dungeon Survivors/Run Definition", fileName = "Run")]
    public class RunDefinition : ScriptableObject
    {
        [SerializeField] private PlayerDefinition player;
        [SerializeField] private EnemyDefinition chaser;
        [SerializeField] private WeaponDefinition startingWeapon;

        public RunConfig ToConfig() => new RunConfig(
            Required(player, nameof(player)).ToConfig(),
            Required(chaser, nameof(chaser)).ToConfig(),
            Required(startingWeapon, nameof(startingWeapon)).ToConfig());

        // Unity's == also catches a destroyed or missing asset, which ?? would let through.
        private T Required<T>(T definition, string field) where T : Object =>
            definition != null ? definition : throw new InvalidOperationException($"Run definition '{name}' has no {field} assigned.");
    }
}
