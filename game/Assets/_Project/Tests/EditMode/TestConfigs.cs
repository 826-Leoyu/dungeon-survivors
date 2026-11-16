namespace DungeonSurvivors.Rules.Tests
{
    /// <summary>Valid configs for tests; each test overrides only the numbers it is about.</summary>
    internal static class TestConfigs
    {
        public static PlayerConfig Player(int maxHealth = 100, float hurtCooldown = 0.5f) =>
            new PlayerConfig(maxHealth, hurtCooldown, moveSpeed: 5f);

        public static EnemyConfig Chaser() => new EnemyConfig(maxHealth: 10, moveSpeed: 2f, contactDamage: 10);

        public static WeaponConfig StraightShot() => new WeaponConfig(damage: 5, fireInterval: 0.5f, bulletSpeed: 12f, bulletLifetime: 2f);

        public static RunConfig Run(int maxHealth = 100, float hurtCooldown = 0.5f) =>
            new RunConfig(Player(maxHealth, hurtCooldown), Chaser(), StraightShot());
    }
}
