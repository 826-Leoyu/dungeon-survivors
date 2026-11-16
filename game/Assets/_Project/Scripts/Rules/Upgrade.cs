namespace DungeonSurvivors.Rules
{
    /// <summary>
    /// One of the options a level-up can offer: raising one stat of one weapon. Each can be picked up to its max level.
    /// The names are stable identifiers (play data reports which one was picked), so rename members with care.
    /// </summary>
    public enum Upgrade
    {
        StraightShotDamage,
        StraightShotBulletCount,
        StraightShotFireRate,
    }
}
