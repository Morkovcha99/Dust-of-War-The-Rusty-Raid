using UnityEngine;
using UnityEngine.InputSystem;

namespace DustOfWar.Player
{
    /// <summary>
    /// Main player controller with movement and virtual joystick support
    /// Handles input from all sources (keyboard, mouse, gamepad, touch, virtual joystick)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float spriteRotationOffset = -90f; // Offset to align sprite (-90 = sprite faces up, 0 = sprite faces right)
        
        [Header("Boundaries")]
        [SerializeField] private float horizontalBoundaryLeft = -15f; // Left boundary for horizontal game
        [SerializeField] private float horizontalBoundaryRight = 15f; // Right boundary for horizontal game
        [SerializeField] private float verticalBoundaryTop = 5f; // Top boundary
        [SerializeField] private float verticalBoundaryBottom = -5f; // Bottom boundary
        
        [Header("Input Settings")]
        [SerializeField] private bool preferVirtualJoystick = false; // Set to false to use direct touch/mouse
        [SerializeField] private bool enableMouseInput = true;
        
        [Header("Touch/Mouse Input")]
        [SerializeField] private bool enableTouchInput = true;
        [SerializeField] private float inputDeadZone = 0.1f; // Minimum distance to start moving
        [SerializeField] private float maxInputDistance = 10f; // Max distance for full speed

        private Rigidbody2D rb;
        private DustOfWar.UI.VirtualJoystick virtualJoystick;
        
        private Vector2 currentVelocity;
        private Vector2 targetVelocity;
        private Vector2 inputDirection;
        private Vector2 currentInput = Vector2.zero;
        private bool isMovementEnabled = true;
        private bool isUsingTouch = false;
        private Camera mainCamera;

