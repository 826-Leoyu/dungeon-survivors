using System;
using System.Linq;
using NUnit.Framework;

namespace DungeonSurvivors.Rules.Tests
{
    public class DistinctDrawTests
    {
        private static readonly int[] Pool = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(42)]
        public void DrawFromALargerPool_GivesDistinctItemsFromThePool(int seed)
        {
            var drawn = DistinctDraw.Pick(Pool, 3, new SeededRandom(seed));

            Assert.That(drawn, Has.Count.EqualTo(3));
            Assert.That(drawn, Is.Unique);
            Assert.That(drawn, Is.SubsetOf(Pool));
        }

        [Test]
        public void PoolSmallerThanTheCount_GivesEveryItemOnce()
        {
            var drawn = DistinctDraw.Pick(new[] { 7, 8 }, 3, new SeededRandom(1));

            Assert.That(drawn, Is.EquivalentTo(new[] { 7, 8 }));
        }

        [Test]
        public void EmptyPool_GivesNothing()
        {
            Assert.That(DistinctDraw.Pick(new int[0], 3, new SeededRandom(1)), Is.Empty);
        }

        [Test]
        public void SameSeed_GivesTheSameDraw()
        {
            var first = DistinctDraw.Pick(Pool, 3, new SeededRandom(5));
            var second = DistinctDraw.Pick(Pool, 3, new SeededRandom(5));

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void ManyDraws_EventuallyPickEveryItem()
        {
            var random = new SeededRandom(9);

            var seen = Enumerable.Range(0, 200).SelectMany(_ => DistinctDraw.Pick(Pool, 3, random)).Distinct();

            Assert.That(seen, Is.EquivalentTo(Pool));
        }

        [Test]
        public void NegativeCount_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DistinctDraw.Pick(Pool, -1, new SeededRandom(1)));
        }
    }
}
