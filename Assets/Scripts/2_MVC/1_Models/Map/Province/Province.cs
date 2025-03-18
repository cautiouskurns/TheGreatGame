// Province.cs
using UnityEngine;

public class Province : MonoBehaviour
{
    // References to the MVC components
    public ProvinceModel Model { get; private set; }
    public ProvinceView View { get; private set; }
    public ProvinceController Controller { get; private set; }
    
    // Original fields for backward compatibility
    public int resources = 10;
    public Settlement settlement;
    
    // Convenience properties to maintain compatibility with existing code
    public int x => Model?.X ?? 0;
    public int y => Model?.Y ?? 0;
    public TerrainType terrainType => Model?.TerrainType ?? TerrainType.Plains;
    public Nation ownerNation => Model?.OwnerNation;
    
    // Original Initialize method to maintain compatibility with existing code
    public void Initialize(int xPos, int yPos)
    {
        // Create a new model for this province
        Model = new ProvinceModel(xPos, yPos);
        
        // For backward compatibility
        resources = Model.Resources;
    }
    
    // New MVC-based Initialize method
    public void Initialize(ProvinceModel model, ProvinceView view, ProvinceController controller)
    {
        Model = model;
        View = view;
        Controller = controller;
    }
    
    // Compatibility methods that forward to model/controller
    public void SetOwner(Nation nation)
    {
        if (Model != null)
        {
            Model.SetOwner(nation);
        }
    }
    
    public void SetTerrainType(TerrainType type)
    {
        if (Model != null)
        {
            Model.SetTerrainType(type);
        }
    }
    
    public int CollectResources()
    {
        if (Controller != null)
        {
            return Controller.CollectResources();
        }
        
        // Fallback implementation for backward compatibility
        int collected = resources;
        resources = 0;
        return collected;
    }
    
    public bool CanBuildSettlement()
    {
        return Model?.CanBuildSettlement() ?? false;
    }
}