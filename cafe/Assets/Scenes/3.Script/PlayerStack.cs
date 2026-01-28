using UnityEngine;
using System.Collections.Generic;

public class PlayerStack : MonoBehaviour
{
    public Transform stackParent;
    public List<GameObject> collectedItems = new List<GameObject>();
    public int maxCapacity = 15;
    public float yOffset = 0.3f;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Producer") && collectedItems.Count < maxCapacity)
        {
            if (other.TryGetComponent(out Producer producer))
            {
                GameObject item = producer.GiveItem();
                if (item != null) AddToStack(item);
            }
        }
    }

    void AddToStack(GameObject item)
    {
        collectedItems.Add(item);
        item.transform.SetParent(stackParent);

        // 등에 쌓이는 위치 계산
        Vector3 targetLocalPos = new Vector3(0, (collectedItems.Count - 1) * yOffset, 0);

        // 이동 연출 (부드럽게 촥!)
        item.transform.localPosition = targetLocalPos;
        item.transform.localRotation = Quaternion.identity;

        // 햅틱 효과 대신 작게 점프하는 연출 추가 가능
    }
}