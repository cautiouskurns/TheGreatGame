// Province.cs
using UnityEngine;

public class Province : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color neutralColor = Color.gray; // Default color for unowned provinces
    
    [Header("Border Settings")]
    [SerializeField] private bool showBorder = true;
    [SerializeField] private Color borderColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark gray border
    [SerializeField] private float borderWidth = 0.05f; // Border width in world units
    
    private LineRenderer lineRenderer;
    
    public int x { get; private set; }
    public int y { get; private set; }
    public Nation ownerNation { get; private set; }
    public int resources = 10; // Simple resource value for now
    
    public void Initialize(int xPos, int yPos)
    {
        x = xPos;
        y = yPos;
        
        // Verify SpriteRenderer is assigned
        if (spriteRenderer == null)
        {
            // Try to get it automatically if not assigned in inspector
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
            {
                Debug.LogError($"Province at ({x},{y}) couldn't find SpriteRenderer!");
            }
        }
        
        // Set up the line renderer for borders if needed
        if (showBorder)
        {
            SetupBorder();
        }
        
        // Set initial color to neutral
        if (spriteRenderer != null)
        {
            spriteRenderer.color = neutralColor;
            
            // Set the sorting layer and order to ensure it's above the background
            spriteRenderer.sortingLayerName = "Provinces";
            spriteRenderer.sortingOrder = 0;
        }
    }
    
    private void SetupBorder()
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
        lineRenderer.startColor = borderColor;
        lineRenderer.endColor = borderColor;
        
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
        ownerNation = nation;
        
        // Update visual appearance based on ownership
        if (spriteRenderer != null)
        {
            if (nation != null)
            {
                spriteRenderer.color = nation.nationColor;
                Debug.Log($"Province at ({x},{y}) color set to {nation.nationColor} for nation {nation.nationName}");
            }
            else
            {
                spriteRenderer.color = neutralColor; // Reset to neutral color if no owner
                Debug.Log($"Province at ({x},{y}) color set to neutral");
            }
        }
        else
        {
            Debug.LogError($"Province at ({x},{y}) is missing SpriteRenderer reference!");
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