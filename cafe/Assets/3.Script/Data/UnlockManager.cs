using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance;

    private List<FurnitureDataHolder> allFurniture;
    private int currentOrder = 0; // 현재 해금해야 할 순서

    void Awake() => Instance = this;

    // MapLoader가 가구 소환을 마친 후 호출할 함수
    public void InitUnlockSystem()
    {
        // 씬에 있는 모든 가구 데이터를 가져와서 순서대로 정렬
        allFurniture = FindObjectsByType<FurnitureDataHolder>(FindObjectsSortMode.None)
                        .OrderBy(f => f.data.unlockOrder)
                        .ToList();

        RefreshUnlockZones();
    }

    public void RefreshUnlockZones()
{
    // 1. 아직 해금 안 된 가구 중 가장 낮은 순서 찾기
    var nextToUnlock = allFurniture.FirstOrDefault(f => !f.data.isUnlocked);

    if (nextToUnlock != null)
    {
        currentOrder = nextToUnlock.data.unlockOrder;
        
        // 2. 씬의 모든 언락존을 찾지 말고, 필요한 조건만 체크
        UnlockZone[] allZones = Resources.FindObjectsOfTypeAll<UnlockZone>(); 
        foreach (var zone in allZones)
        {
            // 이미 해금된 가구의 언락존은 절대 켜지지 않게 방어 로직 추가
            if (zone.targetFurniture.data.isUnlocked)
            {
                zone.gameObject.SetActive(false);
                continue;
            }

            // 현재 순서와 일치하는 언락존만 활성화
            bool isNext = zone.targetFurniture.data.unlockOrder == currentOrder;
            zone.gameObject.SetActive(isNext);
        }
    }
}
}