using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float edgeScrollThreshold = 20f;
    [SerializeField] private bool useEdgeScrolling = false;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 2f;  // Maximum zoom in (smaller value = closer)
    [SerializeField] private float maxZoom = 12f; // Maximum zoom out (larger value = farther)
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.Linear(0, 0, 1, 1); // Optional: for smoother zoom
    
    [Header("Boundaries")]
    [SerializeField] private float horizontalLimit = 15f;
    [SerializeField] private float verticalLimit = 10f;
    [SerializeField] private bool useBackgroundAsBounds = true;

    [Header("Visual Settings")]
    [SerializeField] private Color backgroundColor = Color.white;

    [SerializeField] private bool useCustomBackgroundColor = false;
    
    private Camera mainCamera;
    private Vector3 targetPosition;
    private float cameraHeight;
    private float cameraWidth;
    private float targetZoom;
    
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        targetPosition = transform.position;
        
        // Initialize zoom to current orthographic size
        if (mainCamera.orthographic)
        {
            targetZoom = mainCamera.orthographicSize;
        }
        
        UpdateCameraDimensions();
        
        // Apply custom background color if enabled
        if (useCustomBackgroundColor && mainCamera != null)
        {
            SetCameraBackgroundColor(backgroundColor);
        }
    }

    void Update()
    {
        // Process keyboard input for movement
        HandleKeyboardInput();
        
        // Process mouse wheel for zooming
        HandleZoomInput();
        
        // Process edge scrolling (optional)
        if (useEdgeScrolling)
        {
            HandleEdgeScrolling();
        }
        
        // Apply zoom
        ApplyZoom();
        
        // Apply movement with boundaries (called after zoom to use updated camera dimensions)
        MoveCamera();
    }
    
    private void HandleKeyboardInput()
    {
        // Get input axes
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Calculate movement direction
        Vector3 direction = new Vector3(horizontal, vertical, 0).normalized;
        
        // Update target position based on input
        if (direction.magnitude >= 0.1f)
        {
            targetPosition += direction * moveSpeed * Time.deltaTime;
        }
    }
    
    private void HandleEdgeScrolling()
    {
        // Get mouse position
        Vector3 mousePosition = Input.mousePosition;
        
        // Calculate screen edges
        float leftEdge = edgeScrollThreshold;
        float rightEdge = Screen.width - edgeScrollThreshold;
        float bottomEdge = edgeScrollThreshold;
        float topEdge = Screen.height - edgeScrollThreshold;
        
        // Check if mouse is near screen edges
        if (mousePosition.x < leftEdge)
        {
            targetPosition += Vector3.left * moveSpeed * Time.deltaTime;
        }
        else if (mousePosition.x > rightEdge)
        {
            targetPosition += Vector3.right * moveSpeed * Time.deltaTime;
        }
        
        if (mousePosition.y < bottomEdge)
        {
            targetPosition += Vector3.down * moveSpeed * Time.deltaTime;
        }
        else if (mousePosition.y > topEdge)
        {
            targetPosition += Vector3.up * moveSpeed * Time.deltaTime;
        }
    }
    
    private void HandleZoomInput()
    {
        // Get mouse wheel input
        float scrollDelta = Input.mouseScrollDelta.y;
        
        if (scrollDelta != 0)
        {
            // Adjust targetZoom based on scroll direction and zoom speed
            targetZoom -= scrollDelta * zoomSpeed;
            
            // Clamp zoom level between our min and max values
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            
            Debug.Log($"Zoom level adjusted to: {targetZoom}");
        }
    }
    
    private void ApplyZoom()
    {
        if (mainCamera.orthographic)
        {
            // Smoothly interpolate current orthographic size toward target zoom
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize, 
                targetZoom, 
                Time.deltaTime * 10f); // You can adjust 10f to control zoom smoothness
            
            // Update camera dimensions after zoom changes
            UpdateCameraDimensions();
        }
    }
    
    private void UpdateCameraDimensions()
    {
        if (mainCamera.orthographic)
        {
            // Calculate current camera dimensions in world units
            cameraHeight = mainCamera.orthographicSize * 2;
            cameraWidth = cameraHeight * mainCamera.aspect;
        }
    }
    
    private void MoveCamera()
    {
        // Get effective limits based on camera size
        float effectiveHorizontalLimit = horizontalLimit;
        float effectiveVerticalLimit = verticalLimit;
        
        // When using background as bounds, we need to subtract half the camera's view dimensions
        // This ensures the camera doesn't show beyond the background edges
        if (useBackgroundAsBounds)
        {
            effectiveHorizontalLimit -= cameraWidth / 2;
            effectiveVerticalLimit -= cameraHeight / 2;
            
            // Clamp limits to ensure they're valid
            effectiveHorizontalLimit = Mathf.Max(0, effectiveHorizontalLimit);
            effectiveVerticalLimit = Mathf.Max(0, effectiveVerticalLimit);
        }
        
        // Apply boundaries to target position
        targetPosition.x = Mathf.Clamp(targetPosition.x, -effectiveHorizontalLimit, effectiveHorizontalLimit);
        targetPosition.y = Mathf.Clamp(targetPosition.y, -effectiveVerticalLimit, effectiveVerticalLimit);
        
        // Set new position
        transform.position = new Vector3(
            targetPosition.x, 
            targetPosition.y, 
            transform.position.z
        );
    }
    
    // Call this from MapGenerator to automatically set camera boundaries based on map size
    public void SetBoundariesBasedOnMap(int mapWidth, int mapHeight, float tileSize)
    {
        // Set limits based on map size plus some margin
        horizontalLimit = (mapWidth * tileSize / 2f) + 1f;
        verticalLimit = (mapHeight * tileSize / 2f) + 1f;
        
        Debug.Log($"Camera boundaries set to: horizontal ±{horizontalLimit}, vertical ±{verticalLimit}");
    }
    
    // Call this from BackgroundManager to set camera boundaries based on background dimensions
    public void SetCameraLimitsFromBackground(SpriteRenderer background)
    {
        if (background == null || background.sprite == null) return;
        
        // Calculate the actual world-space size of the background
        Vector2 backgroundSize = background.sprite.bounds.size;
        Vector2 worldSize = Vector2.Scale(backgroundSize, background.transform.localScale);
        
        // Set boundaries based on the scaled background size
        horizontalLimit = worldSize.x / 2;
        verticalLimit = worldSize.y / 2;
        
        Debug.Log($"Camera boundaries set from background: ±{horizontalLimit} horizontal, ±{verticalLimit} vertical");
    }

    // Method to change the camera background color
    public void SetCameraBackgroundColor(Color color)
    {
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = color;
            Debug.Log($"Camera background color set to {color}");
        }
    }
    
    // Optional: Public method to zoom to a specific level (useful for UI buttons or gameplay events)
    public void ZoomTo(float zoomLevel)
    {
        targetZoom = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
    }
    
    // Optional: Public methods for zoom in/out buttons
    public void ZoomIn()
    {
        targetZoom = Mathf.Max(targetZoom - zoomSpeed, minZoom);
    }
    
    public void ZoomOut()
    {
        targetZoom = Mathf.Min(targetZoom + zoomSpeed, maxZoom);
    }
}
