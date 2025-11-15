using UnityEngine;
using System.Collections.Generic;

namespace DustOfWar.Enemies
{
    /// <summary>
    /// Balanced enemy spawn manager
    /// Controls spawn rates, enemy limits, and difficulty scaling
    /// </summary>
    public class EnemySpawnManager : MonoBehaviour
    {
        [System.Serializable]
        public class EnemySpawnData
        {
            public GameObject enemyPrefab;
            public float spawnWeight = 1f; // Higher = more likely to spawn
            public int maxOnScreen = 3; // Max enemies of this type at once
            public float minSpawnInterval = 5f; // Minimum time between spawns
            public float spawnDistanceMin = 8f;
            public float spawnDistanceMax = 15f;
        }

        [Header("Spawn Settings")]
        [SerializeField] private EnemySpawnData[] enemyTypes;
        [SerializeField] private int maxTotalEnemies = 8; // Total enemies on screen
        [SerializeField] private float globalSpawnInterval = 3f; // Base spawn interval
        [SerializeField] private float spawnIntervalVariation = 1f; // Random variation

        [Header("Difficulty Scaling")]
        [SerializeField] private float difficultyIncreaseRate = 0.1f; // Per wave
        [SerializeField] private float maxDifficultyMultiplier = 2f;
        [SerializeField] private int currentWave = 1;
        [SerializeField] private float waveDuration = 30f; // Seconds per wave

        [Header("Spawn Patterns")]
        [SerializeField] private bool spawnBehindPlayer = true;
        [SerializeField] private bool spawnAtEdges = true;
        [SerializeField] private float edgeSpawnChance = 0.4f;

        private Transform playerTarget;
        private Dictionary<GameObject, int> activeEnemyCounts = new Dictionary<GameObject, int>();
        private Dictionary<GameObject, float> lastSpawnTimes = new Dictionary<GameObject, float>();
        private float lastGlobalSpawnTime = 0f;
        private float waveStartTime = 0f;
        private float currentDifficulty = 1f;

        private void Start()
        {
            FindPlayerTarget();
            waveStartTime = Time.time;
            
            // Initialize dictionaries
            foreach (var enemyData in enemyTypes)
            {
                if (enemyData.enemyPrefab != null)
                {
                    activeEnemyCounts[enemyData.enemyPrefab] = 0;
                    lastSpawnTimes[enemyData.enemyPrefab] = 0f;
                }
            }
        }

        private void Update()
        {
            if (playerTarget == null)
            {
                FindPlayerTarget();
                return;
            }

            // Update difficulty
            UpdateDifficulty();

            // Check if we can spawn
            if (CanSpawn())
            {
                SpawnEnemy();
            }
        }

        private void FindPlayerTarget()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        private void UpdateDifficulty()
        {
            float timeSinceWaveStart = Time.time - waveStartTime;
            
            if (timeSinceWaveStart >= waveDuration)
            {
                currentWave++;
                waveStartTime = Time.time;
                currentDifficulty = Mathf.Min(1f + (currentWave - 1) * difficultyIncreaseRate, maxDifficultyMultiplier);
            }
        }

        private bool CanSpawn()
        {
            // Check global spawn interval
            float nextSpawnTime = lastGlobalSpawnTime + globalSpawnInterval + Random.Range(-spawnIntervalVariation, spawnIntervalVariation);
            if (Time.time < nextSpawnTime)
            {
                return false;
            }

            // Check total enemy limit
            int totalEnemies = GetTotalEnemyCount();
            if (totalEnemies >= maxTotalEnemies)
            {
                return false;
            }

            return true;
        }

        private int GetTotalEnemyCount()
        {
            int total = 0;
            foreach (var count in activeEnemyCounts.Values)
            {
                total += count;
            }
            return total;
        }

        private void SpawnEnemy()
        {
            // Select enemy type based on weights and limits
            EnemySpawnData selectedEnemy = SelectEnemyType();
            if (selectedEnemy == null || selectedEnemy.enemyPrefab == null)
            {
                return;
            }

            // Check if we can spawn this type
            if (activeEnemyCounts[selectedEnemy.enemyPrefab] >= selectedEnemy.maxOnScreen)
            {
                return;
            }

            // Check spawn interval for this type
            if (Time.time - lastSpawnTimes[selectedEnemy.enemyPrefab] < selectedEnemy.minSpawnInterval)
            {
                return;
            }

            // Calculate spawn position
            Vector3 spawnPosition = CalculateSpawnPosition(selectedEnemy);

            // Spawn enemy
            GameObject enemyObj = Instantiate(selectedEnemy.enemyPrefab, spawnPosition, Quaternion.identity);
            
            // Apply difficulty scaling
            ApplyDifficultyScaling(enemyObj, currentDifficulty);

            // Track enemy
            activeEnemyCounts[selectedEnemy.enemyPrefab]++;
            lastSpawnTimes[selectedEnemy.enemyPrefab] = Time.time;
            lastGlobalSpawnTime = Time.time;

            // Subscribe to death event
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnEnemyDeath += (e) => OnEnemyDeath(selectedEnemy.enemyPrefab);
            }
        }

