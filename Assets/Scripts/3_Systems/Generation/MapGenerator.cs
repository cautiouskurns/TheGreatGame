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
    
    // Keep this public reference for backward compatibility
    public Province[,] provinces;
    
    // MVC components
    private MapModel mapModel;
    private MapController mapController;
    
    void Awake()
    {
        ServiceLocator.Register<MapGenerator>(this);
        
        // Create and initialize the MVC components
        InitializeMVC();
        
        // Generate the map using the controller
        mapController.GenerateMap(provincePrefab, settlementPrefab);
        
        // Set the provinces array reference for backward compatibility
        provinces = mapModel.Provinces;
    }
    
    private void InitializeMVC()
    {
        // Create settings from serialized fields
        MapGenerationSettings settings = new MapGenerationSettings
        {
            WaterPercent = waterPercent,
            MountainPercent = mountainPercent,
            HillPercent = hillPercent,
            ForestPercent = forestPercent,
            DesertPercent = desertPercent,
            TerrainSeed = terrainSeed,
            TerrainScale = terrainScale,
            UnownedProvincePercentage = unownedProvincePercentage,
            InitialSettlementPercent = initialSettlementPercent
        };
        
        // Create the model
        mapModel = new MapModel(mapWidth, mapHeight, tileSize, nations, settings);
        
        // Create the controller
        mapController = new MapController(mapModel, this);
    }
    
    // Public accessor methods for backward compatibility
    public Province GetProvinceAt(int x, int y)
    {
        return mapController.GetProvinceAt(x, y);
    }
    
    public void SelectProvince(Province province)
    {
        mapController.SelectProvince(province);
    }
}