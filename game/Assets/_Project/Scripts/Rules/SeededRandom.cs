namespace DungeonSurvivors.Rules
{
    /// <summary><see cref="IRandom"/> backed by System.Random: the same seed always yields the same sequence.</summary>
    public sealed class SeededRandom : IRandom
    {
        private readonly System.Random random;

        public SeededRandom(int seed)
        {
            random = new System.Random(seed);
        }

        public int Next(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);
    }
}
