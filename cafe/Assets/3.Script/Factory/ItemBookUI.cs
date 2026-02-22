using UnityEngine;

public class ItemBookUI : MonoBehaviour
{
    // 버튼 클릭 이벤트 (인스펙터에서 FacilityData 연결)
    public void OnClickExtract(FacilityData data)
    {
        // 도감에서 인벤토리로 추가 (기존 ID 기반 유지하거나 SO 기반으로 변경 가능)
        if (FactoryInventory.Instance != null && data != null)
        {
            FactoryInventory.Instance.AddToInventory(data.facilityName); // 일단 이름 사용
        }
    }

    public void OnClickBuild(FacilityData data)
    {
        // 새로운 BuildManager로 배치 모드 시작
        if (BuildManager.Instance != null && data != null)
        {
            BuildManager.Instance.StartBuildMode(data);
        }
    }
}