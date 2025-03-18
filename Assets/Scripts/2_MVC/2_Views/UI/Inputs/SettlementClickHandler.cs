using UnityEngine;

[RequireComponent(typeof(Settlement))]
public class SettlementClickHandler : MonoBehaviour
{
    private Settlement settlement;
    private Vector3 originalScale;
    
    private void Start()
    {
        settlement = GetComponent<Settlement>();
        originalScale = transform.localScale;
    }
    
    private void OnMouseDown()
    {
        // Log settlement info to console for quick reference
        Debug.Log($"Settlement: {settlement.settlementName}\n" +
                  $"Size: {settlement.size}\n" +
                  $"Population: {settlement.population}/{settlement.populationCap}\n" +
                  $"Resource Bonus: +{settlement.resourceBonus}");
        
        // Show the settlement info panel if it exists
        if (settlement != null && SettlementInfoPanel.Instance != null)
        {
            SettlementInfoPanel.Instance.ShowSettlementInfo(settlement);
        }
    }
    
    // Add hover effect for better usability
    private void OnMouseEnter()
    {
        // Increase scale slightly to indicate it's hoverable
        transform.localScale = originalScale * 1.1f;
    }
    
    private void OnMouseExit()
    {
        // Restore original scale
        transform.localScale = originalScale;
    }
}