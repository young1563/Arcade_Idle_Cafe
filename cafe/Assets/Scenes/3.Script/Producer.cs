using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Producer : MonoBehaviour
{
    [Header("Item Settings")]
    public GameObject itemPrefab;
    public Transform stackPoint;
    public AnimationCurve produceCurve;
    public float produceSpeed = 0.5f;
    public int maxCapacity = 36; // 예: 4x3x3층 = 36개

    [Header("Grid Settings")]
    public int columns = 4;      // 가로(X) 개수
    public int rows = 3;         // 세로(Z) 개수
    public float xSpacing = 0.6f; // 가로 간격
    public float zSpacing = 0.6f; // 세로 간격
    public float yOffset = 0.5f;  // 층간 높이 간격

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
        // 핵심 로직: 현재 아이템 인덱스로 그리드 위치 계산
        int index = spawnedItems.Count;

        // 1. 몇 번째 층인지 계산 (Y)
        int layer = index / (columns * rows);
        // 2. 해당 층 내에서 몇 번째 칸인지 계산
        int remaining = index % (columns * rows);
        // 3. 해당 칸의 행(Z)과 열(X) 계산
        int row = remaining / columns;
        int col = remaining % columns;

        // 최종 좌표값 계산 (StackPoint 기준 Local 좌표)
        Vector3 localPos = new Vector3(
            col * xSpacing,
            layer * yOffset,
            row * zSpacing
        );

        // 생성 (StackPoint를 부모로 하여 좌표 적용)
        GameObject item = Instantiate(itemPrefab, stackPoint);
        item.transform.localPosition = localPos;
        item.transform.localRotation = Quaternion.identity;

        spawnedItems.Add(item);
        StartCoroutine(ProduceAnimation(item.transform));
    }

    IEnumerator ProduceAnimation(Transform t)
    {
        float elapsed = 0;
        float duration = 0.3f;
        Vector3 originalScale = new Vector3(10, 10, 10); // 설정하신 스케일

        while (elapsed < duration)
        {
            t.localScale = originalScale * produceCurve.Evaluate(elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        t.localScale = originalScale;
    }

    public GameObject GiveItem()
    {
        if (spawnedItems.Count == 0) return null;
        GameObject item = spawnedItems[spawnedItems.Count - 1];
        spawnedItems.RemoveAt(spawnedItems.Count - 1);
        return item;
    }
}