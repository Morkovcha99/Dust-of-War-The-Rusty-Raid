using UnityEngine;
using System.Collections.Generic;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Generates infinite ground tiles (sand) for horizontal scrolling game
    /// </summary>
    public class TileGenerator : MonoBehaviour
    {
        [Header("Tile Settings")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Vector2 tileSize = new Vector2(2f, 2f);
        [SerializeField] private int tilesAhead = 10;
        [SerializeField] private int tilesBehind = 5;
        [SerializeField] private int tilesVertical = 5;
        
        [Header("Generation Settings")]
        [SerializeField] private Transform followTarget; // Player
        [SerializeField] private float updateDistance = 2f; // Regenerate when player moves this far
        
        private Dictionary<Vector2Int, GameObject> activeTiles = new Dictionary<Vector2Int, GameObject>();
        private Vector2Int lastTilePosition;
        private Vector2 lastUpdatePosition;

        private void Start()
        {
            if (followTarget == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    followTarget = player.transform;
                }
            }
            
            lastUpdatePosition = followTarget != null ? (Vector2)followTarget.position : Vector2.zero;
            GenerateInitialTiles();
        }

        private void Update()
        {
            if (followTarget == null) return;

            Vector2 currentPosition = followTarget.position;
            float distanceMoved = Vector2.Distance(currentPosition, lastUpdatePosition);

            if (distanceMoved >= updateDistance)
            {
                UpdateTiles();
                lastUpdatePosition = currentPosition;
            }
        }

        private void GenerateInitialTiles()
        {
            Vector2Int centerTile = WorldToTile(followTarget != null ? (Vector2)followTarget.position : Vector2.zero);
            
            for (int x = -tilesBehind; x <= tilesAhead; x++)
            {
                for (int y = -tilesVertical / 2; y <= tilesVertical / 2; y++)
                {
                    Vector2Int tilePos = centerTile + new Vector2Int(x, y);
                    CreateTile(tilePos);
                }
            }
            
            lastTilePosition = centerTile;
        }

        private void UpdateTiles()
        {
            Vector2Int currentTile = WorldToTile(followTarget.position);
            
            if (currentTile == lastTilePosition) return;

            // Remove tiles that are too far behind
            List<Vector2Int> tilesToRemove = new List<Vector2Int>();
            foreach (var tilePos in activeTiles.Keys)
            {
                if (tilePos.x < currentTile.x - tilesBehind)
                {
                    tilesToRemove.Add(tilePos);
                }
            }

            foreach (var tilePos in tilesToRemove)
            {
                DestroyTile(tilePos);
            }

            // Generate new tiles ahead
            for (int x = lastTilePosition.x + 1; x <= currentTile.x + tilesAhead; x++)
            {
                for (int y = -tilesVertical / 2; y <= tilesVertical / 2; y++)
                {
                    Vector2Int tilePos = new Vector2Int(x, y);
                    if (!activeTiles.ContainsKey(tilePos))
                    {
                        CreateTile(tilePos);
                    }
                }
            }

            lastTilePosition = currentTile;
        }

        private void CreateTile(Vector2Int tilePos)
        {
            if (tilePrefab == null) return;

            Vector2 worldPos = TileToWorld(tilePos);
            GameObject tile = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
            activeTiles[tilePos] = tile;
        }

        private void DestroyTile(Vector2Int tilePos)
        {
            if (activeTiles.TryGetValue(tilePos, out GameObject tile))
            {
                Destroy(tile);
                activeTiles.Remove(tilePos);
            }
        }

        private Vector2Int WorldToTile(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / tileSize.x),
                Mathf.FloorToInt(worldPos.y / tileSize.y)
            );
        }

        private Vector2 TileToWorld(Vector2Int tilePos)
        {
            return new Vector2(
                tilePos.x * tileSize.x + tileSize.x * 0.5f,
                tilePos.y * tileSize.y + tileSize.y * 0.5f
            );
        }
    }
}

