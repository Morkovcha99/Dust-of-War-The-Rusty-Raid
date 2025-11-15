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
        
        [Header("Boundaries")]
        [SerializeField] private float horizontalBoundary = 8f;
        [SerializeField] private float verticalBoundaryTop = 5f;
        [SerializeField] private float verticalBoundaryBottom = -5f;
        
        [Header("Input Settings")]
        [SerializeField] private bool preferVirtualJoystick = true;
        
        [Header("Touch Input")]
        [SerializeField] private bool enableTouchInput = true;
        [SerializeField] private float touchSensitivity = 1f;

        private Rigidbody2D rb;
        private DustOfWar.UI.VirtualJoystick virtualJoystick;
        
        private Vector2 currentVelocity;
        private Vector2 targetVelocity;
        private Vector2 inputDirection;
        private Vector2 currentInput = Vector2.zero;
        private bool isMovementEnabled = true;
        private bool isUsingTouch = false;
        private int activeTouchId = -1;
        private Vector2 touchReferencePosition = Vector2.zero;

        // Events
        public System.Action<Vector2> OnMovementDirectionChanged;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            
            virtualJoystick = FindFirstObjectByType<DustOfWar.UI.VirtualJoystick>();
        }

        private void OnEnable()
        {
            // Subscribe to touch events
            if (enableTouchInput)
            {
                InputSystem.onDeviceChange += OnDeviceChange;
            }
        }

        private void OnDisable()
        {
            if (enableTouchInput)
            {
                InputSystem.onDeviceChange -= OnDeviceChange;
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
            // Priority 2: Touch Input (direct touch position)
            else if (enableTouchInput && Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                input = ProcessTouchInput();
                isUsingTouch = true;
            }

            currentInput = input;
            inputDirection = Vector2.ClampMagnitude(input, 1f);
        }

        private Vector2 ProcessTouchInput()
        {
            if (Touchscreen.current == null) return Vector2.zero;

            var primaryTouch = Touchscreen.current.primaryTouch;

            if (!primaryTouch.press.isPressed)
            {
                activeTouchId = -1;
                return Vector2.zero;
            }

            // Get touch position
            Vector2 touchPosition = primaryTouch.position.ReadValue();
            
            // Initialize reference position on first touch
            if (activeTouchId == -1)
            {
                touchReferencePosition = touchPosition;
                activeTouchId = primaryTouch.touchId.ReadValue();
            }

            // Calculate direction from reference to current touch position
            Vector2 direction = (touchPosition - touchReferencePosition) * touchSensitivity / Screen.width;
            
            return Vector2.ClampMagnitude(direction, 1f);
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
                float targetAngle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                
                // Smooth rotation towards target angle
                float angle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void ApplyBoundaries()
        {
            Vector3 position = transform.position;
            
            // Clamp position within boundaries
            position.x = Mathf.Clamp(position.x, -horizontalBoundary, horizontalBoundary);
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
            // Draw boundaries in editor
            Gizmos.color = Color.yellow;
            
            // Top and bottom boundaries
            Gizmos.DrawLine(new Vector3(-horizontalBoundary, verticalBoundaryTop, 0),
                          new Vector3(horizontalBoundary, verticalBoundaryTop, 0));
            Gizmos.DrawLine(new Vector3(-horizontalBoundary, verticalBoundaryBottom, 0),
                          new Vector3(horizontalBoundary, verticalBoundaryBottom, 0));
            
            // Left and right boundaries
            Gizmos.DrawLine(new Vector3(-horizontalBoundary, verticalBoundaryBottom, 0),
                          new Vector3(-horizontalBoundary, verticalBoundaryTop, 0));
            Gizmos.DrawLine(new Vector3(horizontalBoundary, verticalBoundaryBottom, 0),
                          new Vector3(horizontalBoundary, verticalBoundaryTop, 0));
        }
    }
}

