using UnityEngine;

public class ItemBookUI : MonoBehaviour
{
    // 버튼 클릭 이벤트 (인스펙터에서 ID 입력)
    public void OnClickExtract(string machineID)
    {
        // 1. 도감에서 인벤토리로 추가
        FactoryInventory.Instance.AddToInventory(machineID);
    }

    public void OnClickBuild(string machineID)
    {
        // 2. 인벤토리에서 꺼내 배치 모드 시작
        PlacementManager.Instance.StartPlacement(machineID);
    }
}