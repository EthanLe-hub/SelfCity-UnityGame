using System.Collections.Generic; // We are using System.Collections.Generic for List<T> and other collection types. 
using UnityEngine; // We need to use UnityEngine for MonoBehaviour and other Unity types.

namespace LifeCraft.Shop // This namespace is for all shop-related classes. 
{
    [CreateAssetMenu(fileName = "BuildingShopDatabase", menuName = "Shop/Building Shop Database")]
    public class BuildingShopDatabase : ScriptableObject // This class is a scriptable object that holds a list of building shop items. 
    {
        public List<BuildingShopItem> buildings; // This is a list of building shop items that can be purchased in the shop.
    }
}