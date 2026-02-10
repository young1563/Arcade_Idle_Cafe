using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("손님 설정")]
    // 여러 모델을 넣을 수 있도록 배열로 변경
    public GameObject[] customerPrefabs;
    public Transform spawnPoint;
    public float spawnInterval = 3f;

    [Header("인원 제한")]
    public int maxCustomerCount = 5; // 최대 손님 수

    void Start()
    {
        // 0초 뒤 시작, spawnInterval 간격으로 반복
        InvokeRepeating("Spawn", 0f, spawnInterval);
    }

    void Spawn()
    {
        // 1. 인원 제한 체크 (매니저의 리스트 개수 확인)
        if (CustomerManager.Instance != null)
        {
            if (CustomerManager.Instance.waitingCustomers.Count >= maxCustomerCount)
            {
                Debug.Log("줄이 꽉 찼습니다. 손님 생성을 건너뜁니다.");
                return;
            }
        }

        // 2. 프리팹 리스트가 비어있는지 체크
        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogError("Customer Prefabs 배열이 비어있습니다!");
            return;
        }

        // 3. 랜덤 프리팹 선택
        int randomIndex = Random.Range(0, customerPrefabs.Length);
        GameObject selectedPrefab = customerPrefabs[randomIndex];

        // 4. 손님 생성
        GameObject obj = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
        Customer customer = obj.GetComponent<Customer>();

        // 5. CustomerManager에 등록 및 초기화
        if (customer != null && CustomerManager.Instance != null)
        {
            // Init 함수로 첫 번째 대기 지점과 퇴장 지점 전달
            customer.Init(CustomerManager.Instance.queuePoints[0], CustomerManager.Instance.exitPoint);

            // 매니저 리스트에 추가 (줄 서기 로직 시작)
            CustomerManager.Instance.AddCustomer(customer);
        }
    }
}