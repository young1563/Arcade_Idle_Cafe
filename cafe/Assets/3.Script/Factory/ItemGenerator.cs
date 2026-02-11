using UnityEngine;
using System.Collections;

public class ItemGenerator : MonoBehaviour
{
    public GameObject itemPrefab; // 생성할 아이템 프리팹
    public Transform spawnPoint;  // 생성 위치
    public float interval = 2.0f; // 생성 간격

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            // 앞에 아이템이 이미 있는지 체크 (막힘 방지)
            if (!Physics.CheckSphere(spawnPoint.position, 0.2f))
            {
                GameObject obj = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
                obj.tag = "FactoryItem"; // 태그 확인 필수
            }
        }
    }
}