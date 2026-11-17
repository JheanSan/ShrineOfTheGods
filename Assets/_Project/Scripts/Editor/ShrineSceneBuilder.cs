using System.IO;
using Shrine.Core;
using Shrine.Data;
using Shrine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shrine.Editor
{
    /// <summary>Generates the single game scene and all design assets. Run once via menu or CLI.</summary>
    public static class ShrineSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Shrine.unity";
        private const string DataRoot = "Assets/_Project/Resources/Shrine";

        [MenuItem("Shrine/Build Shrine Scene")]
        public static void Build()
        {
            CreateDataAssets();
            ConfigureSpriteBorders();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("ShrineCamera", typeof(Camera), typeof(AudioListener));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = ShrinePalette.MidnightInk;
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            new GameObject("ShrineApp", typeof(ShrineApp));

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            PlayerSettings.productName = "Shrine of the Gods";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Shrine scene and design assets built successfully.");
        }

        private static void CreateDataAssets()
        {
            EnsureFolder("Assets/_Project/Resources");
            EnsureFolder(DataRoot);
            EnsureFolder(DataRoot + "/Cards");
            EnsureFolder(DataRoot + "/Patrons");
            EnsureFolder(DataRoot + "/Omens");

            BalanceConfig balance = LoadOrCreate<BalanceConfig>(DataRoot + "/BalanceConfig.asset");
            balance.ResetDefaults();
            EditorUtility.SetDirty(balance);

            foreach (CardData data in ContentFactory.Cards)
            {
                CardDefinition definition = LoadOrCreate<CardDefinition>(DataRoot + "/Cards/" + data.Id + ".asset");
                definition.Set(data);
                EditorUtility.SetDirty(definition);
            }
            foreach (PatronData data in ContentFactory.Patrons)
            {
                PatronDefinition definition = LoadOrCreate<PatronDefinition>(DataRoot + "/Patrons/" + data.Kind + ".asset");
                definition.Set(data);
                EditorUtility.SetDirty(definition);
            }
            foreach (OmenData data in ContentFactory.Omens)
            {
                OmenDefinition definition = LoadOrCreate<OmenDefinition>(DataRoot + "/Omens/" + data.Id + ".asset");
                definition.Set(data);
                EditorUtility.SetDirty(definition);
            }
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void ConfigureSpriteBorders()
        {
            SetBorder("Assets/Resources/FotosMainMenu/MainMenuBoton.png", new Vector4(120, 22, 120, 22));
            SetBorder("Assets/Resources/FotosManageDeckScene/Fondo de cartas seleccion.jpeg",
                new Vector4(60, 50, 60, 50));
        }

        private static void SetBorder(string path, Vector4 border)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            if (importer.spriteBorder == border) return;
            importer.spriteBorder = border;
            importer.spritePixelsPerUnit = 100f;
            importer.SaveAndReimport();
        }
    }
}
