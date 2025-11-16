using UnityEngine;
using System.Collections.Generic;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Spawns obstacles (explosive barrels and rocks) on a static map
    /// Obstacles are rare and randomly placed across the entire map
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Obstacle Prefabs")]
        [SerializeField] private GameObject explosiveBarrelPrefab;
        [SerializeField] private GameObject rockPrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private int totalObstacles = 50; // Total number of obstacles to spawn
        [SerializeField] private float barrelChance = 0.3f; // 30% of obstacles are barrels
        [SerializeField] private float rockChance = 0.7f; // 70% are rocks
        [SerializeField] private float minDistanceBetweenObstacles = 3f;
        [SerializeField] private int maxAttemptsPerObstacle = 50; // Max attempts to find valid position
        [SerializeField] private Vector2 tileSize = new Vector2(2f, 2f);
        
        [Header("Map Settings")]
        [SerializeField] private TileGenerator tileGenerator; // Reference to tile generator
        [SerializeField] private int tilesHorizontal = 100;
        [SerializeField] private int tilesVertical = 10;
        [SerializeField] private Vector2 mapStartPosition = Vector2.zero;
        
        [Header("Spawn Area")]
        [SerializeField] private float spawnYMin = -4f;
        [SerializeField] private float spawnYMax = 4f;

        private List<Vector2> spawnedObstaclePositions = new List<Vector2>();
        private List<GameObject> spawnedObstacles = new List<GameObject>();

        private void Start()
        {
            // Get map settings from TileGenerator if available
            if (tileGenerator != null)
            {
                Bounds mapBounds = tileGenerator.GetMapBounds();
                Vector2 actualTileSize = tileGenerator.GetTileSize();
                tilesHorizontal = Mathf.RoundToInt(mapBounds.size.x / actualTileSize.x);
                tilesVertical = Mathf.RoundToInt(mapBounds.size.y / actualTileSize.y);
                mapStartPosition = mapBounds.min;
                tileSize = actualTileSize; // Sync tile size
            }

            GenerateObstacles();
        }

        private void GenerateObstacles()
        {
            // Calculate map bounds
            float mapWidth = tilesHorizontal * tileSize.x;
            float mapHeight = tilesVertical * tileSize.y;
            float mapXMin = mapStartPosition.x;
            float mapXMax = mapStartPosition.x + mapWidth;
            float mapYMin = Mathf.Max(mapStartPosition.y, spawnYMin);
            float mapYMax = Mathf.Min(mapStartPosition.y + mapHeight, spawnYMax);

            // Validate map bounds
            if (mapWidth <= 0 || mapHeight <= 0)
            {
                Debug.LogError("ObstacleSpawner: Invalid map bounds! Cannot generate obstacles.");
                return;
            }

            int obstaclesSpawned = 0;
            int attempts = 0;
            int maxTotalAttempts = totalObstacles * maxAttemptsPerObstacle * 3;

            // Calculate grid dimensions for even distribution
            int gridCols = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(totalObstacles * (mapWidth / Mathf.Max(mapHeight, 0.1f)))));
            int gridRows = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(totalObstacles * (mapHeight / Mathf.Max(mapWidth, 0.1f)))));
            
            // Ensure we have enough grid cells
            while (gridCols * gridRows < totalObstacles && (gridCols * gridRows) < 1000)
            {
                if (mapWidth > mapHeight)
                    gridCols++;
                else
                    gridRows++;
            }

            float cellWidth = mapWidth / Mathf.Max(1, gridCols);
            float cellHeight = mapHeight / Mathf.Max(1, gridRows);

            // Create list of grid cell indices
            List<int> gridCellIndices = new List<int>();
            for (int i = 0; i < gridCols * gridRows; i++)
            {
                gridCellIndices.Add(i);
            }

            // Shuffle grid cell indices for randomness
            for (int i = 0; i < gridCellIndices.Count; i++)
            {
                int temp = gridCellIndices[i];
                int randomIndex = Random.Range(i, gridCellIndices.Count);
                gridCellIndices[i] = gridCellIndices[randomIndex];
                gridCellIndices[randomIndex] = temp;
            }

            // Spawn obstacles in grid cells
            for (int cellIndex = 0; cellIndex < gridCellIndices.Count && obstaclesSpawned < totalObstacles; cellIndex++)
            {
                int cellIdx = gridCellIndices[cellIndex];
                int cellX = cellIdx % gridCols;
                int cellY = cellIdx / gridCols;

                // Calculate cell bounds
                float cellXMin = mapXMin + cellX * cellWidth;
                float cellXMax = mapXMin + (cellX + 1) * cellWidth;
                float cellYMin = mapYMin + cellY * cellHeight;
                float cellYMax = mapYMin + (cellY + 1) * cellHeight;

                // Try to spawn obstacle in this cell
                bool spawnedInCell = false;
                for (int attempt = 0; attempt < maxAttemptsPerObstacle && !spawnedInCell; attempt++)
                {
                    attempts++;
                    if (attempts > maxTotalAttempts) break;

                    // Generate random position within cell (with some margin from edges)
                    float marginX = Mathf.Max(0.1f, cellWidth * 0.1f);
                    float marginY = Mathf.Max(0.1f, cellHeight * 0.1f);
                    Vector2 randomPos = new Vector2(
                        Random.Range(cellXMin + marginX, cellXMax - marginX),
                        Random.Range(cellYMin + marginY, cellYMax - marginY)
                    );

                    // Clamp to map bounds
                    randomPos.x = Mathf.Clamp(randomPos.x, mapXMin, mapXMax);
                    randomPos.y = Mathf.Clamp(randomPos.y, mapYMin, mapYMax);

                    // Check if position is valid (far enough from other obstacles)
                    if (IsPositionValid(randomPos))
                    {
                        SpawnObstacle(randomPos);
                        spawnedObstaclePositions.Add(randomPos);
                        obstaclesSpawned++;
                        spawnedInCell = true;
                    }
                }
            }

            // If we haven't spawned enough obstacles, fill remaining with random positions
            while (obstaclesSpawned < totalObstacles && attempts < maxTotalAttempts)
            {
                attempts++;

                // Generate random position within map bounds
                Vector2 randomPos = new Vector2(
                    Random.Range(mapXMin, mapXMax),
                    Random.Range(mapYMin, mapYMax)
                );

                // Check if position is valid (far enough from other obstacles)
                if (IsPositionValid(randomPos))
                {
                    SpawnObstacle(randomPos);
                    spawnedObstaclePositions.Add(randomPos);
                    obstaclesSpawned++;
                }
            }

            Debug.Log($"Generated {spawnedObstacles.Count} obstacles on map (attempts: {attempts}, grid: {gridCols}x{gridRows})");
        }

        private bool IsPositionValid(Vector2 position)
        {
            foreach (var obstaclePos in spawnedObstaclePositions)
            {
                if (Vector2.Distance(position, obstaclePos) < minDistanceBetweenObstacles)
                {
                    return false;
                }
            }
            return true;
        }

        private void SpawnObstacle(Vector2 position)
        {
            float randomValue = Random.value;
            GameObject obstaclePrefab = null;

            if (randomValue < barrelChance && explosiveBarrelPrefab != null)
            {
                obstaclePrefab = explosiveBarrelPrefab;
            }
            else if (rockPrefab != null)
            {
                obstaclePrefab = rockPrefab;
            }

            if (obstaclePrefab != null)
            {
                GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity, transform);
                spawnedObstacles.Add(obstacle);
            }
        }

        /// <summary>
        /// Regenerate all obstacles
        /// </summary>
        public void RegenerateObstacles()
        {
            // Clear existing obstacles
            foreach (var obstacle in spawnedObstacles)
            {
                if (obstacle != null)
                {
                    DestroyImmediate(obstacle);
                }
            }
            spawnedObstacles.Clear();
            spawnedObstaclePositions.Clear();

            // Regenerate
            GenerateObstacles();
        }
    }
}

