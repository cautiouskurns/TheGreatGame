using System.Collections.Generic;
using UnityEngine;

public class NationBorderManager : MonoBehaviour
{
    [SerializeField] private float borderWidth = 0.15f;
    [SerializeField] private float borderOffset = 0.05f; // Offset from tile edges
    [SerializeField] private bool useSimpleOutlines = false; // Set to true to use simpler box outlines
    [SerializeField] private bool debugDrawGrid = false; // For debugging
    
    private Dictionary<Nation, LineRenderer> nationBorders = new Dictionary<Nation, LineRenderer>();
    private MapGenerator mapGenerator;
    private float tileSize = 1f; // Default tile size
    
    // Direction vectors for checking neighbors (right, up, left, down)
    private static readonly int[] dx = { 1, 0, -1, 0 };
    private static readonly int[] dy = { 0, 1, 0, -1 };
    
    void OnEnable()
    {
        Province.OnOwnershipChanged += OnProvinceOwnershipChanged;
    }
    
    void OnDisable()
    {
        Province.OnOwnershipChanged -= OnProvinceOwnershipChanged;
    }
    
    void Start()
    {
        mapGenerator = FindAnyObjectByType<MapGenerator>();
        if (mapGenerator != null && mapGenerator.provinces != null && mapGenerator.provinces.Length > 0)
        {
            // Estimate tile size based on the distance between provinces
            if (mapGenerator.provinces.GetLength(0) > 1)
            {
                Province p1 = mapGenerator.provinces[0, 0];
                Province p2 = mapGenerator.provinces[1, 0];
                if (p1 != null && p2 != null)
                {
                    tileSize = Vector3.Distance(p1.transform.position, p2.transform.position);
                }
            }
        }
        
        RedrawAllNationBorders();
    }
    
    private void OnProvinceOwnershipChanged(Province province, Nation oldOwner, Nation newOwner)
    {
        if (oldOwner != null)
        {
            UpdateNationBorder(oldOwner);
        }
        
        if (newOwner != null && newOwner != oldOwner)
        {
            UpdateNationBorder(newOwner);
        }
    }
    
    public void RedrawAllNationBorders()
    {
        // Clear existing borders
        foreach (var renderer in nationBorders.Values)
        {
            if (renderer != null && renderer.gameObject != null)
            {
                Destroy(renderer.gameObject);
            }
        }
        nationBorders.Clear();
        
        // Draw new borders for all nations
        Nation[] nations = FindObjectsByType<Nation>(FindObjectsSortMode.None);
        foreach (Nation nation in nations)
        {
            UpdateNationBorder(nation);
        }
    }
    
