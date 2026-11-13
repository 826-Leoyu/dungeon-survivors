using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonSurvivors.EditorTools
{
    /// <summary>
    /// Builds the Playground test scene (player, walls, obstacles, following camera, Run, enemy spawning, starting weapon) from code,
    /// so the scene is reproducible and reviewable instead of hand-assembled.
    /// Run from the menu, or in batch mode with
    /// <c>-executeMethod DungeonSurvivors.EditorTools.PlaygroundSceneBuilder.Build</c>.
    /// Re-running overwrites the scene and the player, enemy and bullet prefabs, so tune values on the prefabs
    /// or change them here; manual edits to the generated scene are lost on the next rebuild.
    /// </summary>
    public static class PlaygroundSceneBuilder
    {
        private const string Root = "Assets/_Project";
        private const string TemplateScenePath = "Assets/Scenes/SampleScene.unity";
        private const string ScenePath = Root + "/Scenes/Playground.unity";
        private const string PrefabPath = Root + "/Prefabs/Player.prefab";
        private const string EnemyPrefabPath = Root + "/Prefabs/ChaserEnemy.prefab";
        private const string BulletPrefabPath = Root + "/Prefabs/Bullet.prefab";
        private const string SpritePath = Root + "/Art/Placeholder/WhiteSquare.png";
        private const int SpriteSize = 16;

        // Much larger than the camera's view (about 18 x 10), so enemies spawned just off-screen are still inside the walls.
        private static readonly Vector2 ArenaSize = new Vector2(80f, 80f);
        private const int EnemyPoolSize = 300;
        // Far more than the default fire rate keeps alive at once (lifetime / interval = 4); headroom for faster weapons.
        private const int BulletPoolSize = 100;
        private const float WallThickness = 1f;

        [MenuItem("Dungeon Survivors/Rebuild Playground Scene")]
        public static void Build()
        {
            var sprite = CreatePlaceholderSprite();
            var playerPrefab = CreatePlayerPrefab(sprite);
            var enemyPrefab = CreateEnemyPrefab(sprite);
            var bulletPrefab = CreateBulletPrefab(sprite);

            // Start from the URP 2D template scene so the camera and Global Light 2D are already set up.
            EnsureFolder(Path.GetDirectoryName(ScenePath));
            AssetDatabase.DeleteAsset(ScenePath);
            AssetDatabase.CopyAsset(TemplateScenePath, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.transform.position = Vector3.zero;

            var camera = Camera.main;
            var follow = new SerializedObject(camera.gameObject.AddComponent<CameraFollow>());
            follow.FindProperty("target").objectReferenceValue = player.transform;
            follow.ApplyModifiedPropertiesWithoutUndo();

            BuildArena(sprite);
            var runHost = new GameObject("RunHost").AddComponent<RunHost>();
            BuildEnemySpawning(runHost, enemyPrefab, player.transform, camera);
            BuildStartingWeapon(runHost, bulletPrefab, player.transform);

            EditorSceneManager.SaveScene(scene);
            SetAsFirstBuildScene(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PlaygroundSceneBuilder] Built {ScenePath}");
        }

        private static Sprite CreatePlaceholderSprite()
        {
            EnsureFolder(Path.GetDirectoryName(SpritePath));
            if (!File.Exists(SpritePath))
            {
                var texture = new Texture2D(SpriteSize, SpriteSize);
                texture.SetPixels(Enumerable.Repeat(Color.white, SpriteSize * SpriteSize).ToArray());
                File.WriteAllBytes(SpritePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(SpritePath);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = SpriteSize; // 1 sprite = 1 world unit, so scale == size in units
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }

        private static GameObject CreatePlayerPrefab(Sprite sprite)
        {
            EnsureFolder(Path.GetDirectoryName(PrefabPath));

            var go = new GameObject("Player") { tag = "Player" };
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.35f, 0.75f, 1f);
            renderer.sortingOrder = 10;
            go.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            // A circle slides along walls and around corners instead of catching on them like a box.
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            go.AddComponent<PlayerMovement>(); // RequireComponent adds the Rigidbody2D
            PlayerMovement.ConfigureBody(go.GetComponent<Rigidbody2D>());

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateEnemyPrefab(Sprite sprite)
        {
            EnsureFolder(Path.GetDirectoryName(EnemyPrefabPath));

            var go = new GameObject("ChaserEnemy");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.9f, 0.3f, 0.3f);
            renderer.sortingOrder = 5; // under the player
            go.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            // A trigger reports touching the player without pushing it or blocking other enemies.
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = true;

            go.AddComponent<ChaserEnemy>(); // RequireComponent adds the Rigidbody2D
            ChaserEnemy.ConfigureBody(go.GetComponent<Rigidbody2D>());

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, EnemyPrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateBulletPrefab(Sprite sprite)
        {
            EnsureFolder(Path.GetDirectoryName(BulletPrefabPath));

            var go = new GameObject("Bullet");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(1f, 0.9f, 0.3f);
            renderer.sortingOrder = 15; // over the player and enemies
            go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = true;

            go.AddComponent<Bullet>(); // RequireComponent adds the Rigidbody2D
            Bullet.ConfigureBody(go.GetComponent<Rigidbody2D>());

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, BulletPrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void BuildEnemySpawning(RunHost runHost, GameObject enemyPrefab, Transform player, Camera camera)
        {
            var pool = CreatePool("EnemyPool", enemyPrefab, EnemyPoolSize);

            var spawner = new SerializedObject(new GameObject("EnemySpawner").AddComponent<EnemySpawner>());
            spawner.FindProperty("runHost").objectReferenceValue = runHost;
            spawner.FindProperty("pool").objectReferenceValue = pool;
            spawner.FindProperty("target").objectReferenceValue = player;
            spawner.FindProperty("viewCamera").objectReferenceValue = camera;
            spawner.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildStartingWeapon(RunHost runHost, GameObject bulletPrefab, Transform player)
        {
            var pool = CreatePool("BulletPool", bulletPrefab, BulletPoolSize);

            var weapon = new SerializedObject(new GameObject("StraightShotWeapon").AddComponent<StraightShotWeapon>());
            weapon.FindProperty("runHost").objectReferenceValue = runHost;
            weapon.FindProperty("bulletPool").objectReferenceValue = pool;
            weapon.FindProperty("shooter").objectReferenceValue = player;
            weapon.ApplyModifiedPropertiesWithoutUndo();
        }

        private static PrefabPool CreatePool(string name, GameObject prefab, int maxSize)
        {
            var pool = new GameObject(name).AddComponent<PrefabPool>();
            var settings = new SerializedObject(pool);
            settings.FindProperty("prefab").objectReferenceValue = prefab;
            settings.FindProperty("maxSize").intValue = maxSize;
            settings.ApplyModifiedPropertiesWithoutUndo();
            return pool;
        }

        private static void BuildArena(Sprite sprite)
        {
            var arena = new GameObject("Arena");
            var wallColor = new Color(0.45f, 0.4f, 0.5f);
            float halfWidth = ArenaSize.x / 2f, halfHeight = ArenaSize.y / 2f, thickness = WallThickness;

            // Outer walls (the side walls overlap the corners so there are no gaps).
            CreateBlock("Wall_Top", arena.transform, sprite, wallColor, new Vector2(0f, halfHeight + thickness / 2f), new Vector2(ArenaSize.x + 2f * thickness, thickness));
            CreateBlock("Wall_Bottom", arena.transform, sprite, wallColor, new Vector2(0f, -halfHeight - thickness / 2f), new Vector2(ArenaSize.x + 2f * thickness, thickness));
            CreateBlock("Wall_Left", arena.transform, sprite, wallColor, new Vector2(-halfWidth - thickness / 2f, 0f), new Vector2(thickness, ArenaSize.y));
            CreateBlock("Wall_Right", arena.transform, sprite, wallColor, new Vector2(halfWidth + thickness / 2f, 0f), new Vector2(thickness, ArenaSize.y));

            // Obstacles to test sliding along edges and around corners.
            var obstacleColor = new Color(0.55f, 0.35f, 0.3f);
            CreateBlock("Obstacle_Pillar", arena.transform, sprite, obstacleColor, new Vector2(-6f, 3f), new Vector2(2f, 2f));
            CreateBlock("Obstacle_LongWall", arena.transform, sprite, obstacleColor, new Vector2(4f, -3f), new Vector2(6f, 1f));
            CreateBlock("Obstacle_Small", arena.transform, sprite, obstacleColor, new Vector2(7f, 4f), new Vector2(1f, 1f));
        }

        private static void CreateBlock(string name, Transform parent, Sprite sprite, Color color, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;

            // No Rigidbody2D: a lone collider is static, the cheapest kind for level geometry.
            go.AddComponent<BoxCollider2D>();
            go.isStatic = true;
        }

        private static void SetAsFirstBuildScene(string path)
        {
            var others = EditorBuildSettings.scenes.Where(s => s.path != path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) }
                .Concat(others.Select(s => new EditorBuildSettingsScene(s.path, false)))
                .ToArray();
        }

        private static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
