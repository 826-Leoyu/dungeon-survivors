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

        public static OrbitingKnivesUpgradeConfig KnifeUpgrades(
            int damagePerLevel = 3, int maxDamageLevel = 5, int maxKnifeCountLevel = 4, float radiusPerLevel = 0.5f, int maxRadiusLevel = 3,
            float rotationSpeedPerLevel = 60f, int maxRotationSpeedLevel = 3) =>
            new OrbitingKnivesUpgradeConfig(
                damagePerLevel, maxDamageLevel, maxKnifeCountLevel, radiusPerLevel, maxRadiusLevel, rotationSpeedPerLevel, maxRotationSpeedLevel);

        public static OrbitingKnivesConfig OrbitingKnives(
            int damage = 4, int knifeCount = 2, float radius = 1.5f, float rotationSpeed = 180f, float hitInterval = 0.5f,
            OrbitingKnivesUpgradeConfig upgrades = null) =>
            new OrbitingKnivesConfig(damage, knifeCount, radius, rotationSpeed, hitInterval, upgrades ?? KnifeUpgrades());

        public static ProgressionConfig Progression(int firstLevelXp = 5, int xpGrowth = 5) => new ProgressionConfig(firstLevelXp, xpGrowth);

        public static RunConfig Run(
            int maxHealth = 100, float hurtCooldown = 0.5f, int firstLevelXp = 5, int xpGrowth = 5, WeaponUpgradeConfig upgrades = null,
            OrbitingKnivesUpgradeConfig knifeUpgrades = null) =>
            new RunConfig(
                Player(maxHealth, hurtCooldown), Chaser(), StraightShot(upgrades), OrbitingKnives(upgrades: knifeUpgrades), Progression(firstLevelXp, xpGrowth));
    }
}
