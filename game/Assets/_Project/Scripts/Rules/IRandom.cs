namespace DungeonSurvivors.Rules
{
    /// <summary>Source of randomness for the rules layer, injected so a Run is reproducible from a seed.</summary>
    public interface IRandom
    {
        /// <summary>Returns an integer in [minInclusive, maxExclusive).</summary>
        int Next(int minInclusive, int maxExclusive);
    }
}
