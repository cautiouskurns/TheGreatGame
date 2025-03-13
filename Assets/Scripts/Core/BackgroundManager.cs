using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private int backgroundSortingOrder = -10;
    [SerializeField] private string backgroundSortingLayer = "Background";
    
    public enum BackgroundFitMode
    {
        PreserveOriginal,  // Keep original size and aspect ratio
        FitToScreen,       // Fit the entire image on screen, preserve aspect ratio
        FillScreen,        // Fill the entire screen, preserve aspect ratio (may crop)
        StretchToScreen    // Stretch to fill screen, may distort aspect ratio
    }
    
    [SerializeField] private BackgroundFitMode fitMode = BackgroundFitMode.PreserveOriginal;
    [SerializeField] private float scaleFactor;  // Additional scale factor
    [SerializeField] private Vector2 offsetPosition = Vector2.zero;  // Offset from center
    
    void Start()
    {
        SetupBackground();
        
        // Apply the fixed scale immediately
        if (fitMode == BackgroundFitMode.PreserveOriginal)
        {
            ApplyFixedScale();
        }
    }
    
    private void SetupBackground()
    {
        if (backgroundRenderer != null)
        {
            // Set the sorting layer and order for the background
            backgroundRenderer.sortingLayerName = backgroundSortingLayer;
            backgroundRenderer.sortingOrder = backgroundSortingOrder;
            
            // Ensure it's positioned behind everything
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, pos.y, 1);
            
            Debug.Log($"Background set to sorting layer: {backgroundSortingLayer}, order: {backgroundSortingOrder}");
        }
        else
        {
            Debug.LogWarning("No background renderer assigned to BackgroundManager");
        }
    }
    
    // New method for keeping a fixed scale regardless of map size
    public void ApplyFixedScale()
    {
        if (backgroundRenderer == null) return;
        
        // Apply the fixed scale factor
        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        
        // Apply position offset
        transform.position = new Vector3(offsetPosition.x, offsetPosition.y, 1f);
        
        Debug.Log($"Background scale fixed at: {scaleFactor}");
        
        // Update camera bounds based on this background
        UpdateCameraBounds();
    }
    
    private void UpdateCameraBounds()
    {
        CameraController cameraController = FindAnyObjectByType<CameraController>();
        if (cameraController != null && backgroundRenderer != null)
        {
            cameraController.SetCameraLimitsFromBackground(backgroundRenderer);
        }
    }
    
    // Call this to handle the background's size and placement
    public void ScaleBackgroundToMap(int mapWidth, int mapHeight, float tileSize)
    {
        if (backgroundRenderer == null || backgroundRenderer.sprite == null) 
        {
            Debug.LogWarning("Background renderer or sprite is missing");
            return;
        }
        
        // If we want to preserve the original size with fixed scale,
        // just apply the fixed scale and return immediately
        if (fitMode == BackgroundFitMode.PreserveOriginal)
        {
            ApplyFixedScale();
            return; // Important: exit the method early to prevent any map-based scaling
        }
        
        // Calculate the size of the map in world units
        float mapWorldWidth = mapWidth * tileSize;
        float mapWorldHeight = mapHeight * tileSize;
        
        // Get background sprite dimensions
        Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;
        float spriteWidth = spriteSize.x;
        float spriteHeight = spriteSize.y;
        
        // Calculate scale based on fit mode
        float scaleX = 2.5f;
        float scaleY = 2.5f;
        
        switch (fitMode)
        {
            case BackgroundFitMode.FitToScreen:
                // Scale to fit the entire sprite within the map bounds (may have empty space)
                scaleX = mapWorldWidth / spriteWidth;
                scaleY = mapWorldHeight / spriteHeight;
                // Use the smaller scale to ensure it fits
                float fitScale = Mathf.Min(scaleX, scaleY);
                scaleX = fitScale;
                scaleY = fitScale;
                break;
                
            case BackgroundFitMode.FillScreen:
                // Scale to fill the entire map area (may crop the image)
                scaleX = mapWorldWidth / spriteWidth;
                scaleY = mapWorldHeight / spriteHeight;
                // Use the larger scale to ensure it fills
                float fillScale = Mathf.Max(scaleX, scaleY);
                scaleX = fillScale;
                scaleY = fillScale;
                break;
                
            case BackgroundFitMode.StretchToScreen:
                // Stretch to exactly match map dimensions (may distort)
                scaleX = mapWorldWidth / spriteWidth;
                scaleY = mapWorldHeight / spriteHeight;
                break;
        }
        
        // Apply the calculated scale with the additional scale factor
        transform.localScale = new Vector3(
            scaleX * scaleFactor,
            scaleY * scaleFactor,
            1f
        );
        
        // Apply any additional offset
        transform.position = new Vector3(offsetPosition.x, offsetPosition.y, 1f);
        
        Debug.Log($"Background scaled with mode: {fitMode}, scale: ({scaleX}, {scaleY})");
    }
}