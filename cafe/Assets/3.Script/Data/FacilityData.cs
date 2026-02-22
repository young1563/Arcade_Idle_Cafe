using UnityEngine;

public enum FacilityType
{
    Generator,  // Produces basic items
    Processor,  // Transforms 1 item into another
    Merger,     // Transforms multiple items into 1
    Conveyor,   // Moves items
    Seller      // Sells items
}

[System.Serializable]
public struct Recipe
{
    public FacilityItemData[] inputItems;
    public FacilityItemData outputItem;
    public float processTime;
}

[CreateAssetMenu(fileName = "NewFacilityData", menuName = "Factory/Facility Data")]
public class FacilityData : ScriptableObject
{
    public string facilityName;
    public string facilityDescription;
    public int price;
    public GameObject prefab;
    public FacilityType type;
    public Recipe recipe;
    public Sprite icon;
}
