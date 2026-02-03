using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; // DOTween 필수

public class Producer : MonoBehaviour
{
    [Header("Item Settings")]
    public GameObject itemPrefab;
    public Transform stackPoint;
    public AnimationCurve produceCurve; // 0에서 1로 가는 커브 추천
    public float produceSpeed = 0.5f;
    public int maxCapacity = 24;

    [Header("Grid Settings")]
    public int columns = 4;
    public int rows = 3;
    public float xSpacing = 0.6f;
    public float zSpacing = 0.6f;
    public float yOffset = 0.5f;

    public List<GameObject> spawnedItems = new List<GameObject>();
    private float _timer;

    void Update()
    {
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
        int index = spawnedItems.Count;

        // 그리드 위치 계산
        int layer = index / (columns * rows);
        int remaining = index % (columns * rows);
        int row = remaining / columns;
        int col = remaining % columns;

        Vector3 localPos = new Vector3(
            col * xSpacing,
            layer * yOffset,
            row * zSpacing
        );

        // 생성
        GameObject item = Instantiate(itemPrefab, stackPoint);
        // 1. 프리팹이 가진 원래 스케일(25, 25, 25)을 변수에 담습니다.
        Vector3 originalScale = item.transform.localScale;

        item.transform.localPosition = localPos;
        item.transform.localRotation = Quaternion.identity;

        // 2. 시작은 0으로
        item.transform.localScale = Vector3.zero;

        // 3. Vector3.one 대신 저장해둔 originalScale로 키웁니다.
        item.transform.DOScale(originalScale, 0.3f).SetEase(produceCurve);

        spawnedItems.Add(item);
    }

    public GameObject GiveItem()
    {
        if (spawnedItems.Count == 0) return null;

        int lastIndex = spawnedItems.Count - 1;
        GameObject item = spawnedItems[lastIndex];
        spawnedItems.RemoveAt(lastIndex);

        // [핵심] 가져가는 순간 애니메이션 강제 종료 및 크기 고정
        // DOKill()을 해주면 더 이상 스케일이 변하지 않습니다.
        item.transform.DOKill();
        // 여기서도 Vector3.one 대신 실제 프리팹의 스케일로 고정해야 합니다.
        // 만약 모든 디저트가 똑같이 25라면 직접 넣어도 되지만, 
        // 프리팹마다 다를 수 있으니 아래처럼 처리하는게 가장 안전합니다.
        item.transform.localScale = itemPrefab.transform.localScale;

        // 부모 해제
        item.transform.SetParent(null);
        return item;
    }
}