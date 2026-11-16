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
    /// Builds the Playground test scene (player, walls, obstacles, following adaptive camera, Run, enemy spawning with XP gem drops,
    /// starting weapon, touch joystick, level-up choice panel) from code,
    /// so the scene is reproducible and reviewable instead of hand-assembled.
    /// Run from the menu, or in batch mode with
    /// <c>-executeMethod DungeonSurvivors.EditorTools.PlaygroundSceneBuilder.Build</c>.
    /// Re-running overwrites the scene and the player, enemy, bullet and XP gem prefabs; manual edits to them are lost on the next rebuild.
    /// Tuning numbers live in the definition assets under <c>Config/</c>, which are created only when missing, so a rebuild keeps them.
    /// </summary>
    public static class PlaygroundSceneBuilder
    {
        private const string Root = "Assets/_Project";
        private const string TemplateScenePath = "Assets/Scenes/SampleScene.unity";
        private const string ScenePath = Root + "/Scenes/Playground.unity";
        private const string PrefabPath = Root + "/Prefabs/Player.prefab";
        private const string EnemyPrefabPath = Root + "/Prefabs/ChaserEnemy.prefab";
        private const string BulletPrefabPath = Root + "/Prefabs/Bullet.prefab";
        private const string GemPrefabPath = Root + "/Prefabs/XpGem.prefab";
        private const string SpritePath = Root + "/Art/Placeholder/WhiteSquare.png";
        private const string ConfigFolder = Root + "/Config";
        private const string RunDefinitionPath = ConfigFolder + "/Run.asset";
        private const string PlayerDefinitionPath = ConfigFolder + "/Player.asset";
        private const string ChaserDefinitionPath = ConfigFolder + "/ChaserEnemy.asset";
        private const string StraightShotDefinitionPath = ConfigFolder + "/StraightShot.asset";
        private const string ProgressionDefinitionPath = ConfigFolder + "/Progression.asset";
        private const int SpriteSize = 16;

        // Much larger than the camera's view (AdaptiveCamera's default visible area, about 18 x 10 turned either way), so enemies spawned just off-screen are still inside the walls.
        private static readonly Vector2 ArenaSize = new Vector2(80f, 80f);
        private const int EnemyPoolSize = 300;
        // Far more than the default fire rate keeps alive at once (lifetime / interval = 4); headroom for faster weapons.
        private const int BulletPoolSize = 100;
        // Gems wait on the floor until picked up, so they pile up faster than enemies; at the cap new drops are skipped.
        private const int GemPoolSize = 500;
        private const float WallThickness = 1f;

        // Canvas scaler reference: a landscape 1080p screen, matching width and height equally so portrait phones get a similar size.
        private static readonly Vector2 UiReferenceResolution = new Vector2(1920f, 1080f);
        // Written into the joystick and used as the base circle's radius, so the circle shows the full-speed distance.
        private const float JoystickRadius = 80f;
        private const float JoystickKnobSize = 70f;
        // Below every other canvas (default 0): overlay canvases with a higher sort order are hit first.
        private const int JoystickCanvasSortOrder = -100;
        // Above the game UI, so the level-up choice covers it and takes every press while open.
        private const int LevelUpCanvasSortOrder = 100;
        private const int LevelUpOptionCount = Rules.Run.ChoicesPerLevelUp;
        // Stacked vertically so the options fit a portrait phone as well as a landscape screen.
        private static readonly Vector2 LevelUpOptionSize = new Vector2(720f, 180f);
        private const float LevelUpOptionSpacing = 30f;

        [MenuItem("Dungeon Survivors/Rebuild Playground Scene")]
        public static void Build()
        {
            var sprite = CreatePlaceholderSprite();
            var playerPrefab = CreatePlayerPrefab(sprite);
            var enemyPrefab = CreateEnemyPrefab(sprite);
            var bulletPrefab = CreateBulletPrefab(sprite);
            var gemPrefab = CreateGemPrefab(sprite);

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
            // Loaded only now: opening the scene in Single mode unloads assets loaded before it, which would wire a destroyed reference.
            var runHost = BuildRunHost(LoadOrCreateRunDefinition());
            Wire(player.GetComponent<PlayerMovement>(), "runHost", runHost);
            BuildEnemySpawning(runHost, enemyPrefab, gemPrefab, player.transform, camera);
            BuildStartingWeapon(runHost, bulletPrefab, player.transform);
            BuildTouchJoystick();
            BuildLevelUpPanel(runHost);

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

        private static GameObject CreateGemPrefab(Sprite sprite)
        {
            // No collider: the gem checks its distance to the player itself. A small diamond drawn under the enemies.
            var go = new GameObject("XpGem");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.3f, 1f, 0.5f);
            renderer.sortingOrder = 2;
            go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, 45f);

            go.AddComponent<XpGem>();
            return SaveAsPrefab(go, GemPrefabPath);
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

        /// <summary>
        /// Loads the Run definition, or creates it, and fills any of its empty slots (player, enemy, weapon, progression) with the
        /// definition at its usual path, created at its code defaults when missing. Assigned slots and existing assets are left untouched,
        /// so a rebuild keeps their tuning and a Run definition from before a slot existed gets it filled.
        /// </summary>
        private static RunDefinition LoadOrCreateRunDefinition()
        {
            var runDefinition = AssetDatabase.LoadAssetAtPath<RunDefinition>(RunDefinitionPath);
            if (runDefinition == null) runDefinition = CreateAsset<RunDefinition>(RunDefinitionPath);

            var definition = new SerializedObject(runDefinition);
            FillIfEmpty<PlayerDefinition>(definition, "player", PlayerDefinitionPath);
            FillIfEmpty<EnemyDefinition>(definition, "chaser", ChaserDefinitionPath);
            FillIfEmpty<WeaponDefinition>(definition, "startingWeapon", StraightShotDefinitionPath);
            FillIfEmpty<ProgressionDefinition>(definition, "progression", ProgressionDefinitionPath);
            if (definition.ApplyModifiedPropertiesWithoutUndo()) EditorUtility.SetDirty(runDefinition);
            return runDefinition;
        }

        private static void FillIfEmpty<T>(SerializedObject definition, string field, string path) where T : ScriptableObject
        {
            var slot = definition.FindProperty(field);
            if (slot.objectReferenceValue == null) slot.objectReferenceValue = LoadOrCreate<T>(path);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            return existing != null ? existing : CreateAsset<T>(path);
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            EnsureFolder(Path.GetDirectoryName(path));
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static RunHost BuildRunHost(RunDefinition definition)
        {
            var runHost = new GameObject("RunHost").AddComponent<RunHost>();
            Wire(runHost, "definition", definition);
            return runHost;
        }

        /// <summary>Sets one serialized object reference, as dragging it into the Inspector would.</summary>
        private static void Wire(Object component, string field, Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildEnemySpawning(RunHost runHost, GameObject enemyPrefab, GameObject gemPrefab, Transform player, Camera camera)
        {
            var pool = CreatePool("EnemyPool", enemyPrefab, EnemyPoolSize);
            var gemPool = CreatePool("XpGemPool", gemPrefab, GemPoolSize);

            var spawner = new SerializedObject(new GameObject("EnemySpawner").AddComponent<EnemySpawner>());
            spawner.FindProperty("runHost").objectReferenceValue = runHost;
            spawner.FindProperty("pool").objectReferenceValue = pool;
            spawner.FindProperty("gemPool").objectReferenceValue = gemPool;
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

            // Below every other canvas, so any button on a normal canvas takes the press and the joystick never appears under it.
            var area = CreateJoystickArea(CreateOverlayCanvas("JoystickCanvas", JoystickCanvasSortOrder));

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

        /// <summary>A screen-space overlay canvas scaled from the reference resolution; higher <paramref name="sortOrder"/> draws on top and is hit first.</summary>
        private static Transform CreateOverlayCanvas(string name, int sortOrder)
        {
            var canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

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
            Stretch(area.rectTransform);
            return area.gameObject;
        }

        /// <summary>
        /// The level-up choice: a dimmed full-screen panel, hidden until a level-up, with a title and one button per option stacked in the middle.
        /// The dim backdrop catches presses, so taps on the panel never reach the joystick.
        /// </summary>
        private static void BuildLevelUpPanel(RunHost runHost)
        {
            var root = new GameObject("LevelUpPanel", typeof(RectTransform));
            root.transform.SetParent(CreateOverlayCanvas("LevelUpCanvas", LevelUpCanvasSortOrder), false);
            Stretch((RectTransform)root.transform);

            var backdrop = NewUiImage("Panel", root.transform, null, new Color(0f, 0f, 0f, 0.6f));
            backdrop.raycastTarget = true;
            Stretch(backdrop.rectTransform);

            var options = new GameObject("Options", typeof(RectTransform), typeof(VerticalLayoutGroup));
            options.transform.SetParent(backdrop.transform, false);
            var layout = options.GetComponent<VerticalLayoutGroup>();
            layout.spacing = LevelUpOptionSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            Stretch((RectTransform)options.transform);

            var title = NewUiText("Title", options.transform, "Level Up!", 64, Color.white);
            title.rectTransform.sizeDelta = new Vector2(LevelUpOptionSize.x, 100f);

            var buttons = new Button[LevelUpOptionCount];
            var labels = new Text[LevelUpOptionCount];
            for (var i = 0; i < LevelUpOptionCount; i++)
            {
                buttons[i] = NewOptionButton($"Option{i + 1}", options.transform, out labels[i]);
            }

            var panel = new SerializedObject(root.AddComponent<LevelUpPanel>());
            panel.FindProperty("runHost").objectReferenceValue = runHost;
            panel.FindProperty("panel").objectReferenceValue = backdrop.gameObject;
            WireArray(panel.FindProperty("optionButtons"), buttons);
            WireArray(panel.FindProperty("optionLabels"), labels);
            panel.ApplyModifiedPropertiesWithoutUndo();

            // Hidden in the saved scene too, so it never flashes on the first frame.
            backdrop.gameObject.SetActive(false);
        }

        private static Button NewOptionButton(string name, Transform parent, out Text label)
        {
            var background = NewUiImage(name, parent, AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"), Color.white);
            background.type = Image.Type.Sliced;
            background.raycastTarget = true;
            background.rectTransform.sizeDelta = LevelUpOptionSize;
            var button = background.gameObject.AddComponent<Button>();

            label = NewUiText("Label", background.transform, name, 40, new Color(0.15f, 0.15f, 0.2f));
            Stretch(label.rectTransform);
            return button;
        }

        /// <summary>A centred UI Text that does not catch presses.</summary>
        private static Text NewUiText(string name, Transform parent, string content, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>Makes <paramref name="rect"/> fill its parent.</summary>
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void WireArray(SerializedProperty array, Object[] values)
        {
            array.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
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
