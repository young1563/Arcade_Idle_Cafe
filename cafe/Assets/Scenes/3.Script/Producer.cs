using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Producer : MonoBehaviour
{
    [Header("Settings")]
    public GameObject itemPrefab;    // 생산할 디저트 프리팹
    public Transform stackPoint;     // 쌓일 위치
    public float produceSpeed = 0.5f; // 생산 속도 (초)
    public int maxCapacity = 10;     // 카운터 최대 보관량

    [Header("Stack Info")]
    public List<GameObject> spawnedItems = new List<GameObject>();
    public float yOffset = 0.4f;     // 쌓이는 간격

    private float _timer;

    void Update()
    {
        // 보관량이 가득 차지 않았을 때만 생산
        if (spawnedItems.Count < maxCapacity)
        {
            _timer += Time.deltaTime;
            if (_timer >= produceSpeed)
            {
                ProduceItem();
                _timer = 0;
            }
        }
    }

    void ProduceItem()
    {
        // 1. 생성 위치 계산
        Vector3 targetPos = stackPoint.position + new Vector3(0, spawnedItems.Count * yOffset, 0);

        // 2. 생성 및 리스트 등록
        GameObject item = Instantiate(itemPrefab, targetPos, Quaternion.identity, stackPoint);
        spawnedItems.Add(item);

        // 3. 최적화: 물리 비활성화 (생산된 상태에선 물리 연산이 필요 없음)
        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;

        // 4. 연출: 팅~ 하며 나타나는 효과 (없으면 심심함)
        item.transform.localScale = Vector3.zero;
        item.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    // 플레이어가 가져갈 때 호출할 함수
    public GameObject GiveItem()
    {
        if (spawnedItems.Count == 0) return null;

        GameObject item = spawnedItems[spawnedItems.Count - 1];
        spawnedItems.RemoveAt(spawnedItems.Count - 1);
        return item;
    }
}