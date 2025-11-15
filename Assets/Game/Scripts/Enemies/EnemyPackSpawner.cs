using UnityEngine;
using System.Collections.Generic;

namespace DustOfWar.Enemies
{
    /// <summary>
    /// Spawner for Pack Hounds - spawns groups of 3-5 fast ramming enemies
    /// </summary>
    public class EnemyPackSpawner : MonoBehaviour
    {
        [Header("Pack Spawn Settings")]
        [SerializeField] private GameObject packHoundPrefab;
        [SerializeField] private int packSizeMin = 3;
        [SerializeField] private int packSizeMax = 5;
        [SerializeField] private float spawnRadius = 12f;
        [SerializeField] private float minDistanceFromPlayer = 8f;
        [SerializeField] private float packSpreadRadius = 3f;

        [Header("Spawn Timing")]
        [SerializeField] private float spawnInterval = 20f; // Longer interval between packs
        [SerializeField] private bool autoSpawn = true;
        [SerializeField] private float firstSpawnDelay = 10f; // Later first spawn
        [SerializeField] private int maxActivePacks = 2; // Max packs active at once

        private Transform playerTarget;
        private float lastSpawnTime = 0f;
        private List<EnemyPackHound> activePack = new List<EnemyPackHound>();
        private int activePackCount = 0;

        private void Start()
        {
            FindPlayerTarget();
            lastSpawnTime = Time.time - spawnInterval + firstSpawnDelay;
        }

        private void Update()
        {
            // Clean up destroyed pack members
            int beforeCount = activePack.Count;
            activePack.RemoveAll(hound => hound == null);
            
            // Update active pack count
            activePackCount = activePack.Count;

            if (autoSpawn && packHoundPrefab != null)
            {
                // Only spawn if we have room for more packs
                if (activePackCount < maxActivePacks * packSizeMax)
                {
                    if (Time.time - lastSpawnTime >= spawnInterval)
                    {
                        SpawnPack();
                        lastSpawnTime = Time.time;
                    }
                }
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

        /// <summary>
        /// Spawn a pack of hounds
        /// </summary>
        public void SpawnPack()
        {
            if (packHoundPrefab == null || playerTarget == null) return;

            int packSize = Random.Range(packSizeMin, packSizeMax + 1);
            Vector3 spawnCenter = CalculateSpawnPosition();

            for (int i = 0; i < packSize; i++)
            {
                // Spread pack members around spawn center
                Vector2 randomOffset = Random.insideUnitCircle * packSpreadRadius;
                Vector3 spawnPosition = spawnCenter + new Vector3(randomOffset.x, randomOffset.y, 0f);

                GameObject houndObj = Instantiate(packHoundPrefab, spawnPosition, Quaternion.identity);
                EnemyPackHound hound = houndObj.GetComponent<EnemyPackHound>();
                
                if (hound != null)
                {
                    activePack.Add(hound);
                }
            }
        }

        private Vector3 CalculateSpawnPosition()
        {
            if (playerTarget == null)
            {
                return transform.position;
            }

            // Spawn behind or to the side of player
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minDistanceFromPlayer, spawnRadius);
            
            Vector3 playerPos = playerTarget.position;
            Vector3 spawnPos = playerPos + new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                0f
            );

            return spawnPos;
        }

        /// <summary>
        /// Get number of active pack members
        /// </summary>
        public int GetActivePackSize()
        {
            return activePack.Count;
        }

        /// <summary>
        /// Set spawn interval
        /// </summary>
        public void SetSpawnInterval(float interval)
        {
            spawnInterval = Mathf.Max(1f, interval);
        }
    }
}

