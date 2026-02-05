using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject customerPrefab; // 위에서 만든 Customer 프리팹

    [Header("Target")]
    public Transform counterTransform; // 손님이 걸어갈 목적지 (카운터 앞)

    [Header("Settings")]
    public float spawnInterval = 5f; // 손님이 나오는 간격 (초)
    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= spawnInterval)
        {
            SpawnCustomer();
            _timer = 0;
        }
    }

    void SpawnCustomer()
    {
        if (customerPrefab == null || counterTransform == null) return;

        // 입구 위치에서 손님 생성
        GameObject customerObj = Instantiate(customerPrefab, transform.position, Quaternion.identity);

        // Customer 스크립트를 가져와 초기화 (목적지 전달 및 모델 랜덤 설정)
        Customer customer = customerObj.GetComponent<Customer>();
        if (customer != null)
        {
            customer.Init(counterTransform);
        }
    }
}