        private EnemySpawnData SelectEnemyType()
        {
            if (enemyTypes == null || enemyTypes.Length == 0) return null;

            // Filter available enemies (not at max count)
            List<EnemySpawnData> availableEnemies = new List<EnemySpawnData>();
            List<float> weights = new List<float>();

            foreach (var enemyData in enemyTypes)
            {
                if (enemyData.enemyPrefab == null) continue;
                if (activeEnemyCounts[enemyData.enemyPrefab] >= enemyData.maxOnScreen) continue;
                if (Time.time - lastSpawnTimes[enemyData.enemyPrefab] < enemyData.minSpawnInterval) continue;

                availableEnemies.Add(enemyData);
                weights.Add(enemyData.spawnWeight);
            }

            if (availableEnemies.Count == 0) return null;

            // Weighted random selection
            float totalWeight = 0f;
            foreach (float weight in weights)
            {
                totalWeight += weight;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            for (int i = 0; i < availableEnemies.Count; i++)
            {
                currentWeight += weights[i];
                if (randomValue <= currentWeight)
                {
                    return availableEnemies[i];
                }
            }

            return availableEnemies[0];
        }

        private Vector3 CalculateSpawnPosition(EnemySpawnData enemyData)
        {
            if (playerTarget == null)
            {
                return transform.position;
            }

            Vector3 playerPos = playerTarget.position;
            Vector3 spawnPos;

            // Decide spawn pattern
            bool spawnAtEdge = spawnAtEdges && Random.value < edgeSpawnChance;

            if (spawnAtEdge)
            {
                // Spawn at screen edge (horizontal game - prefer left/right edges)
                Camera cam = Camera.main;
                if (cam != null)
                {
                    float screenHeight = cam.orthographicSize * 2f;
                    float screenWidth = screenHeight * cam.aspect;
                    
                    // For horizontal game, prefer spawning from left/right (70% chance)
                    int edge;
                    if (Random.value < 0.7f)
                    {
                        // Left or Right
                        edge = Random.Range(1, 3); // 1 = Right, 2 = Left (will be handled as default)
                        if (edge == 2) edge = 3; // Map to Left
                    }
                    else
                    {
                        // Top or Bottom (less common in horizontal game)
                        edge = Random.value < 0.5f ? 0 : 2; // 0 = Top, 2 = Bottom
                    }
                    
                    float distance = Random.Range(enemyData.spawnDistanceMin, enemyData.spawnDistanceMax);
                    
                    switch (edge)
                    {
                        case 0: // Top (less common)
                            spawnPos = playerPos + new Vector3(
                                Random.Range(-screenWidth * 0.3f, screenWidth * 0.3f),
                                screenHeight * 0.5f + distance,
                                0f
                            );
                            break;
                        case 1: // Right (common)
                            spawnPos = playerPos + new Vector3(
                                screenWidth * 0.5f + distance,
                                Random.Range(-screenHeight * 0.3f, screenHeight * 0.3f),
                                0f
                            );
                            break;
                        case 2: // Bottom (less common)
                            spawnPos = playerPos + new Vector3(
                                Random.Range(-screenWidth * 0.3f, screenWidth * 0.3f),
                                -screenHeight * 0.5f - distance,
                                0f
                            );
                            break;
                        default: // Left (common)
                            spawnPos = playerPos + new Vector3(
                                -screenWidth * 0.5f - distance,
                                Random.Range(-screenHeight * 0.3f, screenHeight * 0.3f),
                                0f
                            );
                            break;
                    }
                }
                else
                {
                    spawnPos = GetRandomSpawnPosition(playerPos, enemyData);
                }
            }
            else if (spawnBehindPlayer)
            {
                // Spawn behind player (for horizontal game, usually left side)
                Vector2 playerDirection = playerTarget.right;
                float angle = Mathf.Atan2(playerDirection.y, playerDirection.x) + Mathf.PI; // Behind player
                float distance = Random.Range(enemyData.spawnDistanceMin, enemyData.spawnDistanceMax);
                spawnPos = playerPos + new Vector3(
                    Mathf.Cos(angle) * distance,
                    Mathf.Sin(angle) * distance + Random.Range(-2f, 2f), // Add some vertical variation
                    0f
                );
            }
            else
            {
                spawnPos = GetRandomSpawnPosition(playerPos, enemyData);
            }

            return spawnPos;
        }

        private Vector3 GetRandomSpawnPosition(Vector3 playerPos, EnemySpawnData enemyData)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(enemyData.spawnDistanceMin, enemyData.spawnDistanceMax);
            return playerPos + new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                0f
            );
        }

        private void ApplyDifficultyScaling(GameObject enemyObj, float difficulty)
        {
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Slightly increase health with difficulty
                float healthMultiplier = 1f + (difficulty - 1f) * 0.3f;
                enemy.SetMaxHealth(enemy.GetMaxHealth() * healthMultiplier);
            }
        }

        private void OnEnemyDeath(GameObject enemyPrefab)
        {
            if (activeEnemyCounts.ContainsKey(enemyPrefab))
            {
                activeEnemyCounts[enemyPrefab] = Mathf.Max(0, activeEnemyCounts[enemyPrefab] - 1);
            }
        }

        /// <summary>
        /// Get current wave number
        /// </summary>
        public int GetCurrentWave()
        {
            return currentWave;
        }

        /// <summary>
        /// Get current difficulty multiplier
        /// </summary>
        public float GetDifficulty()
        {
            return currentDifficulty;
        }

        /// <summary>
        /// Get total active enemies
        /// </summary>
        public int GetTotalActiveEnemies()
        {
            return GetTotalEnemyCount();
        }
    }
}

