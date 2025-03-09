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
    
    public int x { get; private set; }
    public int y { get; private set; }
    
    // Property with custom getter/setter to update the serialized field
    public Nation ownerNation 
    { 
        get { return _ownerNation; }
        private set { _ownerNation = value; }
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
        
        // Set province color to green
        if (spriteRenderer != null)
        {
            spriteRenderer.color = provinceColor;
            
            // Set the sorting layer and order to ensure it's above the background
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
    
    public void SetOwner(Nation nation)
    {
        Nation previousOwner = ownerNation;
        ownerNation = nation;
        
        // No change to the tile color - it stays green
        
        // Trigger event for territory border update
        if (previousOwner != nation)
        {
            OnOwnershipChanged?.Invoke(this, previousOwner, nation);
            Debug.Log($"Province at ({x},{y}) ownership changed from {(previousOwner?.nationName ?? "none")} to {(nation?.nationName ?? "none")}");
        }
    }
    
    // Simple method for collecting resources
    public int CollectResources()
    {
        int collected = resources;
        resources = 0; // Reset resources
        return collected;
    }
    
    // Reset resources at end of turn
    public void RegenerateResources()
    {
        resources = 10;
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