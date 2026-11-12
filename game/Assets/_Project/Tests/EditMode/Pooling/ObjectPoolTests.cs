using System;
using NUnit.Framework;

namespace DungeonSurvivors.Pooling.Tests
{
    public class ObjectPoolTests
    {
        private sealed class Item
        {
            public int ResetCount;
        }

        [Test]
        public void GetAfterRelease_ReusesTheReleasedObject()
        {
            var pool = new ObjectPool<Item>(() => new Item());
            pool.TryGet(out var first);

            pool.Release(first);
            pool.TryGet(out var second);

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void Release_RunsTheResetCallback()
        {
            var pool = new ObjectPool<Item>(() => new Item(), reset: item => item.ResetCount++);
            pool.TryGet(out var item);

            pool.Release(item);

            Assert.That(item.ResetCount, Is.EqualTo(1));
        }

        [Test]
        public void GetAtCapacity_RefusesToCreate()
        {
            var created = 0;
            var pool = new ObjectPool<Item>(() => { created++; return new Item(); }, maxSize: 2);
            pool.TryGet(out _);
            pool.TryGet(out _);

            var got = pool.TryGet(out var third);

            Assert.That(got, Is.False);
            Assert.That(third, Is.Null);
            Assert.That(created, Is.EqualTo(2));
        }

        [Test]
        public void GetAtCapacity_AfterARelease_ReusesTheReleasedObject()
        {
            var pool = new ObjectPool<Item>(() => new Item(), maxSize: 1);
            pool.TryGet(out var first);
            pool.Release(first);

            var got = pool.TryGet(out var second);

            Assert.That(got, Is.True);
            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void ReleasingTheSameObjectTwice_IsIgnored()
        {
            var pool = new ObjectPool<Item>(() => new Item(), reset: item => item.ResetCount++);
            pool.TryGet(out var item);
            pool.Release(item);

            var releasedAgain = pool.Release(item);
            pool.TryGet(out var a);
            pool.TryGet(out var b);

            Assert.That(releasedAgain, Is.False);
            Assert.That(item.ResetCount, Is.EqualTo(1));
            Assert.That(a, Is.Not.SameAs(b));
        }

        [Test]
        public void ReleasingAnObjectFromElsewhere_IsRejected()
        {
            var pool = new ObjectPool<Item>(() => new Item());

            Assert.Throws<ArgumentException>(() => pool.Release(new Item()));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveMaxSize_IsRejected(int maxSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ObjectPool<Item>(() => new Item(), maxSize: maxSize));
        }
    }
}
