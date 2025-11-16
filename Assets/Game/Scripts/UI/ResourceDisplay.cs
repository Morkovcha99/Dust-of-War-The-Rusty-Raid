using UnityEngine;
using TMPro;
using DustOfWar.Resources;
using DustOfWar.Gameplay;

namespace DustOfWar.UI
{
    /// <summary>
    /// UI component that displays current resource counts during gameplay
    /// </summary>
    public class ResourceDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI rustyBoltsText;
        [SerializeField] private TextMeshProUGUI fuelCanistersText;
        
        [Header("Settings")]
        [SerializeField] private bool showSessionResources = true; // Show session resources or total saved resources
        [SerializeField] private string rustyBoltsPrefix = "Болты: ";
        [SerializeField] private string fuelCanistersPrefix = "Горючее: ";
        
        private GameStatsManager statsManager;
        private SaveSystem saveSystem;

        private void Awake()
        {
            statsManager = GameStatsManager.Instance;
            saveSystem = SaveSystem.Instance;
        }

        private void Start()
        {
            UpdateDisplay();
            
            // Subscribe to resource collection events
            if (statsManager != null)
            {
                statsManager.OnResourceCollected += OnResourceCollected;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (statsManager != null)
            {
                statsManager.OnResourceCollected -= OnResourceCollected;
            }
        }

        private void OnResourceCollected(ResourcePickup.ResourceType type, int totalCount)
        {
            UpdateDisplay();
        }

        /// <summary>
        /// Update resource display text
        /// </summary>
        public void UpdateDisplay()
        {
            int rustyBolts = 0;
            int fuelCanisters = 0;

            if (showSessionResources)
            {
                // Show session resources from GameStatsManager
                if (statsManager != null)
                {
                    rustyBolts = statsManager.GetResourceCount(ResourcePickup.ResourceType.RustyBolt);
                    fuelCanisters = statsManager.GetResourceCount(ResourcePickup.ResourceType.FuelCanister);
                }
            }
            else
            {
                // Show total saved resources from SaveSystem
                if (saveSystem != null)
                {
                    rustyBolts = saveSystem.LoadRustyBolts();
                    fuelCanisters = saveSystem.LoadFuelCanisters();
                }
            }

            // Update UI text
            if (rustyBoltsText != null)
            {
                rustyBoltsText.text = $"{rustyBoltsPrefix}{rustyBolts}";
            }

            if (fuelCanistersText != null)
            {
                fuelCanistersText.text = $"{fuelCanistersPrefix}{fuelCanisters}";
            }
        }

        /// <summary>
        /// Set whether to show session or total resources
        /// </summary>
        public void SetShowSessionResources(bool showSession)
        {
            showSessionResources = showSession;
            UpdateDisplay();
        }
    }
}




