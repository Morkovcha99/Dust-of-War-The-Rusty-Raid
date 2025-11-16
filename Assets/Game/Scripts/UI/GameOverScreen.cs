using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DustOfWar.Resources;

namespace DustOfWar.UI
{
    /// <summary>
    /// Game Over screen that appears when player dies
    /// Shows statistics: resources collected, time played, enemies killed
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Image darkeningOverlay;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI enemiesKilledText;
        [SerializeField] private Transform resourcesContainer;
        [SerializeField] private GameObject resourceItemPrefab; // Prefab with TextMeshProUGUI component for resource display

        [Header("Settings")]
        [SerializeField] private float darkeningAlpha = 0.7f;
        [SerializeField] private float fadeInDuration = 0.5f;

        private bool isShowing = false;
        private Canvas canvas;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // High priority
            }

            // Hide panel initially
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            if (darkeningOverlay != null)
            {
                darkeningOverlay.gameObject.SetActive(false);
                Color color = darkeningOverlay.color;
                color.a = 0f;
                darkeningOverlay.color = color;
            }
        }

        private DustOfWar.Player.PlayerVehicle playerVehicle;

        private void Start()
        {
            // Find and subscribe to player death event
            FindAndSubscribeToPlayer();
        }

        private void FindAndSubscribeToPlayer()
        {
            if (playerVehicle != null) return;

            playerVehicle = FindFirstObjectByType<DustOfWar.Player.PlayerVehicle>();
            if (playerVehicle != null)
            {
                playerVehicle.OnVehicleDestroyed += ShowGameOver;
            }
            else
            {
                // Try again next frame if player not found yet
                Invoke(nameof(FindAndSubscribeToPlayer), 0.1f);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (playerVehicle != null)
            {
                playerVehicle.OnVehicleDestroyed -= ShowGameOver;
            }
        }

        /// <summary>
        /// Show game over screen with statistics
        /// </summary>
        public void ShowGameOver()
        {
            if (isShowing) return;
            isShowing = true;

            // Stop game time
            Time.timeScale = 0f;

            // Stop tracking stats
            if (DustOfWar.Gameplay.GameStatsManager.Instance != null)
            {
                DustOfWar.Gameplay.GameStatsManager.Instance.StopTracking();
            }

            // Save progress to permanent storage
            if (DustOfWar.Gameplay.SaveSystem.Instance != null)
            {
                DustOfWar.Gameplay.SaveSystem.Instance.SaveSessionResources();
            }

            // Show UI
            if (darkeningOverlay != null)
            {
                darkeningOverlay.gameObject.SetActive(true);
                StartCoroutine(FadeInDarkening());
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                UpdateStatistics();
            }
        }

        private System.Collections.IEnumerator FadeInDarkening()
        {
            if (darkeningOverlay == null) yield break;

            float elapsed = 0f;
            Color color = darkeningOverlay.color;
            color.a = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled time since game is paused
                color.a = Mathf.Lerp(0f, darkeningAlpha, elapsed / fadeInDuration);
                darkeningOverlay.color = color;
                yield return null;
            }

            color.a = darkeningAlpha;
            darkeningOverlay.color = color;
        }

        private void UpdateStatistics()
        {
            if (DustOfWar.Gameplay.GameStatsManager.Instance == null) return;

            var statsManager = DustOfWar.Gameplay.GameStatsManager.Instance;
            var saveSystem = DustOfWar.Gameplay.SaveSystem.Instance;

            // Update time (current session)
            if (timeText != null)
            {
                timeText.text = $"Время этой битвы: {statsManager.GetFormattedPlayTime()}";
            }

            // Update enemies killed (current session)
            if (enemiesKilledText != null)
            {
                enemiesKilledText.text = $"Врагов уничтожено: {statsManager.GetEnemiesKilled()}";
            }

            // Update resources (show both session and total saved)
            if (resourcesContainer != null)
            {
                // Clear existing resource items
                foreach (Transform child in resourcesContainer)
                {
                    Destroy(child.gameObject);
                }

                // Create resource items from current session
                Dictionary<DustOfWar.Resources.ResourcePickup.ResourceType, int> sessionResources = statsManager.GetAllResources();
                
                // Show session resources and total saved
                if (saveSystem != null)
                {
                    // Rusty Bolts - always show with numbers
                    int sessionBolts = sessionResources != null && sessionResources.ContainsKey(DustOfWar.Resources.ResourcePickup.ResourceType.RustyBolt) 
                        ? sessionResources[DustOfWar.Resources.ResourcePickup.ResourceType.RustyBolt] : 0;
                    int totalBolts = saveSystem.LoadRustyBolts();
                    CreateResourceItem(DustOfWar.Resources.ResourcePickup.ResourceType.RustyBolt, sessionBolts, totalBolts);

                    // Fuel Canisters - always show with numbers
                    int sessionFuel = sessionResources != null && sessionResources.ContainsKey(DustOfWar.Resources.ResourcePickup.ResourceType.FuelCanister) 
                        ? sessionResources[DustOfWar.Resources.ResourcePickup.ResourceType.FuelCanister] : 0;
                    int totalFuel = saveSystem.LoadFuelCanisters();
                    CreateResourceItem(DustOfWar.Resources.ResourcePickup.ResourceType.FuelCanister, sessionFuel, totalFuel);
                }
                else
                {
                    // Fallback: show only session resources - always show with numbers
                    if (sessionResources != null)
                    {
                        foreach (var resource in sessionResources)
                        {
                            CreateResourceItem(resource.Key, resource.Value, 0);
                        }
                    }
                    else
                    {
                        // Show at least Rusty Bolts and Fuel Canisters even if 0
                        CreateResourceItem(DustOfWar.Resources.ResourcePickup.ResourceType.RustyBolt, 0, 0);
                        CreateResourceItem(DustOfWar.Resources.ResourcePickup.ResourceType.FuelCanister, 0, 0);
                    }
                }
            }
        }

        private void CreateResourceItem(DustOfWar.Resources.ResourcePickup.ResourceType type, int sessionCount, int totalCount = 0)
        {
            if (resourceItemPrefab == null || resourcesContainer == null) return;

            GameObject item = Instantiate(resourceItemPrefab, resourcesContainer);
            TextMeshProUGUI textComponent = item.GetComponent<TextMeshProUGUI>();
            
            if (textComponent != null)
            {
                string resourceName = GetResourceName(type);
                if (totalCount > 0)
                {
                    textComponent.text = $"{resourceName}: +{sessionCount} (Всего: {totalCount})";
                }
                else
                {
                    textComponent.text = $"{resourceName}: {sessionCount}";
                }
            }
        }

        private string GetResourceName(DustOfWar.Resources.ResourcePickup.ResourceType type)
        {
            switch (type)
            {
                case DustOfWar.Resources.ResourcePickup.ResourceType.RustyBolt:
                    return "Ржавые болты";
                case DustOfWar.Resources.ResourcePickup.ResourceType.FuelCanister:
                    return "Канистры с горючим";
                case DustOfWar.Resources.ResourcePickup.ResourceType.HealthPickup:
                    return "Восстановление здоровья";
                case DustOfWar.Resources.ResourcePickup.ResourceType.UpgradePart:
                    return "Детали для улучшения";
                default:
                    return type.ToString();
            }
        }

        /// <summary>
        /// Restart game (called from UI button)
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            if (DustOfWar.Gameplay.SceneTransitionManager.Instance != null)
            {
                DustOfWar.Gameplay.SceneTransitionManager.Instance.RestartCurrentScene();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }

        /// <summary>
        /// Return to menu (called from UI button)
        /// </summary>
        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            if (DustOfWar.Gameplay.SceneTransitionManager.Instance != null)
            {
                // Load first scene (usually menu/garage)
                DustOfWar.Gameplay.SceneTransitionManager.Instance.LoadSceneByIndex(0);
            }
            else
            {
                // Fallback: try to load by name
                UnityEngine.SceneManagement.SceneManager.LoadScene("HangarScene");
            }
        }
    }
}

