// Assets/Scripts/Economy/EconomyManager.cs
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private float baseGrowthRate = 0.1f;
    
    void Awake()
    {
        ServiceLocator.Register<EconomyManager>(this);
    }
    
    public void ProcessProvinceResources()
    {
        MapGenerator mapGen = ServiceLocator.Get<MapGenerator>();
        if (mapGen == null || mapGen.provinces == null) return;
        
        foreach (Province province in mapGen.provinces)
        {
            //province.RegenerateResources();
        }
    }
    
    public void ProcessSettlements()
    {
        Settlement[] settlements = FindObjectsByType<Settlement>(FindObjectsSortMode.None);
        
        foreach (Settlement settlement in settlements)
        {
            ProcessSettlementGrowth(settlement);
        }
    }
    
    public void CollectNationResources(Nation nation)
    {
        if (nation == null) return;
        
        ResourcePackage resources = new ResourcePackage();
        
        foreach (Province province in nation.controlledProvinces)
        {
            int provinceResources = province.CollectResources();
            
            // Distribute resources based on terrain type and buildings
            resources.gold += provinceResources;
            resources.food += provinceResources;
            resources.production += provinceResources;
        }
        
        // Apply resources to nation
        nation.gold += resources.gold;
        nation.food += resources.food;
        nation.production += resources.production;
    }
    
    private void ProcessSettlementGrowth(Settlement settlement)
    {
        // Calculate growth based on various factors
        float adjustedGrowthRate = baseGrowthRate;
        
        // Apply terrain modifiers
        if (settlement.province != null)
        {
            switch (settlement.province.terrainType)
            {
                case TerrainType.Plains: adjustedGrowthRate *= 1.2f; break;
                case TerrainType.Desert: adjustedGrowthRate *= 0.7f; break;
                // Other terrain modifiers...
            }
        }
        
        // Calculate and apply growth
        int growthAmount = Mathf.RoundToInt(settlement.population * adjustedGrowthRate);
        settlement.population = Mathf.Min(settlement.population + growthAmount, settlement.populationCap);
        
        // Check for settlement upgrade
        if (settlement.population >= settlement.populationCap && settlement.size != Settlement.Size.Metropolis)
        {
            UpgradeSettlement(settlement);
        }
        
        // Update resource production
        UpdateSettlementProduction(settlement);
    }
    
    private void UpgradeSettlement(Settlement settlement)
    {
        // Logic moved from Settlement class
        switch (settlement.size)
        {
            case Settlement.Size.Village:
                settlement.size = Settlement.Size.Town;
                settlement.populationCap = 1500;
                break;
            // Other cases...
        }
        
        settlement.UpdateVisuals();
    }
    
    private void UpdateSettlementProduction(Settlement settlement)
    {
        // Calculate resource production based on settlement size and population
        float populationScale = Mathf.Clamp01((float)settlement.population / settlement.populationCap);
        
        switch (settlement.size)
        {
            case Settlement.Size.Village:
                settlement.resourceBonus = Mathf.RoundToInt(5 + (5 * populationScale));
                break;
            // Other cases...
        }
    }
}