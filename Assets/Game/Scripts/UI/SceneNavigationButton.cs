using UnityEngine;
using UnityEngine.UI;
using DustOfWar.Gameplay;

namespace DustOfWar.UI
{
    /// <summary>
    /// Simple button component for scene navigation
    /// Can be attached to UI buttons for scene transitions
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SceneNavigationButton : MonoBehaviour
    {
        public enum NavigationType
        {
            Next,           // Load next scene
            Previous,       // Load previous scene
            Restart,        // Restart current scene
            ByIndex,        // Load scene by build index
            ByName          // Load scene by name
        }

        [Header("Navigation Settings")]
        [SerializeField] private NavigationType navigationType = NavigationType.Next;
        
        [Header("Scene Settings (for ByIndex or ByName)")]
        [SerializeField] private int targetSceneIndex = 0;
        [SerializeField] private string targetSceneName = "SampleScene";

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            if (SceneTransitionManager.Instance == null)
            {
                Debug.LogError("SceneNavigationButton: SceneTransitionManager.Instance is null! Make sure SceneTransitionManager exists in the scene.");
                return;
            }

            switch (navigationType)
            {
                case NavigationType.Next:
                    SceneTransitionManager.Instance.LoadNextScene();
                    break;

                case NavigationType.Previous:
                    SceneTransitionManager.Instance.LoadPreviousScene();
                    break;

                case NavigationType.Restart:
                    SceneTransitionManager.Instance.RestartCurrentScene();
                    break;

                case NavigationType.ByIndex:
                    SceneTransitionManager.Instance.LoadSceneByIndex(targetSceneIndex);
                    break;

                case NavigationType.ByName:
                    SceneTransitionManager.Instance.LoadSceneByName(targetSceneName);
                    break;
            }
        }

        /// <summary>
        /// Set navigation type programmatically
        /// </summary>
        public void SetNavigationType(NavigationType type)
        {
            navigationType = type;
        }

        /// <summary>
        /// Set target scene index (for ByIndex type)
        /// </summary>
        public void SetTargetSceneIndex(int index)
        {
            targetSceneIndex = index;
        }

        /// <summary>
        /// Set target scene name (for ByName type)
        /// </summary>
        public void SetTargetSceneName(string name)
        {
            targetSceneName = name;
        }
    }
}

