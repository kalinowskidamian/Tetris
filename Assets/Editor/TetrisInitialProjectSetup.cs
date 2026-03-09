#if UNITY_EDITOR
using System.IO;
using Tetris.Bootstrap;
using Tetris.Core;
using Tetris.Gameplay;
using Tetris.Input;
using Tetris.UI;
using Tetris.VFX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tetris.Editor
{
    public static class TetrisInitialProjectSetup
    {
        private const string RootAssetsPath = "Assets";
        private static readonly string[] RootFolders =
        {
            "Art", "Audio", "Materials", "Prefabs", "Resources", "Scenes", "Scripts", "Settings", "UI", "VFX", "Editor"
        };

        private static readonly string[] ScriptFolders =
        {
            "Core", "Bootstrap", "Gameplay", "Input", "UI", "Audio", "VFX", "Data", "Utils"
        };

        [MenuItem("Tools/Tetris/Apply Initial Project Setup")]
        public static void ApplyInitialProjectSetup()
        {
            CreateFolders();
            CreateOrUpdateConfigAssets();
            CreateOrUpdateScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Tetris initial project setup completed.");
        }

        private static void CreateFolders()
        {
            foreach (string folder in RootFolders)
            {
                EnsureFolder($"{RootAssetsPath}/{folder}");
            }

            EnsureFolder("Assets/Resources/Configs");
            foreach (string folder in ScriptFolders)
            {
                EnsureFolder($"Assets/Scripts/{folder}");
            }
        }

        private static void CreateOrUpdateConfigAssets()
        {
            GameConfig gameConfig = EnsureScriptableObject<GameConfig>("Assets/Resources/Configs/GameConfig.asset");
            VisualThemeConfig visualConfig = EnsureScriptableObject<VisualThemeConfig>("Assets/Resources/Configs/VisualThemeConfig.asset");
            UIThemeConfig uiConfig = EnsureScriptableObject<UIThemeConfig>("Assets/Resources/Configs/UIThemeConfig.asset");
            VFXFeedbackConfig vfxConfig = EnsureScriptableObject<VFXFeedbackConfig>("Assets/Resources/Configs/VFXFeedbackConfig.asset");
            ProjectConfigRegistry registry = EnsureScriptableObject<ProjectConfigRegistry>("Assets/Resources/Configs/ProjectConfigRegistry.asset");

            SerializedObject registrySerialized = new(registry);
            registrySerialized.FindProperty("gameConfig").objectReferenceValue = gameConfig;
            registrySerialized.FindProperty("visualThemeConfig").objectReferenceValue = visualConfig;
            registrySerialized.FindProperty("uiThemeConfig").objectReferenceValue = uiConfig;
            registrySerialized.FindProperty("vfxFeedbackConfig").objectReferenceValue = vfxConfig;
            registrySerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject gameConfigSerialized = new(gameConfig);
            gameConfigSerialized.FindProperty("initialSceneName").stringValue = "MainMenu";
            gameConfigSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject vfxSerialized = new(vfxConfig);
            vfxSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(registry);
            EditorUtility.SetDirty(gameConfig);
            EditorUtility.SetDirty(vfxConfig);
        }

        private static void CreateOrUpdateScenes()
        {
            CreateBootstrapScene("Assets/Scenes/Bootstrap.unity");
            CreateMainMenuScene("Assets/Scenes/MainMenu.unity");
            CreateGameplayScene("Assets/Scenes/Gameplay.unity");
        }

        private static void CreateBootstrapScene(string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Bootstrap";

            GameObject bootstrapRoot = new("AppBootstrap");
            bootstrapRoot.AddComponent<AppBootstrap>();

            SaveScene(scenePath, scene);
        }

        private static void CreateMainMenuScene(string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";

            CreateEnvironmentRoot();
            CreateUIStructure("MainMenu");

            GameObject screenRoot = new("MainMenuScreenRoot", typeof(RectTransform), typeof(MainMenuScreenRoot));
            RectTransform rectTransform = screenRoot.GetComponent<RectTransform>();
            rectTransform.SetParent(GameObject.Find("SafeAreaRoot").transform, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            SaveScene(scenePath, scene);
        }

        private static void CreateGameplayScene(string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Gameplay";

            CreateEnvironmentRoot();
            CreateGameplaySceneStructure();

            SaveScene(scenePath, scene);
        }

        private static void CreateEnvironmentRoot()
        {
            GameObject environmentRoot = new("EnvironmentRoot");
            GameObject cameraObject = new("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(environmentRoot.transform, false);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Camera cameraComponent = cameraObject.GetComponent<Camera>();
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.03f, 0.04f, 0.09f);
        }

        private static void CreateUIStructure(string sceneLabel)
        {
            GameObject root = new($"{sceneLabel}Root");

            GameObject canvasObject = new("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasScalerBinder));
            canvasObject.transform.SetParent(root.transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            GameObject safeAreaRoot = new("SafeAreaRoot", typeof(RectTransform), typeof(SafeAreaFitter));
            RectTransform safeAreaRect = safeAreaRoot.GetComponent<RectTransform>();
            safeAreaRect.SetParent(canvasObject.transform, false);
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;

            GameObject eventSystem = new("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            eventSystem.transform.SetParent(root.transform, false);
        }

        private static void CreateGameplaySceneStructure()
        {
            CreateUIStructure("Gameplay");

            Transform safeAreaRoot = GameObject.Find("SafeAreaRoot").transform;
            GameObject gameplayScreenRoot = new("GameplayScreenRoot", typeof(RectTransform), typeof(GameplayScreenRoot), typeof(GameplayLayoutRoot));
            RectTransform gameplayRect = gameplayScreenRoot.GetComponent<RectTransform>();
            gameplayRect.SetParent(safeAreaRoot, false);
            gameplayRect.anchorMin = Vector2.zero;
            gameplayRect.anchorMax = Vector2.one;
            gameplayRect.offsetMin = Vector2.zero;
            gameplayRect.offsetMax = Vector2.zero;

            GameObject backgroundRoot = new("BackgroundRoot", typeof(RectTransform));
            ConfigureChildLayout(backgroundRoot.GetComponent<RectTransform>(), gameplayRect, new Vector2(0f, 0f), new Vector2(1f, 1f));

            GameObject boardRoot = new("BoardRoot", typeof(RectTransform), typeof(BoardLayoutAnchor));
            ConfigureChildLayout(boardRoot.GetComponent<RectTransform>(), gameplayRect, new Vector2(0.1f, 0.16f), new Vector2(0.9f, 0.78f));

            GameObject hudRoot = new("HUDRoot", typeof(RectTransform), typeof(HUDLayoutAnchor));
            ConfigureChildLayout(hudRoot.GetComponent<RectTransform>(), gameplayRect, new Vector2(0f, 0.78f), new Vector2(1f, 1f));

            GameObject controlsRoot = new("ControlsRoot", typeof(RectTransform), typeof(ControlsLayoutAnchor));
            ConfigureChildLayout(controlsRoot.GetComponent<RectTransform>(), gameplayRect, new Vector2(0f, 0f), new Vector2(1f, 0.16f));

            GameObject feedbackRoot = new("FeedbackRoot", typeof(RectTransform), typeof(FeedbackLayoutAnchor), typeof(ScreenFeedbackController));
            ConfigureChildLayout(feedbackRoot.GetComponent<RectTransform>(), gameplayRect, new Vector2(0f, 0f), new Vector2(1f, 1f));

            GameObject inputRoot = new("GameplayInputRouter", typeof(GameplayInputRouter));
            GameObject gameplayRoot = GameObject.Find("GameplayRoot");
            inputRoot.transform.SetParent(gameplayRoot.transform, false);

            GameplayRootController gameplayController = gameplayRoot.AddComponent<GameplayRootController>();
            SerializedObject gameplayControllerSerialized = new(gameplayController);
            gameplayControllerSerialized.FindProperty("boardAnchor").objectReferenceValue = boardRoot.GetComponent<BoardLayoutAnchor>();
            gameplayControllerSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureChildLayout(RectTransform child, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            child.SetParent(parent, false);
            child.anchorMin = anchorMin;
            child.anchorMax = anchorMax;
            child.offsetMin = Vector2.zero;
            child.offsetMax = Vector2.zero;
        }

        private static void SaveScene(string scenePath, Scene scene)
        {
            EnsureFolder(Path.GetDirectoryName(scenePath)?.Replace("\\", "/") ?? "Assets/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static T EnsureScriptableObject<T>(string assetPath) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            T created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, assetPath);
            return created;
        }

        private static void EnsureFolder(string fullPath)
        {
            string[] parts = fullPath.Split('/');
            string currentPath = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string nextPath = $"{currentPath}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }

                currentPath = nextPath;
            }
        }
    }
}
#endif
