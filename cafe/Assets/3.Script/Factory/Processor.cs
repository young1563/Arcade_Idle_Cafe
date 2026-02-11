using UnityEngine;
using System.Collections;

public class Processor : MonoBehaviour
{
    public Transform inputPoint;  // 재료가 멈출 지점
    public Transform outputPoint; // 결과물이 나갈 지점
    public float processTime = 2.0f; // 가공 시간
    public GameObject resultPrefab; // 결과물 프리팹 (예: 식빵)

    private bool _isProcessing = false;

    private void OnTriggerEnter(Collider other)
    {
        // 가공 중이 아닐 때만 재료를 받음
        if (!_isProcessing && other.CompareTag("FactoryItem"))
        {
            StartCoroutine(ProcessRoutine(other.gameObject));
        }
    }

    IEnumerator ProcessRoutine(GameObject ingredient)
    {
        _isProcessing = true;

        // 1. 재료를 입구 위치에 고정시키고 벨트 이동 정지
        ingredient.transform.position = inputPoint.position;
        FactoryItem item = ingredient.GetComponent<FactoryItem>();
        if (item != null) item.SetTarget(null, 0);

        Debug.Log("가공 시작...");
        yield return new WaitForSeconds(processTime);

        // 2. 재료 파괴 및 결과물 생성
        Destroy(ingredient);
        GameObject result = Instantiate(resultPrefab, outputPoint.position, Quaternion.identity);
        result.tag = "FactoryItem";

        Debug.Log("가공 완료! 결과물 배출");
        _isProcessing = false;
    }
}