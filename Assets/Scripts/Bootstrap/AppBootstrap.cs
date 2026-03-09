using Tetris.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tetris.Bootstrap
{
    public sealed class AppBootstrap : MonoBehaviour
    {
        private const string RegistryResourcePath = "Configs/ProjectConfigRegistry";

        [SerializeField] private ProjectConfigRegistry configRegistry;

        private async void Start()
        {
            if (configRegistry == null)
            {
                configRegistry = Resources.Load<ProjectConfigRegistry>(RegistryResourcePath);
            }

            if (configRegistry == null || configRegistry.GameConfig == null)
            {
                Debug.LogError($"{nameof(AppBootstrap)} could not find {nameof(ProjectConfigRegistry)} in Resources/{RegistryResourcePath}.");
                return;
            }

            ApplyRuntimeSettings(configRegistry.GameConfig);
            await LoadInitialSceneAsync(configRegistry.GameConfig);
        }

        private static void ApplyRuntimeSettings(GameConfig gameConfig)
        {
            Application.targetFrameRate = gameConfig.TargetFrameRate;
            Screen.orientation = gameConfig.LockToPortrait ? ScreenOrientation.Portrait : ScreenOrientation.AutoRotation;
        }

        private static async Awaitable LoadInitialSceneAsync(GameConfig gameConfig)
        {
            string sceneToLoad = gameConfig.InitialSceneName;
            if (gameConfig.UseLoadingScene && !string.IsNullOrWhiteSpace(gameConfig.LoadingSceneName))
            {
                sceneToLoad = gameConfig.LoadingSceneName;
            }

            if (string.IsNullOrWhiteSpace(sceneToLoad))
            {
                Debug.LogError($"{nameof(GameConfig)} does not define a valid initial scene name.");
                return;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
            if (loadOperation == null)
            {
                Debug.LogError($"Failed to load scene '{sceneToLoad}'. Ensure the scene is in Build Settings.");
                return;
            }

            while (!loadOperation.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }
}
