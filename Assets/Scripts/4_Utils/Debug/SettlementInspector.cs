// SettlementInspector.cs - Create this as a new file
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(Settlement))]
public class SettlementInspector : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the settlement being inspected
        Settlement settlement = (Settlement)target;
        
        // Draw default inspector properties
        DrawDefaultInspector();
        
        // Add a separator
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        // Display settlement information in a nice format
        EditorGUILayout.LabelField("Settlement Details", EditorStyles.boldLabel);
        
        GUI.enabled = false; // Make these fields read-only
        
        EditorGUILayout.LabelField("Name", settlement.settlementName);
        EditorGUILayout.LabelField("Size", settlement.size.ToString());
        EditorGUILayout.LabelField("Population", settlement.population.ToString());
        EditorGUILayout.LabelField("Resource Bonus", settlement.resourceBonus.ToString());
        
        if (settlement.province != null)
        {
            EditorGUILayout.LabelField("Province Location", $"({settlement.province.x}, {settlement.province.y})");
            EditorGUILayout.LabelField("Province Terrain", settlement.province.terrainType.ToString());
        }
        else
        {
            EditorGUILayout.LabelField("Province", "None Assigned");
        }
        
        // Add a growth estimation
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Growth Preview", EditorStyles.boldLabel);
        
        int projectedPopulation = settlement.population;
        int growthAmount = Mathf.RoundToInt(projectedPopulation * settlement.growthRate);
        projectedPopulation = Mathf.Min(projectedPopulation + growthAmount, settlement.populationCap);
        
        EditorGUILayout.LabelField("Next Turn Population", projectedPopulation.ToString());
        EditorGUILayout.LabelField("Growth Until Cap", 
            $"{projectedPopulation}/{settlement.populationCap} " +
            $"({(projectedPopulation * 100f / settlement.populationCap).ToString("F1")}%)");
        
        int turnsToGrow = CalculateTurnsUntilNextSize(settlement);
        if (turnsToGrow >= 0)
        {
            EditorGUILayout.LabelField("Turns Until Next Size", turnsToGrow.ToString());
        }
        else
        {
            EditorGUILayout.LabelField("Turns Until Next Size", "N/A (Max Size)");
        }
        
        GUI.enabled = true; // Re-enable GUI
        
        // Add buttons to manually control the settlement (helpful for testing)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Increase Population (+100)"))
        {
            settlement.population = Mathf.Min(settlement.population + 100, settlement.populationCap);
            EditorUtility.SetDirty(settlement);
        }
        
        if (GUILayout.Button("Simulate Turn"))
        {
            settlement.ProcessTurn();
            EditorUtility.SetDirty(settlement);
        }
    }
    
    private int CalculateTurnsUntilNextSize(Settlement settlement)
    {
        // If already at max size, return -1
        if (settlement.size == Settlement.Size.Metropolis)
        {
            return -1;
        }
        
        // Calculate turns to reach population cap
        int currentPop = settlement.population;
        int targetPop = settlement.populationCap;
        float growthRate = settlement.growthRate;
        
        int turns = 0;
        while (currentPop < targetPop && turns < 100) // 100 is a safety limit
        {
            int growth = Mathf.RoundToInt(currentPop * growthRate);
            if (growth <= 0) return 999; // No growth, would take forever
            
            currentPop = Mathf.Min(currentPop + growth, targetPop);
            turns++;
        }
        
        return turns;
    }
}
#endif