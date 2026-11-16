using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// An XP gem dropped by a dying enemy. It sits still until the player comes within the pickup radius, then gives its XP to the Run
    /// and goes back to its pool. Pooled: the enemy that drops it hands it its XP and scene references with <see cref="Init"/>.
    /// No collider: a distance check against the Run's pickup radius keeps the radius a single tuning number instead of a collider size on a prefab.
    /// </summary>
    public class XpGem : MonoBehaviour
    {
        private int xp;
        private Transform player;
        private RunHost runHost;
        private PrefabPool pool;
        private float pickupRadiusSquared;

        /// <summary>Gives a freshly dropped gem its XP, who can pick it up, which Run it reports to, and which pool to return to.</summary>
        public void Init(int xp, Transform player, RunHost runHost, PrefabPool pool)
        {
            this.xp = xp;
            this.player = player;
            this.runHost = runHost;
            this.pool = pool;
            var radius = runHost.Run.Config.Player.PickupRadius;
            pickupRadiusSquared = radius * radius;
        }

        private void Update()
        {
            if (runHost.Run.IsOver) return;

            // Squared distances avoid a square root per gem per frame.
            if (((Vector2)(player.position - transform.position)).sqrMagnitude > pickupRadiusSquared) return;

            runHost.Run.GainXp(xp);
            pool.Release(gameObject);
        }
    }
}
