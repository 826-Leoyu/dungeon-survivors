using System;
using DungeonSurvivors.Pooling;
using UnityEngine;

namespace DungeonSurvivors
{
    /// <summary>
    /// Presentation adapter over <see cref="ObjectPool{T}"/>: instantiates copies of a prefab under this GameObject, activates them when handed
    /// out and deactivates them when released. Pooled objects reset their own state in OnEnable / OnDisable.
    /// Rules for callers: give instances back with <see cref="Release"/> and never Destroy them (the pool would later hand out a destroyed
    /// object); call Release from gameplay code, not from the instance's own OnDisable.
    /// </summary>
    public class PrefabPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        [Tooltip("Most instances this pool will ever create. At the cap, TryGet fails and the caller skips the spawn. Read once, on first use.")]
        [SerializeField, Min(1)] private int maxSize = 200;

        private ObjectPool<GameObject> pool;
        private Transform inactiveHolder;

        // Built on first use rather than in Awake, so other scripts can call TryGet from their own Awake regardless of execution order.
        private ObjectPool<GameObject> Pool
        {
            get
            {
                if (pool != null) return pool;
                if (prefab == null) throw new InvalidOperationException($"{nameof(PrefabPool)} on '{name}' has no prefab assigned.");
                return pool = new ObjectPool<GameObject>(CreateInstance, reset: instance => instance.SetActive(false), maxSize: maxSize);
            }
        }

        /// <summary>
        /// Places an instance at <paramref name="position"/> (with the prefab's rotation) and activates it. Returns false when the pool is at its cap.
        /// The position is set before activation, so the instance's Awake / OnEnable already see where it spawned.
        /// </summary>
        public bool TryGet(Vector3 position, out GameObject instance)
        {
            if (!Pool.TryGet(out instance)) return false;

            instance.transform.SetPositionAndRotation(position, prefab.transform.rotation);
            instance.SetActive(true);
            return true;
        }

        /// <summary>Deactivates <paramref name="instance"/> and keeps it for reuse. Releasing an already-released instance is ignored and returns false.</summary>
        public bool Release(GameObject instance) => Pool.Release(instance);

        private GameObject CreateInstance()
        {
            // Instantiating under an inactive parent keeps the new copy from running Awake / OnEnable before it is first handed out.
            var instance = Instantiate(prefab, InactiveHolder);
            instance.SetActive(false);
            instance.transform.SetParent(transform, worldPositionStays: false);
            return instance;
        }

        private Transform InactiveHolder
        {
            get
            {
                if (inactiveHolder != null) return inactiveHolder;
                var holder = new GameObject("Instantiating");
                holder.SetActive(false);
                holder.transform.SetParent(transform, worldPositionStays: false);
                return inactiveHolder = holder.transform;
            }
        }
    }
}
