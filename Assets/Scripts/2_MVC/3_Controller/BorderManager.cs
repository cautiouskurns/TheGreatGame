// Assets/Scripts/Map/Border/BorderManager.cs
using System.Collections.Generic;
using UnityEngine;

public class BorderManager : MonoBehaviour
{
    [Header("Province Border Settings")]
    [SerializeField] private float provinceBorderWidth = 0.05f;
    [SerializeField] private Color defaultProvinceBorderColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    
    [Header("Nation Border Settings")]
    [SerializeField] private float nationBorderWidth = 0.15f;
    [SerializeField] private float nationBorderOffset = 0.15f;
    
    private Dictionary<Province, LineRenderer> provinceBorders = new Dictionary<Province, LineRenderer>();
    private Dictionary<Nation, LineRenderer> nationBorders = new Dictionary<Nation, LineRenderer>();
    
    void Awake()
    {
        ServiceLocator.Register<BorderManager>(this);
    }
    
    void OnEnable()
    {
        Province.OnOwnershipChanged += HandleProvinceOwnershipChanged;
    }
    
    void OnDisable()
    {
        Province.OnOwnershipChanged -= HandleProvinceOwnershipChanged;
    }
    
    public void CreateProvinceBorder(Province province)
    {
        if (province == null) return;
        
        GameObject borderObj = new GameObject($"Border_{province.x}_{province.y}");
        borderObj.transform.parent = province.transform;
        borderObj.transform.localPosition = Vector3.zero;
        
        LineRenderer lineRenderer = borderObj.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 5; // Square has 5 points (closing the loop)
        lineRenderer.startWidth = provinceBorderWidth;
        lineRenderer.endWidth = provinceBorderWidth;
        lineRenderer.useWorldSpace = false; // Use local coordinates
        lineRenderer.loop = true; // Create a closed shape
        
        // Make sure the line renderer is in front of the sprite
        lineRenderer.sortingLayerName = "Provinces";
        lineRenderer.sortingOrder = 1; // One higher than the province sprite
        
        // Set the color
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        SetLineRendererColor(lineRenderer, defaultProvinceBorderColor);
        
        // Get the sprite bounds from the province
        SpriteRenderer spriteRenderer = province.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            float halfWidth = spriteSize.x / 2;
            float halfHeight = spriteSize.y / 2;
            
            // Define the square outline slightly inside the sprite edges
            float inset = provinceBorderWidth / 2; // Inset a bit to avoid edge artifacts
            
            lineRenderer.SetPosition(0, new Vector3(-halfWidth + inset, -halfHeight + inset, -0.01f));
            lineRenderer.SetPosition(1, new Vector3(halfWidth - inset, -halfHeight + inset, -0.01f));
            lineRenderer.SetPosition(2, new Vector3(halfWidth - inset, halfHeight - inset, -0.01f));
            lineRenderer.SetPosition(3, new Vector3(-halfWidth + inset, halfHeight - inset, -0.01f));
            lineRenderer.SetPosition(4, new Vector3(-halfWidth + inset, -halfHeight + inset, -0.01f));
        }
        
        // Store the LineRenderer for later updates
        provinceBorders[province] = lineRenderer;
        
        // Set initial color based on ownership
        UpdateProvinceBorderColor(province);
    }
    
    public void UpdateProvinceBorderColor(Province province)
    {
        if (!provinceBorders.TryGetValue(province, out LineRenderer lineRenderer))
            return;
            
        if (province.ownerNation != null)
            SetLineRendererColor(lineRenderer, province.ownerNation.nationColor);
        else
            SetLineRendererColor(lineRenderer, defaultProvinceBorderColor);
    }
    
    public void RedrawNationBorders()
    {
        // Clear existing nation borders
        foreach (var renderer in nationBorders.Values)
            Destroy(renderer.gameObject);
        nationBorders.Clear();
            
        // Draw new borders for all nations
        Nation[] nations = FindObjectsByType<Nation>(FindObjectsSortMode.None);
        foreach (Nation nation in nations)
            DrawNationBorder(nation);
    }
    
    private void DrawNationBorder(Nation nation)
    {
        if (nation == null || nation.controlledProvinces.Count == 0)
        {
            // Remove any existing border for this nation
            if (nationBorders.TryGetValue(nation, out LineRenderer existingRenderer) && existingRenderer != null)
            {
                Destroy(existingRenderer.gameObject);
                nationBorders.Remove(nation);
            }
            return;
        }
        
        // Create or get LineRenderer for this nation
        LineRenderer lineRenderer;
        if (!nationBorders.TryGetValue(nation, out lineRenderer) || lineRenderer == null)
        {
            GameObject borderObj = new GameObject($"NationBorder_{nation.nationName}");
            borderObj.transform.parent = transform;
            lineRenderer = borderObj.AddComponent<LineRenderer>();
            ConfigureLineRenderer(lineRenderer, nation.nationColor);
            nationBorders[nation] = lineRenderer;
        }
        
        // Generate border points
        List<Vector2> borderPoints = GenerateNationBorderPoints(nation);
        
        // Apply the border to the line renderer
        if (borderPoints.Count > 1)
        {
            lineRenderer.positionCount = borderPoints.Count;
            for (int i = 0; i < borderPoints.Count; i++)
            {
                lineRenderer.SetPosition(i, new Vector3(borderPoints[i].x, borderPoints[i].y, -0.05f));
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }

    private void ConfigureLineRenderer(LineRenderer lineRenderer, Color color)
    {
        lineRenderer.startWidth = nationBorderWidth;
        lineRenderer.endWidth = nationBorderWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        
        // Set the material and color
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        Color borderColor = new Color(color.r, color.g, color.b, 0.9f);
        lineRenderer.startColor = borderColor;
        lineRenderer.endColor = borderColor;
        
        // Ensure it renders above provinces
        lineRenderer.sortingLayerName = "Provinces";
        lineRenderer.sortingOrder = 2;
    }

    private List<Vector2> GenerateNationBorderPoints(Nation nation)
    {
        // This is a simplified placeholder - you should implement proper border generation
        // based on your existing NationBorderManager logic
        List<Vector2> points = new List<Vector2>();
        
        // Find bounds of the territory
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        
        foreach (Province province in nation.controlledProvinces)
        {
            Vector3 position = province.transform.position;
            
            minX = Mathf.Min(minX, position.x - 0.5f);
            minY = Mathf.Min(minY, position.y - 0.5f);
            maxX = Mathf.Max(maxX, position.x + 0.5f);
            maxY = Mathf.Max(maxY, position.y + 0.5f);
        }
        
        // Add some padding
        float padding = nationBorderOffset;
        minX -= padding;
        minY -= padding;
        maxX += padding;
        maxY += padding;
        
        // Create a simple rectangle
        points.Add(new Vector2(minX, minY));
        points.Add(new Vector2(maxX, minY));
        points.Add(new Vector2(maxX, maxY));
        points.Add(new Vector2(minX, maxY));
        points.Add(new Vector2(minX, minY)); // Close the loop
        
        return points;
    }
    
    private void HandleProvinceOwnershipChanged(Province province, Nation oldOwner, Nation newOwner)
    {
        UpdateProvinceBorderColor(province);
        
        if (oldOwner != null)
            DrawNationBorder(oldOwner);
            
        if (newOwner != null && newOwner != oldOwner)
            DrawNationBorder(newOwner);
    }
    
    private void SetLineRendererColor(LineRenderer lineRenderer, Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}