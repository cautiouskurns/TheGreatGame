using System.Collections.Generic;
using UnityEngine;

public class NationBorderManager : MonoBehaviour
{
    [SerializeField] private float borderWidth = 0.15f;
    [SerializeField] private float borderOffset = 0.0f; // Set to zero for tight borders
    [SerializeField] private bool useSimpleOutlines = false;
    [SerializeField] private bool debugDrawGrid = false;
    
    private Dictionary<Nation, LineRenderer> nationBorders = new Dictionary<Nation, LineRenderer>();
    private MapGenerator mapGenerator;
    private float tileSize = 1f;
    
    // Direction vectors for checking neighbors
    private static readonly int[] dx = { 1, 0, -1, 0 }; // Right, Up, Left, Down
    private static readonly int[] dy = { 0, 1, 0, -1 };
    
    // Edge segments for each direction
    private class EdgeSegment
    {
        public Vector2 start;
        public Vector2 end;
        public bool isVisited;
        
        public EdgeSegment(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
            this.isVisited = false;
        }
    }
    
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
        
        // Draw new borders for all nations using the new method
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
        
        // Generate the border points using our new precise method
        List<Vector2> borderPoints;
        
        if (useSimpleOutlines)
        {
            borderPoints = GenerateSimpleBorder(nation);
        }
        else
        {
            borderPoints = GeneratePreciseBorder(nation);
        }
        
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
    
    // Generate precise borders by following province edges
    private List<Vector2> GeneratePreciseBorder(Nation nation)
    {
        if (mapGenerator == null || mapGenerator.provinces == null)
            return new List<Vector2>();
        
        int mapWidth = mapGenerator.provinces.GetLength(0);
        int mapHeight = mapGenerator.provinces.GetLength(1);
        
        // Build a 2D array representing territory ownership
        bool[,] ownedCells = new bool[mapWidth, mapHeight];
        
        // Mark territories owned by this nation
        foreach (Province province in nation.controlledProvinces)
        {
            if (province.x >= 0 && province.x < mapWidth && province.y >= 0 && province.y < mapHeight)
            {
                ownedCells[province.x, province.y] = true;
            }
        }
        
        // Find all border edges (between owned and non-owned cells)
        List<EdgeSegment> allEdges = new List<EdgeSegment>();
        
        // For each owned cell, check if any of its edges are borders
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (ownedCells[x, y])
                {
                    // Check all 4 directions for borders
                    for (int dir = 0; dir < 4; dir++)
                    {
                        int nx = x + dx[dir];
                        int ny = y + dy[dir];
                        
                        // If neighbor is outside map bounds or not owned by this nation, this is a border edge
                        if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight || !ownedCells[nx, ny])
                        {
                            // Add this edge segment to our list
                            EdgeSegment segment = CreateEdgeSegment(x, y, dir);
                            allEdges.Add(segment);
                        }
                    }
                }
            }
        }
        
        // Now we need to connect these edges into a continuous loop
        List<Vector2> borderPoints = ConnectEdgesToLoop(allEdges);
        
        // Apply smoothing to the border if desired
        if (borderPoints.Count > 3)
        {
            borderPoints = SmoothPoints(borderPoints, 0.1f);
        }
        
        return borderPoints;
    }
    
    // Create an edge segment for a cell in the given direction
    private EdgeSegment CreateEdgeSegment(int x, int y, int direction)
    {
        float cellSize = tileSize;
        
        // Calculate the map offset
        MapGenerator mapGen = FindAnyObjectByType<MapGenerator>();
        int mapWidth = mapGen.provinces.GetLength(0);
        int mapHeight = mapGen.provinces.GetLength(1);
        
        float offsetX = -((mapWidth * tileSize) / 2.0f);
        float offsetY = -((mapHeight * tileSize) / 2.0f);
        
        // Adjust for tile centering
        offsetX += tileSize / 2.0f;
        offsetY += tileSize / 2.0f;
        
        // Base coordinates of the province
        float baseX = offsetX + (x * cellSize);
        float baseY = offsetY + (y * cellSize);
        
        // Half dimensions of each province
        float half = cellSize / 2.0f;
        
        Vector2 start, end;
        
        // Create segment based on direction
        switch (direction)
        {
            case 0: // Right edge
                start = new Vector2(baseX + half, baseY - half);
                end = new Vector2(baseX + half, baseY + half);
                break;
            case 1: // Top edge
                start = new Vector2(baseX - half, baseY + half);
                end = new Vector2(baseX + half, baseY + half);
                break;
            case 2: // Left edge
                start = new Vector2(baseX - half, baseY + half);
                end = new Vector2(baseX - half, baseY - half);
                break;
            case 3: // Bottom edge
                start = new Vector2(baseX + half, baseY - half);
                end = new Vector2(baseX - half, baseY - half);
                break;
            default:
                start = end = Vector2.zero;
                break;
        }
        
        // Add a slight offset to ensure the border sits just outside the province
        Vector2 normalizedDir = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-normalizedDir.y, normalizedDir.x) * borderOffset;
        start += perpendicular;
        end += perpendicular;
        
        return new EdgeSegment(start, end);
    }
    
    // Connect edge segments into a continuous loop
    private List<Vector2> ConnectEdgesToLoop(List<EdgeSegment> edges)
    {
        List<Vector2> result = new List<Vector2>();
        
        if (edges.Count == 0) return result;
        
        // Use a simpler approach for small territories
        if (edges.Count <= 10)
        {
            // Add all start points to the result
            foreach (var edge in edges)
            {
                result.Add(edge.start);
            }
            // Close the loop if possible
            if (result.Count > 0)
            {
                result.Add(result[0]);
            }
            return result;
        }
        
        // Try to connect edges into a proper loop
        EdgeSegment current = edges[0];
        result.Add(current.start);
        current.isVisited = true;
        
        int safetyCounter = edges.Count * 2; // Prevent infinite loops
        bool foundNext = true;
        
        while (foundNext && safetyCounter > 0)
        {
            safetyCounter--;
            result.Add(current.end);
            
            foundNext = false;
            float smallestDistance = float.MaxValue;
            EdgeSegment nextSegment = null;
            
            // Find the closest segment to the current end point
            foreach (var segment in edges)
            {
                if (!segment.isVisited)
                {
                    float distToStart = Vector2.Distance(current.end, segment.start);
                    if (distToStart < smallestDistance)
                    {
                        smallestDistance = distToStart;
                        nextSegment = segment;
                    }
                    
                    float distToEnd = Vector2.Distance(current.end, segment.end);
                    if (distToEnd < smallestDistance)
                    {
                        smallestDistance = distToEnd;
                        nextSegment = segment;
                        // Need to reverse this segment
                        Vector2 temp = segment.start;
                        segment.start = segment.end;
                        segment.end = temp;
                    }
                }
            }
            
            // If we found a close enough segment, continue the loop
            if (nextSegment != null && smallestDistance < tileSize * 1.5f)
            {
                current = nextSegment;
                current.isVisited = true;
                foundNext = true;
            }
        }
        
        // Close the loop if possible
        if (result.Count > 2)
        {
            float closeDistance = Vector2.Distance(result[0], result[result.Count - 1]);
            if (closeDistance < tileSize * 1.5f)
            {
                result.Add(result[0]); // Complete the loop
            }
        }
        
        // If we couldn't create a proper loop, try a simpler approach
        if (result.Count < 3)
        {
            result.Clear();
            foreach (var edge in edges)
            {
                result.Add(edge.start);
            }
            if (result.Count > 0)
            {
                result.Add(result[0]);
            }
        }
        
        return result;
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
