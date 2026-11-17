namespace DungeonSurvivors.Rules.Tests
{
    /// <summary>Valid configs for tests; each test overrides only the numbers it is about.</summary>
    internal static class TestConfigs
    {
        public static PlayerConfig Player(int maxHealth = 100, float hurtCooldown = 0.5f) =>
            new PlayerConfig(maxHealth, hurtCooldown, moveSpeed: 5f, pickupRadius: 1.5f);

        public static EnemyConfig Chaser() => new EnemyConfig(maxHealth: 10, moveSpeed: 2f, contactDamage: 10, xpDrop: 1);

        public static WeaponUpgradeConfig Upgrades(
            int damagePerLevel = 2, int maxDamageLevel = 5, int maxBulletCountLevel = 3, float fireIntervalMultiplier = 0.5f, int maxFireRateLevel = 5) =>
            new WeaponUpgradeConfig(damagePerLevel, maxDamageLevel, maxBulletCountLevel, fireIntervalMultiplier, maxFireRateLevel);

        public static WeaponConfig StraightShot(WeaponUpgradeConfig upgrades = null) =>
            new WeaponConfig(damage: 5, fireInterval: 0.5f, bulletSpeed: 12f, bulletLifetime: 2f, spreadDegrees: 15f, upgrades ?? Upgrades());

        public static ProgressionConfig Progression(int firstLevelXp = 5, int xpGrowth = 5) => new ProgressionConfig(firstLevelXp, xpGrowth);

        public static RunConfig Run(
            int maxHealth = 100, float hurtCooldown = 0.5f, int firstLevelXp = 5, int xpGrowth = 5, WeaponUpgradeConfig upgrades = null) =>
            new RunConfig(Player(maxHealth, hurtCooldown), Chaser(), StraightShot(upgrades), Progression(firstLevelXp, xpGrowth));
    }
}
