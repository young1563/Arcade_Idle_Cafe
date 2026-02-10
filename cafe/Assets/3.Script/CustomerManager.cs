using UnityEngine;
using System.Collections.Generic;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("연결 설정")]
    public Counter counter;             // Counter 스크립트 연결
    public GameObject moneyPrefab;      // 생성할 돈 프리팹
    public Transform exitPoint;         // 손님이 나갈 위치
    public GameObject itemPrefab;       // 손님 손에 쥐어줄 디저트 프리팹

    [Header("줄 서기 설정")]
    public Transform[] queuePoints;     // 줄 서는 위치들 (0번 인덱스가 맨 앞)
    public List<Customer> waitingCustomers = new List<Customer>();

    [Header("판매 로직")]
    public float sellInterval = 0.5f;   // 판매 간격
    private float _sellTimer;
    private bool _isPlayerInZone = false;

    void Awake() => Instance = this;

    // 플레이어가 판매 구역(Trigger)에 들어왔는지 체크
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) _isPlayerInZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _isPlayerInZone = false;
    }

    void Update()
    {
        // 플레이어가 영역 내에 있고, 줄 선 손님이 있으며, 카운터에 아이템이 있을 때
        if (_isPlayerInZone && waitingCustomers.Count > 0 && counter.counterItems.Count > 0)
        {
            _sellTimer += Time.deltaTime;
            if (_sellTimer >= sellInterval)
            {
                TrySellItem();
                _sellTimer = 0;
            }
        }
        else
        {
            _sellTimer = 0; // 조건 안 맞으면 타이머 초기화
        }
    }

    void TrySellItem()
    {
        // 1. 카운터에서 아이템 하나 꺼내기 (Counter.cs의 함수 호출)
        GameObject itemFromCounter = counter.GiveToCustomer();

        if (itemFromCounter != null)
        {
            // 카운터에서 꺼낸 물리적인 아이템 오브젝트는 파괴 (또는 풀링)
            Destroy(itemFromCounter);

            // 2. 맨 앞 손님 정보 가져오기
            Customer firstCustomer = waitingCustomers[0];

            // 3. 손님에게 아이템 전달 (Customer.cs의 함수 호출)
            // 손님의 자식 오브젝트 중 손 위치를 지정하거나 transform을 직접 넘깁니다.
            firstCustomer.GetItem(itemPrefab, firstCustomer.transform);

            // 4. 돈 생성
            SpawnMoney();

            // 5. 리스트에서 제거 및 줄 갱신
            waitingCustomers.RemoveAt(0);
            UpdateQueue();
        }
    }

    // 새로운 손님이 들어올 때 호출할 함수 (Spawner 등에서 사용)
    public void AddCustomer(Customer newCustomer)
    {
        waitingCustomers.Add(newCustomer);
        UpdateQueue();
    }

    // 손님들을 다음 칸으로 이동시킴
    public void UpdateQueue()
    {
        for (int i = 0; i < waitingCustomers.Count; i++)
        {
            if (i < queuePoints.Length)
            {
                waitingCustomers[i].MoveTo(queuePoints[i].position);
            }
        }
    }

    void SpawnMoney()
    {
        // 판매 구역 근처에 돈 생성
        Instantiate(moneyPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
    }
}