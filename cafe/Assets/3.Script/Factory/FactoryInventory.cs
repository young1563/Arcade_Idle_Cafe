using UnityEngine;
using System.Collections.Generic;

public class FactoryInventory : MonoBehaviour
{
    public static FactoryInventory Instance;

    // 현재 인벤토리에 수집된 설비 수량 <ID, 개수>
    public Dictionary<string, int> inventoryItems = new Dictionary<string, int>();

    void Awake() => Instance = this;

    // 언락존에서 호출하여 해금
    public void UnlockMachine(string id)
    {
        if (FactoryDataManager.Instance.machineTable.ContainsKey(id))
        {
            FactoryDataManager.Instance.machineTable[id].isUnlocked = true;
            Debug.Log($"[도감] {id} 잠금 해제!");
        }
    }

    // 도감 UI에서 '꺼내기' 버튼 클릭 시 호출
    public void AddToInventory(string id)
    {
        if (!FactoryDataManager.Instance.machineTable[id].isUnlocked)
        {
            Debug.Log("아직 해금되지 않은 설비입니다.");
            return;
        }

        if (inventoryItems.ContainsKey(id)) inventoryItems[id]++;
        else inventoryItems.Add(id, 1);

        Debug.Log($"[인벤토리] {id} 획득! 현재 수량: {inventoryItems[id]}");
    }
}