        // Events
        public System.Action<Vector2> OnMovementDirectionChanged;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            
            virtualJoystick = FindFirstObjectByType<DustOfWar.UI.VirtualJoystick>();
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }

        private void OnEnable()
        {
            // Subscribe to touch events
            if (enableTouchInput)
            {
                try
                {
                    UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
                }
                catch (System.Exception)
                {
                    // Input System may not be available in some contexts
                }
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from touch events safely
            if (enableTouchInput)
            {
                try
                {
                    UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnDeviceChange;
                }
                catch (System.Exception)
                {
                    // Input System may not be available in some contexts
                }
            }
        }

        private void Update()
        {
            if (!isMovementEnabled) return;

            ProcessInput();
            ApplyMovement();
            ApplyBoundaries();
            UpdateRotation();
        }

        private void ProcessInput()
        {
            Vector2 input = Vector2.zero;

            // Priority 1: Virtual Joystick (if available and preferred)
            if (preferVirtualJoystick && virtualJoystick != null && virtualJoystick.IsActive())
            {
                input = virtualJoystick.GetInput();
                isUsingTouch = true;
            }
            // Priority 2: Touch Input (move towards touch position)
            else if (enableTouchInput && Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                input = ProcessTouchInput();
                isUsingTouch = true;
            }
            // Priority 3: Mouse Input (move towards mouse position)
            else if (enableMouseInput && Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                input = ProcessMouseInput();
                isUsingTouch = false;
            }

            currentInput = input;
            inputDirection = Vector2.ClampMagnitude(input, 1f);
        }

        private Vector2 ProcessTouchInput()
        {
            if (Touchscreen.current == null || mainCamera == null) return Vector2.zero;

            var primaryTouch = Touchscreen.current.primaryTouch;
            if (!primaryTouch.press.isPressed) return Vector2.zero;

            // Get touch position in screen coordinates
            Vector2 screenPosition = primaryTouch.position.ReadValue();
            
            // Convert to world position
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));
            Vector2 targetPosition = new Vector2(worldPosition.x, worldPosition.y);
            
            // Calculate direction from player to touch position
            Vector2 direction = targetPosition - (Vector2)transform.position;
            float distance = direction.magnitude;

            // Apply dead zone
            if (distance < inputDeadZone)
            {
                return Vector2.zero;
            }

            // Normalize direction and apply distance scaling
            direction.Normalize();
            
            // Scale input strength based on distance (up to maxInputDistance)
            float inputStrength = Mathf.Clamp01(distance / maxInputDistance);
            
            return direction * inputStrength;
        }

        private Vector2 ProcessMouseInput()
        {
            if (Mouse.current == null || mainCamera == null) return Vector2.zero;
            if (!Mouse.current.leftButton.isPressed) return Vector2.zero;

            // Get mouse position in screen coordinates
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            
            // Convert to world position
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));
            Vector2 targetPosition = new Vector2(worldPosition.x, worldPosition.y);
            
            // Calculate direction from player to mouse position
            Vector2 direction = targetPosition - (Vector2)transform.position;
            float distance = direction.magnitude;

            // Apply dead zone
            if (distance < inputDeadZone)
            {
                return Vector2.zero;
            }

            // Normalize direction and apply distance scaling
            direction.Normalize();
            
            // Scale input strength based on distance (up to maxInputDistance)
            float inputStrength = Mathf.Clamp01(distance / maxInputDistance);
            
            return direction * inputStrength;
        }

        private void ApplyMovement()
        {
            targetVelocity = inputDirection * moveSpeed;

            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, 
                Time.deltaTime * (inputDirection.magnitude > 0.01f ? acceleration : deceleration));

            rb.linearVelocity = currentVelocity;
            
            // Notify listeners of movement direction change
            if (inputDirection.magnitude > 0.01f)
            {
                OnMovementDirectionChanged?.Invoke(inputDirection);
            }
        }

        private void UpdateRotation()
        {
            // Rotate vehicle to face movement direction
            if (inputDirection.magnitude > 0.01f)
            {
                // Calculate target angle based on movement direction
                float targetAngle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg;
                
                // Apply sprite rotation offset (to align sprite orientation)
                targetAngle += spriteRotationOffset;
                
                float currentAngle = transform.eulerAngles.z;
                
                // Smooth rotation towards target angle
                float angle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void ApplyBoundaries()
        {
            Vector3 position = transform.position;
            
            // Clamp position within boundaries (horizontal game)
            position.x = Mathf.Clamp(position.x, horizontalBoundaryLeft, horizontalBoundaryRight);
            position.y = Mathf.Clamp(position.y, verticalBoundaryBottom, verticalBoundaryTop);
            
            transform.position = position;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            // Handle device connection/disconnection
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed)
            {
                // Auto-detect if touch is available
                if (Touchscreen.current != null)
                {
                    preferVirtualJoystick = true;
                }
            }
        }

        /// <summary>
        /// Enable or disable movement
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            isMovementEnabled = enabled;
            if (!enabled)
            {
                rb.linearVelocity = Vector2.zero;
                currentVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Set movement speed
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0f, speed);
        }

        /// <summary>
        /// Get current movement speed
        /// </summary>
        public float GetMoveSpeed()
        {
            return moveSpeed;
        }

        /// <summary>
        /// Get current movement direction
        /// </summary>
        public Vector2 GetMovementDirection()
        {
            return inputDirection;
        }

        /// <summary>
        /// Get current input direction
        /// </summary>
        public Vector2 GetCurrentInput()
        {
            return currentInput;
        }

        /// <summary>
        /// Check if currently using touch input
        /// </summary>
        public bool IsUsingTouch()
        {
            return isUsingTouch;
        }

        /// <summary>
        /// Set whether to prefer virtual joystick input
        /// </summary>
        public void SetPreferVirtualJoystick(bool prefer)
        {
            preferVirtualJoystick = prefer;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw boundaries in editor (horizontal game)
            Gizmos.color = Color.yellow;
            
            // Top and bottom boundaries
            Gizmos.DrawLine(new Vector3(horizontalBoundaryLeft, verticalBoundaryTop, 0),
                          new Vector3(horizontalBoundaryRight, verticalBoundaryTop, 0));
            Gizmos.DrawLine(new Vector3(horizontalBoundaryLeft, verticalBoundaryBottom, 0),
                          new Vector3(horizontalBoundaryRight, verticalBoundaryBottom, 0));
            
            // Left and right boundaries
            Gizmos.DrawLine(new Vector3(horizontalBoundaryLeft, verticalBoundaryBottom, 0),
                          new Vector3(horizontalBoundaryLeft, verticalBoundaryTop, 0));
            Gizmos.DrawLine(new Vector3(horizontalBoundaryRight, verticalBoundaryBottom, 0),
                          new Vector3(horizontalBoundaryRight, verticalBoundaryTop, 0));
        }
    }
}

