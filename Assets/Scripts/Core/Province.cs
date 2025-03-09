// Province.cs
using UnityEngine;

public class Province : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color neutralColor = Color.gray; // Default color for unowned provinces
    
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
        
        // Set initial color to neutral
        if (spriteRenderer != null)
        {
            spriteRenderer.color = neutralColor;
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
}