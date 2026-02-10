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
        // 리스트가 비어있는지 확인
        if (allFurniture == null || allFurniture.Count == 0) return;

        var nextToUnlock = allFurniture.FirstOrDefault(f => !f.data.isUnlocked);
        if (nextToUnlock != null)
        {
            currentOrder = nextToUnlock.data.unlockOrder;

            // Inactive 상태인 것까지 포함해서 모두 찾기
            var allZones = Object.FindObjectsByType<UnlockZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var zone in allZones)
            {
                // [에러 방지] targetFurniture가 연결되어 있는지 반드시 확인
                if (zone == null || zone.targetFurniture == null || zone.targetFurniture.data == null)
                {
                    continue;
                }

                if (zone.targetFurniture.data.isUnlocked)
                {
                    zone.gameObject.SetActive(false);
                    continue;
                }

                bool isNext = zone.targetFurniture.data.unlockOrder == currentOrder;
                zone.gameObject.SetActive(isNext);
            }
        }
    }
}