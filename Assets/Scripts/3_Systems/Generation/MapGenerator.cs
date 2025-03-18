// MapGenerator.cs
using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private GameObject provincePrefab;
    [SerializeField] private int mapWidth = 10;
    [SerializeField] private int mapHeight = 8;
    [SerializeField] private float tileSize = 1f;
    
    [Header("Nation Settings")]
    [SerializeField] private Nation[] nations;
    [SerializeField] [Range(0f, 1f)] private float unownedProvincePercentage = 0.3f;
    
    [Header("Terrain Settings")]
    [SerializeField] private float waterPercent = 0.3f;
    [SerializeField] private float mountainPercent = 0.1f;
    [SerializeField] private float hillPercent = 0.15f;
    [SerializeField] private float forestPercent = 0.2f;
    [SerializeField] private float desertPercent = 0.1f;
    [SerializeField] private int terrainSeed = 0;
    [SerializeField] private float terrainScale = 10f;
    
    [Header("Settlement Settings")]
    [SerializeField] private GameObject settlementPrefab;
    [SerializeField] private float initialSettlementPercent = 0.1f;
    
    public Province[,] provinces;
    
    void Awake()
    {
        ServiceLocator.Register<MapGenerator>(this);
        GenerateMap();
        AssignInitialProvinces();
    }
        
    public void GenerateMap()
    {
        // Set random seed for reproducible terrain generation
        if (terrainSeed != 0)
        {
            Random.InitState(terrainSeed);
        }
        
        provinces = new Province[mapWidth, mapHeight];
        
        // Calculate the offset to center the grid in world space
        Vector3 startPosition = new Vector3(
            -(mapWidth * tileSize) / 2f, 
            -(mapHeight * tileSize) / 2f, 
            0);
        
        // Generate a noise map for terrain
        float[,] heightMap = GenerateNoiseMap(mapWidth, mapHeight);
        
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
                    
                    // Assign a terrain type based on the height map
                    AssignTerrainType(province, heightMap[x, y]);
                    
                    // Set initial owner to null (which will use neutral color)
                    province.SetOwner(null);
                }
            }
        }
        
        // Place initial settlements if settlement prefab is assigned
        if (settlementPrefab != null)
        {
            PlaceInitialSettlements();
        }

        // Create province borders using BorderManager
        BorderManager borderManager = ServiceLocator.Get<BorderManager>();
        if (borderManager != null)
        {
            foreach (var province in provinces)
            {
                borderManager.CreateProvinceBorder(province);
            }
        }

        // After generating the map, configure the background
        BackgroundManager backgroundManager = FindAnyObjectByType<BackgroundManager>();
        if (backgroundManager != null)
        {
            // Apply fixed scale and update camera bounds
            backgroundManager.ApplyFixedScale();
        }
    }
    
    // Generate Perlin noise map for terrain distribution
    private float[,] GenerateNoiseMap(int width, int height)
    {
        float[,] noiseMap = new float[width, height];
        
        float offsetX = Random.Range(0f, 99999f);
        float offsetY = Random.Range(0f, 99999f);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculate sample positions
                float sampleX = x / terrainScale + offsetX;
                float sampleY = y / terrainScale + offsetY;
                
                // Generate noise value
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                
                noiseMap[x, y] = perlinValue;
            }
        }
        
        return noiseMap;
    }
    
    // Assign terrain type based on height value
    private void AssignTerrainType(Province province, float heightValue)
    {
        // Determine terrain type based on height thresholds
        TerrainType terrainType;
        
        if (heightValue < waterPercent)
        {
            terrainType = TerrainType.Water;
        }
        else if (heightValue < waterPercent + desertPercent)
        {
            terrainType = TerrainType.Desert;
        }
        else if (heightValue < waterPercent + desertPercent + forestPercent)
        {
            terrainType = TerrainType.Forest;
        }
        else if (heightValue < waterPercent + desertPercent + forestPercent + hillPercent)
        {
            terrainType = TerrainType.Hills;
        }
        else if (heightValue < waterPercent + desertPercent + forestPercent + hillPercent + mountainPercent)
        {
            terrainType = TerrainType.Mountains;
        }
        else
        {
            terrainType = TerrainType.Plains;
        }
        
        // Set the terrain type on the province
        province.SetTerrainType(terrainType);
    }
    
    // Place initial settlements on suitable provinces
    private void PlaceInitialSettlements()
    {
        // Calculate how many settlements to place
        int totalSettlements = Mathf.RoundToInt(mapWidth * mapHeight * initialSettlementPercent);
        
        // Keep track of provinces that can have settlements
        List<Province> availableProvinces = new List<Province>();
        
        // Collect all valid provinces (not water, etc.)
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Province province = provinces[x, y];
                
                // Skip water provinces or other invalid terrain for settlements
                if (province.terrainType != TerrainType.Water && province.CanBuildSettlement())
                {
                    availableProvinces.Add(province);
                }
            }
        }
        
        // Place settlements
        for (int i = 0; i < totalSettlements && availableProvinces.Count > 0; i++)
        {
            // Pick a random available province
            int index = Random.Range(0, availableProvinces.Count);
            Province province = availableProvinces[index];
            
            // Create settlement
            GameObject settlementObj = Instantiate(settlementPrefab, province.transform.position, Quaternion.identity);
            settlementObj.transform.parent = transform;
            
            // Initialize settlement
            Settlement settlement = settlementObj.GetComponent<Settlement>();
            if (settlement != null)
            {
                settlement.Initialize(province, GenerateSettlementName());
            }
            
            // Remove this province from available list
            availableProvinces.RemoveAt(index);
        }
    }
    
    // Generate a random settlement name
    private string GenerateSettlementName()
    {
        string[] prefixes = { "New ", "Fort ", "Port ", "North ", "South ", "East ", "West ", "Upper ", "Lower " };
        string[] bases = { "York", "Chester", "Field", "Town", "Hill", "Dale", "Haven", "Port", "Bridge", "Ford" };
        
        string prefix = Random.value > 0.5f ? prefixes[Random.Range(0, prefixes.Length)] : "";
        string baseName = bases[Random.Range(0, bases.Length)];
        
        return prefix + baseName;
    }
    
    // Assign initial provinces to nations (keeping your existing method)
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
                // Only add non-water provinces as available for nations
                if (provinces[x, y].terrainType != TerrainType.Water)
                {
                    availableProvinces.Add(provinces[x, y]);
                }
            }
        }
        
        // Calculate how many provinces will be assigned to nations
        int provinceCount = availableProvinces.Count;
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
        
        // After assigning provinces, redraw nation borders
        BorderManager borderManager = ServiceLocator.Get<BorderManager>();
        if (borderManager != null)
        {
            borderManager.RedrawNationBorders();
        }
        else
        {
            Debug.LogWarning("BorderManager not registered. Nation borders will not be drawn.");
        }
    }
    
    // Helper method to assign provinces in clusters (keeping your existing implementation)
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
    
    // Helper method to get adjacent provinces (keeping your existing implementation)
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
    
    // Helper method to check if two provinces are adjacent (keeping your existing implementation)
    private bool AreProvincesAdjacent(Province a, Province b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }
}