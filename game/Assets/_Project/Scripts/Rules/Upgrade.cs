namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// One of the options a level-up can offer: raising one stat of one weapon, or getting a new weapon. Each can be picked up to its max level.
    /// The names are stable identifiers (play data reports which one was picked), so rename members with care.
    /// </summary>
    public enum Upgrade
    {
        StraightShotDamage,
        StraightShotBulletCount,
        StraightShotFireRate,

        /// <summary>Gets the orbiting knives. Their stat upgrades are offered only after this one is picked.</summary>
        OrbitingKnivesUnlock,
        OrbitingKnivesDamage,
        OrbitingKnivesCount,
        OrbitingKnivesRadius,
        OrbitingKnivesRotationSpeed,
    }
}
