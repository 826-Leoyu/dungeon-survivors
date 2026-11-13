using DungeonSurvivors.Rules;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// Spawns chaser enemies from a pool at a fixed rate, on a ring just outside the camera's view. Stops once the Run is over.
    /// The fixed rate is a placeholder until the spawn director (W2).
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private RunHost runHost;
        [SerializeField] private PrefabPool pool;
        [SerializeField] private Transform target;
        [SerializeField] private Camera viewCamera;

        [Tooltip("Seconds between spawns. Read once, at start.")]
        [SerializeField, Min(0.01f)] private float spawnInterval = 0.5f;

        [Tooltip("Gap between the screen edge and the spawn point. Keep it larger than the enemy's radius so it spawns fully off-screen.")]
        [SerializeField, Min(0f)] private float margin = 1f;

        private SpawnTimer timer;

        private void Awake()
        {
            timer = new SpawnTimer(spawnInterval);
        }

        private void Update()
        {
            if (runHost.Run.IsOver) return;

            var due = timer.Tick(Time.deltaTime);
            for (var i = 0; i < due; i++)
            {
                if (!TrySpawn()) break;
            }
        }

        // Returns false when the pool is at its cap; the spawn is skipped rather than creating more enemies.
        private bool TrySpawn()
        {
            // Orthographic camera: orthographicSize is half the view's height in world units.
            var halfHeight = viewCamera.orthographicSize;
            var halfWidth = halfHeight * viewCamera.aspect;
            var (x, y) = SpawnRing.PickOffset(halfWidth, halfHeight, margin, runHost.Random);

            var centre = viewCamera.transform.position;
            if (!pool.TryGet(new Vector3(centre.x + x, centre.y + y, 0f), out var instance)) return false;

            instance.GetComponent<ChaserEnemy>().Init(target, runHost, pool);
            return true;
        }

        [ContextMenu("Debug: Kill All Enemies")]
        private void DebugKillAll()
        {
            foreach (var enemy in pool.GetComponentsInChildren<ChaserEnemy>()) enemy.TakeHit(int.MaxValue);
        }
    }
}
