using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("손님 설정")]
    public GameObject[] customerPrefabs; // 랜덤 모델 배열
    public Transform spawnPoint;         // 생성 위치
    public float spawnInterval = 3f;     // 생성 간격

    [Header("인원 제한")]
    public int maxCustomerCount = 5;     // 최대 손님 수

    void Start()
    {
        InvokeRepeating("Spawn", 0f, spawnInterval);
    }

    void Spawn()
    {
        // 1. 인원 제한 체크
        if (CustomerManager.Instance != null)
        {
            if (CustomerManager.Instance.waitingCustomers.Count >= maxCustomerCount)
            {
                return;
            }
        }

        if (customerPrefabs == null || customerPrefabs.Length == 0) return;

        // 2. 랜덤 프리팹 선택 및 생성
        int randomIndex = Random.Range(0, customerPrefabs.Length);
        GameObject obj = Instantiate(customerPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);

        Customer customer = obj.GetComponent<Customer>();

        // 3. 초기화 (경유지 정보 포함)
        if (customer != null && CustomerManager.Instance != null)
        {
            // Init 시점에 매니저의 [첫번째 대기열], [최종 출구], [경유지]를 전달합니다.
            customer.Init(
                CustomerManager.Instance.queuePoints[0],
                CustomerManager.Instance.exitPoint,
                CustomerManager.Instance.exitStartPosition
            );

            CustomerManager.Instance.AddCustomer(customer);
        }
    }
}