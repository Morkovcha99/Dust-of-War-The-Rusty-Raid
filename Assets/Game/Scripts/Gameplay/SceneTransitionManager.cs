using UnityEngine;
using UnityEngine.SceneManagement;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Manages transitions between scenes and ensures data persistence
    /// Saves progress when switching between menu and game scenes
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager instance;
        public static SceneTransitionManager Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Called when a scene is loaded
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Sync PlayerData with SaveSystem when loading menu scene
            if (PlayerData.Instance != null)
            {
                PlayerData.Instance.LoadFromSaveSystem();
            }
            
            // Sync PlayerProgress with SaveSystem
            if (PlayerProgress.Instance != null)
            {
                PlayerProgress.Instance.LoadProgress();
            }
            
            // Reset stats for new game session if entering game scene
            if (GameStatsManager.Instance != null)
            {
                // Check if this is a game scene (you may need to adjust scene name checking)
                if (scene.name.Contains("Game") || scene.name.Contains("Sample"))
                {
                    GameStatsManager.Instance.ResetStats();
                }
            }
        }

        /// <summary>
        /// Save all progress before scene transition
        /// </summary>
        public void SaveBeforeSceneTransition()
        {
            // Save session resources to permanent storage
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveSessionResources();
            }

            // Save player progress
            if (PlayerProgress.Instance != null)
            {
                PlayerProgress.Instance.SaveProgress();
            }
        }

        /// <summary>
        /// Load game scene
        /// </summary>
        public void LoadGameScene(string sceneName = "SampleScene")
        {
            SaveBeforeSceneTransition();
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Load menu/garage scene
        /// </summary>
        public void LoadMenuScene(string sceneName = "MainMenu")
        {
            SaveBeforeSceneTransition();
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Restart current scene
        /// </summary>
        public void RestartCurrentScene()
        {
            SaveBeforeSceneTransition();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Load next scene in build order
        /// </summary>
        public void LoadNextScene()
        {
            SaveBeforeSceneTransition();
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int nextIndex = currentIndex + 1;
            
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                // Loop back to first scene if at the end
                SceneManager.LoadScene(0);
            }
        }

        /// <summary>
        /// Load previous scene in build order
        /// </summary>
        public void LoadPreviousScene()
        {
            SaveBeforeSceneTransition();
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int previousIndex = currentIndex - 1;
            
            if (previousIndex >= 0)
            {
                SceneManager.LoadScene(previousIndex);
            }
            else
            {
                // Loop to last scene if at the beginning
                SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
            }
        }

        /// <summary>
        /// Load scene by build index
        /// </summary>
        public void LoadSceneByIndex(int sceneIndex)
        {
            if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SaveBeforeSceneTransition();
                SceneManager.LoadScene(sceneIndex);
            }
            else
            {
                Debug.LogError($"Scene index {sceneIndex} is out of range! Available scenes: 0-{SceneManager.sceneCountInBuildSettings - 1}");
            }
        }

        /// <summary>
        /// Load scene by name
        /// </summary>
        public void LoadSceneByName(string sceneName)
        {
            SaveBeforeSceneTransition();
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Get current scene index
        /// </summary>
        public int GetCurrentSceneIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }

        /// <summary>
        /// Get current scene name
        /// </summary>
        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Check if there is a next scene
        /// </summary>
        public bool HasNextScene()
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            return currentIndex < SceneManager.sceneCountInBuildSettings - 1;
        }

        /// <summary>
        /// Check if there is a previous scene
        /// </summary>
        public bool HasPreviousScene()
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            return currentIndex > 0;
        }
    }
}

