// Province.cs
using UnityEngine;

public class Province : MonoBehaviour
{
    // References to the MVC components
    public ProvinceModel Model { get; private set; }
    public ProvinceView View { get; private set; }
    public ProvinceController Controller { get; private set; }
    
    // Convenience properties to maintain compatibility with existing code
    public int x => Model.X;
    public int y => Model.Y;
    public TerrainType terrainType => Model.TerrainType;
    public Nation ownerNation => Model.OwnerNation;
    
    public void Initialize(ProvinceModel model, ProvinceView view, ProvinceController controller)
    {
        Model = model;
        View = view;
        Controller = controller;
    }
    
    // Add an overload for compatibility with the old method:
    public void Initialize(int xPos, int yPos)
    {
        // Create default MVC components for backward compatibility
        ProvinceModel model = new ProvinceModel(xPos, yPos);
        ProvinceView view = GetComponent<ProvinceView>();
        if (view == null)
        {
            view = gameObject.AddComponent<ProvinceView>();
        }
        ProvinceController controller = new ProvinceController(model, view);
        
        // Use the actual initialization method
        Initialize(model, view, controller);
    }
    
    // Convenience methods to maintain compatibility
    public void SetOwner(Nation nation) => Model.SetOwner(nation);
    public void SetTerrainType(TerrainType type) => Model.SetTerrainType(type);
    public int CollectResources() => Controller.CollectResources();
    public bool CanBuildSettlement() => Model.CanBuildSettlement();
}