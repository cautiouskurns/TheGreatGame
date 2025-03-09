using UnityEngine;

[RequireComponent(typeof(Province))]
public class ProvinceOutline : MonoBehaviour
{
    [SerializeField] private Color outlineColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private float outlineWidth = 0.05f;
    
    private LineRenderer lineRenderer;
    private Province province;
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        province = GetComponent<Province>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        CreateOutline();
    }
    
    private void CreateOutline()
    {
        // Create and configure LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 5;
        lineRenderer.startWidth = outlineWidth;
        lineRenderer.endWidth = outlineWidth;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;
        
        // Make sure it's rendered above the province
        lineRenderer.sortingLayerName = "Provinces";
        lineRenderer.sortingOrder = 1;
        
        // Set positions based on sprite size
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector2 size = spriteRenderer.sprite.bounds.size;
            float halfWidth = size.x / 2;
            float halfHeight = size.y / 2;
            float inset = outlineWidth / 2;
            
            lineRenderer.SetPosition(0, new Vector3(-halfWidth + inset, -halfHeight + inset, -0.01f));
            lineRenderer.SetPosition(1, new Vector3(halfWidth - inset, -halfHeight + inset, -0.01f));
            lineRenderer.SetPosition(2, new Vector3(halfWidth - inset, halfHeight - inset, -0.01f));
            lineRenderer.SetPosition(3, new Vector3(-halfWidth + inset, halfHeight - inset, -0.01f));
            lineRenderer.SetPosition(4, new Vector3(-halfWidth + inset, -halfHeight + inset, -0.01f));
        }
    }
}
