using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Customer : MonoBehaviour
{
    [Header("Movement & Detection")]
    public float moveSpeed = 3f;
    public float buyInterval = 0.5f; // 카운터에서 아이템을 체크하는 간격

    [Header("Money Settings")]
    public GameObject moneyPrefab;   // 생성할 돈 프리팹
    public int rewardAmount = 50;    // 아이템 하나당 가격

    private Counter _targetCounter;  // 감지된 카운터
    private bool _hasPurchased = false; // 구매 완료 여부
    private Transform _itemHoldPoint; // 손님이 아이템을 들 위치 (머리 위 등)

    void Awake()
    {
        // 아이템을 들 위치가 없다면 자식으로 생성
        GameObject holdPoint = new GameObject("ItemHoldPoint");
        holdPoint.transform.SetParent(this.transform);
        holdPoint.transform.localPosition = new Vector3(0, 2f, 0);
        _itemHoldPoint = holdPoint.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 카운터 영역에 진입했을 때
        if (other.CompareTag("Counter") && !_hasPurchased)
        {
            _targetCounter = other.GetComponent<Counter>();
            if (_targetCounter != null)
            {
                StartCoroutine(BuyRoutine());
            }
        }
    }

    IEnumerator BuyRoutine()
    {
        // 카운터에 아이템이 생길 때까지 대기하며 체크
        while (!_hasPurchased)
        {
            if (_targetCounter != null && _targetCounter.counterItems.Count > 0)
            {
                // 카운터에서 아이템 하나 가져오기
                GameObject item = _targetCounter.GiveToCustomer();
                if (item != null)
                {
                    _hasPurchased = true;
                    HandlePurchase(item);
                }
            }
            // 너무 자주 체크하지 않도록 대기
            yield return new WaitForSeconds(buyInterval);
        }
    }

    void HandlePurchase(GameObject item)
    {
        // 1. 아이템을 손님에게 귀속시키고 점프 연출
        item.transform.SetParent(_itemHoldPoint);
        item.transform.DOLocalJump(Vector3.zero, 2f, 1, 0.3f).OnComplete(() => {
            item.transform.localRotation = Quaternion.identity;

            // 2. 돈 생성 및 날리기 연출
            SpawnMoneyEffect(item.transform.position);

            // 3. 아이템 파괴 및 퇴장
            Destroy(item, 0.2f);
            LeaveShop();
        });
    }

    void SpawnMoneyEffect(Vector3 spawnPos)
    {
        if (moneyPrefab == null) return;

        // 아이템이 있던 자리에서 돈 생성
        GameObject money = Instantiate(moneyPrefab, spawnPos, Quaternion.identity);

        // 플레이어 태그를 가진 오브젝트 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && money.TryGetComponent(out MoneyItem moneyItem))
        {
            // MoneyItem 스크립트의 비행 로직 실행
            moneyItem.FlyToPlayer(player.transform, rewardAmount);
        }
    }

    void LeaveShop()
    {
        Debug.Log("구매 완료! 손님이 매장을 떠납니다.");

        // 간단한 퇴장 로직 (예: 뒤로 돌아서 직진 후 파괴)
        transform.DORotate(new Vector3(0, 180, 0), 0.5f);
        transform.DOMove(transform.position + transform.forward * -10f, 5f)
                 .SetEase(Ease.Linear)
                 .OnComplete(() => Destroy(gameObject));
    }
}