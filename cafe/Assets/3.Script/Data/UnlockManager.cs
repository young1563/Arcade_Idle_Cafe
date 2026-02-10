using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance;

    [Header("해금 순서대로 가구를 넣어주세요")]
    public List<FurnitureDataHolder> allFurniture = new List<FurnitureDataHolder>();

    void Awake() => Instance = this;

    void Start()
    {
        // 시작 시 해금 안 된 가구는 끄고, 첫 번째 언락존만 활성화
        RefreshUnlockZones();
    }

    public void RefreshUnlockZones()
    {
        // 아직 해금되지 않은 첫 번째 가구 찾기
        var nextToUnlock = allFurniture.FirstOrDefault(f => !f.data.isUnlocked);

        // 모든 언락존을 찾아서 상태 업데이트
        UnlockZone[] allZones = Object.FindObjectsByType<UnlockZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var zone in allZones)
        {
            if (nextToUnlock != null && zone.targetFurniture == nextToUnlock)
            {
                zone.gameObject.SetActive(true);
            }
            else
            {
                zone.gameObject.SetActive(false);
            }
        }
    }
}