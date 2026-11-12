using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DungeonSurvivors.Pooling
{
    /// <summary>
    /// Reuses objects instead of creating and destroying them. Pure C#: the presentation layer supplies callbacks that create and reset
    /// the actual objects (e.g. GameObjects).
    /// </summary>
    public sealed class ObjectPool<T> where T : class
    {
        private readonly Func<T> create;
        private readonly Action<T> reset;
        private readonly int maxSize;
        private readonly Stack<T> inactive = new Stack<T>();
        private readonly HashSet<T> owned = new HashSet<T>(ReferenceComparer.Instance);
        private readonly HashSet<T> active = new HashSet<T>(ReferenceComparer.Instance);

        /// <param name="create">Makes a new object when none is waiting for reuse. Must not return null.</param>
        /// <param name="reset">The reset callback: runs on every object given back with <see cref="Release"/> (e.g. deactivate it).</param>
        /// <param name="maxSize">Most objects this pool will ever create, counting both handed-out and waiting ones.</param>
        public ObjectPool(Func<T> create, Action<T> reset = null, int maxSize = int.MaxValue)
        {
            this.create = create ?? throw new ArgumentNullException(nameof(create));
            this.reset = reset;
            if (maxSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxSize), maxSize, "Must be positive.");
            this.maxSize = maxSize;
        }

        /// <summary>
        /// Hands out a released object if one is waiting, otherwise creates one. Returns false (and null) when every object is handed out and
        /// the pool already holds <c>maxSize</c> objects: the caller skips the spawn rather than exceeding the cap.
        /// </summary>
        public bool TryGet(out T item)
        {
            if (inactive.Count > 0)
            {
                item = inactive.Pop();
            }
            else if (owned.Count < maxSize)
            {
                item = create() ?? throw new InvalidOperationException("The create callback returned null.");
                owned.Add(item);
            }
            else
            {
                item = null;
                return false;
            }

            active.Add(item);
            return true;
        }

        /// <summary>
        /// Gives <paramref name="item"/> back to the pool for reuse and runs the reset callback. Releasing an object that is already in the pool is ignored and returns false,
        /// so two systems releasing the same object in one frame is harmless. Releasing an object this pool never created is a caller bug and throws.
        /// </summary>
        public bool Release(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!owned.Contains(item)) throw new ArgumentException("Object was not created by this pool.", nameof(item));
            if (!active.Remove(item)) return false;

            reset?.Invoke(item);
            inactive.Push(item);
            return true;
        }

        /// <summary>Identity comparison, so pooled types that override Equals are still tracked per instance.</summary>
        private sealed class ReferenceComparer : IEqualityComparer<T>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            public bool Equals(T x, T y) => ReferenceEquals(x, y);

            public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
