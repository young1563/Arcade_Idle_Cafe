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
        allFurniture = FindObjectsOfType<FurnitureDataHolder>()
                        .OrderBy(f => f.data.unlockOrder)
                        .ToList();

        RefreshUnlockZones();
    }

    public void RefreshUnlockZones()
    {
        // 아직 해금되지 않은 가구 중 가장 낮은 unlockOrder를 찾음
        var nextToUnlock = allFurniture.FirstOrDefault(f => !f.data.isUnlocked);

        if (nextToUnlock != null)
        {
            currentOrder = nextToUnlock.data.unlockOrder;
            Debug.Log($"다음 해금 목표: {nextToUnlock.data.prefabName} (순서: {currentOrder})");

            // 모든 언락존을 돌면서 현재 순서인 것만 켜줌
            foreach (var zone in FindObjectsOfType<UnlockZone>(true))
            {
                // 언락존이 담당하는 가구의 order가 현재 순서와 같으면 활성화
                bool isNext = zone.targetFurniture.data.unlockOrder == currentOrder;
                zone.gameObject.SetActive(isNext);
            }
        }
        else
        {
            Debug.Log("축하합니다! 모든 가구를 해금했습니다.");
        }
    }
}