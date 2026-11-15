using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DungeonSurvivors.EditorTools
{
    /// <summary>
    /// Builds the Playground test scene (player, walls, obstacles, following adaptive camera, Run, enemy spawning, starting weapon,
    /// touch joystick) from code,
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

        // Much larger than the camera's view (AdaptiveCamera's default visible area, about 18 x 10 turned either way), so enemies spawned just off-screen are still inside the walls.
        private static readonly Vector2 ArenaSize = new Vector2(80f, 80f);
        private const int EnemyPoolSize = 300;
        // Far more than the default fire rate keeps alive at once (lifetime / interval = 4); headroom for faster weapons.
        private const int BulletPoolSize = 100;
        private const float WallThickness = 1f;

        // Canvas scaler reference: a landscape 1080p screen, matching width and height equally so portrait phones get a similar size.
        private static readonly Vector2 UiReferenceResolution = new Vector2(1920f, 1080f);
        // Written into the joystick and used as the base circle's radius, so the circle shows the full-speed distance.
        private const float JoystickRadius = 80f;
        private const float JoystickKnobSize = 70f;
        // Below every other canvas (default 0): overlay canvases with a higher sort order are hit first.
        private const int JoystickCanvasSortOrder = -100;

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
            camera.gameObject.AddComponent<AdaptiveCamera>();

            BuildArena(sprite);
            var runHost = new GameObject("RunHost").AddComponent<RunHost>();
            BuildEnemySpawning(runHost, enemyPrefab, player.transform, camera);
            BuildStartingWeapon(runHost, bulletPrefab, player.transform);
            BuildTouchJoystick();

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
            // A solid circle slides along walls and around corners instead of catching on them like a box.
            var go = NewCircleSprite("Player", sprite, new Color(0.35f, 0.75f, 1f), sortingOrder: 10, size: 0.8f, isTrigger: false);
            go.tag = "Player";

            go.AddComponent<PlayerMovement>(); // RequireComponent adds the Rigidbody2D
            PlayerMovement.ConfigureBody(go.GetComponent<Rigidbody2D>());
            return SaveAsPrefab(go, PrefabPath);
        }

        private static GameObject CreateEnemyPrefab(Sprite sprite)
        {
            // A trigger reports touching the player without pushing it or blocking other enemies. Drawn under the player.
            var go = NewCircleSprite("ChaserEnemy", sprite, new Color(0.9f, 0.3f, 0.3f), sortingOrder: 5, size: 0.8f, isTrigger: true);

            go.AddComponent<ChaserEnemy>(); // RequireComponent adds the Rigidbody2D
            ChaserEnemy.ConfigureBody(go.GetComponent<Rigidbody2D>());
            return SaveAsPrefab(go, EnemyPrefabPath);
        }

        private static GameObject CreateBulletPrefab(Sprite sprite)
        {
            // A trigger, like the enemies, so it reports hits without pushing anything. Drawn over the player and enemies.
            var go = NewCircleSprite("Bullet", sprite, new Color(1f, 0.9f, 0.3f), sortingOrder: 15, size: 0.3f, isTrigger: true);

            go.AddComponent<Bullet>(); // RequireComponent adds the Rigidbody2D
            Bullet.ConfigureBody(go.GetComponent<Rigidbody2D>());
            return SaveAsPrefab(go, BulletPrefabPath);
        }

        /// <summary>A square placeholder sprite of <paramref name="size"/> world units with a circle collider filling it.</summary>
        private static GameObject NewCircleSprite(string name, Sprite sprite, Color color, int sortingOrder, float size, bool isTrigger)
        {
            var go = new GameObject(name);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            go.transform.localScale = new Vector3(size, size, 1f);

            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = isTrigger;
            return go;
        }

        /// <summary>Saves the temporary scene object <paramref name="go"/> as a prefab asset, then destroys the object.</summary>
        private static GameObject SaveAsPrefab(GameObject go, string path)
        {
            EnsureFolder(Path.GetDirectoryName(path));
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
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

        private static void BuildTouchJoystick()
        {
            // Routes pointer and touch input to UI; with no actions assigned, the module uses the Input System's default UI actions.
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var area = CreateJoystickArea(CreateJoystickCanvas());

            var circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            var stickBase = NewUiImage("Base", area.transform, circle, new Color(1f, 1f, 1f, 0.25f)).rectTransform;
            stickBase.sizeDelta = Vector2.one * (2f * JoystickRadius);
            var knob = NewUiImage("Knob", stickBase, circle, new Color(1f, 1f, 1f, 0.6f)).rectTransform;
            knob.sizeDelta = Vector2.one * JoystickKnobSize;

            var joystick = new SerializedObject(area.AddComponent<FloatingJoystick>());
            joystick.FindProperty("radius").floatValue = JoystickRadius;
            joystick.FindProperty("stickBase").objectReferenceValue = stickBase;
            joystick.FindProperty("knob").objectReferenceValue = knob;
            joystick.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// The joystick gets its own canvas, sorted below every other one, so any button added later on a normal canvas
        /// takes the press and the joystick never appears under it.
        /// </summary>
        private static Transform CreateJoystickCanvas()
        {
            var canvasObject = new GameObject("JoystickCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = JoystickCanvasSortOrder;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = UiReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f; // weigh width and height equally
            return canvasObject.transform;
        }

        /// <summary>The full-screen, fully transparent element that catches presses; a transparent Image still receives raycasts.</summary>
        private static GameObject CreateJoystickArea(Transform canvas)
        {
            var area = NewUiImage("TouchJoystick", canvas, null, Color.clear);
            area.raycastTarget = true;
            var rect = area.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return area.gameObject;
        }

        /// <summary>A UI Image that does not catch presses; the caller turns raycasts back on where it wants them.</summary>
        private static Image NewUiImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
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
