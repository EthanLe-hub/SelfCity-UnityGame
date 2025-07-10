using System.Collections.Generic; // Using the System.Collections.Generic namespace for List<T>, which will be used to store decoration items. 
using UnityEngine; // We need UnityEngine for ScriptableObject. 

[CreateAssetMenu(fileName = "DecorationDatabase", menuName = "Shop/Decoration Database")] // Create a new ScriptableObject for the decoration database. 
public class DecorationDatabase : ScriptableObject // DecorationDatabase inherits from ScriptableObject to allow it to be used as a database. 
{
    [Header("Decorations for Free & Premium Players")] // Header for organization in the Unity Inspector. 
    public List<string> freeAndPremiumDecorations; // List of decoration names available for both free and premium players. 

    [Header("Decorations for Premium Only Players")] // Header for organization in the Unity Inspector. 
    public List<string> premiumOnlyDecorations; // List of decoration names available only for premium players. 
}