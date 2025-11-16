using UnityEngine;
using System.Collections.Generic;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Tracks game statistics: resources collected, time played, enemies killed, etc.
    /// </summary>
    public class GameStatsManager : MonoBehaviour
    {
        private static GameStatsManager instance;
        public static GameStatsManager Instance => instance;

        [Header("Stats")]
        private Dictionary<DustOfWar.Resources.ResourcePickup.ResourceType, int> resourcesCollected = new Dictionary<DustOfWar.Resources.ResourcePickup.ResourceType, int>();
        private float gameStartTime;
        private int enemiesKilled = 0;
        private bool isGameActive = true;

        // Events
        public System.Action<DustOfWar.Resources.ResourcePickup.ResourceType, int> OnResourceCollected;
        public System.Action<int> OnEnemyKilled;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeStats();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            gameStartTime = Time.time;
        }

        private void InitializeStats()
        {
            // Initialize all resource types to 0
            foreach (DustOfWar.Resources.ResourcePickup.ResourceType type in System.Enum.GetValues(typeof(DustOfWar.Resources.ResourcePickup.ResourceType)))
            {
                resourcesCollected[type] = 0;
            }
        }

        /// <summary>
        /// Record resource collection
        /// </summary>
        public void CollectResource(DustOfWar.Resources.ResourcePickup.ResourceType type, int amount = 1)
        {
            if (!isGameActive) return;

            if (!resourcesCollected.ContainsKey(type))
            {
                resourcesCollected[type] = 0;
            }

            resourcesCollected[type] += amount;
            OnResourceCollected?.Invoke(type, resourcesCollected[type]);
        }

        /// <summary>
        /// Record enemy kill
        /// </summary>
        public void RecordEnemyKill()
        {
            if (!isGameActive) return;

            enemiesKilled++;
            OnEnemyKilled?.Invoke(enemiesKilled);
        }

        /// <summary>
        /// Get total play time in seconds
        /// </summary>
        public float GetPlayTime()
        {
            if (!isGameActive) return 0f;
            return Time.time - gameStartTime;
        }

        /// <summary>
        /// Get formatted play time string (MM:SS)
        /// </summary>
        public string GetFormattedPlayTime()
        {
            float time = GetPlayTime();
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        /// <summary>
        /// Get resources collected of specific type
        /// </summary>
        public int GetResourceCount(DustOfWar.Resources.ResourcePickup.ResourceType type)
        {
            return resourcesCollected.ContainsKey(type) ? resourcesCollected[type] : 0;
        }

        /// <summary>
        /// Get all resources collected
        /// </summary>
        public Dictionary<DustOfWar.Resources.ResourcePickup.ResourceType, int> GetAllResources()
        {
            return new Dictionary<DustOfWar.Resources.ResourcePickup.ResourceType, int>(resourcesCollected);
        }

        /// <summary>
        /// Get total enemies killed
        /// </summary>
        public int GetEnemiesKilled()
        {
            return enemiesKilled;
        }

        /// <summary>
        /// Stop tracking (when game ends)
        /// </summary>
        public void StopTracking()
        {
            isGameActive = false;
        }

        /// <summary>
        /// Reset all stats (for new game session)
        /// </summary>
        public void ResetStats()
        {
            InitializeStats();
            enemiesKilled = 0;
            gameStartTime = Time.time;
            isGameActive = true;
        }

        /// <summary>
        /// Get total resources collected this session
        /// </summary>
        public int GetTotalResourcesCollected()
        {
            int total = 0;
            foreach (var resource in resourcesCollected.Values)
            {
                total += resource;
            }
            return total;
        }
    }
}

