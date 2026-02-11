using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class PlayerStack : MonoBehaviour
{
    public Transform stackParent;
    public List<GameObject> stackedItems = new List<GameObject>();
    public float itemHeight = 0.3f;
    public int maxCapacity = 10; // 최대 소지 개수
    public Animator animator; // 플레이어의 Animator 컴포넌트 연결

    [Header("수집 설정")]
    public float collectInterval = 0.1f; // 아이템 간 수집 간격
    private float lastCollectTime;

    void OnTriggerStay(Collider other)
    {
        // 1. 수집 간격 체크 및 용량 체크
        if (Time.time - lastCollectTime < collectInterval) return;
        if (stackedItems.Count >= maxCapacity) return;

        if (other.CompareTag("Producer"))
        {
            if (other.TryGetComponent(out Producer producer))
            {
                GameObject item = producer.GiveItem();
                if (item != null)
                {
                    AddStack(item);
                    lastCollectTime = Time.time; // 시간 갱신
                }
            }
        }
    }

    public void AddStack(GameObject item)
    {
        stackedItems.Add(item);

        // 2. 물리 컴포넌트 비활성화 (필수)
        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col)) col.enabled = false;

        item.transform.SetParent(stackParent);

        float targetY = (stackedItems.Count - 1) * itemHeight;

        // 3. 기존 트윈이 있다면 제거 (안정성)
        item.transform.DOKill();

        item.transform.DOLocalJump(new Vector3(0, targetY, 0), 2f, 1, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                item.transform.localRotation = Quaternion.identity;
                item.transform.localPosition = new Vector3(0, targetY, 0);
            });

        UpdateAnimator(); // 애니메이션 상태 갱신
    }

    public GameObject RemoveStack()
    {
        if (stackedItems.Count == 0) return null;

        GameObject lastItem = stackedItems[stackedItems.Count - 1];
        stackedItems.RemoveAt(stackedItems.Count - 1);

        // 제거할 때도 트윈을 꺼주는 것이 안전합니다.
        lastItem.transform.DOKill();

        UpdateAnimator(); // 애니메이션 상태 갱신
        return lastItem;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        // 아이템이 하나라도 있으면 true, 없으면 false
        bool isCarrying = stackedItems.Count > 0;
        Debug.Log($"애니메이터 갱신 중: {isCarrying}"); // 이 로그가 찍히는지 확인
        animator.SetBool("IsCarrying", isCarrying);
    }
}