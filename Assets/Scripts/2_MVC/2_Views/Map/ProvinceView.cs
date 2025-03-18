using UnityEngine;

// ProvinceView.cs
public class ProvinceView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Border Settings")]
    [SerializeField] private bool showProvinceBorder = true;
    [SerializeField] private Color provinceBorderColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private float borderWidth = 0.05f;
    
    [Header("Terrain Colors")]
    [SerializeField] private Color plainsColor = new Color(0.4f, 0.8f, 0.4f);
    [SerializeField] private Color hillsColor = new Color(0.6f, 0.7f, 0.3f);
    [SerializeField] private Color mountainsColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color forestColor = new Color(0.2f, 0.6f, 0.2f);
    [SerializeField] private Color desertColor = new Color(0.9f, 0.9f, 0.6f);
    [SerializeField] private Color waterColor = new Color(0.2f, 0.4f, 0.8f);
    
    private LineRenderer lineRenderer;
    private ProvinceModel model;

    public void Initialize(ProvinceModel provinceModel)
    {
        model = provinceModel;
        
        // Set up rendering components
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (showProvinceBorder)
        {
            SetupProvinceBorder();
        }
        
        // Set the sorting layer
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "Provinces";
            spriteRenderer.sortingOrder = 0;
        }
        
        // Subscribe to model changes
        ProvinceModel.OnOwnershipChanged += HandleOwnershipChanged;
        
        // Initial visual update
        UpdateTerrainVisuals();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        ProvinceModel.OnOwnershipChanged -= HandleOwnershipChanged;
    }
    
    private void HandleOwnershipChanged(ProvinceModel provinceModel, Nation oldOwner, Nation newOwner)
    {
        if (provinceModel == model)
        {
            UpdateBorderColor(newOwner?.nationColor ?? provinceBorderColor);
        }
    }
    
    // Visual setup methods
    private void SetupProvinceBorder()
    {
        // Get or add LineRenderer component
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Configure the line renderer
        lineRenderer.positionCount = 5; // Square has 5 points (closing the loop)
        lineRenderer.startWidth = borderWidth;
        lineRenderer.endWidth = borderWidth;
        lineRenderer.useWorldSpace = false; // Use local coordinates
        lineRenderer.loop = true; // Create a closed shape
        
        // Make sure the line renderer is in front of the sprite
        lineRenderer.sortingLayerName = "Provinces";
        lineRenderer.sortingOrder = 1; // One higher than the province sprite
        
        // Set the color
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = provinceBorderColor;
        lineRenderer.endColor = provinceBorderColor;
        
        // Get the sprite bounds
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            float halfWidth = spriteSize.x / 2;
            float halfHeight = spriteSize.y / 2;
            
            // Define the square outline slightly inside the sprite edges
            float inset = borderWidth / 2; // Inset a bit to avoid edge artifacts
            
            lineRenderer.SetPosition(0, new Vector3(-halfWidth + inset, -halfHeight + inset, -0.01f));
            lineRenderer.SetPosition(1, new Vector3(halfWidth - inset, -halfHeight + inset, -0.01f));
            lineRenderer.SetPosition(2, new Vector3(halfWidth - inset, halfHeight - inset, -0.01f));
            lineRenderer.SetPosition(3, new Vector3(-halfWidth + inset, halfHeight - inset, -0.01f));
            lineRenderer.SetPosition(4, new Vector3(-halfWidth + inset, -halfHeight + inset, -0.01f));
        }
    }
    
    private void UpdateTerrainVisuals()
    {
        if (spriteRenderer == null) return;
        
        // Set color based on terrain type
        switch (model.TerrainType)
        {
            case TerrainType.Plains:
                spriteRenderer.color = plainsColor;
                break;
            case TerrainType.Hills:
                spriteRenderer.color = hillsColor;
                break;
            case TerrainType.Mountains:
                spriteRenderer.color = mountainsColor;
                break;
            case TerrainType.Forest:
                spriteRenderer.color = forestColor;
                break;
            case TerrainType.Desert:
                spriteRenderer.color = desertColor;
                break;
            case TerrainType.Water:
                spriteRenderer.color = waterColor;
                break;
        }
    }
    
    
    public void UpdateBorderColor(Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }
}