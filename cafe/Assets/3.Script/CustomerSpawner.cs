using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab; // 1번에서 만든 프리팹
    public Transform spawnPoint;      // 생성 위치
    public float spawnInterval = 3f;  // 생성 간격

    void Start()
    {
        InvokeRepeating("Spawn", 0f, spawnInterval);
    }

    void Spawn()
    {
        // 1. 손님 생성
        GameObject obj = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        Customer customer = obj.GetComponent<Customer>();

        // 2. CustomerManager에 등록 (줄 서기 시작)
        if (customer != null && CustomerManager.Instance != null)
        {
            // Init 함수로 탈출 지점과 목표 지점 전달
            customer.Init(CustomerManager.Instance.queuePoints[0], CustomerManager.Instance.exitPoint);
            // 매니저 리스트에 추가
            CustomerManager.Instance.AddCustomer(customer);
        }
    }
}