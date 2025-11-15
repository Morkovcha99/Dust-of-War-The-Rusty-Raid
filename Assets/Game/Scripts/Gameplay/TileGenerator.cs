using UnityEngine;
using System.Collections.Generic;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Generates a large static map with sand tiles for horizontal game
    /// Generates entire map at start, not dynamically
    /// </summary>
    public class TileGenerator : MonoBehaviour
    {
        [Header("Tile Settings")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Vector2 tileSize = new Vector2(1f, 1f); // Size of one tile (should match sprite size)
        [SerializeField] private bool autoDetectTileSize = true; // Automatically detect tile size from sprite
        [SerializeField] private bool spritePivotIsCenter = true; // If true, sprite pivot is at center; if false, at bottom-left
        
        [Header("Map Size")]
        [SerializeField] private int tilesHorizontal = 100; // Total tiles horizontally
        [SerializeField] private int tilesVertical = 10; // Total tiles vertically
        [SerializeField] private Vector2 mapStartPosition = Vector2.zero; // Starting position of map (bottom-left corner)
        
        [Header("Randomization")]
        [SerializeField] private bool randomizeTileRotation = false;
        [SerializeField] private bool randomizeTileScale = false;
        [SerializeField] private float scaleVariation = 0.1f; // ±10% scale variation

        private List<GameObject> generatedTiles = new List<GameObject>();
        private Vector2 actualTileSize;

        private void Start()
        {
            // Detect tile size if enabled
            if (autoDetectTileSize && tilePrefab != null)
            {
                DetectTileSize();
            }
            else
            {
                actualTileSize = tileSize;
            }

            GenerateMap();
        }

        private void DetectTileSize()
        {
            SpriteRenderer spriteRenderer = tilePrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // Get sprite size in world units
                actualTileSize = spriteRenderer.sprite.bounds.size;
                Debug.Log($"Auto-detected tile size: {actualTileSize}");
            }
            else
            {
                // Fallback to configured size
                actualTileSize = tileSize;
                Debug.LogWarning("Could not auto-detect tile size, using configured size");
            }
        }

        private void GenerateMap()
        {
            if (tilePrefab == null)
            {
                Debug.LogWarning("Tile Prefab is not assigned in TileGenerator!");
                return;
            }

            // Clear existing tiles if regenerating
            ClearMap();

            // Generate tiles
            for (int x = 0; x < tilesHorizontal; x++)
            {
                for (int y = 0; y < tilesVertical; y++)
                {
                    CreateTile(x, y);
                }
            }

            Debug.Log($"Generated {generatedTiles.Count} tiles for map (Tile size: {actualTileSize})");
        }

        private void CreateTile(int x, int y)
        {
            Vector2 worldPos;
            
            if (spritePivotIsCenter)
            {
                // If pivot is at center, offset by half tile size
                worldPos = new Vector2(
                    mapStartPosition.x + x * actualTileSize.x + actualTileSize.x * 0.5f,
                    mapStartPosition.y + y * actualTileSize.y + actualTileSize.y * 0.5f
                );
            }
            else
            {
                // If pivot is at bottom-left, place directly
                worldPos = new Vector2(
                    mapStartPosition.x + x * actualTileSize.x,
                    mapStartPosition.y + y * actualTileSize.y
                );
            }

            GameObject tile = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
            
            // Randomize rotation if enabled
            if (randomizeTileRotation)
            {
                float randomRotation = Random.Range(0f, 360f);
                tile.transform.rotation = Quaternion.Euler(0, 0, randomRotation);
            }

            // Randomize scale if enabled
            if (randomizeTileScale)
            {
                float scale = 1f + Random.Range(-scaleVariation, scaleVariation);
                tile.transform.localScale = Vector3.one * scale;
            }

            generatedTiles.Add(tile);
        }

        private void ClearMap()
        {
            foreach (var tile in generatedTiles)
            {
                if (tile != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(tile);
                    }
                    else
                    {
                        DestroyImmediate(tile);
                    }
                }
            }
            generatedTiles.Clear();
        }

        /// <summary>
        /// Regenerate the entire map
        /// </summary>
        public void RegenerateMap()
        {
            GenerateMap();
        }

        /// <summary>
        /// Get map bounds
        /// </summary>
        public Bounds GetMapBounds()
        {
            Vector2 mapSize = new Vector2(
                tilesHorizontal * actualTileSize.x,
                tilesVertical * actualTileSize.y
            );
            
            Vector2 mapCenter = new Vector2(
                mapStartPosition.x + mapSize.x * 0.5f,
                mapStartPosition.y + mapSize.y * 0.5f
            );

            return new Bounds(mapCenter, mapSize);
        }

        /// <summary>
        /// Get actual tile size being used
        /// </summary>
        public Vector2 GetTileSize()
        {
            return actualTileSize;
        }
    }
}

