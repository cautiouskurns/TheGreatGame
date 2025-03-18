// Province.cs
using UnityEngine;

public class Province : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color provinceColor = new Color(0.4f, 0.8f, 0.4f); // Default green color for all provinces
    
    [Header("Border Settings")]
    [SerializeField] private bool showProvinceBorder = true;
    [SerializeField] private Color provinceBorderColor = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark gray border
    [SerializeField] private float borderWidth = 0.05f; // Border width in world units
    
    private LineRenderer lineRenderer;
    
    [Header("Ownership")]
    [SerializeField] private Nation _ownerNation = null; // Will be visible in inspector
    
    [Header("Terrain")]
    [SerializeField] private TerrainType _terrainType = TerrainType.Plains;
    [SerializeField] private Color plainsColor = new Color(0.4f, 0.8f, 0.4f);
    [SerializeField] private Color hillsColor = new Color(0.6f, 0.7f, 0.3f);
    [SerializeField] private Color mountainsColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color forestColor = new Color(0.2f, 0.6f, 0.2f);
    [SerializeField] private Color desertColor = new Color(0.9f, 0.9f, 0.6f);
    [SerializeField] private Color waterColor = new Color(0.2f, 0.4f, 0.8f);
    
    // Resource values for different terrain types
    private int[] terrainResourceValues = new int[6] 
    {
        15, // Plains
        12, // Hills
        8,  // Mountains
        10, // Forest
        5,  // Desert
        20  // Water (coastal trade)
    };
    
    // Whether settlements can be built on this terrain type
    private bool[] terrainSettlementAllowed = new bool[6]
    {
        true,  // Plains
        true,  // Hills
        false, // Mountains
        true,  // Forest
        true,  // Desert
        false  // Water
    };
    
    public Settlement settlement; // Reference to a settlement if one exists on this province
    
    public int x { get; private set; }
    public int y { get; private set; }
    
    // Property with custom getter/setter to update the serialized field
    public Nation ownerNation 
    { 
        get { return _ownerNation; }
        private set { _ownerNation = value; }
    }
    
    // Terrain type property
    public TerrainType terrainType
    {
        get { return _terrainType; }
        private set 
        { 
            _terrainType = value;
            UpdateTerrainVisuals();
        }
    }
    
    public int resources = 10; // Simple resource value for now
    
    // Event triggered when ownership changes
    public delegate void OwnershipChanged(Province province, Nation oldOwner, Nation newOwner);
    public static event OwnershipChanged OnOwnershipChanged;
    
    public void Initialize(int xPos, int yPos)
    {
        x = xPos;
        y = yPos;
        
        // Verify SpriteRenderer is assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError($"Province at ({x},{y}) couldn't find SpriteRenderer!");
            }
        }
        
        // Set up the line renderer for province borders if needed
        if (showProvinceBorder)
        {
            SetupProvinceBorder();
        }
        
        // Set initial terrain visuals (will use Plains by default)
        UpdateTerrainVisuals();
        
        // Set the sorting layer and order to ensure it's above the background
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "Provinces";
            spriteRenderer.sortingOrder = 0;
        }
    }
    
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
    
    // Method to set terrain type
    public void SetTerrainType(TerrainType type)
    {
        terrainType = type;
    }
    
    // Update visuals based on terrain type
    private void UpdateTerrainVisuals()
    {
        if (spriteRenderer == null) return;
        
        // Set color based on terrain type
        switch (_terrainType)
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
    
    public void SetOwner(Nation nation)
    {
        Nation previousOwner = ownerNation;
        ownerNation = nation;
        
        // Trigger event for territory border update
        if (previousOwner != nation)
        {
            OnOwnershipChanged?.Invoke(this, previousOwner, nation);
        }
    }
    
    // Generate resources based on terrain type
    public void RegenerateResources()
    {
        // Get resource value based on terrain type
        resources = terrainResourceValues[(int)_terrainType];
        
        // Add settlement bonus if a settlement exists here
        if (settlement != null)
        {
            resources += settlement.resourceBonus;
        }
    }
    
    // Simple method for collecting resources
    public int CollectResources()
    {
        int collected = resources;
        resources = 0; // Reset resources
        return collected;
    }
    
    // Check if a settlement can be built on this terrain
    public bool CanBuildSettlement()
    {
        return terrainSettlementAllowed[(int)_terrainType];
    }
    
    // Optional: Method to toggle border visibility
    public void SetBorderVisibility(bool visible)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = visible;
        }
    }
    
    // Optional: Method to change border color
    public void SetBorderColor(Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }
}