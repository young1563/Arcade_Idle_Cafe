using UnityEngine;
using System.Collections.Generic;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("연결 설정")]
    public Counter counter;
    public GameObject moneyPrefab;
    public Transform exitPoint;         // 최종 사라지는 지점
    public Transform exitStartPosition; // [추가] 옆으로 비켜나가는 경유지 지점
    public GameObject itemPrefab;

    [Header("줄 서기 설정")]
    public Transform[] queuePoints;
    public List<Customer> waitingCustomers = new List<Customer>();

    [Header("판매 로직")]
    public float sellInterval = 0.5f;
    private float _sellTimer;
    private bool _isPlayerInZone = false;

    void Awake() => Instance = this;

    void Start()
    {
        // 자식 오브젝트에 Counter가 있을 경우 자동 연결 (보험)
        if (counter == null) counter = GetComponentInChildren<Counter>();
    }

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
        if (_isPlayerInZone && waitingCustomers.Count > 0 && counter.counterItems.Count > 0)
        {
            _sellTimer += Time.deltaTime;
            if (_sellTimer >= sellInterval)
            {
                TrySellItem();
                _sellTimer = 0;
            }
        }
    }

    void TrySellItem()
    {
        GameObject itemFromCounter = counter.GiveToCustomer();

        if (itemFromCounter != null)
        {
            Destroy(itemFromCounter);

            // 1. 맨 앞 손님 추출 및 리스트에서 제거
            Customer firstCustomer = waitingCustomers[0];
            waitingCustomers.RemoveAt(0);

            // 2. 손님에게 아이템 전달 (내부에서 Leave 호출됨)
            if (itemPrefab != null)
            {
                firstCustomer.GetItem(itemPrefab, firstCustomer.transform);
            }

            // 3. 돈 생성
            SpawnMoney();

            // 4. 나머지 손님들 줄 이동
            UpdateQueue();

            Debug.Log("판매 완료: 손님이 경유지를 거쳐 퇴장합니다.");
        }
    }

    public void AddCustomer(Customer newCustomer)
    {
        waitingCustomers.Add(newCustomer);
        UpdateQueue();
    }

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
        Instantiate(moneyPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
    }
}