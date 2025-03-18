// MapGenerationSettings.cs
using UnityEngine;

[System.Serializable]
public class MapGenerationSettings
{
    // Terrain generation settings
    public float WaterPercent = 0.3f;
    public float MountainPercent = 0.1f;
    public float HillPercent = 0.15f;
    public float ForestPercent = 0.2f;
    public float DesertPercent = 0.1f;
    public int TerrainSeed = 0;
    public float TerrainScale = 10f;
    
    // Nation settings
    public float UnownedProvincePercentage = 0.3f;
    
    // Settlement settings
    public float InitialSettlementPercent = 0.1f;
}