    private void UpdateNationBorder(Nation nation)
    {
        if (nation.controlledProvinces == null || nation.controlledProvinces.Count == 0)
        {
            // Remove any existing border for this nation
            if (nationBorders.ContainsKey(nation) && nationBorders[nation] != null)
            {
                Destroy(nationBorders[nation].gameObject);
                nationBorders.Remove(nation);
            }
            return;
        }
        
        // Create or get LineRenderer for this nation
        LineRenderer lineRenderer;
        if (!nationBorders.TryGetValue(nation, out lineRenderer) || lineRenderer == null)
        {
            GameObject borderObj = new GameObject($"Border_{nation.nationName}");
            borderObj.transform.parent = transform;
            lineRenderer = borderObj.AddComponent<LineRenderer>();
            ConfigureLineRenderer(lineRenderer, nation.nationColor);
            nationBorders[nation] = lineRenderer;
        }
        
        // Generate the border points
        List<Vector2> borderPoints;
        if (useSimpleOutlines)
        {
            borderPoints = GenerateSimpleBorder(nation);
        }
        else
        {
            borderPoints = GenerateContourBorder(nation);
        }
        
        // Apply points to the line renderer
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
            // If we couldn't generate proper border points, hide the line renderer
            lineRenderer.positionCount = 0;
        }
    }
    
    private void ConfigureLineRenderer(LineRenderer lineRenderer, Color color)
    {
        lineRenderer.startWidth = borderWidth;
        lineRenderer.endWidth = borderWidth;
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
    
    // New contour-based border generation that follows the actual shape
    private List<Vector2> GenerateContourBorder(Nation nation)
    {
        if (mapGenerator == null || mapGenerator.provinces == null)
            return new List<Vector2>();
        
        int mapWidth = mapGenerator.provinces.GetLength(0);
        int mapHeight = mapGenerator.provinces.GetLength(1);
        
        // Create a grid to mark which cells are owned by this nation
        bool[,] owned = new bool[mapWidth, mapHeight];
        
        foreach (Province province in nation.controlledProvinces)
        {
            if (province.x >= 0 && province.x < mapWidth && province.y >= 0 && province.y < mapHeight)
            {
                owned[province.x, province.y] = true;
            }
        }
        
        // Create a larger grid for edge detection (double resolution)
        int gridWidth = mapWidth * 2;
        int gridHeight = mapHeight * 2;
        bool[,] grid = new bool[gridWidth, gridHeight];
        
        // Fill the high-resolution grid
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (owned[x, y])
                {
                    // Each province fills 2x2 cells in the high-res grid
                    grid[x*2, y*2] = true;
                    grid[x*2+1, y*2] = true;
                    grid[x*2, y*2+1] = true;
                    grid[x*2+1, y*2+1] = true;
                }
            }
        }
        
        // Debug draw the grid
        if (debugDrawGrid)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Color debugColor = grid[x, y] ? Color.green : Color.red;
                    Debug.DrawRay(GridToWorld(x, y, gridWidth, gridHeight), Vector3.up * 0.1f, debugColor, 5f);
                }
            }
        }
        
        // Find contour edges - these are edges between owned and unowned cells
        List<Vector2> contourPoints = new List<Vector2>();
        bool[,] visited = new bool[gridWidth, gridHeight];
        
        // Find a starting edge cell - look for the leftmost then bottommost owned cell
        int startX = -1, startY = -1;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] && ((x == 0 || !grid[x-1, y]) || (y == 0 || !grid[x, y-1])))
                {
                    startX = x;
                    startY = y;
                    break;
                }
            }
            if (startX != -1) break;
        }
        
        if (startX == -1) // No edge found
            return GenerateSimpleBorder(nation); // Fall back to simple border
        
        // Trace the contour
        Vector2 startPoint = GridToWorld(startX, startY, gridWidth, gridHeight);
        contourPoints.Add(startPoint);
        int x1 = startX;
        int y1 = startY;
        visited[x1, y1] = true;
        
        // Follow the contour using the right-hand rule
        int dir = 0; // 0=right, 1=up, 2=left, 3=down
        bool foundPath = false;
        int maxSteps = gridWidth * gridHeight * 2; // Prevent infinite loops
        int steps = 0;
        
        while (steps < maxSteps)
        {
            steps++;
            
            // Try turning right first (relative to current direction)
            int newDir = (dir + 3) % 4;
            int newX = x1 + dx[newDir];
            int newY = y1 + dy[newDir];
            
            // Check if we can go this way
            if (IsValidGridCell(newX, newY, gridWidth, gridHeight) && grid[newX, newY])
            {
                // We can go right, so turn right and move
                dir = newDir;
                x1 = newX;
                y1 = newY;
                
                if (!visited[x1, y1])
                {
                    visited[x1, y1] = true;
                    Vector2 worldPos = GridToWorld(x1, y1, gridWidth, gridHeight);
                    contourPoints.Add(worldPos);
                }
                
                // If we're back at start, we've completed the loop
                if (x1 == startX && y1 == startY)
                {
                    foundPath = true;
                    break;
                }
                
                continue;
            }
            
            // Try going straight
            newDir = dir;
            newX = x1 + dx[newDir];
            newY = y1 + dy[newDir];
            
            if (IsValidGridCell(newX, newY, gridWidth, gridHeight) && grid[newX, newY])
            {
                // We can go straight, so move
                x1 = newX;
                y1 = newY;
                
                if (!visited[x1, y1])
                {
                    visited[x1, y1] = true;
                    Vector2 worldPos = GridToWorld(x1, y1, gridWidth, gridHeight);
                    contourPoints.Add(worldPos);
                }
                
                // If we're back at start, we've completed the loop
                if (x1 == startX && y1 == startY)
                {
                    foundPath = true;
                    break;
                }
                
                continue;
            }
            
            // Try turning left
            newDir = (dir + 1) % 4;
            newX = x1 + dx[newDir];
            newY = y1 + dy[newDir];
            
            if (IsValidGridCell(newX, newY, gridWidth, gridHeight) && grid[newX, newY])
            {
                // We can go left, so turn left and move
                dir = newDir;
                x1 = newX;
                y1 = newY;
                
                if (!visited[x1, y1])
                {
                    visited[x1, y1] = true;
                    Vector2 worldPos = GridToWorld(x1, y1, gridWidth, gridHeight);
                    contourPoints.Add(worldPos);
                }
                
                // If we're back at start, we've completed the loop
                if (x1 == startX && y1 == startY)
                {
                    foundPath = true;
                    break;
                }
                
                continue;
            }
            
            // We can't go anywhere, turn around
            dir = (dir + 2) % 4;
        }
        
        // If we couldn't find a complete path, fall back to simple border
        if (!foundPath || contourPoints.Count < 3)
        {
            return GenerateSimpleBorder(nation);
        }
        
        // Add first point at the end to close the loop
        if (contourPoints.Count > 0)
        {
            contourPoints.Add(contourPoints[0]);
        }
        
        // Apply smoothing to the contour (optional)
        List<Vector2> smoothedPoints = SmoothPoints(contourPoints, 0.25f);
        
        // Return the resulting contour
        return smoothedPoints;
    }
    
    // Helper to convert grid coordinates to world coordinates
    private Vector2 GridToWorld(int gridX, int gridY, int gridWidth, int gridHeight)
    {
        float halfGridWidth = gridWidth * 0.5f;
        float halfGridHeight = gridHeight * 0.5f;
        
        // Calculate cell size in the high-resolution grid
        float cellSize = tileSize * 0.5f; // Each province is 2x2 cells in the high-res grid
        
        // Apply offset from center and border offset
        float worldX = (gridX - halfGridWidth + 0.5f) * cellSize + borderOffset;
        float worldY = (gridY - halfGridHeight + 0.5f) * cellSize + borderOffset;
        
        return new Vector2(worldX, worldY);
    }
    
    // Helper to check if grid coordinates are valid
    private bool IsValidGridCell(int x, int y, int width, int height)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
    
    // Simple smooth operation on the points to make the border look better
    private List<Vector2> SmoothPoints(List<Vector2> points, float smoothFactor)
    {
        if (points.Count < 3)
            return points;
        
        List<Vector2> smoothed = new List<Vector2>();
        
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 prev = points[(i + points.Count - 1) % points.Count];
            Vector2 curr = points[i];
            Vector2 next = points[(i + 1) % points.Count];
            
            // Simple Catmull-Rom spline-like smoothing
            Vector2 smoothedPoint = curr + smoothFactor * ((prev - curr) + (next - curr));
            smoothed.Add(smoothedPoint);
        }
        
        return smoothed;
    }
    
    // Simple rectangle border for fallback
    private List<Vector2> GenerateSimpleBorder(Nation nation)
    {
        List<Vector2> points = new List<Vector2>();
        
        if (nation.controlledProvinces == null || nation.controlledProvinces.Count == 0)
            return points;
        
        // Find bounds of the territory
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        
        foreach (Province province in nation.controlledProvinces)
        {
            Vector3 position = province.transform.position;
            float halfTileSize = tileSize * 0.5f;
            
            minX = Mathf.Min(minX, position.x - halfTileSize);
            minY = Mathf.Min(minY, position.y - halfTileSize);
            maxX = Mathf.Max(maxX, position.x + halfTileSize);
            maxY = Mathf.Max(maxY, position.y + halfTileSize);
        }
        
        // Add some padding
        float padding = borderOffset;
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
}
