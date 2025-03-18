using UnityEngine;

// ProvinceController.cs
public class ProvinceController
{
    private ProvinceModel model;
    private ProvinceView view;
    
    // Event for selection
    public delegate void ProvinceSelected(ProvinceModel province);
    public static event ProvinceSelected OnProvinceSelected;
    
    public ProvinceController(ProvinceModel model, ProvinceView view)
    {
        this.model = model;
        this.view = view;
        
        // Initialize view with model
        view.Initialize(model);
    }
    
    // Logic methods
    public void RegenerateResources()
    {
        // Get base resource value for terrain
        int baseResources = model.GetResourceValue();
        
        // Add settlement bonus if applicable
        if (model.Settlement != null)
        {
            baseResources += model.Settlement.resourceBonus;
        }
        
        model.Resources = baseResources;
    }
    
    public int CollectResources()
    {
        int collected = model.Resources;
        model.Resources = 0;
        return collected;
    }
    
    // Called by click handlers or other systems
    public void OnClick()
    {
        // First, trigger the event for any listeners
        OnProvinceSelected?.Invoke(model);
        
        // As a fallback, use the existing MapGenerator system
        var mapGenerator = ServiceLocator.Get<MapGenerator>();
        if (mapGenerator != null)
        {
            // We can use the Province wrapper to work with existing code
            Province provinceWrapper = view.GetComponent<Province>();
            if (provinceWrapper != null)
            {
                // If there's a click handler in the MapGenerator, we could call it here
                // Or add a temporary method to MapGenerator to handle province selection
                // For now, just logging the selection
                Debug.Log($"Province selected at {model.X}, {model.Y}");
            }
        }
    }
    
    // Other logical operations
    public void ProcessTurn()
    {
        RegenerateResources();
        
        // Other turn-based logic
    }
}