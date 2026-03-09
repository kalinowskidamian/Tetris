using UnityEngine;

namespace Tetris.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Tetris/Config/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Startup")]
        [SerializeField] private string initialSceneName = "MainMenu";
        [SerializeField] private bool useLoadingScene = false;
        [SerializeField] private string loadingSceneName = string.Empty;

        [Header("Mobile")]
        [SerializeField] private bool lockToPortrait = true;
        [SerializeField] private int targetFrameRate = 60;

        public string InitialSceneName => initialSceneName;
        public bool UseLoadingScene => useLoadingScene;
        public string LoadingSceneName => loadingSceneName;
        public bool LockToPortrait => lockToPortrait;
        public int TargetFrameRate => targetFrameRate;
    }
}
