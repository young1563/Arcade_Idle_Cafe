using UnityEngine;

public class ConveyorBelt : BaseFacility
{
    public Transform endpoint; // 벨트의 끝 지점 (프리팹 자식으로 생성 권장)
    public float moveSpeed = 1.5f;

    protected override void Start()
    {
        // Override BaseFacility Start to avoid Generator logic if accidentally set
    }

    // 입구에 아이템이 들어오면 호출
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FactoryItem"))
        {
            FactoryItem item = other.GetComponent<FactoryItem>();
            if (item != null)
            {
                // Disable physics if it was enabled (e.g. from the sell portal pile)
                if (other.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
                
                item.SetTarget(endpoint, moveSpeed);
            }
        }
    }
}