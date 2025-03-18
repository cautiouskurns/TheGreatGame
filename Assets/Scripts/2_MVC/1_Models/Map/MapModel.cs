// MapModel.cs
using UnityEngine;

public class MapModel
{
    // Map dimensions and properties
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float TileSize { get; private set; }
    
    // Raw data - we'll still use the original Province array for now
    public Province[,] Provinces { get; set; }
    
    // Settings
    public MapGenerationSettings Settings { get; private set; }
    
    // Nations
    public Nation[] Nations { get; private set; }
    
    public MapModel(int width, int height, float tileSize, Nation[] nations, MapGenerationSettings settings)
    {
        Width = width;
        Height = height;
        TileSize = tileSize;
        Nations = nations;
        Settings = settings;
        
        // The actual Provinces array will be created by the MapGenerator
    }
    
    public Vector3 CalculateWorldPosition(int x, int y)
    {
        Vector3 startPosition = new Vector3(
            -(Width * TileSize) / 2f,
            -(Height * TileSize) / 2f,
            0);
            
        return startPosition + new Vector3(x * TileSize, y * TileSize, 0);
    }
    
    public bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}