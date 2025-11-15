using UnityEngine;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Camera follow system for 2D horizontal side-scrolling game
    /// Smoothly follows the player with configurable offset and smoothing
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private bool autoFindPlayer = true;
        
        [Header("Follow Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private bool useSmoothing = true;
        
        [Header("Bounds Settings")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;
        [SerializeField] private float minY = -10f;
        [SerializeField] private float maxY = 10f;
        
        [Header("Camera Settings")]
        [SerializeField] private float orthographicSize = 5f;
        [SerializeField] private bool followOnStart = true;

        private Camera cam;
        private Vector3 velocity = Vector3.zero;
        private bool isFollowing = true;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = orthographicSize;
            }

            // Auto-find player if enabled
            if (autoFindPlayer && target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
                else
                {
                    // Try to find PlayerController
                    DustOfWar.Player.PlayerController playerController = FindFirstObjectByType<DustOfWar.Player.PlayerController>();
                    if (playerController != null)
                    {
                        target = playerController.transform;
                    }
                }
            }
        }

        private void Start()
        {
            if (!followOnStart && target != null)
            {
                // Set initial position without smoothing
                transform.position = target.position + offset;
            }
        }

        private void LateUpdate()
        {
            if (!isFollowing || target == null) return;

            // Calculate desired position
            Vector3 targetPosition = target.position + offset;

            // Apply bounds if enabled
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
            }

            // Apply smoothing or snap to position
            if (useSmoothing)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / smoothSpeed);
            }
            else
            {
                transform.position = targetPosition;
            }
        }

        /// <summary>
        /// Set the target to follow
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Enable or disable camera following
        /// </summary>
        public void SetFollowing(bool following)
        {
            isFollowing = following;
        }

        /// <summary>
        /// Set camera offset
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        /// <summary>
        /// Set camera smoothing speed
        /// </summary>
        public void SetSmoothSpeed(float speed)
        {
            smoothSpeed = Mathf.Max(0.1f, speed);
        }

        /// <summary>
        /// Set camera bounds
        /// </summary>
        public void SetBounds(float minX, float maxX, float minY, float maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            useBounds = true;
        }

        /// <summary>
        /// Disable bounds
        /// </summary>
        public void DisableBounds()
        {
            useBounds = false;
        }

        /// <summary>
        /// Set camera orthographic size
        /// </summary>
        public void SetOrthographicSize(float size)
        {
            orthographicSize = Mathf.Max(1f, size);
            if (cam != null)
            {
                cam.orthographicSize = orthographicSize;
            }
        }

        /// <summary>
        /// Get current target
        /// </summary>
        public Transform GetTarget()
        {
            return target;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw camera bounds
            if (useBounds)
            {
                Gizmos.color = Color.cyan;
                Vector3 bottomLeft = new Vector3(minX, minY, transform.position.z);
                Vector3 bottomRight = new Vector3(maxX, minY, transform.position.z);
                Vector3 topLeft = new Vector3(minX, maxY, transform.position.z);
                Vector3 topRight = new Vector3(maxX, maxY, transform.position.z);

                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomRight, topRight);
                Gizmos.DrawLine(topRight, topLeft);
                Gizmos.DrawLine(topLeft, bottomLeft);
            }
        }
    }
}

