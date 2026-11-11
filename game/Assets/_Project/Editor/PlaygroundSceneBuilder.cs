using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonSurvivors.EditorTools
{
    /// <summary>
    /// Builds the Playground test scene (player, walls, obstacles, following camera) from code,
    /// so the scene is reproducible and reviewable instead of hand-assembled.
    /// Run from the menu, or in batch mode with
    /// <c>-executeMethod DungeonSurvivors.EditorTools.PlaygroundSceneBuilder.Build</c>.
    /// Re-running overwrites the scene and the player prefab, so tune values on the prefab
    /// or change them here; manual edits to the generated scene are lost on the next rebuild.
    /// </summary>
    public static class PlaygroundSceneBuilder
    {
        private const string Root = "Assets/_Project";
        private const string TemplateScenePath = "Assets/Scenes/SampleScene.unity";
        private const string ScenePath = Root + "/Scenes/Playground.unity";
        private const string PrefabPath = Root + "/Prefabs/Player.prefab";
        private const string SpritePath = Root + "/Art/Placeholder/WhiteSquare.png";
        private const int SpriteSize = 16;

        private static readonly Vector2 ArenaSize = new Vector2(24f, 16f);
        private const float WallThickness = 1f;

        [MenuItem("Dungeon Survivors/Rebuild Playground Scene")]
        public static void Build()
        {
            var sprite = CreatePlaceholderSprite();
            var playerPrefab = CreatePlayerPrefab(sprite);

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
