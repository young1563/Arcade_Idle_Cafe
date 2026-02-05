using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Customer : MonoBehaviour
{
    [Header("Model Settings")]
    // 프로젝트 창의 모델 프리팹들을 여기에 드래그해서 넣으세요
    public List<GameObject> characterPrefabs = new List<GameObject>();
    public Transform itemHoldPoint; // 아이템이 붙을 위치 (머리 위 등)

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float buyInterval = 0.5f;

    [Header("Reward Settings")]
    public GameObject moneyPrefab;
    public int rewardAmount = 50;

    private Transform _targetCounter;
    private GameObject _currentModel;
    private bool _isWaiting = false;

    /// <summary>
    /// Spawner에서 생성 직후 호출하여 목적지와 모델을 설정합니다.
    /// </summary>
    public void Init(Transform counterPos)
    {
        _targetCounter = counterPos;

        // 1. 프로젝트 에셋 중 하나를 복제 생성하여 자식으로 붙임
        SpawnRandomModel();

        // 2. 카운터로 이동 시작
        MoveToCounter();
    }

    void SpawnRandomModel()
    {
        if (characterPrefabs.Count == 0)
        {
            Debug.LogWarning("Customer: characterPrefabs 리스트가 비어있습니다!");
            return;
        }

        // 랜덤 프리팹 선택
        int randomIndex = Random.Range(0, characterPrefabs.Count);
        GameObject prefab = characterPrefabs[randomIndex];

        if (prefab != null)
        {
            // 프리팹을 내 자식으로 생성
            _currentModel = Instantiate(prefab, transform);

            // 좌표 및 스케일 초기화
            _currentModel.transform.localPosition = Vector3.zero;
            _currentModel.transform.localRotation = Quaternion.identity;
            _currentModel.transform.localScale = Vector3.one;
        }
    }

    void MoveToCounter()
    {
        if (_targetCounter == null) return;

        float distance = Vector3.Distance(transform.position, _targetCounter.position);
        float duration = distance / moveSpeed;

        // 목적지 바라보기
        transform.LookAt(_targetCounter.position);

        // DOTween을 이용한 직선 이동
        transform.DOMove(_targetCounter.position, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                _isWaiting = true;
                StartCoroutine(CheckCounterRoutine());
            });
    }

    IEnumerator CheckCounterRoutine()
    {
        while (_isWaiting)
        {
            // 카운터 스크립트 참조
            if (_targetCounter.TryGetComponent(out Counter counter))
            {
                if (counter.counterItems.Count > 0)
                {
                    GameObject item = counter.GiveToCustomer();
                    if (item != null)
                    {
                        _isWaiting = false;
                        BuyItem(item);
                    }
                }
            }
            yield return new WaitForSeconds(buyInterval);
        }
    }

    void BuyItem(GameObject item)
    {
        // 아이템을 손님 머리 위 지점으로 이동
        item.transform.SetParent(itemHoldPoint);
        item.transform.DOLocalJump(Vector3.zero, 2f, 1, 0.3f).OnComplete(() => {
            item.transform.localRotation = Quaternion.identity;

            // 돈 생성 및 보상 처리
            SpawnMoneyEffect(item.transform.position);

            // 아이템 제거 및 퇴장
            Destroy(item);
            LeaveShop();
        });
    }

    void SpawnMoneyEffect(Vector3 pos)
    {
        if (moneyPrefab == null) return;

        GameObject money = Instantiate(moneyPrefab, pos, Quaternion.identity);
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && money.TryGetComponent(out MoneyItem moneyItem))
        {
            moneyItem.FlyToPlayer(player.transform, rewardAmount);
        }
    }

    void LeaveShop()
    {
        // 180도 회전 후 앞으로 직진하여 퇴장 연출
        transform.DORotate(new Vector3(0, 180, 0), 0.5f);
        transform.DOMove(transform.position + transform.forward * -10f, 5f)
            .SetEase(Ease.Linear)
            .OnComplete(() => Destroy(gameObject));
    }
}