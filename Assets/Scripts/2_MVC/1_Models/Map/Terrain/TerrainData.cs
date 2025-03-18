using UnityEngine;

[CreateAssetMenu(fileName = "TerrainData", menuName = "Game/Terrain Data")]
public class TerrainData : ScriptableObject
{
    public TerrainType terrainType;
    public string terrainName;
    public Sprite terrainSprite;
    public Color terrainTint = Color.white;

    [Header("Resource Properties")]
    public int baseResourceOutput = 10;
    public float foodModifier = 1.0f;
    public float goldModifier = 1.0f;
    public float productionModifier = 1.0f;

    [Header("Movement Properties")]
    public int movementCost = 1;
    public bool isPassable = true;

    [Header("Settlement Properties")]
    public bool canBuildSettlement = true;
    public float settlementGrowthModifier = 1.0f;
}
