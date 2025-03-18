// MapController.cs
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    private MapModel _model;
    private MapGenerator _generator; // Keep a reference to the original MapGenerator for now
    
    public MapController(MapModel model, MapGenerator generator)
    {
        _model = model;
        _generator = generator;
        
        ServiceLocator.Register<MapController>(this);
    }
    
    public void GenerateMap(GameObject provincePrefab, GameObject settlementPrefab)
    {
        // Create the provinces array
        _model.Provinces = new Province[_model.Width, _model.Height];
        
        // Set random seed for reproducible terrain generation
        if (_model.Settings.TerrainSeed != 0)
        {
            Random.InitState(_model.Settings.TerrainSeed);
        }
        
        // Generate a noise map for terrain
        float[,] heightMap = GenerateNoiseMap(_model.Width, _model.Height);
        
        for (int x = 0; x < _model.Width; x++)
        {
            for (int y = 0; y < _model.Height; y++)
            {
                // Calculate position
                Vector3 position = _model.CalculateWorldPosition(x, y);
                
                // Create province gameObject
                GameObject provinceObj = Object.Instantiate(provincePrefab, position, Quaternion.identity);
                provinceObj.name = $"Province_{x}_{y}";
                provinceObj.transform.parent = _generator.transform;
                
                // Get and setup province component
                Province province = provinceObj.GetComponent<Province>();
                if (province != null)
                {
                    province.Initialize(x, y);
                    _model.Provinces[x, y] = province;
                    
                    // Assign a terrain type based on the height map
                    AssignTerrainType(province, heightMap[x, y]);
                    
                    // Set initial owner to null
                    province.SetOwner(null);
                }
            }
        }
        
        // Place initial settlements if settlement prefab is assigned
        if (settlementPrefab != null)
        {
            PlaceInitialSettlements(settlementPrefab);
        }
        
        // Call the method to assign initial provinces
        AssignInitialProvinces();
        
        // Set up visual elements
        SetupVisuals();
    }
    
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
                float sampleX = x / _model.Settings.TerrainScale + offsetX;
                float sampleY = y / _model.Settings.TerrainScale + offsetY;
                
                // Generate noise value
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                
                noiseMap[x, y] = perlinValue;
            }
        }
        
        return noiseMap;
    }
    
    private void AssignTerrainType(Province province, float heightValue)
    {
        // Determine terrain type based on height thresholds
        TerrainType terrainType;
        
        var settings = _model.Settings;
        
        if (heightValue < settings.WaterPercent)
        {
            terrainType = TerrainType.Water;
        }
        else if (heightValue < settings.WaterPercent + settings.DesertPercent)
        {
            terrainType = TerrainType.Desert;
        }
        else if (heightValue < settings.WaterPercent + settings.DesertPercent + settings.ForestPercent)
        {
            terrainType = TerrainType.Forest;
        }
        else if (heightValue < settings.WaterPercent + settings.DesertPercent + settings.ForestPercent + settings.HillPercent)
        {
            terrainType = TerrainType.Hills;
        }
        else if (heightValue < settings.WaterPercent + settings.DesertPercent + settings.ForestPercent + settings.HillPercent + settings.MountainPercent)
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
    
    private void PlaceInitialSettlements(GameObject settlementPrefab)
    {
        // Calculate how many settlements to place
        int totalSettlements = Mathf.RoundToInt(_model.Width * _model.Height * _model.Settings.InitialSettlementPercent);
        
        // Keep track of provinces that can have settlements
        List<Province> availableProvinces = new List<Province>();
        
        // Collect all valid provinces (not water, etc.)
        for (int x = 0; x < _model.Width; x++)
        {
            for (int y = 0; y < _model.Height; y++)
            {
                Province province = _model.Provinces[x, y];
                
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
            GameObject settlementObj = Object.Instantiate(settlementPrefab, province.transform.position, Quaternion.identity);
            settlementObj.transform.parent = _generator.transform;
            
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
    
    private string GenerateSettlementName()
    {
        string[] prefixes = { "New ", "Fort ", "Port ", "North ", "South ", "East ", "West ", "Upper ", "Lower " };
        string[] bases = { "York", "Chester", "Field", "Town", "Hill", "Dale", "Haven", "Port", "Bridge", "Ford" };
        
        string prefix = Random.value > 0.5f ? prefixes[Random.Range(0, prefixes.Length)] : "";
        string baseName = bases[Random.Range(0, bases.Length)];
        
        return prefix + baseName;
    }
    
    private void AssignInitialProvinces()
    {
        if (_model.Nations == null || _model.Nations.Length == 0)
        {
            Debug.LogWarning("No nations assigned. Provinces will remain unowned.");
            return;
        }
        
        // Clear any existing province assignments
        foreach (Nation nation in _model.Nations)
        {
            nation.controlledProvinces.Clear();
        }
        
        List<Province> availableProvinces = new List<Province>();
        
        // Gather all non-water provinces
        for (int x = 0; x < _model.Width; x++)
        {
            for (int y = 0; y < _model.Height; y++)
            {
                if (_model.Provinces[x, y].terrainType != TerrainType.Water)
                {
                    availableProvinces.Add(_model.Provinces[x, y]);
                }
            }
        }
        
        // Calculate how many provinces will be assigned to nations
        int provinceCount = availableProvinces.Count;
        int provinceToAssign = Mathf.RoundToInt(provinceCount * (1 - _model.Settings.UnownedProvincePercentage));
        
        // Calculate how many provinces per nation (roughly equal distribution)
        int provincesPerNation = provinceToAssign / _model.Nations.Length;
        
        // For each nation, select a cluster of provinces to assign
        foreach (Nation nation in _model.Nations)
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
        
        Debug.Log($"Map generated with {provinceToAssign} provinces assigned to nations and {availableProvinces.Count} unowned provinces.");
    }
    
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
    
    private List<Province> GetAdjacentProvinces(int x, int y, List<Province> availableProvinces)
    {
        List<Province> adjacent = new List<Province>();
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };
        
        for (int i = 0; i < 4; i++)
        {
            int newX = x + dx[i];
            int newY = y + dy[i];
            
            if (_model.IsValidCoordinate(newX, newY))
            {
                Province adjacentProvince = _model.Provinces[newX, newY];
                if (availableProvinces.Contains(adjacentProvince))
                {
                    adjacent.Add(adjacentProvince);
                }
            }
        }
        
        return adjacent;
    }
    
    private void SetupVisuals()
    {
        // Create province borders using BorderManager
        BorderManager borderManager = ServiceLocator.Get<BorderManager>();
        if (borderManager != null)
        {
            foreach (var province in _model.Provinces)
            {
                borderManager.CreateProvinceBorder(province);
            }
            
            // Redraw nation borders
            borderManager.RedrawNationBorders();
        }
        
        // Configure the background
        BackgroundManager backgroundManager = Object.FindAnyObjectByType<BackgroundManager>();
        if (backgroundManager != null)
        {
            backgroundManager.ApplyFixedScale();
        }
    }
    
    // Public methods to interact with the map
    public Province GetProvinceAt(int x, int y)
    {
        if (_model.IsValidCoordinate(x, y))
        {
            return _model.Provinces[x, y];
        }
        return null;
    }
    
    public void SelectProvince(Province province)
    {
        if (province != null)
        {
            Debug.Log($"Province selected at {province.x}, {province.y} with terrain: {province.terrainType}");
            // Add more selection logic if needed
        }
    }
}