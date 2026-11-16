using System;
using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// Owns the scene's <see cref="Rules.Run"/>: builds it from the Run definition, ticks it every frame, and reacts when it ends.
    /// Other components report events to <see cref="Run"/>, read its state, and read their tuning numbers from <see cref="Rules.Run.Config"/>;
    /// they never decide rules themselves.
    /// </summary>
    public class RunHost : MonoBehaviour
    {
        [Tooltip("The tuning numbers for this Run. Read once, when the Run starts.")]
        [SerializeField] private RunDefinition definition;

        [Tooltip("Seed for this Run's randomness. 0 picks a new seed every play, logged so a Run can be replayed.")]
        [SerializeField] private int seed;

        private Run run;
        private IRandom random;
        private bool endHandled;

        // Built on first use rather than in Awake, so other scripts can use the Run from their own Awake regardless of execution order.
        public Run Run
        {
            get
            {
                EnsureBuilt();
                return run;
            }
        }

        /// <summary>The Run's source of randomness, shared so the whole Run replays from one seed.</summary>
        public IRandom Random
        {
            get
            {
                EnsureBuilt();
                return random;
            }
        }

        private void EnsureBuilt()
        {
            if (run != null) return;

            if (definition == null) throw new InvalidOperationException($"{name} has no Run definition assigned.");
            var config = definition.ToConfig();

            var actualSeed = seed != 0 ? seed : Environment.TickCount;
            Debug.Log($"[RunHost] Starting Run with seed {actualSeed}");

            random = new SeededRandom(actualSeed);
            run = new Run(config, random);
        }

        private void Update()
        {
            if (Run.IsOver)
            {
                if (!endHandled) HandleEnd();
                return;
            }

            Run.Tick(Time.deltaTime);
        }

        // Placeholder until the game-over flow (#18): log the result and freeze the game.
        private void HandleEnd()
        {
            endHandled = true;
            Debug.LogWarning($"[RunHost] Run over: {Run.Outcome} after {Run.ElapsedTime:F1}s, {Run.Kills} kills");
            Time.timeScale = 0f;
        }

        private void OnDestroy()
        {
            // Time scale is global and outlives this scene, so undo the freeze.
            if (endHandled) Time.timeScale = 1f;
        }
    }
}
