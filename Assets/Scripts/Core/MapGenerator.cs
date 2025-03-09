// MapGenerator.cs
using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject provincePrefab;
    [SerializeField] private int mapWidth = 10;
    [SerializeField] private int mapHeight = 8;
    [SerializeField] private float tileSize = 1f;
    
    // Add nations array for initial province assignment
    [SerializeField] private Nation[] nations;
    
    // Percentage of provinces that will remain unowned (0.0 - 1.0)
    [SerializeField] [Range(0f, 1f)] private float unownedProvincePercentage = 0.3f;
    
    public Province[,] provinces;
    
    void Awake()
    {
        GenerateMap();
        AssignInitialProvinces();
    }
    
    public void GenerateMap()
    {
        provinces = new Province[mapWidth, mapHeight];
        
        // Calculate the offset to center the grid in world space
        // This will place the center of the grid at (0,0) in world space
        Vector3 startPosition = new Vector3(
            -(mapWidth * tileSize) / 2f, 
            -(mapHeight * tileSize) / 2f, 
            0);
        
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Calculate position relative to the centered grid
                Vector3 position = startPosition + new Vector3(x * tileSize, y * tileSize, 0);
                
                // Create province gameObject
                GameObject provinceObj = Instantiate(provincePrefab, position, Quaternion.identity);
                provinceObj.name = $"Province_{x}_{y}";
                provinceObj.transform.parent = transform;
                
                // Get and setup province component
                Province province = provinceObj.GetComponent<Province>();
                if (province != null)
                {
                    province.Initialize(x, y);
                    provinces[x, y] = province;
                    
                    // Set initial owner to null (which will use neutral color)
                    province.SetOwner(null);
                }
            }
        }
    }
    
    // New method to assign initial provinces to nations
    private void AssignInitialProvinces()
    {
        if (nations == null || nations.Length == 0)
        {
            Debug.LogWarning("No nations assigned to MapGenerator. Provinces will remain unowned.");
            return;
        }
        
        // Clear any existing province assignments
        foreach (Nation nation in nations)
        {
            nation.controlledProvinces.Clear();
        }
        
        List<Province> availableProvinces = new List<Province>();
        
        // Gather all provinces
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                availableProvinces.Add(provinces[x, y]);
            }
        }
        
        // Calculate how many provinces will be assigned to nations
        int provinceCount = mapWidth * mapHeight;
        int provinceToAssign = Mathf.RoundToInt(provinceCount * (1 - unownedProvincePercentage));
        
        // Calculate how many provinces per nation (roughly equal distribution)
        int provincesPerNation = provinceToAssign / nations.Length;
        
        // For each nation, select a cluster of provinces to assign
        foreach (Nation nation in nations)
        {
            if (availableProvinces.Count == 0) break;
            
            // Select a random starting province for this nation
            int randomIndex = Random.Range(0, availableProvinces.Count);
            Province startingProvince = availableProvinces[randomIndex];
            
            // Add it to the nation
            nation.AddProvince(startingProvince);
            availableProvinces.Remove(startingProvince);
            
            // Try to expand from that starting point
            AssignClusteredProvincesToNation(nation, startingProvince, provincesPerNation - 1, availableProvinces);
        }
        
        // The remaining provinces in availableProvinces stay unowned
        Debug.Log($"Map generated with {provinceToAssign} provinces assigned to nations and {availableProvinces.Count} unowned provinces.");
    }
    
    // Helper method to assign provinces in clusters
    private void AssignClusteredProvincesToNation(Nation nation, Province startingPoint, int count, List<Province> availableProvinces)
    {
        if (count <= 0 || availableProvinces.Count == 0) return;
        
        // Create a queue for breadth-first expansion
        Queue<Province> expansionQueue = new Queue<Province>();
        expansionQueue.Enqueue(startingPoint);
        
        int provinceAssigned = 0;
        
        while (expansionQueue.Count > 0 && provinceAssigned < count && availableProvinces.Count > 0)
        {
            Province current = expansionQueue.Dequeue();
            
            // Find adjacent available provinces
            List<Province> adjacentProvinces = GetAdjacentProvinces(current.x, current.y, availableProvinces);
            
            foreach (Province adjacent in adjacentProvinces)
            {
                if (provinceAssigned >= count) break;
                
                nation.AddProvince(adjacent);
                availableProvinces.Remove(adjacent);
                expansionQueue.Enqueue(adjacent);
                provinceAssigned++;
            }
        }
    }
    
    // Helper method to get adjacent provinces that are in the availableProvinces list
    private List<Province> GetAdjacentProvinces(int x, int y, List<Province> availableProvinces)
    {
        List<Province> adjacent = new List<Province>();
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };
        
        for (int i = 0; i < 4; i++)
        {
            int newX = x + dx[i];
            int newY = y + dy[i];
            
            if (newX >= 0 && newX < mapWidth && newY >= 0 && newY < mapHeight)
            {
                Province adjacentProvince = provinces[newX, newY];
                if (availableProvinces.Contains(adjacentProvince))
                {
                    adjacent.Add(adjacentProvince);
                }
            }
        }
        
        return adjacent;
    }
    
    // Helper method to check if two provinces are adjacent
    private bool AreProvincesAdjacent(Province a, Province b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }
}
