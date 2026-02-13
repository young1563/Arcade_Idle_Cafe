using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Merger : MonoBehaviour
{
    [Header("설정")]
    public float processTime = 3.0f;
    public GameObject resultPrefab; // 합성 결과물 (예: 크림빵)
    public Transform outputPoint;   // 결과물 배출구

    [Header("입력 슬롯")]
    public Transform[] inputPoints; // 재료들이 멈춰서 대기할 위치들 (예: 2개)

    // 현재 슬롯에 들어온 아이템들을 추적하는 리스트
    private List<GameObject> _currentIngredients = new List<GameObject>();
    private bool _isProcessing = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FactoryItem"))
        {
            // 이미 가공 중이거나 슬롯이 꽉 찼으면 무시
            if (_isProcessing || _currentIngredients.Count >= inputPoints.Length) return;

            StartCoroutine(AcceptIngredient(other.gameObject));
        }
    }

    IEnumerator AcceptIngredient(GameObject ingredient)
    {
        // 1. 재료를 비어있는 입력 슬롯으로 이동시키기
        int slotIndex = _currentIngredients.Count;
        _currentIngredients.Add(ingredient);

        // 벨트 이동 중지 및 위치 고정
        FactoryItem item = ingredient.GetComponent<FactoryItem>();
        if (item != null) item.SetTarget(null, 0);
        ingredient.transform.position = inputPoints[slotIndex].position;

        Debug.Log($"재료 입고! ({_currentIngredients.Count}/{inputPoints.Length})");

        // 2. 모든 재료가 모였는지 확인
        if (_currentIngredients.Count == inputPoints.Length)
        {
            yield return StartCoroutine(ProcessRoutine());
        }
    }

    IEnumerator ProcessRoutine()
    {
        _isProcessing = true;
        Debug.Log("합성 시작...");

        yield return new WaitForSeconds(processTime);

        // 3. 재료들 모두 파괴
        foreach (var ingredient in _currentIngredients)
        {
            Destroy(ingredient);
        }
        _currentIngredients.Clear();

        // 4. 결과물 생성
        GameObject result = Instantiate(resultPrefab, outputPoint.position, Quaternion.identity);
        result.tag = "FactoryItem";

        Debug.Log("합성 완료! 상위 아이템 배출");
        _isProcessing = false;
    }
}