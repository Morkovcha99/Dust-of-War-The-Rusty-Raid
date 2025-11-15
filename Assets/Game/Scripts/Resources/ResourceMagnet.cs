using UnityEngine;
using System.Collections.Generic;

namespace DustOfWar.Resources
{
    /// <summary>
    /// Attracts resources to the player
    /// Used by ResourceMagnet upgrade
    /// </summary>
    public class ResourceMagnet : MonoBehaviour
    {
        [Header("Magnet Settings")]
        [SerializeField] private float baseRadius = 3f;
        [SerializeField] private float baseSpeed = 5f;
        [SerializeField] private LayerMask resourceLayer = -1;

        private float radiusMultiplier = 1f;
        private float speedMultiplier = 1f;
        private List<GameObject> attractedResources = new List<GameObject>();

        private void Update()
        {
            float currentRadius = baseRadius * radiusMultiplier;
            float currentSpeed = baseSpeed * speedMultiplier;

            // Find resources in range
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, currentRadius, resourceLayer);

            foreach (Collider2D collider in colliders)
            {
                // Check if it's a resource (by tag or component)
                if (collider.CompareTag("Resource") || collider.GetComponent<ResourcePickup>() != null)
                {
                    GameObject resource = collider.gameObject;
                    if (!attractedResources.Contains(resource))
                    {
                        attractedResources.Add(resource);
                    }

                    // Attract resource
                    Vector2 direction = (transform.position - resource.transform.position).normalized;
                    Rigidbody2D rb = resource.GetComponent<Rigidbody2D>();
                    
                    if (rb != null)
                    {
                        rb.linearVelocity = direction * currentSpeed;
                    }
                    else
                    {
                        resource.transform.position = Vector2.MoveTowards(
                            resource.transform.position,
                            transform.position,
                            currentSpeed * Time.deltaTime
                        );
                    }
                }
            }

            // Clean up destroyed resources
            attractedResources.RemoveAll(r => r == null);
        }

        public void SetRadiusMultiplier(float multiplier)
        {
            radiusMultiplier = Mathf.Max(0.1f, multiplier);
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = Mathf.Max(0.1f, multiplier);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            float currentRadius = baseRadius * radiusMultiplier;
            DrawWireCircle(transform.position, currentRadius);
        }

        private void DrawWireCircle(Vector3 center, float radius, int segments = 32)
        {
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);
            for (int i = 1; i <= segments; i++)
            {
                float angle = (float)i / segments * 2f * Mathf.PI;
                Vector3 nextPoint = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0
                );
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
    }
}

