using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseFacility : MonoBehaviour
{
    [Header("Data")]
    public FacilityData facilityData;

    [Header("Ports")]
    public Transform[] inputPoints;
    public Transform outputPoint;

    protected List<GameObject> activeIngredients = new List<GameObject>();
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
        if (facilityData.type == FacilityType.Conveyor || facilityData.type == FacilityType.Seller) return;

        if (other.CompareTag("FactoryItem"))
        {
            if (isProcessing) return;

            // Check if we can accept this ingredient (simplified for now)
            if (activeIngredients.Count < facilityData.recipe.inputItems.Length)
            {
                AcceptIngredient(other.gameObject);
            }
        }
    }

    protected virtual void AcceptIngredient(GameObject ingredient)
    {
        activeIngredients.Add(ingredient);
        
        // Disable movement and position at input point
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
