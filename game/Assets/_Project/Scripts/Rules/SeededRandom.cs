namespace DungeonSurvivors.Rules
{
    /// <summary><see cref="IRandom"/> backed by System.Random: the same seed always yields the same sequence.</summary>
    public sealed class SeededRandom : IRandom
    {
        private const float OneMinusEpsilon = 0.99999994f; // largest float below 1

        private readonly System.Random random;

        public SeededRandom(int seed)
        {
            random = new System.Random(seed);
        }

        public int Next(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);

        // Clamped because rounding a double just below 1 to float can give exactly 1.
        public float NextFloat() => System.Math.Min((float)random.NextDouble(), OneMinusEpsilon);
    }
}
