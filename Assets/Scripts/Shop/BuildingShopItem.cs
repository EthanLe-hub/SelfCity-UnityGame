using UnityEngine; // We need to use UnityEngine for MonoBehaviour and other Unity types. 

namespace LifeCraft.Shop // This namespace is for all shop-related classes. 
{
    [System.Serializable] // This attribute allows the class to be serialized, which is useful for saving and loading data. 
    public class BuildingShopItem // This class represents an item that can be bought in the building shop. 
    {
        public string name; // The name of the building item. 
        public string description; // A description of the building item. 
        public int price; // The price of the building item in the shop. 
        // Removed icon field - now gets sprite from CityBuilder
    }
}