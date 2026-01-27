using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerStack : MonoBehaviour
{
    public Transform stackParent; // 아이템이 쌓일 위치 (플레이어 등 쪽)
    public List<GameObject> collectedItems = new List<GameObject>();
    public int maxCapacity = 10; // 초기 수용량 (업그레이드 요소)
    public float yOffset = 0.4f; // 아이템 간 높이 간격

    public void AddItem(GameObject item)
    {
        if (collectedItems.Count >= maxCapacity) return;

        // 1. 물리 비활성화 (최적화의 핵심)
        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col)) col.enabled = false;

        // 2. 리스트 등록 및 부모 설정
        collectedItems.Add(item);
        item.transform.SetParent(stackParent);

        // 3. 애니메이션 연출 (촥촥 쌓이는 느낌)
        Vector3 targetPos = new Vector3(0, (collectedItems.Count - 1) * yOffset, 0);
        StartCoroutine(MoveToStack(item.transform, targetPos));
    }

    IEnumerator MoveToStack(Transform item, Vector3 target)
    {
        float elapsed = 0;
        Vector3 startPos = item.localPosition;
        while (elapsed < 0.15f) // 0.15초 내에 빠르게 이동
        {
            item.localPosition = Vector3.Lerp(startPos, target, elapsed / 0.15f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        item.localPosition = target;
        item.localRotation = Quaternion.identity;
    }
}