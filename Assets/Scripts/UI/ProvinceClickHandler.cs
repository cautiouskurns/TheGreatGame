// ProvinceClickHandler.cs
using UnityEngine;

public class ProvinceClickHandler : MonoBehaviour
{
    private Province province;
    
    void Start()
    {
        province = GetComponent<Province>();
    }
    
    void OnMouseDown()
    {
        if (province != null)
        {
            // Try to claim this province
            GameManager.Instance.ClaimProvince(province);
        }
    }
}