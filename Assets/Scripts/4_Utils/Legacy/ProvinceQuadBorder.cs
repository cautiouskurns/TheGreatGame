using UnityEngine;

public class ProvinceQuadBorder : MonoBehaviour
{
    [SerializeField] private Province province;
    [SerializeField] private float borderWidth = 0.1f;
    [SerializeField] private Color borderColor = Color.black;
    
    private GameObject[] borderQuads = new GameObject[4];
    
    void Start()
    {
        if (province == null)
            province = GetComponent<Province>();
        
        CreateBorderQuads();
        
        // Subscribe to ownership changes
        Province.OnOwnershipChanged += OnProvinceOwnershipChanged;
    }
    
    void OnDestroy()
    {
        Province.OnOwnershipChanged -= OnProvinceOwnershipChanged;
    }
    
    private void OnProvinceOwnershipChanged(Province changedProvince, Nation oldOwner, Nation newOwner)
    {
        if (changedProvince == province)
        {
            UpdateBorderColor(newOwner != null ? newOwner.nationColor : borderColor);
        }
    }
    
    private void CreateBorderQuads()
    {
        // Get sprite size
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;
            
        Vector2 size = spriteRenderer.sprite.bounds.size;
        float width = size.x;
        float height = size.y;
        
        // Create four quads for each border side
        CreateQuad(0, new Vector2(width, borderWidth), new Vector3(0, -height/2 + borderWidth/2, -0.01f)); // Bottom
        CreateQuad(1, new Vector2(width, borderWidth), new Vector3(0, height/2 - borderWidth/2, -0.01f));  // Top
        CreateQuad(2, new Vector2(borderWidth, height), new Vector3(-width/2 + borderWidth/2, 0, -0.01f)); // Left
        CreateQuad(3, new Vector2(borderWidth, height), new Vector3(width/2 - borderWidth/2, 0, -0.01f));  // Right
        
        // Set initial color
        UpdateBorderColor(province.ownerNation != null ? province.ownerNation.nationColor : borderColor);
    }
    
    private void CreateQuad(int index, Vector2 size, Vector3 localPosition)
    {
        GameObject quad = new GameObject($"Border_{index}");
        quad.transform.parent = transform;
        quad.transform.localPosition = localPosition;
        quad.transform.localRotation = Quaternion.identity;
        quad.transform.localScale = new Vector3(size.x, size.y, 1);
        
        SpriteRenderer renderer = quad.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateRectangleSprite(Color.white);
        renderer.sortingLayerName = "Provinces";
        renderer.sortingOrder = 1;
        
        borderQuads[index] = quad;
    }
    
    private void UpdateBorderColor(Color color)
    {
        foreach (GameObject quad in borderQuads)
        {
            if (quad != null)
            {
                SpriteRenderer renderer = quad.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = color;
                }
            }
        }
    }
    
    // Helper to create a simple white rectangle sprite
    private Sprite CreateRectangleSprite(Color color)
    {
        Texture2D texture = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = color;
        
        texture.SetPixels(colors);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }
}
