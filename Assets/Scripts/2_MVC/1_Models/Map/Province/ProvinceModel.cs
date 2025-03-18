using UnityEngine;
using System.Collections.Generic;

// ProvinceModel.cs
public class ProvinceModel
{
    // Core data
    public int X { get; private set; }
    public int Y { get; private set; }
    public TerrainType TerrainType { get; private set; }
    public Nation OwnerNation { get; private set; }
    public int Resources { get; set; }
    public Settlement Settlement { get; set; }

    // Resource yields based on terrain
    private static readonly Dictionary<TerrainType, int> TerrainResourceValues = new Dictionary<TerrainType, int>
    {
        { TerrainType.Plains, 15 },
        { TerrainType.Hills, 12 },
        { TerrainType.Mountains, 8 },
        { TerrainType.Forest, 10 },
        { TerrainType.Desert, 5 },
        { TerrainType.Water, 20 }
    };

    // Settlement building permissions
    private static readonly Dictionary<TerrainType, bool> TerrainSettlementAllowed = new Dictionary<TerrainType, bool>
    {
        { TerrainType.Plains, true },
        { TerrainType.Hills, true },
        { TerrainType.Mountains, false },
        { TerrainType.Forest, true },
        { TerrainType.Desert, true },
        { TerrainType.Water, false }
    };

    // Constructor
    public ProvinceModel(int x, int y)
    {
        X = x;
        Y = y;
        TerrainType = TerrainType.Plains;
        Resources = 10;
    }

    // Event for ownership changes
    public delegate void OwnershipChanged(ProvinceModel province, Nation oldOwner, Nation newOwner);
    public static event OwnershipChanged OnOwnershipChanged;

    // Pure data methods
    public void SetTerrainType(TerrainType type)
    {
        TerrainType = type;
    }

    public void SetOwner(Nation nation)
    {
        Nation previousOwner = OwnerNation;
        OwnerNation = nation;
        
        if (previousOwner != nation)
        {
            OnOwnershipChanged?.Invoke(this, previousOwner, nation);
        }
    }

    public bool CanBuildSettlement()
    {
        return TerrainSettlementAllowed[TerrainType];
    }

    public int GetResourceValue()
    {
        return TerrainResourceValues[TerrainType];
    }
}