// Nation.cs
using System.Collections.Generic;
using UnityEngine;

public class Nation : MonoBehaviour
{
    public string nationName;
    public Color nationColor;
    public bool isPlayerControlled = false;
    
    public int gold = 100;
    public int food = 100;
    public int production = 100;
    public List<Province> controlledProvinces = new List<Province>();
    
    public void AddProvince(Province province)
    {
        if (!controlledProvinces.Contains(province))
        {
            controlledProvinces.Add(province);
            province.SetOwner(this);
            Debug.Log($"Nation {nationName} added province ({province.x},{province.y})");
        }
    }
    
    public void RemoveProvince(Province province)
    {
        if (controlledProvinces.Contains(province))
        {
            controlledProvinces.Remove(province);
        }
    }
    
    public void CollectAllResources()
    {
        foreach (Province province in controlledProvinces)
        {
            int collectedResources = province.CollectResources();
            gold += collectedResources;
            food += collectedResources;
            production += collectedResources;
        }
        
        Debug.Log($"{nationName} now has {gold} gold, {food} food, {production} production");
    }
    
    // Very simple AI for non-player nation
    public void ExecuteAITurn()
    {
        if (isPlayerControlled) return;
        
        // Collect resources
        CollectAllResources();
        
        // Very basic AI decision: try to claim a neighboring province
        TryExpandTerritory();
    }
    
    private void TryExpandTerritory()
    {
        // Get neighboring provinces that aren't owned
        Province targetProvince = FindNeighboringUnownedProvince();
        
        if (targetProvince != null && gold >= 50)
        {
            // Try to claim it
            gold -= 50;
            AddProvince(targetProvince);
            Debug.Log($"{nationName} claimed a new province for 50 gold");
        }
    }
    
    private Province FindNeighboringUnownedProvince()
    {
        // This is a placeholder - you'd need to implement proper adjacency checking
        // with the map generator to make this work properly
        
        MapGenerator mapGen = FindAnyObjectByType<MapGenerator>();
        if (mapGen == null || mapGen.provinces == null) return null;
        
        foreach (Province ownedProvince in controlledProvinces)
        {
            // Check adjacent tiles
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };
            
            for (int i = 0; i < 4; i++)
            {
                int nx = ownedProvince.x + dx[i];
                int ny = ownedProvince.y + dy[i];
                
                // Check bounds
                if (nx >= 0 && nx < mapGen.provinces.GetLength(0) && 
                    ny >= 0 && ny < mapGen.provinces.GetLength(1))
                {
                    Province neighbor = mapGen.provinces[nx, ny];
                    if (neighbor.ownerNation == null)
                    {
                        return neighbor;
                    }
                }
            }
        }
        
        return null;
    }
}