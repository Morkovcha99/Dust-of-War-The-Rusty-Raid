using UnityEngine;
using System.Collections.Generic;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Spawns obstacles (explosive barrels and rocks) on the map
    /// Obstacles are rare and randomly placed
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Obstacle Prefabs")]
        [SerializeField] private GameObject explosiveBarrelPrefab;
        [SerializeField] private GameObject rockPrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private float spawnChancePerTile = 0.02f; // 2% chance per tile
        [SerializeField] private float barrelChance = 0.3f; // 30% of obstacles are barrels
        [SerializeField] private float rockChance = 0.7f; // 70% are rocks
        [SerializeField] private float minDistanceBetweenObstacles = 3f;
        [SerializeField] private int tilesAhead = 15;
        [SerializeField] private int tilesBehind = 5;
        [SerializeField] private Vector2 tileSize = new Vector2(2f, 2f);
        
        [Header("Spawn Area")]
        [SerializeField] private float spawnYMin = -4f;
        [SerializeField] private float spawnYMax = 4f;

        private Transform followTarget;
        private List<Vector2> spawnedObstaclePositions = new List<Vector2>();
        private Vector2Int lastSpawnedTile = new Vector2Int(int.MinValue, 0);
        private float cleanupDistance = 20f;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                followTarget = player.transform;
            }
        }

        private void Update()
        {
            if (followTarget == null) return;

            Vector2Int currentTile = WorldToTile(followTarget.position);
            
            // Spawn obstacles in new tiles
            for (int x = lastSpawnedTile.x; x <= currentTile.x + tilesAhead; x++)
            {
                if (x > lastSpawnedTile.x)
                {
                    TrySpawnObstaclesInTile(x);
                }
            }

            lastSpawnedTile = currentTile;

            // Clean up obstacles that are too far behind
            CleanupDistantObstacles();
        }

        private void TrySpawnObstaclesInTile(int tileX)
        {
            // Check each vertical position in tile
            for (float y = spawnYMin; y <= spawnYMax; y += tileSize.y)
            {
                if (Random.value < spawnChancePerTile)
                {
                    Vector2 spawnPos = new Vector2(
                        tileX * tileSize.x + Random.Range(-tileSize.x * 0.4f, tileSize.x * 0.4f),
                        y + Random.Range(-tileSize.y * 0.3f, tileSize.y * 0.3f)
                    );

                    // Check if position is far enough from other obstacles
                    if (IsPositionValid(spawnPos))
                    {
                        SpawnObstacle(spawnPos);
                        spawnedObstaclePositions.Add(spawnPos);
                    }
                }
            }
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
                Instantiate(obstaclePrefab, position, Quaternion.identity, transform);
            }
        }

        private void CleanupDistantObstacles()
        {
            if (followTarget == null) return;

            List<GameObject> obstaclesToDestroy = new List<GameObject>();
            
            foreach (Transform child in transform)
            {
                if (Vector2.Distance(child.position, followTarget.position) > cleanupDistance)
                {
                    obstaclesToDestroy.Add(child.gameObject);
                    spawnedObstaclePositions.Remove(child.position);
                }
            }

            foreach (var obstacle in obstaclesToDestroy)
            {
                Destroy(obstacle);
            }
        }

        private Vector2Int WorldToTile(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / tileSize.x),
                Mathf.FloorToInt(worldPos.y / tileSize.y)
            );
        }
    }
}

