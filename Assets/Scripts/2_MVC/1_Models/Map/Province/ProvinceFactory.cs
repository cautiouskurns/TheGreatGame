using UnityEngine;

public class ProvinceFactory : MonoBehaviour
{
    public static Province CreateProvince(GameObject prefab, Vector3 position, int x, int y)
    {
        // Instantiate the prefab
        GameObject provinceObj = Instantiate(prefab, position, Quaternion.identity);
        provinceObj.name = $"Province_{x}_{y}";
        
        // Create the model
        ProvinceModel model = new ProvinceModel(x, y);
        
        // Get the view component
        ProvinceView view = provinceObj.GetComponent<ProvinceView>();
        
        // Create the controller and link everything
        ProvinceController controller = new ProvinceController(model, view);
        
        // Store references using a component to link Unity object with MVC
        Province province = provinceObj.GetComponent<Province>() ?? provinceObj.AddComponent<Province>();
        province.Initialize(model, view, controller);
        
        return province;
    }
}
