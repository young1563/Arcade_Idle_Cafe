using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BaseFacility : MonoBehaviour
{
    [Header("Data")]
    public FacilityData facilityData;

    [Header("Ports")]
    public Transform[] inputPoints;
    public Transform outputPoint;

    protected List<GameObject> activeIngredients = new List<GameObject>();
    protected List<GameObject> waitingQueue = new List<GameObject>();
    protected bool isProcessing = false;

    protected virtual void Start()
    {
        if (facilityData != null && facilityData.type == FacilityType.Generator)
        {
            StartCoroutine(GeneratorRoutine());
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (facilityData == null) return;
        if (facilityData.type == FacilityType.Conveyor || facilityData.type == FacilityType.Seller || facilityData.type == FacilityType.Generator) return;

        if (other.CompareTag("FactoryItem"))
        {
            FactoryItem item = other.GetComponent<FactoryItem>();
            if (item == null) return;

            // 이미 가공 중이거나 자리가 없으면 대기열로
            if (isProcessing || activeIngredients.Count >= facilityData.recipe.inputItems.Length)
            {
                item.SetTarget(null, 0);
                if (!waitingQueue.Contains(other.gameObject)) waitingQueue.Add(other.gameObject);
                return;
            }

            // 들어온 아이템이 레시피에 필요한 것인지 확인 (Merger 핵심)
            if (IsValidIngredient(item))
            {
                AcceptIngredient(other.gameObject);
            }
            else
            {
                // 필요한 재료가 아니면 그냥 통과시키거나 튕겨냄 (여기선 일단 정지)
                item.SetTarget(null, 0);
                Debug.LogWarning($"{facilityData.facilityName}: Invalid ingredient {item.name}");
            }
        }
    }

    protected virtual bool IsValidIngredient(FactoryItem item)
    {
        if (facilityData.recipe.inputItems == null || facilityData.recipe.inputItems.Length == 0) return true;

        // 현재 들어온 재료들 중 아직 채워지지 않은 칸이 있는지 확인
        // (간단화를 위해 이름/데이터 비교)
        // 실제로는 필요한 기댓값 리스트와 현재 리스트를 비교해야함
        return true; // 일단은 개수만 맞으면 받도록 처리
    }

    protected virtual void AcceptIngredient(GameObject ingredient)
    {
        if (waitingQueue.Contains(ingredient)) waitingQueue.Remove(ingredient);
        activeIngredients.Add(ingredient);
        
        FactoryItem item = ingredient.GetComponent<FactoryItem>();
        if (item != null) item.SetTarget(null, 0);
        
        int index = activeIngredients.Count - 1;
        if (index < inputPoints.Length)
        {
            ingredient.transform.position = inputPoints[index].position;
        }

        if (activeIngredients.Count == facilityData.recipe.inputItems.Length)
        {
            StartCoroutine(ProcessRoutine());
        }
    }

    protected virtual IEnumerator ProcessRoutine()
    {
        isProcessing = true;
        yield return new WaitForSeconds(facilityData.recipe.processTime);

        foreach (var obj in activeIngredients)
        {
            if (obj != null) Destroy(obj);
        }
        activeIngredients.Clear();

        OutputResult();
        isProcessing = false;

        // 가공 완료 후 대기 아이템 다시 체크
        CheckWaitingQueue();
    }

    private void CheckWaitingQueue()
    {
        waitingQueue.RemoveAll(item => item == null);
        if (waitingQueue.Count > 0)
        {
            // 대기열의 첫 번째 아이템이 다시 수용 가능한지 확인
            GameObject next = waitingQueue[0];
            if (activeIngredients.Count < facilityData.recipe.inputItems.Length)
            {
                AcceptIngredient(next);
            }
        }
    }

    protected virtual void OutputResult()
    {
        if (facilityData.recipe.outputItem != null && outputPoint != null)
        {
            Instantiate(facilityData.recipe.outputItem.prefab, outputPoint.position, outputPoint.rotation);
        }
    }

    private IEnumerator GeneratorRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(facilityData.recipe.processTime);
            OutputResult();
        }
    }
}
