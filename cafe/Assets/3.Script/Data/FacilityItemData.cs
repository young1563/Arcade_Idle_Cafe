using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Factory/Item Data")]
public class FacilityItemData : ScriptableObject
{
    public string itemName;
    public int salePrice;
    public GameObject prefab;
    public Sprite icon;
}
