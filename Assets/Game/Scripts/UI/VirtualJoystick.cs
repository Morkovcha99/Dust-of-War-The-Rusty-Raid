using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DustOfWar.UI
{
    /// <summary>
    /// Virtual joystick UI component for touch input
    /// Supports both fixed position and dynamic (follows touch) modes
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Joystick Settings")]
        [SerializeField] private bool isFixedPosition = true;
        [SerializeField] private float joystickRange = 100f;
        [SerializeField] private float deadZone = 0.1f;
        [SerializeField] private bool snapToDirection = false;
        [SerializeField] private int snapDirections = 8; // 4 for cardinal, 8 for cardinal + diagonal
        
        [Header("Visual Settings")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image handleImage;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private bool hideWhenInactive = false;
        [SerializeField] private float hideDelay = 1f;
        
        [Header("Smooth Movement")]
        [SerializeField] private bool useSmoothing = true;
        [SerializeField] private float smoothingSpeed = 10f;

        private RectTransform backgroundRect;
        private RectTransform handleRect;
        private Canvas parentCanvas;
        private Vector2 joystickCenter;
        private Vector2 currentInput = Vector2.zero;
        private Vector2 targetInput = Vector2.zero;
        private bool isActive = false;
        private float hideTimer = 0f;

        // Events
        public System.Action<Vector2> OnInputChanged;
        public System.Action OnJoystickActivated;
        public System.Action OnJoystickDeactivated;

        private void Awake()
        {
            // Get references
            if (backgroundImage != null)
            {
                backgroundRect = backgroundImage.GetComponent<RectTransform>();
            }
            else
            {
                backgroundRect = GetComponent<RectTransform>();
            }

            if (handleImage != null)
            {
                handleRect = handleImage.GetComponent<RectTransform>();
            }
            else if (transform.childCount > 0)
            {
                handleRect = transform.GetChild(0).GetComponent<RectTransform>();
            }

            parentCanvas = GetComponentInParent<Canvas>();
            
            // Initialize joystick center
            if (isFixedPosition)
            {
                joystickCenter = backgroundRect.anchoredPosition;
            }
            
            // Set initial visual state
            UpdateVisualState(false);
        }

        private void Update()
        {
            // Handle smoothing
            if (useSmoothing && !isActive)
            {
                targetInput = Vector2.zero;
                currentInput = Vector2.Lerp(currentInput, targetInput, smoothingSpeed * Time.deltaTime);
                
                if (currentInput.magnitude < deadZone)
                {
                    currentInput = Vector2.zero;
                }
                
                UpdateHandlePosition();
                
                // Handle hiding
                if (hideWhenInactive && isActive == false)
                {
                    hideTimer += Time.deltaTime;
                    if (hideTimer >= hideDelay)
                    {
                        SetJoystickVisible(false);
                    }
                }
            }
            
            // Notify listeners of input change
            if (currentInput.magnitude > deadZone || isActive)
            {
                OnInputChanged?.Invoke(currentInput);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            SetJoystickActive(true);
            
            if (!isFixedPosition)
            {
                // Move joystick to touch position
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform,
                    eventData.position,
                    parentCanvas.worldCamera,
                    out localPoint);
                
                joystickCenter = localPoint;
                backgroundRect.anchoredPosition = joystickCenter;
            }
            
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isActive) return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                backgroundRect,
                eventData.position,
                parentCanvas.worldCamera,
                out localPoint);

            Vector2 input = localPoint - joystickCenter;
            
            // Apply range limit
            if (input.magnitude > joystickRange)
            {
                input = input.normalized * joystickRange;
            }

            // Apply snap to direction if enabled
            if (snapToDirection && input.magnitude > deadZone * joystickRange)
            {
                float angle = Mathf.Atan2(input.y, input.x);
                float snapAngle = Mathf.Round(angle / (2f * Mathf.PI / snapDirections)) * (2f * Mathf.PI / snapDirections);
                input = new Vector2(Mathf.Cos(snapAngle), Mathf.Sin(snapAngle)) * input.magnitude;
            }

            // Normalize input based on range
            float magnitude = input.magnitude / joystickRange;
            if (magnitude < deadZone)
            {
                targetInput = Vector2.zero;
            }
            else
            {
                // Apply dead zone curve
                magnitude = (magnitude - deadZone) / (1f - deadZone);
                targetInput = input.normalized * magnitude;
            }

            if (!useSmoothing)
            {
                currentInput = targetInput;
            }
            else
            {
                currentInput = Vector2.Lerp(currentInput, targetInput, smoothingSpeed * Time.deltaTime);
            }

            UpdateHandlePosition();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            SetJoystickActive(false);
            targetInput = Vector2.zero;
            
            if (!useSmoothing)
            {
                currentInput = Vector2.zero;
                UpdateHandlePosition();
            }
        }

        private void UpdateHandlePosition()
        {
            if (handleRect == null) return;

            Vector2 handlePosition = joystickCenter + (currentInput * joystickRange);
            handleRect.anchoredPosition = handlePosition;
        }

        private void SetJoystickActive(bool active)
        {
            if (isActive == active) return;

            isActive = active;
            UpdateVisualState(active);

            if (active)
            {
                OnJoystickActivated?.Invoke();
                hideTimer = 0f;
            }
            else
            {
                OnJoystickDeactivated?.Invoke();
                hideTimer = 0f;
            }
        }

        private void UpdateVisualState(bool active)
        {
            Color color = active ? activeColor : inactiveColor;

            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }

            if (handleImage != null)
            {
                handleImage.color = color;
            }

            if (hideWhenInactive)
            {
                SetJoystickVisible(active || hideTimer < hideDelay);
            }
        }

        private void SetJoystickVisible(bool visible)
        {
            float alpha = visible ? 1f : 0f;
            Color bgColor = backgroundImage != null ? backgroundImage.color : Color.white;
            Color handleColor = handleImage != null ? handleImage.color : Color.white;

            bgColor.a = alpha;
            handleColor.a = alpha;

            if (backgroundImage != null)
            {
                backgroundImage.color = bgColor;
            }

            if (handleImage != null)
            {
                handleImage.color = handleColor;
            }
        }

        /// <summary>
        /// Get current joystick input (normalized)
        /// </summary>
        public Vector2 GetInput()
        {
            return currentInput;
        }

        /// <summary>
        /// Check if joystick is currently active
        /// </summary>
        public bool IsActive()
        {
            return isActive || currentInput.magnitude > deadZone;
        }

        /// <summary>
        /// Reset joystick to center
        /// </summary>
        public void ResetJoystick()
        {
            currentInput = Vector2.zero;
            targetInput = Vector2.zero;
            SetJoystickActive(false);
            UpdateHandlePosition();
        }

        /// <summary>
        /// Set joystick range
        /// </summary>
        public void SetJoystickRange(float range)
        {
            joystickRange = Mathf.Max(10f, range);
        }

        /// <summary>
        /// Set joystick position (for fixed position mode)
        /// </summary>
        public void SetJoystickPosition(Vector2 position)
        {
            joystickCenter = position;
            if (backgroundRect != null)
            {
                backgroundRect.anchoredPosition = position;
            }
        }
    }
}

